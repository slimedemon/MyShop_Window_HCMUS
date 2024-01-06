using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyShop.Model;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using MyShop.Repository;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;
using Windows.ApplicationModel.Contacts;
using Windows.ApplicationModel.VoiceCommands;
using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using SkiaSharp;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.Drawing;
using System.Windows.Markup;
using MyShop.Services;
using System.Configuration;

namespace MyShop.ViewModel
{
    partial class BestSellerStatisticsViewModel : ViewModelBase,INotifyPropertyChanged
    {
        public ICommand Load_page { get; set; }

        private IStatisticRepository _statisticRepository;
        public List<ISeries> TopYearlyBestSellerSeries { get; set; }
        public IEnumerable<ISeries> TopMonthlyBestSellerSeries { get; set; }
        public ISeries[] TopWeeklyBestSellerSeries { get; set; }


        [ObservableProperty]
        private Axis[] _xAxes = {
            new Axis {
                        SeparatorsPaint = new SolidColorPaint(new SKColor(220, 220, 220)),
                        Name="Top 5 best selling books of the week",
                        NameTextSize=15
            },

        };

        [ObservableProperty]
        private Axis[] _yAxes = { 
            new Axis 
            { 
                IsVisible = false,
            }
        };

        public LabelVisual TopMonthlyBestSellerTitle { get; set; } =
            new LabelVisual
            {
                Text = "Top 5 best selling books of the month",
                TextSize = 15,

                Padding = new LiveChartsCore.Drawing.Padding(15),
                Paint = new SolidColorPaint(SKColors.DarkSlateGray)
            };


        public Axis[] YearlyXAxes { get; set; } =
        {
            new Axis
            {
                Name = "Top 5 best selling books of the year",
                NameTextSize=15,
               Labels = null,
                TextSize=10
            }
        };

        public Axis[] YearlyYAxes { get; set; } =
        {
        new Axis
        {
            Name = "",
            NamePadding = new LiveChartsCore.Drawing.Padding(0, 15),

            LabelsPaint = new SolidColorPaint
            {
                Color = SKColors.Blue,
                FontFamily = "Times New Roman",
                SKFontStyle = new SKFontStyle(SKFontStyleWeight.ExtraBold, SKFontStyleWidth.Normal, SKFontStyleSlant.Italic)
            },
        }
    };

        [Obsolete]
        public BestSellerStatisticsViewModel()
        {
            _statisticRepository = new StatisticRepository();
            Load_page = new RelayCommand(Load_Dashboard);
        }

        private async void Load_Dashboard()
        {
            //monthly revenue
            char seperator = '/';
            int day = 1;
            int month = DateTimeOffset.Now.Month;
            int year = DateTimeOffset.Now.Year;

            String year_month_day = new StringBuilder().Append(year).Append(seperator).Append(month).Append(seperator).Append(day).ToString();
            DateTime startMonthlyDate = DateTime.Parse(year_month_day);

            //weekly revenue
            DateTime startWeeklyDate = DateTime.Parse(year_month_day);
            var getWeekTask = await _statisticRepository.GetListOfWeeks();
            if (getWeekTask == null)
            {
                await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                // purpose: continue flow
                getWeekTask = new List<Tuple<int, DateTime>>();
            }

            //top 5 best selling books of the week
            var top5WeeklyBook = await _statisticRepository.GetTop5ProductStatistic(startWeeklyDate.Date, DateTimeOffset.Now.Date);
            if (top5WeeklyBook == null)
            { 
                await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                // purpose: continue flow
                top5WeeklyBook = new List<Tuple<string, int>>();
            }

            TopWeeklyBestSellerSeries = top5WeeklyBook
                .Select(x => new RowSeries<ObservableValue>
                {
                    Values = new[] { new ObservableValue(x.Item2) },
                    Name = x.Item1,
                    Stroke = null,
                    MaxBarWidth = 80,
                    DataLabelsSize = 10,
                    DataLabelsPaint = new SolidColorPaint(new SKColor(245, 245, 245)),
                    DataLabelsPosition = DataLabelsPosition.Right,
                    DataLabelsTranslate = new LvcPoint(-1, 0),
                    DataLabelsFormatter = point => $"{point.Context.Series.Name} {point.PrimaryValue}"
                })
                .OrderByDescending(x => ((ObservableValue[])x.Values!)[0].Value)
                .ToArray();

            //top 5 best selling books of the month
            var top5MonthlyBook = await _statisticRepository.GetTop5ProductStatistic(startMonthlyDate.Date, DateTimeOffset.Now.Date);

            if (top5MonthlyBook == null)
            {
                await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                // purpose: continue flow
                top5MonthlyBook = new List<Tuple<string, int>>();
            }

            TopMonthlyBestSellerSeries = top5MonthlyBook.AsLiveChartsPieSeries((value, series) =>
            {
                // here you can configure the series assigned to each value.
                series.Name = $"{value.Item1}";
                series.DataLabelsPaint = new SolidColorPaint(new SKColor(30, 30, 30));
                series.DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer;
                series.DataLabelsSize = 10;
                //series.DataLabelsRotation = LiveCharts.TangentAngle + 90;
                series.Mapping = (value, p) => {
                    p.PrimaryValue = value.Item2;

                };
                series.LegendShapeSize = 40;
                series.DataLabelsFormatter = p => $"{value.Item1} {p.StackedValue.Share:P2}";
            });

            //top 5 best selling books of the year
            month = 1;
            year_month_day = new StringBuilder().Append(year).Append(seperator).Append(month).Append(seperator).Append(day).ToString();
            DateTime startYearlyDate = DateTime.Parse(year_month_day);
            var top5YearlyBook = await _statisticRepository.GetTop5ProductStatistic(startYearlyDate.Date, DateTimeOffset.Now.Date);
            if (top5YearlyBook == null)
            {
                await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                // purpose: continue flow
                top5YearlyBook = new List<Tuple<string, int>>();
            }

            List<string> labels = new List<string>();

            top5YearlyBook.ForEach(book =>
            {
                labels.Add(book.Item1);
            });

            YearlyXAxes[0].Labels = labels;
            TopYearlyBestSellerSeries = new List<ISeries>
            {
                new ColumnSeries<Tuple<string, int>>
                {
                    Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 2 },
                    Values = top5YearlyBook,

                    Fill = new SolidColorPaint(SKColors.Blue),

                    Mapping = (taskItem, point) =>
                    {
                        point.PrimaryValue = (int)taskItem.Item2;
                        point.SecondaryValue = point.Context.Index;
                    },
                    TooltipLabelFormatter = point => $"{point.Model.Item1.ToString()}: {point.PrimaryValue.ToString()}"
                }
            };
        }
    }
}
