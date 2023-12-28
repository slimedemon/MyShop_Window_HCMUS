using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using Microsoft.UI.Xaml;
using MyShop.Model;
using MyShop.Repository;
using MyShop.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyShop.ViewModel
{
    public class DatabaseConfigurationViewModel : ViewModelBase
    {
        private string _serverAddress;
        private string _databaseName;
        private string _dbUsername;
        private string _dbPassword;
        private string _connectionString;
        private string _type;

        public string ServerAddress { get => _serverAddress; set => _serverAddress = value; }
        public string DatabaseName { get => _databaseName; set => _databaseName = value; }
        public string DbUsername { get => _dbUsername; set => _dbUsername = value; }
        public string DbPassword { get => _dbPassword; set => _dbPassword = value; }
        public string CustomConnectionString { get; set; }
        public string ConnectionString 
        { 
            get => _connectionString; 
            set => _connectionString = value; 
        }

        public RelayCommand SaveCommand { get; set; }
        public RelayCommand BackCommand { get; set; }
        public RelayCommand WindowsAuthenticationCommand { get; set; }
        public RelayCommand StandardSecurityCommand { get; set; }
        public RelayCommand CustomConnectionCommand { get; set; }
        public RelayCommand LoadedCommand { get; set; }

        public DatabaseConfigurationViewModel()
        {
            _type = "Windows Authentication";
            SaveCommand = new RelayCommand(ExecuteSaveCommand);
            BackCommand = new RelayCommand(ExecuteBackCommand);
            WindowsAuthenticationCommand = new RelayCommand(ExecuteWindowsAuthenticationCommand);
            StandardSecurityCommand = new RelayCommand(ExecuteStandardSecurityCommand);
            CustomConnectionCommand = new RelayCommand(ExecuteCustomConnectionCommand);
            LoadedCommand = new RelayCommand(PageLoaded);
        }

        private async void ExecuteSaveCommand()
        {
            var builder = new SqlConnectionStringBuilder();
            builder.DataSource = ServerAddress;
            builder.InitialCatalog = DatabaseName;
            builder.TrustServerCertificate = true;

            // Create connection string
            if (_type == null || _type.Equals(""))
            {
                // Default = Windows Authentication
                builder.IntegratedSecurity = true;
                ConnectionString = builder.ConnectionString;
            }
            else if (_type.Equals("Windows Authentication"))
            {
                builder.IntegratedSecurity = true;
                ConnectionString = builder.ConnectionString;
            }
            else if (_type.Equals("Standard Security"))
            {

                builder.UserID = DbUsername ?? "";
                builder.Password = DbPassword ?? "";
                ConnectionString = builder.ConnectionString;
            }
            else if (_type.Equals("Custom Connection String"))
            {
                ConnectionString = CustomConnectionString;
            }

            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // Encrypt password
            if (DbPassword != null)
            {
                var passwordInBytes = Encoding.UTF8.GetBytes(DbPassword);
                var entropy = new byte[20];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(entropy);
                }

                var cypherText = ProtectedData.Protect(
                    passwordInBytes,
                    entropy,
                    DataProtectionScope.CurrentUser
                );

                var passwordIn64 = Convert.ToBase64String(cypherText);
                var entropyIn64 = Convert.ToBase64String(entropy);
                configuration.AppSettings.Settings["DbPassword"].Value = passwordIn64;
                configuration.AppSettings.Settings["DbEntropy"].Value = entropyIn64;
            }

            configuration.AppSettings.Settings["DbUsername"].Value = DbUsername ?? "";
            configuration.AppSettings.Settings["ServerAddress"].Value = ServerAddress ?? "";
            configuration.AppSettings.Settings["DatabaseName"].Value = DatabaseName ?? "";
            configuration.AppSettings.Settings["ConnectionString"].Value = ConnectionString ?? "";

            configuration.AppSettings.SectionInformation.ForceSave = true;
            configuration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            await App.MainRoot.ShowDialog("Success", "Database configuration is saved!");
        }

        private void ExecuteBackCommand()
        {
            ParentPageNavigation.ViewModel = new LoginViewModel();
        }

        private void ExecuteWindowsAuthenticationCommand()
        {
            _type = "Windows Authentication";
        }

        private void ExecuteStandardSecurityCommand()
        {
            _type = "Standard Security";
        }

        private void ExecuteCustomConnectionCommand()
        {
            _type = "Custom Connection String";
        }

        public void OnTextChanged(object sender, RoutedEventArgs e)
        {
            ConnectionString = CustomConnectionString;
        }

        private void PageLoaded()
        {
            //get from local
            DbUsername = ConfigurationManager.AppSettings["DbUsername"];
            ServerAddress = ConfigurationManager.AppSettings["ServerName"];
            DatabaseName = ConfigurationManager.AppSettings["DatabaseName"];
            ConnectionString = ConfigurationManager.AppSettings["ConnectionString"];

            string dbPasswordIn64 = ConfigurationManager.AppSettings["DbPassword"];
            string dbEntropyIn64 = ConfigurationManager.AppSettings["DbEntropy"]!;

            if (dbPasswordIn64.Length != 0)
            {
                byte[] dbEntropyInBytes = Convert.FromBase64String(dbEntropyIn64);
                byte[] dbCypherTextInBytes = Convert.FromBase64String(dbPasswordIn64);

                byte[] dbPasswordInBytes = ProtectedData.Unprotect(dbCypherTextInBytes,
                    dbEntropyInBytes,
                    DataProtectionScope.CurrentUser
                );

                DbPassword = Encoding.UTF8.GetString(dbPasswordInBytes);
            }

            if (ServerAddress == null || ServerAddress.Equals(""))
            {
                ServerAddress = ".\\SQLEXPRESS";
            }

            if (DatabaseName == null || DatabaseName.Equals(""))
            {
                DatabaseName = "MyShopDB";
            }

            if (ConnectionString == null || ConnectionString.Equals(""))
            {
                ConnectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=MyShopDB;Integrated Security=True;Trust Server Certificate=True";
            }

            CustomConnectionString = ConnectionString;
        }
    }
}
