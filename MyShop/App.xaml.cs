using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using MyShop.Services;
using MyShop.View;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using Windows.Storage;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow() { Content = new RootPage() };
            m_window.Title = "MyShop - Bookstore";

            // resize
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(m_window);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.SetIcon(@"Images/book-shop.ico");
            appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 1600, Height = 900 });

            // move to center screen
            PointInt32 CenteredPosition = appWindow.Position;
            DisplayArea displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Nearest);
            CenteredPosition.X = (displayArea.WorkArea.Width - appWindow.Size.Width) / 2;
            CenteredPosition.Y = (displayArea.WorkArea.Height - appWindow.Size.Height) / 2;
            appWindow.Move(CenteredPosition);

            m_window.ExtendsContentIntoTitleBar = false;
            m_window.SetTitleBar(null);
            m_window.Activate();
            MainRoot = m_window.Content as FrameworkElement;

            // initialize the App.config 
            try { initializeAppConfig(); }
            catch (Exception ex) 
            {
                // write the exception to the log file in D:\MyShop\Logs
                string logPath = @"D:\MyShop\Logs";
                string logFile = "MyShopLog.txt";
                string logFullPath = System.IO.Path.Combine(logPath, logFile);
                if (!Directory.Exists(logPath)) Directory.CreateDirectory(logPath);
                File.AppendAllText(logFullPath, DateTime.Now.ToString() + " - " + ex.Message + "\r\n");
            }

            



        }

        private void initializeAppConfig()
        {
            // check if the App.config file exists in the local folder
            string fileName = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "App.config");
            if (!File.Exists(fileName))
            {
                // initialize the App.config file
                string content = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n<configuration>\r\n\t<appSettings>\r\n\t\t<add key=\"Username\" value=\"\"/>\r\n\t\t<add key=\"Password\" value=\"\"/>\r\n\t\t<add key=\"Entropy\" value=\"\"/>\r\n\t\t<add key=\"RememberPage\" value=\"False\"/>\r\n\t\t<add key=\"CurrentPage\" value=\"DashboardPage\"/>\r\n\t\t<add key=\"ItemsPerPage\" value=\"10\"/>\r\n\t\t<add key=\"DbUsername\" value=\"\"/>\r\n\t\t<add key=\"DbPassword\" value=\"\"/>\r\n\t\t<add key=\"DbEntropy\" value=\"\"/>\r\n\t\t<add key=\"ServerAddress\" value=\"\"/>\r\n\t\t<add key=\"DatabaseName\" value=\"\"/>\r\n\t\t<add key=\"ConnectionString\" value=\"Data Source=.\\SQLEXPRESS;Initial Catalog=MyShopDB;Integrated Security=True;Trust Server Certificate=True\"/>\r\n\t\t<add key=\"dbConnectStatus\" value=\"\"/>\r\n\t</appSettings>\r\n</configuration>";
                File.WriteAllText(fileName, content);
            }

            // change the App.config to the local folder
            ChangeAppConfig(fileName);
        }

        /// <summary>
        /// Use your own App.Config file instead of the default.
        /// </summary>
        /// <param name="NewAppConfigFullPathName"></param>
        private void ChangeAppConfig(string NewAppConfigFullPathName)
        {
            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", NewAppConfigFullPathName);
            ResetConfigMechanism();
            return;
        }

        /// <summary>
        /// Remove cached values from ClientConfigPaths.
        /// Call this after changing path to App.Config.
        /// </summary>
        private void ResetConfigMechanism()
        {
            BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Static;
            typeof(ConfigurationManager)
                .GetField("s_initState", Flags)
                .SetValue(null, 0);

            typeof(ConfigurationManager)
                .GetField("s_configSystem", Flags)
                .SetValue(null, null);

            typeof(ConfigurationManager)
                .Assembly.GetTypes()
                .Where(x => x.FullName == "System.Configuration.ClientConfigPaths")
                .First()
                .GetField("s_current", Flags)
                .SetValue(null, null);
            return;
        }



        private Microsoft.UI.Xaml.Window m_window;
        public static FrameworkElement MainRoot { get; private set; }
    }
}
