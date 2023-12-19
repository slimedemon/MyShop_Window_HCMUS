using CommunityToolkit.Mvvm.Input;
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
        public RelayCommand TrustedConnectionCommand { get; set; }
        public RelayCommand StandardSecurityCommand { get; set; }
        public RelayCommand CustomConnectionCommand { get; set; }
        public RelayCommand LoadedCommand { get; set; }

        public LoginDatabaseViewModel()
        {
            _type = "Trusted Connection";
            SaveCommand = new RelayCommand(ExecuteSaveCommand);
            BackCommand = new RelayCommand(ExecuteBackCommand);
            TrustedConnectionCommand = new RelayCommand(ExecuteTrustedConnectionCommand);
            StandardSecurityCommand = new RelayCommand(ExecuteStandardSecurityCommand);
            CustomConnectionCommand = new RelayCommand(ExecuteCustomConnectionCommand);
            LoadedCommand = new RelayCommand(PageLoaded);
        }

        private async void ExecuteSaveCommand()
        {
            // Create connection string
            if (_type.Equals("Trusted Connection"))
            {
                string template = $"Server={ServerAddress};Database={DatabaseName};Trusted_Connection=True;";
                ConnectionString = template;
            }
            else if (_type.Equals("Standard Security"))
            {
                string template = $"Server={ServerAddress};Database={DatabaseName};User Id={DbUsername};Password={DbPassword};";
                ConnectionString = template;
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
                sysconfig.AppSettings.Settings["dbPassword"].Value = passwordIn64;
                sysconfig.AppSettings.Settings["dbEntropy"].Value = entropyIn64;
            }

            if (DbUsername != null) sysconfig.AppSettings.Settings["dbUsername"].Value = DbUsername;
            if (ServerAddress != null) sysconfig.AppSettings.Settings["serverAddress"].Value = ServerAddress;
            if (DatabaseName != null) sysconfig.AppSettings.Settings["databaseName"].Value = DatabaseName;
            if (ConnectionString != null) sysconfig.AppSettings.Settings["connectionString"].Value = ConnectionString;

            sysconfig.Save(ConfigurationSaveMode.Full);
            System.Configuration.ConfigurationManager.RefreshSection("appSettings");

            await App.MainRoot.ShowDialog("Success", "Database configuration is saved!");
        }

        private async void ExecuteBackCommand()
        {
            ParentPageNavigation.ViewModel = new LoginViewModel();
        }

        private void ExecuteTrustedConnectionCommand()
        {
            _type = "Trusted Connection";
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
            DbUsername = ConfigurationManager.AppSettings["Username"];
            ServerAddress = ConfigurationManager.AppSettings["serverName"];
            DatabaseName = ConfigurationManager.AppSettings["databaseName"];
            ConnectionString = ConfigurationManager.AppSettings["connectionString"];

            string passwordIn64 = ConfigurationManager.AppSettings["Password"];
            string entropyIn64 = ConfigurationManager.AppSettings["Entropy"]!;

            if (passwordIn64.Length != 0)
            {
                byte[] entropyInBytes = Convert.FromBase64String(entropyIn64);
                byte[] cypherTextInBytes = Convert.FromBase64String(passwordIn64);

                byte[] passwordInBytes = ProtectedData.Unprotect(cypherTextInBytes,
                    entropyInBytes,
                    DataProtectionScope.CurrentUser
                );

                string password = Encoding.UTF8.GetString(passwordInBytes);
                DbPassword = password;
            }

            if (ServerAddress == null || ServerAddress.Equals(""))
            {
                ServerAddress = ".\\SQLEXPRESS";
            }

            if (DatabaseName == null || DatabaseName.Equals(""))
            {
                DatabaseName = "db_book";
            }

            if (ConnectionString == null || ConnectionString.Equals(""))
            {
                ConnectionString = "Server=.\\SQLEXPRESS;Database=db_book;Trusted_Connection=True;TrustServerCertificate=true;";
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

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
