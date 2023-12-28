using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyShop.ViewModel
{
    public class RevenueStatisticsViewModel: ViewModelBase
    {
        private ICommand _revenueItemInvokedCommand;
        public ICommand RevenueItemInvokedCommand => _revenueItemInvokedCommand ?? (_revenueItemInvokedCommand = new RelayCommand<NavigationViewItemInvokedEventArgs>(OnRevenueItemInvoked));
        public PageNavigation RevenueChildPageNavigation { get; set; }

        public RevenueStatisticsViewModel() 
        {
            RevenueChildPageNavigation = new PageNavigation(new DailyRevenueStatisticViewModel());
        }

        private void OnRevenueItemInvoked(NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItem.ToString().Equals("Daily"))
            {
                RevenueChildPageNavigation.ViewModel = new DailyRevenueStatisticViewModel();
            }
            else if (args.InvokedItem.ToString().Equals("Weekly"))
            {
                RevenueChildPageNavigation.ViewModel = new WeeklyRevenueStatisticViewModel();
            }
            else if (args.InvokedItem.ToString().Equals("Monthly"))
            {
                RevenueChildPageNavigation.ViewModel = new MonthlyRevenueStatisticViewModel();
            }
            else if (args.InvokedItem.ToString().Equals("Yearly"))
            {
                RevenueChildPageNavigation.ViewModel = new YearlyRevenueStatisticViewModel();
            }

        }
    }
}
