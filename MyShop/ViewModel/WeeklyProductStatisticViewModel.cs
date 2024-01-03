using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using MyShop.Model;
using MyShop.Repository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView;
using CommunityToolkit.WinUI.UI.Controls.TextToolbarSymbols;
using MyShop.Services;

namespace MyShop.ViewModel
{
    class WeeklyProductStatisticViewModel : ViewModelBase
    {
        public ObservableCollection<Tuple<int, DateTime>> ListOfWeeks { get; private set; }

        private IStatisticRepository _statisticRepository;

        public ICommand Load_page { get; set; }

        public ICommand OnSelectionChangedOfStartDate { get; set; }
        public ICommand OnSelectionChangedOfEndDate { get; set; }

        public int SelectedIndex_StartDate { get; set; }

        public int SelectedIndex_EndDate { get; set; }

        public ObservableCollection<ISeries> WeeklyProductSeries { get; set; }
        static public Dictionary<int, string> NameBookDic;

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

                Labeler = (value) => {
                    String name = null;
                    if(NameBookDic.TryGetValue((int)value,out name)) 
                        return name; 
                    return ""; 
                }
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

        public WeeklyProductStatisticViewModel()
        {
            WeeklyProductSeries = new ObservableCollection<ISeries>();
            NameBookDic = new Dictionary<int, string>();
            _statisticRepository = new StatisticRepository();
           
            SelectedIndex_StartDate = 0;
            SelectedIndex_EndDate = 0;
            ListOfWeeks = new ObservableCollection<Tuple<int, DateTime>>();
            Load_page = new RelayCommand<RoutedEventArgs>(Load_ListOfWeeks);
            OnSelectionChangedOfStartDate = new RelayCommand<SelectionChangedEventArgs>(SelectionChangedOfStartDate);
            OnSelectionChangedOfEndDate = new RelayCommand<SelectionChangedEventArgs>(SelectionChangedOfEndDate);
        }


        private async void DisplayChart()
        {
            NameBookDic.Clear();
            DateTime startDate = ListOfWeeks[SelectedIndex_StartDate].Item2;
            DateTime endDate = ListOfWeeks[SelectedIndex_EndDate].Item2;

            var task = await _statisticRepository.GetProductStatistic(startDate.Date, endDate.Date);
            if (task == null)
            {
                await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                // purpose: continue flow
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
            WeeklyProductSeries.Clear();
            WeeklyProductSeries.Add(series);

            XAxes[0].Name = $"Number of sold books from week {ListOfWeeks[SelectedIndex_StartDate].Item1.ToString()} to week {ListOfWeeks[SelectedIndex_EndDate].Item1.ToString()}";


        }

        private async void Load_ListOfWeeks(RoutedEventArgs e)
        {
            var task = await _statisticRepository.GetListOfWeeks();
            if (task == null)
            {
                await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                // purpose: continue flow
                task = new List<Tuple<int, DateTime>>();
            }

            ListOfWeeks.Clear();

            task.ForEach(taskItem =>
            {
                ListOfWeeks.Add(taskItem);
            });

            SelectedIndex_StartDate = 0;
            SelectedIndex_EndDate = ListOfWeeks.Count()-1;
        }

        private void SelectionChangedOfStartDate(SelectionChangedEventArgs e)
        {
            if (SelectedIndex_StartDate < SelectedIndex_EndDate)
            {
                DisplayChart();
            }
        }

        private void SelectionChangedOfEndDate(SelectionChangedEventArgs e)
        {
            if (SelectedIndex_StartDate < SelectedIndex_EndDate)
            {
                DisplayChart();
            }
        }
    }
}