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

    partial class DashboardViewModel : ViewModelBase,INotifyPropertyChanged
    {
        public class BookQuantity
        {
            public string Name { get; set; }
            public int Quantity { get; set; }
            public int Rank  { get; set; }
           
            public BookQuantity(string name,int quantity, int rank)
            {
                Name= name;
                Quantity= quantity;
                Rank = rank;
            }
        }

        public ObservableCollection<BookQuantity> AllBookQuantity { get; private set; }
        private List<Tuple<string,int>> bookQuantityList;
        public ICommand Load_page { get; set; }

        private IStatisticRepository _statisticRepository;
        public IEnumerable<ISeries> TopMonthlyBestSellerSeries { get; set; }
        public ISeries[] MonthRevenuesOfYearSeries { get; set; }
        public string MonthlyRevenue { get; set; }
        public int NumberOfSoldBooks { get; set; }
        public int NumberOfBooks { get; set; }

        public LabelVisual TopMonthlyBestSellerTitle { get; set; } =
            new LabelVisual
            {
                Text = "Top 5 best selling books of the month",
                TextSize = 15,

                Padding = new LiveChartsCore.Drawing.Padding(15),
                Paint = new SolidColorPaint(SKColors.DarkSlateGray)
            };

        [Obsolete]
        public DashboardViewModel()
        {
            SaveCurrentPage();

            _statisticRepository = new StatisticRepository();
            AllBookQuantity = new ObservableCollection<BookQuantity>();
            Load_page = new RelayCommand(Load_Dashboard);
        }

        private void SaveCurrentPage()
        {
            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (Convert.ToBoolean(ConfigurationManager.AppSettings["RememberPage"]))
            {
                configuration.AppSettings.Settings["CurrentPage"].Value = "DashboardPage";
            }
            else
            {
                configuration.AppSettings.Settings["CurrentPage"].Value = "DashboardPage";
            }

            configuration.Save(ConfigurationSaveMode.Full);
            ConfigurationManager.RefreshSection("appSettings");
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


            var monthlyRevenueTask = await _statisticRepository.GetMonthlyStatistic(startMonthlyDate.Date, DateTimeOffset.Now.Date);
            if (monthlyRevenueTask == null)
            { 
                await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                // purpose: continue flow
                monthlyRevenueTask = new List<Tuple<DateTime, int>>();
            }

            MonthlyRevenue = monthlyRevenueTask.Last().Item2.ToString("C", CultureInfo.GetCultureInfo("vi-VN"));

            //weekly revenue
            DateTime startWeeklyDate = DateTime.Parse(year_month_day);
            var getWeekTask = await _statisticRepository.GetListOfWeeks();
            if (getWeekTask == null)
            {
                await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                // purpose: continue flow
                getWeekTask = new List<Tuple<int, DateTime>>();
            }

            if (getWeekTask.Count - 2 >= 0)
            {
                startWeeklyDate = getWeekTask[getWeekTask.Count - 2].Item2;
            }
            else
            {
                startWeeklyDate = DateTime.Now;
            }

            //daily number of sold books
            NumberOfSoldBooks = await _statisticRepository.GetCurrentMonthlyNumberOfSoldBookStatistic();

            //daily number of orders
            NumberOfBooks = await _statisticRepository.GetNumberOfBooks();

            //top 5 best selling books of the month
            var top5MonthlyBook = await _statisticRepository.GetTop5ProductStatistic(startMonthlyDate.Date, DateTimeOffset.Now.Date);
            if (top5MonthlyBook == null)
            {
                await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                // purpose: continue flow
                top5MonthlyBook = new List<Tuple<string, int>>();
            }

            if (top5MonthlyBook == null)
            {
                top5MonthlyBook = new List<Tuple<string, int>>();
                top5MonthlyBook.Add(new Tuple<string, int>("None", 1));
            }

            TopMonthlyBestSellerSeries = top5MonthlyBook.AsLiveChartsPieSeries((value, series) =>
            {
                // here you can configure the series assigned to each value.
                series.Name = $"{value.Item1}";
                series.DataLabelsPaint = new SolidColorPaint(new SKColor(30, 30, 30));
                series.DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer;
                series.DataLabelsSize = 10;
                series.Mapping = (value, point) => { point.PrimaryValue = value.Item2; };
                series.LegendShapeSize = 40;
                series.DataLabelsFormatter = p => $"{value.Item1} {p.StackedValue.Share:P2}";
            });

            // Statistic about the revenue of this year.
            var monthRevenues = await _statisticRepository.GetMonthRevenuesOfYear();
            if (monthRevenues == null)
            {
                for (int i = 1; i <= DateTime.Now.Month; i++) {
                    monthRevenues.Add(new Tuple<DateTime, int>(new DateTime(DateTime.Now.Year, i, 1, 0, 0, 0), 0));
                }
            }

            Collection<ObservablePoint> collection = new Collection<ObservablePoint>();
            monthRevenues.ForEach(item =>
            {
                collection.Add(new ObservablePoint(item.Item1.Month, item.Item2));
            });

            MonthRevenuesOfYearSeries = new ISeries[] {
                 new LineSeries<ObservablePoint>
                    {
                        Values = collection
                    }
            };

            //books running out of stock
            updateBookQuantityList();
        }


        private async void updateBookQuantityList()
        {
            bookQuantityList = await _statisticRepository.GetProductQuantityStatistic();
            if (bookQuantityList == null)
            { 
                await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                // purpose: continue flow
                bookQuantityList = new List<Tuple<string, int>>();
            }

            bookQuantityList = bookQuantityList.OrderBy(x => x.Item2).Take(5).ToList(); ;
            for (int i = 0; i < bookQuantityList.Count(); i++) {
                AllBookQuantity.Add(new BookQuantity(bookQuantityList[i].Item1, bookQuantityList[i].Item2, i + 1));
            }
        }
    }
}
