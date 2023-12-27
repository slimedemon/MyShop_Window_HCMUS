using CommunityToolkit.Mvvm.Input;
using Microsoft.Identity.Client;
using MyShop.Model;
using MyShop.Repository;
using MyShop.Services;
using MyShop.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using static MyShop.ViewModel.OrderHistoryViewModel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace MyShop.ViewModel
{
    class AccountViewModel : ViewModelBase
    {
        private Account _backupAccount;
        private Account _account;
        private IAccountRepository _accountRepository;
        private RelayCommand _logoutCommand;
        private RelayCommand _updateProfileCommand;
        private RelayCommand _changePasswordCommand;

        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string NewPassword { get; set; }
        public string RetypePassword { get; set; }
        public int MyProperty { get; set; }
        public Account Account { get => _account; set => _account = value; }
        public RelayCommand LogoutCommand { get => _logoutCommand; set => _logoutCommand = value; }
        public RelayCommand UpdateProfileCommand { get => _updateProfileCommand; set => _updateProfileCommand = value; }
        public RelayCommand ChangePasswordCommand { get => _changePasswordCommand; set => _changePasswordCommand= value; }

        public AccountViewModel(Account account)
        {

            Account = account;
            _backupAccount = new Account() 
            {
                Id = account.Id,
                Username = account.Username,
                Password = account.Password,
                Address = account.Address,
                Entropy = account.Entropy,
                Name = account.Name,
                PhoneNumber = account.PhoneNumber,
            };

            Name= account.Name;
            PhoneNumber= account.PhoneNumber;
            Address = account.Address;

            _accountRepository = new AccountRepository();
            LogoutCommand = new RelayCommand(ExecuteLogoutCommand);
            UpdateProfileCommand = new RelayCommand(ExecuteUpdateProfileCommand);
            ChangePasswordCommand = new RelayCommand(ExecuteChangePasswordCommand);
        }

        private async void ExecuteLogoutCommand()
        {
            // set null each property of _account
            if (Account != null)
            {
                Account.Username = "";
                Account.Password = "";
                Account.Entropy = "";
                Account.Id = 0;
                Account.PhoneNumber = "";
                Account.Address = "";
            }

            var confirmed = await App.MainRoot.ShowYesCancelDialog("Logout this account?", "Logout", "Cancel");

            if (confirmed == true)
            {
                ((RootPageViewModel)App.MainRoot.DataContext).ChildPageNavigation.ViewModel = new LoginViewModel();
            }
        }

        private async void ExecuteUpdateProfileCommand()
        {
            var confirmed = await App.MainRoot.ShowYesCancelDialog("Update your personal profile?", "Update", "Cancel");
            if (confirmed == false) return;

            Account.Name = Name;
            Account.PhoneNumber = PhoneNumber;
            Account.Address = Address;

            try
            {
                // Check is null
                if (Name == null)
                {
                    throw new Exception("Not fill name completely!");
                }

                // Check phone number
                if (PhoneNumber != null && !Regex.IsMatch(PhoneNumber, @"[0-9]+"))
                {
                    throw new Exception("Invalid phone number");
                }

                var task = await _accountRepository.UpdateProfile(Account);

                if (task == true)
                {
                    await App.MainRoot.ShowDialog("Success", "Personal Profile is updated successfully!");
                    ParentPageNavigation.ViewModel = new AccountViewModel(Account);
                }
                else
                {
                    throw new Exception("Personal Profile is updated failed! Please check information or database connection again!");
                }
            }
            catch (Exception ex)
            {
                await App.MainRoot.ShowDialog("Failed", ex.Message);
                // restore by using backup
                Account.Name = _backupAccount.Name;
                Account.PhoneNumber = _backupAccount.PhoneNumber;
                Account.Address = _backupAccount.Address;
            }
        }

        private async void ExecuteChangePasswordCommand()
        {
            var confirmed = await App.MainRoot.ShowYesCancelDialog("Change your password?", "Change", "Cancel");
            if (confirmed == false) return;

            Account.Password = NewPassword;

            try
            {
                // Check is null
                if (NewPassword == null || RetypePassword == null)
                {
                    throw new Exception("Not fill out completely!");
                }

                // Check retype password
                if (!NewPassword.Equals(RetypePassword))
                {
                    throw new Exception("Invalid retype password");
                }

                // Check password
                var hasNumber = new Regex(@"[0-9]+");
                var hasUpperChar = new Regex(@"[A-Z]+");
                var hasMinimum8Chars = new Regex(@".{8,}");
                var isValidated = hasNumber.IsMatch(NewPassword) && hasUpperChar.IsMatch(NewPassword) && hasMinimum8Chars.IsMatch(NewPassword);
                if (!isValidated)
                {
                    throw new Exception("Password must contain at least one number letter, one capital letter and length of the password must be more 8 letters");
                }

                var task = await _accountRepository.ChangePassword(Account);
                if (task == true)
                {
                    await App.MainRoot.ShowDialog("Success", "Personal Profile is updated successfully!");
                    ParentPageNavigation.ViewModel = new LoginViewModel();
                }
                else
                {
                    throw new Exception("Personal Profile is updated successfully! Please check information or database connection again!");
                }
            }
            catch (Exception ex)
            {
                await App.MainRoot.ShowDialog("Failed", ex.Message);

                // restore by using backup
                Account.Password = _backupAccount.Password;
            }
        }
    }
}
