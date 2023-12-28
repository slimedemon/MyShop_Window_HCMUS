using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Controls;
using MyShop.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace MyShop.ViewModel
{
    public class MainNavigationViewModel : ViewModelBase
    {
        private Account _account;
        private ICommand _itemInvokedCommand;
        public ICommand ItemInvokedCommand => _itemInvokedCommand ?? (_itemInvokedCommand = new RelayCommand<NavigationViewItemInvokedEventArgs>(OnItemInvoked));

        public MainNavigationViewModel(Account account)
        {
            Account = account;
            LoadCurrentPage();
        }
        public MainNavigationViewModel()
        {
            Account = null;
            LoadCurrentPage();
        }

        private void LoadCurrentPage()
        {
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["RememberPage"]))
            {
                var currentPage = ConfigurationManager.AppSettings["CurrentPage"];
                if (currentPage.Equals("DashboardPage"))
                {
                    ChildPageNavigation = new PageNavigation(new DashboardViewModel());
                }
                else if (currentPage.Equals("StatisticsPage"))
                {
                    ChildPageNavigation = new PageNavigation(new StatisticsViewModel());
                }
                else if (currentPage.Equals("ProductManagementPage"))
                {
                    ChildPageNavigation = new PageNavigation(new ProductManagementViewModel());
                }
                else if (currentPage.Equals("OrderManagementPage"))
                {
                    ChildPageNavigation = new PageNavigation(new OrderManagementViewModel());
                }
                else if (currentPage.Equals("AccountPage"))
                {
                    ChildPageNavigation = new PageNavigation(new AccountViewModel(Account));
                }
                else if (currentPage.Equals("SettingsPage"))
                {
                    ChildPageNavigation = new PageNavigation(new SettingsViewModel());
                }
            }
            else 
            {
                ChildPageNavigation = new PageNavigation(new DashboardViewModel());
            }
        }
        
        private void OnItemInvoked(NavigationViewItemInvokedEventArgs args)
        {
            // could also use a converter on the command parameter if you don't like
            // the idea of passing in a NavigationViewItemInvokedEventArgs
            if (args.InvokedItem.ToString().Equals("Dashboard"))
            {
                ChildPageNavigation.ViewModel = new DashboardViewModel();
            }
            else if (args.InvokedItem.ToString().Equals("Statistics"))
            {
                ChildPageNavigation.ViewModel = new StatisticsViewModel();
            }
            else if (args.InvokedItem.ToString().Equals("Product Management"))
            {
                ChildPageNavigation.ViewModel = new ProductManagementViewModel();
            }
            else if (args.InvokedItem.ToString().Equals("Order Management"))
            {
                ChildPageNavigation.ViewModel = new OrderManagementViewModel();
            }
            else if (args.InvokedItem.ToString().Equals("Account"))
            {
                ChildPageNavigation.ViewModel = new AccountViewModel(Account);
            }
            else if (args.InvokedItem.ToString().Equals("Settings"))
            {
                ChildPageNavigation.ViewModel = new SettingsViewModel();
            }

        }

        public PageNavigation ChildPageNavigation { get; set; }
        public Account Account { get => _account; set => _account = value; }
    }
}
