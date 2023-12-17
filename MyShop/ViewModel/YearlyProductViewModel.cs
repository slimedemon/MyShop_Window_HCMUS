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
using CommunityToolkit.WinUI.UI.Controls.TextToolbarSymbols;

namespace MyShop.ViewModel
{
    class YearlyProductViewModel : ViewModelBase
    {
        public ICommand StartDateChangeCommand { get; private set; }
        public ICommand EndDateChangeCommand { get; private set; }

        private DateTime SelectedStartDate;

        private DateTime SelectedEndDate;

        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset EndDate { get; set; }

        private StatisticRepository _statisticRepository;

        public List<ISeries> YearlyProductSeries { get; set; }
        static public Dictionary<int, String> NameBookDic;

        public Axis[] XAxes { get; set; } =
        {
            new Axis
            {
                Name = "",
                NamePadding = new LiveChartsCore.Drawing.Padding(0, 15),

                LabelsPaint = new SolidColorPaint
                {
                    Color = SKColors.Blue,
                    FontFamily = "Times New Roman",
                    SKFontStyle = new SKFontStyle(SKFontStyleWeight.ExtraLight, SKFontStyleWidth.Normal, SKFontStyleSlant.Italic)
                },
                Labeler = (value) => { String name = null; if(NameBookDic.TryGetValue((int)value,out name)) return name; return "No name"; }
            }
        };

        public Axis[] YAxes { get; set; } =
        {
        new Axis
        {
            Name = "",
            NamePadding = new LiveChartsCore.Drawing.Padding(0, 15),
            MinLimit=0,
            LabelsPaint = new SolidColorPaint
            {
                Color = SKColors.Blue,
                FontFamily = "Times New Roman",
                SKFontStyle = new SKFontStyle(SKFontStyleWeight.ExtraBold, SKFontStyleWidth.Normal, SKFontStyleSlant.Italic)
            },
        }
    };

        public YearlyProductViewModel()
        {
            NameBookDic = new Dictionary<int, String>();
            YearlyProductSeries = new List<ISeries>();
            _statisticRepository = new StatisticRepository();

            Initialize();

            StartDateChangeCommand = new RelayCommand<DatePickerValueChangedEventArgs>(OnStartDateChanged);
            EndDateChangeCommand = new RelayCommand<DatePickerValueChangedEventArgs>(OnEndDateChanged);
        }

        private async void Initialize()
        {
            var task = await _statisticRepository.GetListOfWeeks();
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
            NameBookDic.Clear();
            var task = await _statisticRepository.GetProductStatistic(SelectedStartDate.Date, SelectedEndDate.Date);

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

            YearlyProductSeries.Clear();
            YearlyProductSeries.Add(series);

            XAxes[0].Name = $"Number of sold books from {SelectedStartDate.Date.ToShortDateString()} to {SelectedEndDate.Date.ToShortDateString()}";

          
            
        }

        private void OnStartDateChanged(DatePickerValueChangedEventArgs e)
        {
            char seperator = '/';
            int day = 1;
            int month = 1;
            int year = StartDate.Date.Year;

            String year_month_day = new StringBuilder().Append(year).Append(seperator).Append(month).Append(seperator).Append(day).ToString();

            SelectedStartDate = DateTime.Parse(year_month_day);

            if (SelectedStartDate < SelectedEndDate)
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
