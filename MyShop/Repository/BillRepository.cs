using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using MyShop.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace MyShop.Repository
{
    class BillRepository : RepositoryBase, IBillRepository
    {
        public async Task<int> Add(Bill bill)
        {
            int id = 0;
            var connection = GetConnection();
            
            try
            {
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
                    string sql = "insert into BILL (customer_id,total_price,transaction_date) " +
                        "values (@customer_id,@total_price,@transaction_date); " +
                        "select ident_current('BILL');"; ;
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@customer_id", SqlDbType.Int).Value = bill.CustomerId;
                    command.Parameters.Add("@total_price", SqlDbType.Int).Value = bill.TotalPrice;
                    command.Parameters.Add("@transaction_date", SqlDbType.Date).Value = bill.TransactionDate;
                    id = (int)((decimal)command.ExecuteScalar());
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

        public async Task<bool> Edit(Bill bill)
        {
            bool isSuccessful = false;
            var connection = GetConnection();

            try
            {
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
                    string sql = "update BILL set customer_id=@customer_id,total_price=@total_price,transaction_date=@transaction_date " +
                        "where id=@id";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@id", SqlDbType.Int).Value = bill.Id;
                    command.Parameters.Add("@customer_id", SqlDbType.Int).Value = bill.CustomerId;
                    command.Parameters.Add("@total_price", SqlDbType.Int).Value = bill.TotalPrice;
                    command.Parameters.Add("@transaction_date", SqlDbType.Date).Value = bill.TransactionDate;
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0) { isSuccessful = true; }
                    else { isSuccessful = false; }
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

        public async Task<bool> Remove(int id)
        {
            bool isSuccessful = false;
            var connection = GetConnection();

            try
            {
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
                    string sql = "delete from BILL where id=@id";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0) { isSuccessful = true; }
                    else { isSuccessful = false; }
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

        public async Task<List<Bill>> GetAll(DateOnly? dateFrom, DateOnly? dateTo)
        {
            List<Bill> billList = new List<Bill>();
            var connection = GetConnection();

            try
            {

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
                    string sql;
                    var command = new SqlCommand();

                    if (dateFrom == null || dateTo == null)
                    {
                        sql = "select id,customer_id,total_price,transaction_date from BILL";
                        command = new SqlCommand(sql, connection);
                    }
                    else
                    {
                        sql = "select id,customer_id,total_price,transaction_date from BILL " +
                        "where transaction_date between @date_from and @date_to";
                        command = new SqlCommand(sql, connection);
                        command.Parameters.Add("@date_from", SqlDbType.Date).Value = dateFrom;
                        command.Parameters.Add("@date_to", SqlDbType.Date).Value = dateTo;
                    }

                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int id = reader["id"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["id"]);
                        int customerId = reader["customer_id"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["customer_id"]);
                        int totalPrice = reader["total_price"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["total_price"]);

                        object obj = reader["transaction_date"];
                        DateOnly transactionDate = obj == null || obj == DBNull.Value ? default(DateOnly) : DateOnly.FromDateTime(Convert.ToDateTime(obj));

                        billList.Add(new Bill
                        {
                            Id = id,
                            CustomerId = customerId,
                            TotalPrice = totalPrice,
                            TransactionDate = transactionDate
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                billList = null;
            }
            finally
            {
                connection?.Close();   
            }

            return billList;
        }

        public async Task<Bill> GetById(int id)
        {
            Bill newBill = null;
            var connection = GetConnection();

            try
            {
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
                    string sql = "select id,customer_id,total_price,transaction_date from BILL " +
                        "where id=@id";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int customerId = reader["customer_id"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["customer_id"]);
                        int totalPrice = reader["total_price"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["total_price"]);

                        object obj = reader["transaction_date"];
                        DateOnly transactionDate = obj == null || obj == DBNull.Value ? default(DateOnly) : DateOnly.FromDateTime(Convert.ToDateTime(obj));

                        newBill = new Bill
                        {
                            Id = id,
                            CustomerId = customerId,
                            TotalPrice = totalPrice,
                            TransactionDate = transactionDate
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                newBill = null;
            }
            finally
            {
                connection?.Close();
            }

            return newBill;
        }

        public async Task<Bill> GetByCustomerId(int customerId)
        {
            Bill newBill = null;
            var connection = GetConnection();

            try
            {
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
                    string sql = "select id,customer_id,total_price,transaction_date from BILL " +
                        "where customer_id=@customer_id";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@customer_id", SqlDbType.Int).Value = customerId;
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int id = reader["id"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["id"]);
                        int totalPrice = reader["total_price"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["total_price"]);

                        object obj = reader["transaction_date"];
                        DateOnly transactionDate = obj == null || obj == DBNull.Value ? default(DateOnly) : DateOnly.FromDateTime(Convert.ToDateTime(obj));

                        newBill = new Bill
                        {
                            Id = id,
                            CustomerId = customerId,
                            TotalPrice = totalPrice,
                            TransactionDate = transactionDate
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                newBill = null;
            }
            finally
            {
                connection?.Close();
            }

            return newBill;
        }

        public async Task<List<int>> GetEmptyBillId()
        {
            List<int> ids = new List<int>();
            var connection = GetConnection();

            try
            {
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
                    string sql = "select id from BILL " +
                        "where total_price=@total_price";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@total_price", SqlDbType.Int).Value = 0;
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int id = reader["id"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["id"]);
                        ids.Add(id);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                ids = null;
            }
            finally 
            {
                connection?.Close();
            }
          
            return ids;
        }

        public async Task<List<Order>> GetBillDetailById(int billId)
        {
            List<Order> details = new List<Order>();
            var connection = GetConnection();

            try 
            {
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
                    string sql = "select db.book_id,db.number,b.title,b.price,b.quantity,db.promotion_id,p.name,p.discount from DETAILED_BILL as db join BOOK as b on db.book_id=b.id " +
                        "left join Promotion as p on db.promotion_id = p.id " +
                        "where db.bill_id=@bill_id";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@bill_id", SqlDbType.Int).Value = billId;
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int bookId = reader["book_id"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["book_id"]);
                        int price = reader["price"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["price"]);
                        int number = reader["number"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["number"]);
                        int quantity = reader["quantity"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["quantity"]);
                        string title = Convert.ToString(reader["title"]);
                        int promotionId = reader["promotion_id"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["promotion_id"]);
                        string promotionName= Convert.ToString(reader["name"]);
                        int discount = reader["discount"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["discount"]);

                        details.Add(new Order
                        {
                            BillId = billId,
                            BookId = bookId,
                            Price = price,
                            Number = number,
                            StockQuantity = quantity,
                            BookName = title,
                            PromotionId = promotionId,
                            PromotionName = promotionName,
                            Discount = discount
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                details = null;
            }
            finally
            {
                connection?.Close();
            }
          
            return details;
        }

        public async Task<List<int>> GetBookIdsById(int billId)
        {
            List<int> bookIds = new List<int>();
            var connection = GetConnection();

            try
            {
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
                    string sql = "select book_id from DETAILED_BILL " +
                        "where bill_id=@bill_id";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@bill_id", SqlDbType.Int).Value = billId;
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int bookId = reader["book_id"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["book_id"]);
                        bookIds.Add(bookId);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                bookIds = null;
            }
            finally
            {
                connection?.Close();
            }

            return bookIds;
        }

        public async Task<bool> AddBillDetail(Order billDetail)
        {
            bool isSuccessful = false;

            var connection = GetConnection();

            try
            {
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
                    string sql = "insert into DETAILED_BILL (bill_id,book_id,price,number,promotion_id) " + // revise this
                        "values (@bill_id,@book_id,@price,@number,@promotion_id)";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@bill_id", SqlDbType.Int).Value = billDetail.BillId;
                    command.Parameters.Add("@book_id", SqlDbType.Int).Value = billDetail.BookId;
                    command.Parameters.Add("@price", SqlDbType.Int).Value = billDetail.Price;
                    command.Parameters.Add("@number", SqlDbType.Int).Value = billDetail.Number;
                    command.Parameters.Add("@promotion_id", SqlDbType.Int).Value = billDetail.PromotionId <= 0 ? DBNull.Value: billDetail.PromotionId;
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0) { isSuccessful = true; }
                    else { isSuccessful = false; }
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
        
        public async Task<bool> EditBillDetail(Order billDetail)
        {
            bool isSuccessful = false;
            var connection = GetConnection();

            try
            {
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
                    string sql = "update DETAILED_BILL set price=@price,number=@number,promotion_id=@promotion_id " +
                        "where bill_id = @bill_id and book_id = @book_id";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@bill_id", SqlDbType.Int).Value = billDetail.BillId;
                    command.Parameters.Add("@book_id", SqlDbType.Int).Value = billDetail.BookId;
                    command.Parameters.Add("@price", SqlDbType.Int).Value = billDetail.Price;
                    command.Parameters.Add("@number", SqlDbType.Int).Value = billDetail.Number;
                    command.Parameters.Add("@promotion_id", SqlDbType.Int).Value = billDetail.PromotionId <= 0 ? DBNull.Value : billDetail.PromotionId;
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0) { isSuccessful = true; }
                    else { isSuccessful = false; }

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
        
        public async Task<bool> RemoveBillDetail(int billId, int bookId)
        {
            bool isSuccessful = false;

            var connection = GetConnection();
            try
            {
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
                    string sql = "delete from DETAILED_BILL where bill_id=@bill_id and book_id=@book_id";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@bill_id", SqlDbType.Int).Value = billId;
                    command.Parameters.Add("@book_id", SqlDbType.Int).Value = bookId;
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0) { isSuccessful = true; }
                    else { isSuccessful = false; }
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
