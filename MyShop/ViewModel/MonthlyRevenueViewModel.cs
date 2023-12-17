using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI.Xaml.Controls;
using MyShop.Repository;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace MyShop.ViewModel
{
    class MonthlyRevenueViewModel : ViewModelBase
    {
        private StatisticRepository _statisticRepository;

        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set;}

        public ICommand StartDateChangeCommand { get; set; }
        public ICommand EndDateChangeCommand { get; set; }

        public ObservableCollection<ISeries> MonthlyRevenueSeries { get; set; }

        public Axis[] XAxes { get; set; } =
       {
            new Axis
            {
                Name = "Date",
                MinStep = TimeSpan.TicksPerDay*30,

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
                    return $"{date.Month}.{date.Year}";
                }
            }
        };

        public Axis[] YAxes { get; set; } =
        {
        new Axis
        {
            Name = "Revenue (VND)",
            NamePadding = new LiveChartsCore.Drawing.Padding(0, 15),
            MinLimit=0,
            LabelsPaint = new SolidColorPaint
            {
                Color = SKColors.Blue,
                FontFamily = "Times New Roman",
                SKFontStyle = new SKFontStyle(SKFontStyleWeight.ExtraBold, SKFontStyleWidth.Normal, SKFontStyleSlant.Italic)
            },

            Labeler = (value) => value.ToString("C", CultureInfo.GetCultureInfo("vi-VN"))
        }
    };

        DateTime SelectedStartDate;
        DateTime SelectedEndDate;

        public MonthlyRevenueViewModel()
        {
            _statisticRepository = new StatisticRepository();
            MonthlyRevenueSeries = new ObservableCollection<ISeries>();

            var date = DateTimeOffset.Now;
            StartDate = new DateTimeOffset(new DateTime(date.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            EndDate = DateTimeOffset.Now;

            SelectedStartDate = new DateTime(date.Year, 1, 1, 0, 0, 0);
            SelectedEndDate = DateTime.Now;
            StartDateChangeCommand = new RelayCommand<DatePickerValueChangedEventArgs>(OnStartDateChange);
            EndDateChangeCommand = new RelayCommand<DatePickerValueChangedEventArgs>(OnEndDateChange);

            DisplayChart();
        }

        private async void DisplayChart()
        {
            var task = await _statisticRepository.GetMonthlyStatistic(SelectedStartDate.Date, SelectedEndDate.Date);

            var series = new LineSeries<Tuple<DateTime, int>>()
            {
                GeometryStroke = null,
                GeometryFill = null,
                Mapping = (taskItem, point) =>
                {
                    point.PrimaryValue = (int)taskItem.Item2;
                    point.SecondaryValue = taskItem.Item1.Ticks;
                },
                TooltipLabelFormatter = point => $"{point.Model.Item1.ToShortDateString()} revenue: {point.PrimaryValue.ToString("C", CultureInfo.GetCultureInfo("vi-VN"))}"
            };
           
            series.Values = task;

            MonthlyRevenueSeries.Clear();
            MonthlyRevenueSeries.Add(series);

            XAxes[0].Name = $"Revenue from {SelectedStartDate.Date.ToShortDateString()} to {SelectedEndDate.Date.ToShortDateString()}";

            XAxes[0].Labels = null;
        }

        private void OnStartDateChange(DatePickerValueChangedEventArgs a)
        {
            char seperator = '/';
            int day = 1;
            int month = StartDate.Date.Month;
            int year = StartDate.Date.Year;
            string year_month_day = new StringBuilder().Append(year).Append(seperator).Append(month).Append(seperator).Append(day).ToString();
            SelectedStartDate = DateTime.Parse(year_month_day);
            if (SelectedStartDate.Date < SelectedEndDate.Date)
            {
                DisplayChart();
            }
        }

        private void OnEndDateChange(DatePickerValueChangedEventArgs a)
        {
            char seperator = '/';
            int day = 1;
            int month = EndDate.Date.Month;
            int year = EndDate.Date.Year;
            string year_month_day = new StringBuilder().Append(year).Append(seperator).Append(month).Append(seperator).Append(day).ToString();
            SelectedEndDate = DateTime.Parse(year_month_day);
            if (SelectedStartDate.Date < SelectedEndDate.Date)
            {
                DisplayChart();
            }
        }


    }
}
