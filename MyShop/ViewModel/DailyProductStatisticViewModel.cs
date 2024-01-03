using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using Microsoft.UI.Xaml.Controls;
using MyShop.Model;
using MyShop.Repository;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections;
using CommunityToolkit.WinUI.UI.Controls.TextToolbarSymbols;
using System.Collections.ObjectModel;

namespace MyShop.ViewModel
{
    class DailyProductStatisticViewModel : ViewModelBase
    {
        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset EndDate { get; set; }

        public ICommand DateChangeCommand { get; set; }

        private IStatisticRepository _statisticRepository;
       
        static public Dictionary<int, string> NameBookDic;

        public ObservableCollection<ISeries> DailyProductSeries { get; set; } 

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
            },
        }
    };

        public DailyProductStatisticViewModel()
        {
            DailyProductSeries = new ObservableCollection<ISeries>();
            NameBookDic = new Dictionary<int, string>();
            var date = DateTime.Now;
            StartDate = new DateTimeOffset(new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc));
            EndDate = DateTimeOffset.Now;
            _statisticRepository = new StatisticRepository();
            DateChangeCommand = new RelayCommand<CalendarDatePickerDateChangedEventArgs>(OnDateChange);
            DisplayChart();
        }

        private async void DisplayChart()
        {
            NameBookDic.Clear();
            var task = await _statisticRepository.GetProductStatistic(StartDate.Date, EndDate.Date);
            if (task == null)
            {
                task = new List<Tuple<string, int>>();
            }

            var series = new ColumnSeries<Tuple<string, int>>()
            {
                Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 2 },
                Values = task,

                Fill = new SolidColorPaint(SKColors.Blue),

                Mapping = (taskItem, point) =>
                {
                    point.PrimaryValue = (int)taskItem.Item2;
                    point.SecondaryValue = point.Context.Index;
                    NameBookDic.TryAdd(point.Context.Index, taskItem.Item1);
                },
                TooltipLabelFormatter = point => $"{point.Model.Item1.ToString()}: {point.PrimaryValue.ToString()}"
            };
           
            DailyProductSeries.Clear();
            DailyProductSeries.Add(series);

            XAxes[0].Name = $"Number of sold books from {StartDate.Date.ToShortDateString()} to {EndDate.Date.ToShortDateString()}";

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
