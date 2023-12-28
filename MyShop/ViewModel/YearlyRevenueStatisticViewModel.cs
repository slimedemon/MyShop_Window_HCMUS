using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore;
using Microsoft.UI.Xaml.Controls;
using MyShop.Repository;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using LiveChartsCore.SkiaSharpView;
using System.Globalization;
using MyShop.Services;

namespace MyShop.ViewModel
{
    class YearlyRevenueStatisticViewModel : ViewModelBase
    {
        //public DateTime
        public ICommand StartDateChangeCommand { get; private set; }
        public ICommand EndDateChangeCommand { get; private set; }
        private DateTime SelectedStartDate;
        private DateTime SelectedEndDate;
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        private StatisticRepository _statisticRepository;
        public List<ISeries> YearlyRevenueSeries { get; private set; }

        public Axis[] XAxes { get; set; } =
       {
            new Axis
            {
                Name = "Year",
                MinStep = TimeSpan.TicksPerDay*365,

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
                    return $"{date.Year}";
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

        public YearlyRevenueStatisticViewModel()
        {
            YearlyRevenueSeries = new List<ISeries>();
            _statisticRepository = new StatisticRepository();

            Initialize();

            StartDateChangeCommand = new RelayCommand<DatePickerValueChangedEventArgs>(OnStartDateChanged);
            EndDateChangeCommand = new RelayCommand<DatePickerValueChangedEventArgs>(OnEndDateChanged);
        }

        private async void Initialize() 
        {
            var task = await _statisticRepository.GetListOfWeeks();
            if (task == null)
            { 
                await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                // purpose: continue flow
                task = new List<Tuple<int, DateTime>>();
            }

            DateTime date;
            if (task.Count() > 0) date = task[0].Item2;
            else date = DateTime.Now;

            StartDate = new DateTimeOffset(new DateTime(date.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            EndDate = DateTimeOffset.Now;
            SelectedStartDate = new DateTime(date.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            SelectedEndDate = DateTime.Now;

            DisplayChart();
        }

        private async void DisplayChart()
        {
            var task = await _statisticRepository.GetYearlyStatistic(SelectedStartDate.Date, SelectedEndDate.Date);
            if (task == null)
            { 
                await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                // purpose: continue flow
                task = new List<Tuple<DateTime, int>>();
            }

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

            YearlyRevenueSeries.Clear();
            YearlyRevenueSeries.Add(series);

            XAxes[0].Name = $"Revenue from {SelectedStartDate.Date.ToShortDateString()} to {SelectedEndDate.Date.ToShortDateString()}";

            XAxes[0].Labels = null;
        }
        
        private void OnStartDateChanged(DatePickerValueChangedEventArgs e)
        {
            char seperator = '/';
            int day = 1;
            int month = 1;
            int year = StartDate.Date.Year;

            String year_month_day = new StringBuilder().Append(year).Append(seperator).Append(month).Append(seperator).Append(day).ToString();

            SelectedStartDate = DateTime.Parse(year_month_day);

            if(SelectedStartDate < SelectedEndDate)
            {
                DisplayChart();
            }
        }

        private void OnEndDateChanged(DatePickerValueChangedEventArgs e)
        {
            char seperator = '/';
            int day = 1;
            int month = 1;
            int year = EndDate.Date.Year;

            String year_month_day = new StringBuilder().Append(year).Append(seperator).Append(month).Append(seperator).Append(day).ToString();

            SelectedEndDate = DateTime.Parse(year_month_day);

            if (SelectedStartDate < SelectedEndDate)
            {
                DisplayChart();
            }
        }

    }
}
