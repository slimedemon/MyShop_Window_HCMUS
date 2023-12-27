using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using MyShop.Model;
using MyShop.Repository;
using MyShop.Services;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace MyShop.ViewModel
{


    class WeeklyRevenueViewModel : ViewModelBase
    {

        public ObservableCollection<Tuple<int, DateTime>> ListOfWeeks { get; private set; }

        private IStatisticRepository _statisticRepository;

        public ObservableCollection<ISeries> WeeklyRevenueSeries { get; private set; }

        public ICommand Load_page { get; set; }

        public ICommand OnSelectionChangedOfStartDate { get; set; }
        public ICommand OnSelectionChangedOfEndDate { get; set; }

        public int SelectedIndex_StartDate { get; set; }

        public int SelectedIndex_EndDate { get; set; }

        public Axis[] XAxes { get; set; } =
       {
            new Axis
            {
                Name = "Weeks",
                MinStep = TimeSpan.TicksPerDay*7,
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
                Name = "Revenue (VND)",
                MinLimit = 0,
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

        public WeeklyRevenueViewModel()
        {
            _statisticRepository = new StatisticRepository();
            WeeklyRevenueSeries = new ObservableCollection<ISeries>();

           
            SelectedIndex_StartDate = 0;
            SelectedIndex_EndDate = 0;
            ListOfWeeks = new ObservableCollection<Tuple<int, DateTime>>();
            Load_page = new RelayCommand<RoutedEventArgs>(Load_ListOfWeeks);
            OnSelectionChangedOfStartDate = new RelayCommand<SelectionChangedEventArgs>(SelectionChangedOfStartDate);
            OnSelectionChangedOfEndDate = new RelayCommand<SelectionChangedEventArgs>(SelectionChangedOfEndDate);
        }


        private async void DisplayChart()
        {
            DateTime startDate = ListOfWeeks[SelectedIndex_StartDate].Item2;
            DateTime endDate = ListOfWeeks[SelectedIndex_EndDate].Item2;

            var task = await _statisticRepository.GetWeeklyStatistic(startDate.Date, endDate.Date);
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

            WeeklyRevenueSeries.Clear();
            WeeklyRevenueSeries.Add(series);

            XAxes[0].Name = $"Revenue from {startDate.Date.ToShortDateString()} to {endDate.Date.ToShortDateString()}";
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
            SelectedIndex_EndDate = ListOfWeeks.Count() - 1;
        }

        private void SelectionChangedOfStartDate(SelectionChangedEventArgs e)
        {
            /*            MessageBox.Show("Selected Item: " + ListOfWeeks[SelectedIndex_StartDate].Item2);*/
            if (SelectedIndex_StartDate < SelectedIndex_EndDate)
            {
                DisplayChart();
            }
        }

        private void SelectionChangedOfEndDate(SelectionChangedEventArgs e)
        {
            /*            MessageBox.Show("Selected Item: " + ListOfWeeks[SelectedIndex_EndDate].Item2);*/
            if (SelectedIndex_StartDate < SelectedIndex_EndDate)
            {
                DisplayChart();
            }
        }
    }
}
