using CommunityToolkit.Mvvm.Input;
using MyShop.Model;
using MyShop.Repository;
using MyShop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyShop.ViewModel
{
    public class RegisterViewModel : ViewModelBase
    {
        private Account _account;
        private string _fullName;
        private string _phoneNumber;
        private string _username;
        private string _password;
        private string _retypePassword;
        private string _errorMessage;
        private IAccountRepository _accountRepository;

        public RegisterViewModel()
        {
            Account = new Account();
            _accountRepository = new AccountRepository();
            SignupCommand = new RelayCommand(ExecuteSignupCommand);
            LoginAccountCommand = new RelayCommand(ExecuteLoginAccountCommand);
        }

        private async void ExecuteSignupCommand()
        {
            try
            {
                // Check is null
                if (FullName == null || Password == null || RetypePassword == null || Username == null)
                {
                    throw new Exception("Not fill out completely!");
                }

                // Check retype password
                if (!Password.Equals(RetypePassword))
                {
                    throw new Exception("Invalid retype password");
                }

                // Check username
                if (!Regex.IsMatch(Username, @"^[a-zA-Z0-9_]+$"))
                { 
                    throw new Exception("Username only contains letters: a-z, A-Z, 0-9 and _");
                }

                // Check password
                var hasNumber = new Regex(@"[0-9]+");
                var hasUpperChar = new Regex(@"[A-Z]+");
                var hasMinimum8Chars = new Regex(@".{8,}");
                var isValidated = hasNumber.IsMatch(Password) && hasUpperChar.IsMatch(Password) && hasMinimum8Chars.IsMatch(Password);
                if (!isValidated)
                {
                    throw new Exception("Password must contain at least one number letter, one capital letter and length of the password must be more 8 letters");
                }

                // Check phone number
                if (PhoneNumber!= null && !Regex.IsMatch(PhoneNumber, @"[0-9]+")) 
                {
                    throw new Exception("Invalid phone number");
                }

                Account.Username = Username;
                Account.Name = FullName;
                Account.PhoneNumber = PhoneNumber;
                Account.Address = "";

                // non-encrypt password
                Account.Password = Password;

                var result = await _accountRepository.Add(Account);
                if (!result)
                {
                    throw new Exception("Signup failed");
                }

                // if success => notify
                await App.MainRoot.ShowDialog("Success", "You have SIGN UP an account successfully");
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private void ExecuteLoginAccountCommand()
        {
            ParentPageNavigation.ViewModel = new LoginViewModel();
        }

        public RelayCommand SignupCommand { get;}
        public RelayCommand LoginAccountCommand { get; }
        public Account Account { get=>_account; set => _account = value; }
        public string FullName { get => _fullName; set => _fullName = value; }
        public string PhoneNumber { get => _phoneNumber; set => _phoneNumber = value; }
        public string Username { get => _username; set => _username = value; }
        public string Password { get => _password; set => _password = value; }
        public string RetypePassword { get => _retypePassword; set => _retypePassword = value; }
        public string ErrorMessage { get => _errorMessage; set => _errorMessage = value; }
    }
}
