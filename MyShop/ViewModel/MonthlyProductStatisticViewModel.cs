using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using MyShop.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView;
using System.Collections.ObjectModel;
using CommunityToolkit.WinUI.UI.Controls.TextToolbarSymbols;
using Windows.UI.Shell;
using MyShop.Services;

namespace MyShop.ViewModel
{
    class MonthlyProductStatisticViewModel : ViewModelBase
    {
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }

        public ICommand StartDateChangeCommand { get; set; }
        public ICommand EndDateChangeCommand { get; set; }

        DateTime SelectedStartDate;
        DateTime SelectedEndDate;
        private StatisticRepository _statisticRepository;
        static public Dictionary<int, String> NameBookDic;

        public ObservableCollection<ISeries> MonthlyProductSeries { get; set; } 

        public Axis[] XAxes { get; set; } =
        {
            new Axis
            {
                Name = "",
                NamePadding = new LiveChartsCore.Drawing.Padding(0, 15),
                MinStep = 1,
                LabelsPaint = new SolidColorPaint
                {
                    Color = SKColors.Blue,
                    FontFamily = "Times New Roman",
                    SKFontStyle = new SKFontStyle(SKFontStyleWeight.ExtraLight, SKFontStyleWidth.Normal, SKFontStyleSlant.Italic)
                },
                Labeler = (value) => { String name = null; if(NameBookDic.TryGetValue((int)value,out name)) return name; return ""; }
            }
        };

        public Axis[] YAxes { get; set; } =
        {
        new Axis
        {
            Name = "Books",
            NamePadding = new LiveChartsCore.Drawing.Padding(0, 15),
            MinLimit=0,
            LabelsPaint = new SolidColorPaint
            {
                Color = SKColors.Blue,
                FontFamily = "Times New Roman",
                SKFontStyle = new SKFontStyle(SKFontStyleWeight.ExtraBold, SKFontStyleWidth.Normal, SKFontStyleSlant.Italic)
            }
        }
    };

        public MonthlyProductStatisticViewModel()
        {
            NameBookDic = new Dictionary<int, String>();
            MonthlyProductSeries = new ObservableCollection<ISeries>();
            _statisticRepository = new StatisticRepository();

            var date = DateTimeOffset.Now;
            StartDate = new DateTimeOffset(new DateTime(date.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            EndDate = DateTimeOffset.Now;

            SelectedStartDate = new DateTime(date.Year, 1, 1, 0, 0 ,0);
            SelectedEndDate = DateTime.Now;
            StartDateChangeCommand = new RelayCommand<DatePickerValueChangedEventArgs>(OnStartDateChange);
            EndDateChangeCommand = new RelayCommand<DatePickerValueChangedEventArgs>(OnEndDateChange);

            DisplayChart();
        }

        private async void DisplayChart()
        {
            NameBookDic.Clear();
            var task = await _statisticRepository.GetProductStatistic(SelectedStartDate.Date, SelectedEndDate.Date);
            if (task == null)
            { 
                await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                // purpose: continue flow;
                task = new List<Tuple<string, int>>();
            }

            var series = new ColumnSeries<Tuple<string, int>>()
            {
                Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 2 },
                Fill = new SolidColorPaint(SKColors.Blue),

                Mapping = (taskItem, point) =>
                {
                    point.PrimaryValue = (int)taskItem.Item2;
                    point.SecondaryValue = point.Context.Index;
                    NameBookDic.TryAdd(point.Context.Index, taskItem.Item1);
                },
                TooltipLabelFormatter = point => $"{point.Model.Item1.ToString()}: {point.PrimaryValue.ToString()}"
            };

            series.Values = task;
            MonthlyProductSeries.Clear();
            MonthlyProductSeries.Add(series);

            XAxes[0].Name = $"Number of sold books from {SelectedStartDate.Date.ToShortDateString()} to {SelectedEndDate.Date.ToShortDateString()}";
        }

        private void OnStartDateChange(DatePickerValueChangedEventArgs a)
        {
            char seperator = '/';
            int day = 1;
            int month = StartDate.Date.Month;
            int year = StartDate.Date.Year;
            String year_month_day = new StringBuilder().Append(year).Append(seperator).Append(month).Append(seperator).Append(day).ToString();

            SelectedStartDate = DateTime.Parse(year_month_day);

            if (SelectedStartDate < SelectedEndDate)
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
