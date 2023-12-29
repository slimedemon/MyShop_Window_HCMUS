using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using MyShop.View;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyShop.ViewModel
{
    class StatisticsViewModel : ViewModelBase
    {
        private ICommand _itemInvokedCommand;
        public ICommand ItemInvokedCommand => _itemInvokedCommand ?? (_itemInvokedCommand = new RelayCommand<NavigationViewItemInvokedEventArgs>(OnItemInvoked));
        public PageNavigation ChildPageNavigation { get; set; }

        public StatisticsViewModel()
        {
            SaveCurrentPage();
            ChildPageNavigation = new PageNavigation(new RevenueStatisticsViewModel());
        }

        private void SaveCurrentPage()
        {
            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (Convert.ToBoolean(ConfigurationManager.AppSettings["RememberPage"]))
            {
                configuration.AppSettings.Settings["CurrentPage"].Value = "StatisticsPage";
            }
            else
            {
                configuration.AppSettings.Settings["CurrentPage"].Value = "DashboardPage";
            }

            configuration.Save(ConfigurationSaveMode.Full);
            ConfigurationManager.RefreshSection("appSettings");
        }

        void OnItemInvoked(NavigationViewItemInvokedEventArgs eventArgs) 
        {
            if (eventArgs.InvokedItem.ToString().Equals("Revenue Statistics"))
            {
                ChildPageNavigation.ViewModel = new RevenueStatisticsViewModel();
            }
            else if (eventArgs.InvokedItem.ToString().Equals("Product Statistics"))
            {
                ChildPageNavigation.ViewModel = new ProductStatisticsViewModel();
            }
            else if (eventArgs.InvokedItem.ToString().Equals("Best Seller Statistics"))
            {
                ChildPageNavigation.ViewModel = new BestSellerStatisticsViewModel();
            }
        }
    }

}
