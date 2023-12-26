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

namespace MyShop.ViewModel
{
    public class LoginDatabaseViewModel : ViewModelBase
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
        public string ConnectionString { get => _connectionString; set => _connectionString = value; }

        public RelayCommand SaveCommand { get; set; }
        public RelayCommand BackCommand { get; set; }
        public RelayCommand WindowsAuthenticationCommand { get; set; }
        public RelayCommand StandardSecurityCommand { get; set; }
        public RelayCommand CustomConnectionCommand { get; set; }
        public RelayCommand LoadedCommand { get; set; }

        public LoginDatabaseViewModel()
        {
            _type = "Trusted Connection";
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
            if (_type.Equals("Windows Authentication"))
            {
                builder.IntegratedSecurity = true;
                ConnectionString = builder.ConnectionString;
            }
            else if (_type.Equals("Standard Security"))
            {

                builder.UserID = DbUsername;
                builder.Password = DbPassword;
                ConnectionString = builder.ConnectionString;
            }
            else if (_type.Equals("Custom Connection String"))
            {
                ConnectionString = CustomConnectionString;
            }

            var sysconfig = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

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
                sysconfig.AppSettings.Settings["DbPassword"].Value = passwordIn64;
                sysconfig.AppSettings.Settings["DbEntropy"].Value = entropyIn64;
            }

            if (DbUsername != null) sysconfig.AppSettings.Settings["DbUsername"].Value = DbUsername;
            if (ServerAddress != null) sysconfig.AppSettings.Settings["ServerAddress"].Value = ServerAddress;
            if (DatabaseName != null) sysconfig.AppSettings.Settings["DatabaseName"].Value = DatabaseName;
            if (ConnectionString != null) sysconfig.AppSettings.Settings["ConnectionString"].Value = ConnectionString;

            sysconfig.Save(ConfigurationSaveMode.Full);
            System.Configuration.ConfigurationManager.RefreshSection("AppSettings");

            await App.MainRoot.ShowDialog("Success", "Database configuration is saved!");
        }

        private async void ExecuteBackCommand()
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

        //private string _dbUsername;
        //private string _dbPassword;
        //private string _errorMessage;

        //private IAccountRepository _accountRepository;
        //private RelayCommand _loginCommand;

        //public string DbUsername { get => _dbUsername; set => _dbUsername = value; }
        //public string DbPassword { get => _dbPassword; set => _dbPassword = value; }
        //public string ErrorMessage { get => _errorMessage; set => _errorMessage = value; }
        //public RelayCommand LoginCommand { get => _loginCommand; set => _loginCommand = value; }

        //public LoginDatabaseViewModel()
        //{
        //    _accountRepository = new AccountRepository();
        //    LoginCommand = new RelayCommand(ExecuteLoginCommand);
        //}


        //private async void ExecuteLoginCommand()
        //{
        //    ErrorMessage = String.Empty;
        //    string message = await _accountRepository.AuthenticateDbAccount(
        //        new System.Net.NetworkCredential(DbUsername, DbPassword));

        //    if (message.Equals("TRUE"))
        //    {
        //        Thread.CurrentPrincipal = new GenericPrincipal(
        //            new GenericIdentity(DbUsername), null);
        //        ParentPageNavigation.ViewModel = new LoginViewModel();
        //    }
        //    else
        //    {
        //        ErrorMessage = message;
        //        return;
        //    }

        //    //save to config for local login
        //    var sysconfig = System.Configuration.ConfigurationManager.OpenExeConfiguration(
        //        ConfigurationUserLevel.None);
        //    sysconfig.AppSettings.Settings["dbUsername"].Value = DbUsername;

        //    // Encrypt password
        //    var passwordInBytes = Encoding.UTF8.GetBytes(DbPassword);
        //    var entropy = new byte[20];
        //    using (var rng = RandomNumberGenerator.Create())
        //    {
        //        rng.GetBytes(entropy);
        //    }

        //    var cypherText = ProtectedData.Protect(
        //        passwordInBytes,
        //        entropy,
        //        DataProtectionScope.CurrentUser
        //    );

        //    var passwordIn64 = Convert.ToBase64String(cypherText);
        //    var entropyIn64 = Convert.ToBase64String(entropy);

        //    sysconfig.AppSettings.Settings["dbPassword"].Value = passwordIn64;
        //    sysconfig.AppSettings.Settings["dbEntropy"].Value = entropyIn64;

        //    sysconfig.Save(ConfigurationSaveMode.Full);
        //    System.Configuration.ConfigurationManager.RefreshSection("appSettings");
        //}
    }
}
