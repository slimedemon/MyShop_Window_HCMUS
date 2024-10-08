﻿using CommunityToolkit.Mvvm.Input;
using MyShop.Model;
using MyShop.Repository;
using MyShop.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace MyShop.ViewModel
{
    public partial class LoginViewModel: ViewModelBase
    {
        //-> Fields
        private Account _account;
        private string _errorMessage;
        private bool _isValidData;
        private bool _isRememberAccount;
        private IAccountRepository _accountRepository;

        //-> Constructor
        public LoginViewModel()
        {
            _accountRepository = new AccountRepository();
            Account = new Account();
            LoadedCommand = new RelayCommand(PageLoaded);
            LoginCommand = new RelayCommand(ExecuteLoginCommand);
            RememberAccountCommand = new RelayCommand<bool>(ExecuteRememberAccountCommand);
            ResetCommand = new RelayCommand(ExecuteResetCommand);
            ConfigurationCommand = new RelayCommand(ExecuteConfigurationCommand);
            SignupCommand = new RelayCommand(ExecuteSignupCommand);
        }

        private void ExecuteSignupCommand()
        {
            ParentPageNavigation.ViewModel = new SignupViewModel();
        }

        private void ExecuteConfigurationCommand()
        {
            ParentPageNavigation.ViewModel = new DatabaseConfigurationViewModel();
        }

        private void ExecuteResetCommand()
        {
            ParentPageNavigation.ViewModel = new SignupViewModel();
        }

        private async void ExecuteLoginCommand()
        {
            ErrorMessage = String.Empty;

            try
            {
                if (Account.Username == null || Account.Password == null)
                {
                    throw new Exception("Username and Password cannot be null");
                }

                // Check username
                if (!Regex.IsMatch(Account.Username, @"^[a-zA-Z0-9_]+$"))
                {
                    throw new Exception("Username only contains letters: a-z, A-Z, 0-9 and _");
                }

                // Check password
                var hasNumber = new Regex(@"[0-9]+");
                var hasUpperChar = new Regex(@"[A-Z]+");
                var hasMinimum8Chars = new Regex(@".{8,}");
                var isValidated = hasNumber.IsMatch(Account.Password) && hasUpperChar.IsMatch(Account.Password) && hasMinimum8Chars.IsMatch(Account.Password);
                if (!isValidated)
                {
                    throw new Exception("Password must contain at least one number letter, one capital letter and length of the password must be more 8 letters");
                }

                string message = await _accountRepository.Authenticate(new System.Net.NetworkCredential(Account.Username, Account.Password));

                if (message == null)
                {
                    await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                    message = "Something is broken when system is retrieving data from database!";
                    return;
                }

                if (message.Equals("TRUE"))
                {
                    Thread.CurrentPrincipal = new GenericPrincipal(
                        new GenericIdentity(Account.Username), null);
                    string username = Account.Username;
                    var task = await _accountRepository.GetByUsername(username);

                    if (task == null)
                    {
                        await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                        return;
                    }

                    ParentPageNavigation.ViewModel = new MainNavigationViewModel(task);
                }
                else
                {
                    throw new Exception(message);
                }

                if (IsRememberAccount)
                {
                    //save to config for local login
                    var configuration = System.Configuration.ConfigurationManager.OpenExeConfiguration(
                        ConfigurationUserLevel.None);
                    configuration.AppSettings.Settings["Username"].Value = Account.Username;

                    // Encrypt password
                    var passwordInBytes = Encoding.UTF8.GetBytes(Account.Password);
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

                    configuration.AppSettings.Settings["Password"].Value = passwordIn64;
                    configuration.AppSettings.Settings["Entropy"].Value = entropyIn64;

                    configuration.Save(ConfigurationSaveMode.Full);
                    System.Configuration.ConfigurationManager.RefreshSection("appSettings");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private void PageLoaded()
        {
            //get from local
            string username = ConfigurationManager.AppSettings["Username"]!;
            string passwordIn64 = ConfigurationManager.AppSettings["Password"];
            string entropyIn64 = ConfigurationManager.AppSettings["Entropy"]!;

            if (passwordIn64.Length != 0)
            {
                byte[] entropyInBytes = Convert.FromBase64String(entropyIn64);
                byte[] cypherTextInBytes = Convert.FromBase64String(passwordIn64);

                byte[] passwordInBytes = ProtectedData.Unprotect(cypherTextInBytes,
                    entropyInBytes, DataProtectionScope.CurrentUser);

                string password = Encoding.UTF8.GetString(passwordInBytes);

                Account.Username = username;
                Account.Password = password;
            }
        }

        public void ExecuteRememberAccountCommand(bool isChecked)
        {
            IsRememberAccount = isChecked;
        }

        //-> getter, setter
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                SetProperty(ref _errorMessage, value);
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public bool IsValidData
        {
            get => _isValidData;
            set
            {
                SetProperty(ref _isValidData, value);
                OnPropertyChanged(nameof(IsValidData));
            }
        }

        //-> Commands]
        public RelayCommand SignupCommand { get;}
        public RelayCommand ConfigurationCommand { get; }
        public RelayCommand LoginCommand { get; }
        public RelayCommand LoadedCommand { get; }
        public RelayCommand ResetCommand { get; }
        public RelayCommand<bool> RememberAccountCommand { get; }
        public Account Account { get => _account; set => _account = value; }
        public bool IsRememberAccount { get => _isRememberAccount; set => _isRememberAccount = value; }
    }
}
