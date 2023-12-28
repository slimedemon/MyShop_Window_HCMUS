using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using MyShop.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyShop.ViewModel
{
    class StatisticsViewModel : ViewModelBase
    {
        public StatisticsViewModel()
        {
            RevenueChildPageNavigation = new PageNavigation(new DailyRevenueStatisticViewModel());
            ProductChildPageNavigation = new PageNavigation(new DailyProductStatisticViewModel());
        }

        private ICommand _revenueItemInvokedCommand;
        private ICommand _productItemInvokedCommand;
        public ICommand RevenueItemInvokedCommand => _revenueItemInvokedCommand ?? (_revenueItemInvokedCommand = new RelayCommand<NavigationViewItemInvokedEventArgs>(OnRevenueItemInvoked));
        public ICommand ProductItemInvokedCommand => _productItemInvokedCommand ?? (_productItemInvokedCommand = new RelayCommand<NavigationViewItemInvokedEventArgs>(OnProductItemInvoked));

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
        private void OnProductItemInvoked(NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItem.ToString().Equals("Daily"))
            {
                ProductChildPageNavigation.ViewModel = new DailyProductStatisticViewModel();
            }
            else if (args.InvokedItem.ToString().Equals("Weekly"))
            {
                ProductChildPageNavigation.ViewModel = new WeeklyProductStatisticViewModel();
            }
            else if (args.InvokedItem.ToString().Equals("Monthly"))
            {
                ProductChildPageNavigation.ViewModel = new MonthlyProductStatisticViewModel();
            }
            else if (args.InvokedItem.ToString().Equals("Yearly"))
            {
                ProductChildPageNavigation.ViewModel = new YearlyProductStatisticViewModel();
            }
        }
        public PageNavigation RevenueChildPageNavigation { get; set; }
        public PageNavigation ProductChildPageNavigation { get; set; }
    }

}
