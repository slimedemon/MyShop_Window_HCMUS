using Microsoft.Data.SqlClient;
using MyShop.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyShop.Repository
{
    public class CustomerRepository: RepositoryBase, ICustomerRepository
    {
        public async Task<int> Add(Customer customer)
        {
            var connection = GetConnection();
            int id = 0;

            try
            {
                await Task.Run(() =>
                {
                    connection.Open();
                }).ConfigureAwait(false);

                if (connection != null && connection.State == ConnectionState.Open)
                {
                    string sql = "insert into CUSTOMER (name,phone,address)" +
                        "values (@name, @phone, @address); " +
                        " select ident_current('customer');";
                    var command = new SqlCommand(sql, connection);

                    command.Parameters.Add("@name", SqlDbType.NVarChar).Value = customer.Name == null ? DBNull.Value : customer.Name;
                    command.Parameters.Add("@phone", SqlDbType.VarChar).Value = customer.PhoneNumber == null ? DBNull.Value : customer.PhoneNumber;
                    command.Parameters.Add("@address", SqlDbType.NVarChar).Value = customer.Address == null ? DBNull.Value : customer.Address;

                    try
                    {
                        id = (int)((decimal)command.ExecuteScalar());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                id = -1;
            }
            finally
            { 
                connection?.Close();
            }

            return id;
        }

        public async Task<bool> Edit(Customer customer)
        {
            bool isSuccessful = false;
            var connection = GetConnection();

            try
            {
                await Task.Run(() =>
                {
                    connection.Open();
                }).ConfigureAwait(false);

                if (connection != null && connection.State == ConnectionState.Open)
                {
                    string sql = "update CUSTOMER set name=@name,phone=@phone,address=@address " +
                        "where id = @id";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@id", SqlDbType.NVarChar).Value = customer.Name == null ? DBNull.Value : customer.Id;
                    command.Parameters.Add("@name", SqlDbType.NVarChar).Value = customer.Name == null ? DBNull.Value : customer.Name;
                    command.Parameters.Add("@phone", SqlDbType.VarChar).Value = customer.PhoneNumber == null ? DBNull.Value : customer.PhoneNumber;
                    command.Parameters.Add("@address", SqlDbType.NVarChar).Value = customer.Address == null ? DBNull.Value : customer.Address;
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0) { isSuccessful = true; }
                    else { isSuccessful = false; }

                    connection.Close();
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

        public async Task<List<Customer>> GetAll(DateOnly? dateFrom, DateOnly? dateTo)
        {
            List<Customer> customers = new List<Customer>();
            var connection = GetConnection();

            try
            {
                await Task.Run(() =>
                {
                    connection.Open();
                }).ConfigureAwait(false);

                if (connection != null && connection.State == ConnectionState.Open)
                {
                    string sql = "select c.id,c.name,c.phone,c.address from CUSTOMER as c " +
                        "join BILL as b on b.customer_id = c.id where b.transaction_date between @date_from and @date_to " +
                        "group by c.id, c.name, c.phone, c.address";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@date_from", SqlDbType.Date).Value = dateFrom;
                    command.Parameters.Add("@date_to", SqlDbType.Date).Value = dateTo;

                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int id = reader["id"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["id"]);
                        string name = Convert.ToString(reader["name"]);
                        string phone = Convert.ToString(reader["phone"]);
                        string address = Convert.ToString(reader["address"]);

                        customers.Add(new Customer
                        {
                            Id = id,
                            Name = name,
                            PhoneNumber = phone,
                            Address = address,
                        });
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                customers = null;
            }
            finally
            {
                connection?.Close();
            }

            return customers;
        }

        public async Task<Customer> GetById(int id)
        {
            Customer customer = null;
            var connection = GetConnection();

            try
            {
                await Task.Run(() =>
                {
                    connection.Open();
                }).ConfigureAwait(false);

                if (connection != null && connection.State == ConnectionState.Open)
                {
                    string sql = "select name,phone,address from CUSTOMER " +
                        "where id = @id";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string name = Convert.ToString(reader["name"]);
                        string phone = Convert.ToString(reader["phone"]);
                        string address = Convert.ToString(reader["address"]);

                        customer = new Customer
                        {
                            Id = id,
                            Name = name,
                            PhoneNumber = phone,
                            Address = address,
                        };

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                customer = null;
            }
            finally
            {
                connection?.Close();
            }
          
            return customer;
        }

        public async Task<bool> Remove(int id)
        {
            bool isSuccessful = false;
            var connection = GetConnection();

            try
            {
                await Task.Run(() =>
                {
                    connection.Open();
                }).ConfigureAwait(false);

                if (connection != null && connection.State == ConnectionState.Open)
                {
                    string sql = "delete from CUSTOMER where id = @id";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0) { isSuccessful = true; }
                    else { isSuccessful = false; }

                    connection.Close();
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
    }
}
