using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyShop.ViewModel
{
    public class ProductManagementViewModel : ViewModelBase
    {

        private ICommand _itemInvokedCommand;
        public ICommand ItemInvokedCommand => _itemInvokedCommand ?? (_itemInvokedCommand = new RelayCommand<NavigationViewItemInvokedEventArgs>(OnItemInvoked));
        public ProductManagementViewModel()
        {
            SaveCurrentPage();

            ChildPageNavigation = new PageNavigation(new BookManagementViewModel());
        }

        private void SaveCurrentPage()
        {
            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (Convert.ToBoolean(ConfigurationManager.AppSettings["RememberPage"]))
            {
                configuration.AppSettings.Settings["CurrentPage"].Value = "ProductManagementPage";
            }
            else
            {
                configuration.AppSettings.Settings["CurrentPage"].Value = "DashboardPage";
            }

            configuration.Save(ConfigurationSaveMode.Full);
            ConfigurationManager.RefreshSection("appSettings");
        }


        private void OnItemInvoked(NavigationViewItemInvokedEventArgs args)
        {
            // could also use a converter on the command parameter if you don't like
            // the idea of passing in a NavigationViewItemInvokedEventArgs
            if (args.InvokedItem.ToString().Equals("Book Management"))
            {
                ChildPageNavigation.ViewModel = new BookManagementViewModel();
            }
            else if (args.InvokedItem.ToString().Equals("Genre Management"))
            {
                ChildPageNavigation.ViewModel = new GenreManagementViewModel();
            }

        }
        public PageNavigation ChildPageNavigation { get; set; }
    }
}
