using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
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
    public sealed partial class AccountPage : Page
    {
        public AccountPage()
        {
            this.InitializeComponent();
        }

        private void ShowGroup(string group)
        {
            if (group.Equals("None"))
            {
                ChangePasswordBox.Visibility = Visibility.Collapsed;
                UpdateProfileBox.Visibility = Visibility.Collapsed;
            }
            else if (group.Equals("ShowChangePassword"))
            {
                ChangePasswordBox.Visibility = Visibility.Visible;
                UpdateProfileBox.Visibility = Visibility.Collapsed;
            }
            else if (group.Equals("ShowUpdateProfileBox")) {
                ChangePasswordBox.Visibility = Visibility.Collapsed;
                UpdateProfileBox.Visibility = Visibility.Visible;
            }
        }

        private void Click_ShowChangePasswordBox(object sender, RoutedEventArgs e)
        {
            if (ChangePasswordBox.Visibility == Visibility.Visible)
            {
                ShowGroup("None");
            }
            else 
            {
                ShowGroup("ShowChangePassword");
            }
        }

        private void Click_ShowUpdateProfileBox(object sender, RoutedEventArgs e)
        {
            if (UpdateProfileBox.Visibility == Visibility.Visible)
            {
                ShowGroup("None");
            }
            else 
            {
                ShowGroup("ShowUpdateProfileBox");
            }
        }
    }
}
