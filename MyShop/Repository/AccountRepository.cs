﻿using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using Microsoft.UI.Xaml.Controls;
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
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Graphics.Printing3D;
using Windows.UI.ViewManagement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace MyShop.Repository
{
    public class AccountRepository : RepositoryBase, IAccountRepository
    {
        // return base64 encrypted password, base 64 entropy
        private Tuple<string, string> EncryptPassword(string password) 
        {
            var passwordInBytes = Encoding.UTF8.GetBytes(password);
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

            return new Tuple<string, string>(passwordIn64, entropyIn64);
        }

        private string DecryptPassword(string passwordIn64, string entropyIn64)
        {
            byte[] entropyInBytes = Convert.FromBase64String(entropyIn64);
            byte[] cypherTextInBytes = Convert.FromBase64String(passwordIn64);
            string password;
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

            return password;
        }

        public async Task<bool> Add(Account account)
        {
            bool isSuccessful = true;
            var connection = GetConnection();

            try
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        connection.Open();
                    }
                    catch (Exception ex) { await App.MainRoot.ShowDialog("Error", ex.Message); }
                }).ConfigureAwait(false);

                if (connection != null && connection.State == ConnectionState.Open)
                {
                    string sql = "insert into Account (name, phone, address, username, password, entropy) " +
                        "Values(@name, @phone, @address, @username, @password, @entropy)";

                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@name", SqlDbType.NVarChar).Value = account.Name;
                    command.Parameters.Add("@phone", SqlDbType.VarChar).Value = account.PhoneNumber;
                    command.Parameters.Add("@address", SqlDbType.NVarChar).Value = account.Address;
                    command.Parameters.Add("@username", SqlDbType.NVarChar).Value = account.Username;

                    // encrypt password
                    var tuple = EncryptPassword(account.Password);

                    command.Parameters.Add("@password", SqlDbType.NVarChar).Value = tuple.Item1; // Base64 Encrypted Password
                    command.Parameters.Add("@entropy", SqlDbType.NVarChar).Value = tuple.Item2; // Base64 Entropy

                    var result = command.ExecuteNonQuery();

                    if (result > 0) isSuccessful = true;
                    else isSuccessful = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                isSuccessful = false;
            }
            finally
            {
                connection?.Close();
            }

            return isSuccessful;
        }

        // return null => request failed.
        public async Task<string> Authenticate(NetworkCredential credential)
        {
            string message = "";
            var connection = GetConnection();

            try
            {
                bool isValid = false;
                string password, encryptedPasswordIn64 = string.Empty, entropyIn64 = string.Empty;
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
                    string sql = "select password, entropy from ACCOUNT where username = @username";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@username", SqlDbType.NVarChar).Value = credential.UserName;

                    var reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {

                        while (reader.Read())
                        {
                            encryptedPasswordIn64 = Convert.ToString(reader["password"]);
                            entropyIn64 = Convert.ToString(reader["entropy"]);
                        }

                        password = DecryptPassword(encryptedPasswordIn64, entropyIn64);

                        if (password != null && password.Equals(credential.Password))
                        {
                            isValid = true;
                        }
                        else isValid = false;
                    }
                    else
                    { 
                        isValid = false;
                    }

                    message = isValid ? "TRUE" : "* Invalid username or password!";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                message = null;
            }
            finally
            { 
                connection?.Close();
            }

            return message;
        }

        public async Task<bool> UpdateProfile(Account account)
        {
            bool isSuccessful = false;

            if (account.Name == null || account.Name.Equals(""))
            {
                isSuccessful = false;
            }
            else
            {
                var connection = GetConnection();

                try
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            connection.Open();
                        }
                        catch (Exception ex) { MessageBox.Show(ex.Message); }
                    }).ConfigureAwait(false);

                    if (connection != null && connection.State == ConnectionState.Open)
                    {
                        {
                            string sql = "update ACCOUNT set name=@name, phone=@phone, address=@address where id=@id";
                            var command = new SqlCommand(sql, connection);
                            command.Parameters.Add("@id", SqlDbType.Int).Value = account.Id;
                            command.Parameters.Add("@name", SqlDbType.NVarChar).Value = account.Name == null ? DBNull.Value : account.Name;
                            command.Parameters.Add("@phone", SqlDbType.VarChar).Value = account.PhoneNumber == null ? DBNull.Value : account.PhoneNumber;
                            command.Parameters.Add("@address", SqlDbType.NVarChar).Value = account.Address == null ? DBNull.Value : account.Address;

                            int rows = command.ExecuteNonQuery();
                            if (rows > 0)
                            {
                                isSuccessful = true;
                            }
                            else
                            {
                                isSuccessful = false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    isSuccessful = false;
                }
                finally
                { 
                    connection?.Close();
                }
            }

            return isSuccessful;
        }

        public async Task<bool> ChangePassword(Account account)
        {
            bool isSuccessful = false;

            if (account.Password == null || account.Password.Equals(""))
            {
                isSuccessful = false;
            }
            else
            {
                var connection = GetConnection();

                try
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            connection.Open();
                        }
                        catch (Exception ex) { MessageBox.Show(ex.Message); }
                    }).ConfigureAwait(false);

                    if (connection != null && connection.State == ConnectionState.Open)
                    {
                        // Code saving encrypted password (function similar to register)
                        {
                            var tuple = EncryptPassword(account.Password);

                            string sql = "update ACCOUNT set password=@password, entropy=@entropy where id=@id";
                            var command = new SqlCommand(sql, connection);
                            command.Parameters.Add("@id", SqlDbType.Int).Value = account.Id;
                            command.Parameters.Add("@password", SqlDbType.VarChar).Value = tuple.Item1 == null ? DBNull.Value : tuple.Item1;
                            command.Parameters.Add("@entropy", SqlDbType.VarChar).Value = tuple.Item2 == null ? DBNull.Value : tuple.Item2;

                            int rows = command.ExecuteNonQuery();
                            if (rows > 0)
                            {
                                isSuccessful = true;
                            }
                            else
                            {
                                isSuccessful = false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    isSuccessful = false;
                }
                finally
                { 
                    connection?.Close();
                }
            }

            return isSuccessful;
        }

        public async Task<Account> GetById(int id)
        {
            Account account = new Account();
            var connection = GetConnection();

            try
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        connection.Open();
                    }
                    catch (Exception ex) { await App.MainRoot.ShowDialog("Error", ex.Message); }
                }).ConfigureAwait(false);

                if (connection != null && connection.State == ConnectionState.Open)
                {

                    string sql = "select id, name, phone, address, role_id from ACCOUNT";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@id", SqlDbType.Int).Value = id;

                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string name = Convert.ToString(reader["name"]);
                        string phoneNumber = Convert.ToString(reader["phone"]);
                        string address = Convert.ToString(reader["address"]);
                        int role_id = reader["role_id"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["role_id"]);

                        account = new Account
                        {
                            Id = id,
                            Name = name,
                            PhoneNumber = phoneNumber,
                            Address = address,
                        };
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                account = null;
            }
            finally
            { 
                connection?.Close();
            }

            return account;
        }

        public async Task<Account> GetByUsername(string username)
        {
            Account account = new Account();
            var connection = GetConnection();

            try
            {


                await Task.Run(async () =>
                {
                    try
                    {
                        connection.Open();
                    }
                    catch (Exception ex) { await App.MainRoot.ShowDialog("Error", ex.Message); }
                }).ConfigureAwait(false);

                if (connection != null && connection.State == ConnectionState.Open)
                {

                    string sql = "select id, name, phone, address from ACCOUNT where username=@username";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@username", SqlDbType.NVarChar).Value = username;

                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int id = reader["id"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["id"]);
                        string name = Convert.ToString(reader["name"]);
                        string phoneNumber = Convert.ToString(reader["phone"]);
                        string address = Convert.ToString(reader["address"]);

                        account = new Account
                        {
                            Id = id,
                            Name = name,
                            PhoneNumber = phoneNumber,
                            Address = address,
                            Username = username,
                        };
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                account = null;
            }
            finally
            { 
                connection?.Close();
            }

            return account;
        }

        public void Remove(int id)
        {
            throw new NotImplementedException();
        }
    }
}
