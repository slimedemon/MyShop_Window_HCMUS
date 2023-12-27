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
using Windows.ApplicationModel.VoiceCommands;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        public SettingPage()
        {
            this.InitializeComponent();
        }

        private void ShowItemPerPages(object sender, RoutedEventArgs e)
        {
            if (ItemPerPageGroup.Visibility == Visibility.Collapsed)
            {
                ItemPerPageGroup.Visibility = Visibility.Visible;
            }
            else
            {
                ItemPerPageGroup.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowOpenLastPageGroup(object sender, RoutedEventArgs e)
        {
            if (OpenLastPageGroup.Visibility == Visibility.Collapsed)
            {
                OpenLastPageGroup.Visibility = Visibility.Visible;
            }
            else
            { 
                OpenLastPageGroup.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowImportGroup(object sender, RoutedEventArgs e)
        {
            if (ImportGroup.Visibility == Visibility.Collapsed)
            {
                ImportGroup.Visibility = Visibility.Visible;
            }
            else
            {
                ImportGroup.Visibility = Visibility.Collapsed;
            }
        }
    }
}
