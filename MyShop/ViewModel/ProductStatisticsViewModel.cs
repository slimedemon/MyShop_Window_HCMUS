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
    public class ProductStatisticsViewModel: ViewModelBase
    {
        private ICommand _productItemInvokedCommand;
        public ICommand ProductItemInvokedCommand => _productItemInvokedCommand ?? (_productItemInvokedCommand = new RelayCommand<NavigationViewItemInvokedEventArgs>(OnProductItemInvoked));
        public PageNavigation ProductChildPageNavigation { get; set; }
        public ProductStatisticsViewModel() 
        {
            ProductChildPageNavigation = new PageNavigation(new DailyProductStatisticViewModel());
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
    }
}
