using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Model;
using MyShop.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainNavigationPage : Page
    {
        public static NavigationView NVMain { get; set; }
        public MainNavigationPage()
        {
            this.InitializeComponent();
            NVMain = nvMain;
        }
        private void LoadCurrentPage(object sender, RoutedEventArgs e)
        {
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["RememberPage"]))
            {
                var currentPage = ConfigurationManager.AppSettings["CurrentPage"];

              

                if (currentPage.Equals("DashboardPage"))
                {
                    int index;
                    for (index = 0; index < nvMain.MenuItems.Count; index++)
                    {
                        if (((NavigationViewItem)nvMain.MenuItems[index]).Content.Equals("Dashboard"))
                        {
                            nvMain.SelectedItem = nvMain.MenuItems[index];
                            break;
                        }
                    }
                }
                else if (currentPage.Equals("StatisticsPage"))
                {
                    int index;
                    for (index = 0; index < nvMain.MenuItems.Count; index++)
                    {
                        if (((NavigationViewItem)nvMain.MenuItems[index]).Content.Equals("Statistics"))
                        {
                            nvMain.SelectedItem = nvMain.MenuItems[index];
                            break;
                        }
                    }
                }
                else if (currentPage.Equals("ProductManagementPage"))
                {
                    int index;
                    for (index = 0; index < nvMain.MenuItems.Count; index++)
                    {
                        if (((NavigationViewItem)nvMain.MenuItems[index]).Content.Equals("Product Management"))
                        {
                            nvMain.SelectedItem = nvMain.MenuItems[index];
                            break;
                        }
                    }
                }        
                else if (currentPage.Equals("OrderManagementPage"))
                {
                    int index;
                    for (index = 0; index < nvMain.MenuItems.Count; index++)
                    {
                        if (((NavigationViewItem)nvMain.MenuItems[index]).Content.Equals("Order Management"))
                        {
                            nvMain.SelectedItem = nvMain.MenuItems[index];
                            break;
                        }
                    }

                }
                else if (currentPage.Equals("PromotionManagementPage"))
                {
                    int index;
                    for (index = 0; index < nvMain.MenuItems.Count; index++)
                    {
                        if (((NavigationViewItem)nvMain.MenuItems[index]).Content.Equals("Promotion Management"))
                        {
                            nvMain.SelectedItem = nvMain.MenuItems[index];
                            break;
                        }
                    }

                } 
                else if (currentPage.Equals("CustomerManagementPage"))
                {
                    int index;
                    for (index = 0; index < nvMain.MenuItems.Count; index++)
                    {
                        if (((NavigationViewItem)nvMain.MenuItems[index]).Content.Equals("Customer Management"))
                        {
                            nvMain.SelectedItem = nvMain.MenuItems[index];
                            break;
                        }
                    }

                }
                else if (currentPage.Equals("AccountPage"))
                {
                    int index;
                    for (index = 0; index < nvMain.MenuItems.Count; index++)
                    {
                        if (((NavigationViewItem)nvMain.MenuItems[index]).Content.Equals("Account"))
                        {
                            nvMain.SelectedItem = nvMain.MenuItems[index];
                            break;
                        }
                    }
                }
                else if (currentPage.Equals("SettingsPage"))
                {
                    int index;
                    for (index = 0; index < nvMain.FooterMenuItems.Count; index++)
                    {
                        if (((NavigationViewItem)nvMain.FooterMenuItems[index]).Content.Equals("Settings"))
                        {
                            nvMain.SelectedItem = nvMain.FooterMenuItems[index];
                            break;
                        }
                    }
                }
            }
            else
            {
                int index;
                for (index = 0; index < nvMain.MenuItems.Count; index++)
                {
                    if (((NavigationViewItem)nvMain.MenuItems[index]).Content.Equals("Dashboard"))
                    {
                        nvMain.SelectedItem = nvMain.MenuItems[index];
                        break;
                    }
                }

            }
        }
    }
}
