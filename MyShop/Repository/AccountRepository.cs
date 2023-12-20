﻿using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using Microsoft.VisualBasic.Logging;
using MyShop.Model;
using MyShop.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace MyShop.Repository
{
    public class AccountRepository : RepositoryBase, IAccountRepository
    {
        public async Task<bool> Add(Account account)
        {
            bool isSuccessful = true;
            var connection = GetConnection();

            await Task.Run(() =>
            {
                connection.Open();
            }).ConfigureAwait(false);

            if (connection != null && connection.State == ConnectionState.Open)
            {
                string sql = "insert into Account (fullname, phone, address, username, password) " +
                    "Values(@fullname, @phone, @address, @username, @password)";

                var command = new SqlCommand(sql, connection);
                command.Parameters.Add("@fullname", SqlDbType.NVarChar).Value = account.Name;
                command.Parameters.Add("@phone", SqlDbType.VarChar).Value = account.PhoneNumber;
                command.Parameters.Add("@address", SqlDbType.NVarChar).Value = account.Address;
                command.Parameters.Add("@username", SqlDbType.NVarChar).Value = account.Username;

                // Encrypt password
                var passwordInBytes = Encoding.UTF8.GetBytes(account.Password);
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
                command.Parameters.Add("@password", SqlDbType.VarChar).Value = passwordIn64;

                var result = command.ExecuteNonQuery();

                if (result > 0) isSuccessful = true;
                else isSuccessful = false;

                connection.Close();
            }

            return isSuccessful;
        }

        public async Task<string> AuthenticateAccount(NetworkCredential credential)
        {
            int role_id = 0;
            bool isValidAccount = false;
            string message = string.Empty;
            var connection = GetConnection();
            string password, passwordIn64 = string.Empty, entropyIn64 = string.Empty;
            await Task.Run(() =>
            {
                try
                {
                    connection.Open();
                }
                catch (Exception ex) { message = "Connection timed out!"; }
            }).ConfigureAwait(false);

            if (connection != null && connection.State == ConnectionState.Open)
            {
                string sql = "select password, entropy, role_id from ACCOUNT where username = @username";
                var command = new SqlCommand(sql, connection);
                command.Parameters.Add("@username", SqlDbType.NVarChar).Value = credential.UserName;

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    passwordIn64 = Convert.ToString(reader["password"]);
                    entropyIn64 = Convert.ToString(reader["entropy"]);
                    role_id = Convert.ToInt32(reader["role_id"]);
                }

                //Decrypting..
                
                byte[] entropyInBytes = Convert.FromBase64String(entropyIn64);
                byte[] cypherTextInBytes = Convert.FromBase64String(passwordIn64);
                try
                {
                    byte[] passwordInBytes = ProtectedData.Unprotect(cypherTextInBytes,
                    entropyInBytes,
                    DataProtectionScope.CurrentUser);
                    password = Encoding.UTF8.GetString(passwordInBytes);
                }
                catch (Exception)
                {
                    password = String.Empty;
                }

                if (role_id == 1 && password.Equals(credential.Password))
                {
                    isValidAccount = true;
                }
                else isValidAccount = false;

                message = isValidAccount ? "TRUE" : "* Invalid username or password!";

                connection.Close();

            }


            return message;
        }

        public async void Edit(Account account)
        {
            string message = string.Empty;
            var connection = GetConnection();
            await Task.Run(() =>
            {
                try
                {
                    connection.Open();
                }
                catch (Exception ex) { message = "Connection timed out!"; }
            }).ConfigureAwait(false);

            if (connection != null && connection.State == ConnectionState.Open)
            {
                // Code saving encrypted password (function similar to register)
                {
                    var passwordInBytes = Encoding.UTF8.GetBytes(account.Password);
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


                    string sql = "update ACCOUNT set password=@password, entropy=@entropy where username=@username";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@username", SqlDbType.NVarChar).Value = account.Username;
                    command.Parameters.Add("@password", SqlDbType.NVarChar).Value = passwordIn64;
                    command.Parameters.Add("@entropy", SqlDbType.NVarChar).Value = entropyIn64;

                    int rows = command.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        //App.MainRoot.ShowDialog("Update status", "Your password has been updated!");
                    }
                    else
                    {
                        //App.MainRoot.ShowDialog("Update status", "An error occurred while updating your password!");
                    }
                    connection.Close();
                }
            }
        }

        public async Task<List<Account>> GetCustomers()
        {
            List<Account> accounts = new List<Account>();
            var connection = GetConnection();

            await Task.Run(() =>
            {
                connection.Open();
            }).ConfigureAwait(false);

            if (connection != null && connection.State == ConnectionState.Open)
            {
                string sql = "select id, fullname, phone from ACCOUNT where role_id = @role_id";
                var command = new SqlCommand(sql, connection);
                command.Parameters.Add("@role_id", SqlDbType.Int).Value = 2;

                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int id = Convert.ToInt32(reader["id"]);
                    string name = Convert.ToString(reader["fullname"]);
                    string phoneNumber = Convert.ToString(reader["phone"]);

                    accounts.Add(new Account
                    {
                        Id = id,
                        Name = name,
                        PhoneNumber = phoneNumber,
                    });
                }

                connection.Close();
            }

            return accounts;
        }

        public async Task<Account> GetById(int id)
        {
            Account account = new Account();
            var connection = GetConnection();

            await Task.Run(() =>
            {
                try
                {
                    connection.Open();
                }
                catch (Exception ex) { }
            }).ConfigureAwait(false);

            if (connection != null && connection.State == ConnectionState.Open)
            {

                string sql = "select id, fullname, phone, address, role_id from ACCOUNT where id=@id";
                var command = new SqlCommand(sql, connection);
                command.Parameters.Add("@id", SqlDbType.Int).Value = id;

                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string name = Convert.ToString(reader["fullname"]);
                    string phoneNumber = Convert.ToString(reader["phone"]);
                    string address = Convert.ToString(reader["address"]);
                    int role_id = Convert.ToInt32(reader["role_id"]);

                    account = new Account
                    {
                        Id = id,
                        Name = name,
                        PhoneNumber = phoneNumber,
                        Address = address,
                        Role = (Role)role_id
                    };
                }

                connection.Close();
            }

            return account;
        }

        public async Task<Account> GetByUsername(string username)
        {
            Account account = new Account();
            var connection = GetConnection();

            await Task.Run(() =>
            {
                try
                {
                    connection.Open();
                }
                catch (Exception ex) { }
            }).ConfigureAwait(false);

            if (connection != null && connection.State == ConnectionState.Open)
            {

                string sql = "select id, fullname, phone, address, role_id from ACCOUNT where username=@username";
                var command = new SqlCommand(sql, connection);
                command.Parameters.Add("@username", SqlDbType.NVarChar).Value = username;

                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int id = Convert.ToInt32(reader["id"]);
                    string name = Convert.ToString(reader["fullname"]);
                    string phoneNumber = Convert.ToString(reader["phone"]);
                    string address = Convert.ToString(reader["address"]);
                    int role_id = Convert.ToInt32(reader["role_id"]);

                    account = new Account
                    {
                        Id = id,
                        Name = name,
                        PhoneNumber = phoneNumber,
                        Address = address,
                        Username = username,
                        Role = (Role)role_id
                    };
                }

                connection.Close();
            }

            return account;
        }

        public void Remove(int id)
        {
            throw new NotImplementedException();
        }
    }
}
