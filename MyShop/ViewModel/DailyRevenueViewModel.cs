using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Windows.Foundation;
using SkiaSharp;
using LiveChartsCore.Defaults;
using System.Collections.ObjectModel;
using Windows.Services.TargetedContent;
using System.Globalization;
using MyShop.Model;
using Windows.ApplicationModel.Store;
using LiveChartsCore.SkiaSharpView.Painting;

namespace MyShop.ViewModel
{
    class DailyRevenueViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public ICommand DateChangeCommand { get; set; }
        private IStatisticRepository _statisticRepository;
        
        public ObservableCollection<ISeries> DailyRevenueSeries { get; set; }

        public Axis[] XAxes { get; set; } =
        {
            new Axis
            {
                Name = "Date",
                MinStep = TimeSpan.TicksPerDay,
                
                LabelsPaint = new SolidColorPaint
                {
                    Color = SKColors.Blue,
                    FontFamily = "Times New Roman",
                    SKFontStyle = new SKFontStyle(SKFontStyleWeight.ExtraLight, SKFontStyleWidth.Normal, SKFontStyleSlant.Italic)
                },

                Labeler = (value) => {
                    long v = (long) value;
                    if( v < DateTime.MinValue.Ticks || v > DateTime.MaxValue.Ticks) return "";
                    var date = (new DateTime(v));
                    return $"{date.Day}.{date.Month}.{date.Year}";
                }
            }
        };

        public Axis[] YAxes { get; set; } =
        {
            new Axis
            {
                MinLimit = 0,
                Name = "Revenue (VND)",
                NamePadding = new LiveChartsCore.Drawing.Padding(0, 15),
                LabelsPaint = new SolidColorPaint
                {
                    Color = SKColors.Blue,
                    FontFamily = "Times New Roman",
                    SKFontStyle = new SKFontStyle(SKFontStyleWeight.ExtraBold, SKFontStyleWidth.Normal, SKFontStyleSlant.Italic)
                },

                Labeler = (value) => value.ToString("C", CultureInfo.GetCultureInfo("vi-VN"))
            }
        };

        public DailyRevenueViewModel()
        {
            DateTime date = DateTime.Now;
            StartDate = new DateTimeOffset(new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc));
            EndDate = DateTimeOffset.Now;
            DateChangeCommand = new RelayCommand<CalendarDatePickerDateChangedEventArgs>(OnDateChange);
            _statisticRepository = new StatisticRepository();
            DailyRevenueSeries = new ObservableCollection<ISeries>();
            DisplayChart();
        }

        private async void DisplayChart()
        {
            var dailyStatistic = await _statisticRepository.GetDailyStatistic(StartDate.Date, EndDate.Date);
            var collection = new Collection<DateTimePoint>();

            dailyStatistic.ForEach(item =>
            {
                collection.Add(new DateTimePoint(item.Item1, item.Item2));
            });

            DailyRevenueSeries.Clear();
            DailyRevenueSeries.Add(new LineSeries<DateTimePoint>()
            {
                Values = collection,
                GeometryStroke = null,
                GeometryFill = null,
                TooltipLabelFormatter = point => $"{point.Model.DateTime.ToShortDateString()} revenue: {point.PrimaryValue.ToString("C", CultureInfo.GetCultureInfo("vi-VN"))}"
            });

            XAxes[0].Name = $"Revenue from {StartDate.Date.ToShortDateString()} to {EndDate.Date.ToShortDateString()}";
        }

        private void OnDateChange(CalendarDatePickerDateChangedEventArgs args)
        {
            if (StartDate.Date < EndDate.Date)
            {
                DisplayChart();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}

