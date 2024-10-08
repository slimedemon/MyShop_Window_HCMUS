﻿using Microsoft.Data.SqlClient;
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
    public class StatisticRepository : RepositoryBase, IStatisticRepository
    {
        public async Task<List<Tuple<DateTime, int>>> GetDailyStatistic(DateTime startDate, DateTime endDate)
        {
            var connection = GetConnection();
            

            await Task.Run(() =>
            {
                connection.Open();
            }).ConfigureAwait(false);

            if (connection != null && connection.State == ConnectionState.Open)
            {
                /*                string sql = "select * from BILL as bill" +
                                    "where DATEDIFF(DAY, @startDate, bill.transaction_date) >= 0 and" +
                                    "DATEDIFF(DAY, bill.transaction_date, @endDate) >= 0";*/
                string sql = "GetDailyRevenue";

                var command = new SqlCommand(sql, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@start_date", SqlDbType.Date).Value = startDate.Date;
                command.Parameters.AddWithValue("@end_date", SqlDbType.Date).Value = endDate.Date;
                try
                {
                    List<Tuple<DateTime, int>> result = new List<Tuple<DateTime, int>>();
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        DateTime date = Convert.ToDateTime(reader["date"]);
                        int revenue = Convert.ToInt32(reader["revenue"]);
                        Tuple<DateTime, int> record = new Tuple<DateTime, int>(date, revenue);
                        result.Add(record);
                    }
                    reader.Close();
                    connection.Close();
                    return result;
                }
                catch (Exception ex)
                {
                    connection.Close();
                    MessageBox.Show(ex.Message);
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public async Task<List<Tuple<int, DateTime>>> GetListOfWeeks()
        {
            var connection = GetConnection();
            await Task.Run(() =>
            {
                connection.Open();
            }).ConfigureAwait(false);

            if (connection != null && connection.State == ConnectionState.Open)
            {
                /*                string sql = "select * from BILL as bill" +
                                    "where DATEDIFF(DAY, @startDate, bill.transaction_date) >= 0 and" +
                                    "DATEDIFF(DAY, bill.transaction_date, @endDate) >= 0";*/
                string sql = "GetListOfWeeks";

                var command = new SqlCommand(sql, connection);
                command.CommandType = CommandType.StoredProcedure;
                try
                {
                    List<Tuple<int, DateTime>> result = new List<Tuple<int, DateTime>>();
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int week = reader["week"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["week"]);
                        DateTime start_date = Convert.ToDateTime(reader["start_date"]);
                        Tuple<int, DateTime> record = new Tuple<int, DateTime>(week, start_date);
                        result.Add(record);
                    }
                    reader.Close();
                    connection.Close();
                    return result;
                }
                catch (Exception ex)
                {
                    connection.Close();
                    MessageBox.Show(ex.Message);
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public async Task<List<Tuple<DateTime, int>>> GetWeeklyStatistic(DateTime startDate, DateTime endDate)
        {
            var connection = GetConnection();
            await Task.Run(() =>
            {
                connection.Open();
            }).ConfigureAwait(false);

            if (connection != null && connection.State == ConnectionState.Open)
            {
                /*                string sql = "select * from BILL as bill" +
                                    "where DATEDIFF(DAY, @startDate, bill.transaction_date) >= 0 and" +
                                    "DATEDIFF(DAY, bill.transaction_date, @endDate) >= 0";*/
                string sql = "GetWeeklyRevenue";

                var command = new SqlCommand(sql, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@start_date_start_week", SqlDbType.Date).Value = startDate.Date;
                command.Parameters.AddWithValue("@start_date_end_week", SqlDbType.Date).Value = endDate.Date;
                try
                {
                    List<Tuple<DateTime, int>> result = new List<Tuple<DateTime, int>>();
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        DateTime date = reader["date"].Equals(DBNull.Value) ? DateTime.Now : Convert.ToDateTime(reader["date"]);
                        int revenue = reader["revenue"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["revenue"]);
                        Tuple<DateTime, int> record = new Tuple<DateTime, int>(date, revenue);
                        result.Add(record);
                    }
                    reader.Close();
                    connection.Close();
                    return result;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    connection.Close();
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public async Task<List<Tuple<DateTime, int>>> GetMonthlyStatistic(DateTime startDate, DateTime endDate)
        {
            var connection = GetConnection();
            await Task.Run(() =>
            {
                connection.Open();
            }).ConfigureAwait(false);

            if (connection != null && connection.State == ConnectionState.Open)
            {
                /*                string sql = "select * from BILL as bill" +
                                    "where DATEDIFF(DAY, @startDate, bill.transaction_date) >= 0 and" +
                                    "DATEDIFF(DAY, bill.transaction_date, @endDate) >= 0";*/
                string sql = "GetMonthlyRevenue";

                var command = new SqlCommand(sql, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@start_date_start_month", SqlDbType.Date).Value = startDate.Date;
                command.Parameters.AddWithValue("@start_date_end_month", SqlDbType.Date).Value = endDate.Date;
                try
                {
                    List<Tuple<DateTime, int>> result = new List<Tuple<DateTime, int>>();
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        DateTime date = reader["date"].Equals(DBNull.Value) ? DateTime.Now : Convert.ToDateTime(reader["date"]);
                        int revenue = reader["revenue"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["revenue"]);
                        Tuple<DateTime, int> record = new Tuple<DateTime, int>(date, revenue);
                        result.Add(record);
                    }
                    reader.Close();
                    connection.Close();
                    return result;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    connection.Close();
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public async Task<List<Tuple<DateTime, int>>> GetYearlyStatistic(DateTime startDate, DateTime endDate)
        {
            var connection = GetConnection();
            await Task.Run(() =>
            {
                connection.Open();
            }).ConfigureAwait(false);

            if (connection != null && connection.State == ConnectionState.Open)
            {
                /*                string sql = "select * from BILL as bill" +
                                    "where DATEDIFF(DAY, @startDate, bill.transaction_date) >= 0 and" +
                                    "DATEDIFF(DAY, bill.transaction_date, @endDate) >= 0";*/
                string sql = "GetYearlyRevenue";

                var command = new SqlCommand(sql, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@start_date_start_year", SqlDbType.Date).Value = startDate.Date;
                command.Parameters.AddWithValue("@start_date_end_year", SqlDbType.Date).Value = endDate.Date;
                try
                {
                    List<Tuple<DateTime, int>> result = new List<Tuple<DateTime, int>>();
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        DateTime date = reader["date"].Equals(DBNull.Value) ? DateTime.Now : Convert.ToDateTime(reader["date"]);
                        int revenue = reader["revenue"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["revenue"]);
                        Tuple<DateTime, int> record = new Tuple<DateTime, int>(date, revenue);
                        result.Add(record);
                    }
                    reader.Close();
                    connection.Close();
                    return result;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    connection.Close();
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public async Task<List<Tuple<string, int>>> GetProductStatistic(DateTime startDate, DateTime endDate)
        {
            List<Book> books = new List<Book>();
            var connection = GetConnection();

            await Task.Run(() =>
            {
                connection.Open();
            }).ConfigureAwait(false);

            if (connection != null && connection.State == ConnectionState.Open)
            {
                string sql = "SELECT Book.title, ISNULL(SUM(Detailed_bill.number), 0) AS number " +
                             "FROM Book " +
                             "JOIN Detailed_bill ON Book.id = Detailed_bill.book_id " +
                             "JOIN Bill ON Bill.id = Detailed_bill.bill_id AND Bill.transaction_date BETWEEN @startDate AND @endDate " +
                             "GROUP BY Book.title";

                var command = new SqlCommand(sql, connection);
                command.Parameters.Add("@startDate", SqlDbType.Date).Value = startDate.Date;
                command.Parameters.Add("@endDate", SqlDbType.Date).Value = endDate.Date;

                try
                {
                    List<Tuple<string, int>> result = new List<Tuple<string, int>>();
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string title = Convert.ToString(reader["title"]);
                        int quantity = reader["number"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["number"]);
                        Tuple<string, int> record = new Tuple<string, int>(title, quantity);
                        result.Add(record);
                    }
                    reader.Close();
                    connection.Close();
                    return result;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    connection.Close();
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public async Task<int> GetCurrentMonthlyNumberOfSoldBookStatistic()
        {
            int numberOfSoldBooks = 0;
            var connection = GetConnection();

            DateTime endDate = DateTime.Now;
            DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0);

            await Task.Run(() =>
            {
                connection.Open();
            }).ConfigureAwait(false);

            if (connection != null && connection.State == ConnectionState.Open)
            {
                string sql = "select sum(DETAILED_BILL.number) quantity " +
                             "from DETAILED_BILL join BILL on DETAILED_BILL.bill_id = BILL.id" +
                             " where BILL.transaction_date BETWEEN @startDate AND @endDate";

                var command = new SqlCommand(sql, connection);
                command.Parameters.Add("@startDate", SqlDbType.Date).Value = startDate.Date;
                command.Parameters.Add("@endDate", SqlDbType.Date).Value = endDate.Date;


                try
                {
                    List<Tuple<string, int>> result = new List<Tuple<string, int>>();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        numberOfSoldBooks = reader["quantity"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["quantity"]);
                    }
                    reader.Close();
                    connection.Close();
                    return numberOfSoldBooks;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    connection.Close();
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        public async Task<int> GetNumberOfBooks()
        {
            int numberOfOrder = 0;
            var connection = GetConnection();

            await Task.Run(() =>
            {
                connection.Open();
            }).ConfigureAwait(false);

            if (connection != null && connection.State == ConnectionState.Open)
            {
                string sql = "select count(id) as NumberOfBooks " +
                             "from BOOK";

                var command = new SqlCommand(sql, connection);
                try
                {
                    List<Tuple<string, int>> result = new List<Tuple<string, int>>();
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        numberOfOrder = reader["NumberOfBooks"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["NumberOfBooks"]);
                    }
                    reader.Close();
                    connection.Close();
                    return numberOfOrder;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    connection.Close();
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        public async Task<List<Tuple<string, int>>> GetTop5ProductStatistic(DateTime startDate, DateTime endDate)
        {
            List<Book> books = new List<Book>();
            var connection = GetConnection();

            await Task.Run(() =>
            {
                connection.Open();
            }).ConfigureAwait(false);

            if (connection != null && connection.State == ConnectionState.Open)
            {
                string sql = "select TOP 5 Book.title, sum(Detailed_bill.number) as quantity " +
                             "from Book join Detailed_bill on Book.id=Detailed_bill.book_id " +
                             "join Bill on Bill.id=Detailed_bill.bill_id " +
                             "where Bill.transaction_date between @startDate and @endDate " +
                             "group by Book.title " +
                             "order by quantity desc";

                var command = new SqlCommand(sql, connection);
                command.Parameters.Add("@startDate", SqlDbType.Date).Value = startDate.Date;
                command.Parameters.Add("@endDate", SqlDbType.Date).Value = endDate.Date;

                try
                {
                    List<Tuple<string, int>> result = new List<Tuple<string, int>>();
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string title = Convert.ToString(reader["title"]);
                        int quantity = reader["quantity"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["quantity"]);
                        Tuple<string, int> record = new Tuple<string, int>(title, quantity);
                        result.Add(record);
                    }
                    reader.Close();
                    connection.Close();
                    return result;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    connection.Close();
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public async Task<List<Tuple<string, int>>> GetProductQuantityStatistic()
        {
            List<Book> books = new List<Book>();
            var connection = GetConnection();

            await Task.Run(() =>
            {
                connection.Open();
            }).ConfigureAwait(false);

            if (connection != null && connection.State == ConnectionState.Open)
            {
                string sql = "select book.title, book.quantity " +
                             "from book" +
                             " order by book.quantity asc";

                var command = new SqlCommand(sql, connection);

                try
                {
                    List<Tuple<string, int>> result = new List<Tuple<string, int>>();
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string title = Convert.ToString(reader["title"]);
                        int quantity = reader["quantity"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["quantity"]);
                        Tuple<string, int> record = new Tuple<string, int>(title, quantity);
                        result.Add(record);
                    }
                    reader.Close();
                    connection.Close();
                    return result;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    connection.Close();
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public async Task<List<Tuple<DateTime, int>>> GetMonthRevenuesOfYear()
        {
            DateTime startDate = new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0);
            DateTime endDate = DateTime.Now;

            var connection = GetConnection();
            await Task.Run(() =>
            {
                connection.Open();
            }).ConfigureAwait(false);

            if (connection != null && connection.State == ConnectionState.Open)
            {
                /*                string sql = "select * from BILL as bill" +
                                    "where DATEDIFF(DAY, @startDate, bill.transaction_date) >= 0 and" +
                                    "DATEDIFF(DAY, bill.transaction_date, @endDate) >= 0";*/
                string sql = "GetMonthlyRevenue";

                var command = new SqlCommand(sql, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@start_date_start_month", SqlDbType.Date).Value = startDate.Date;
                command.Parameters.AddWithValue("@start_date_end_month", SqlDbType.Date).Value = endDate.Date;
                try
                {
                    List<Tuple<DateTime, int>> result = new List<Tuple<DateTime, int>>();
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        DateTime date = reader["date"].Equals(DBNull.Value) ? DateTime.Now : Convert.ToDateTime(reader["date"]);
                        int revenue = reader["revenue"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["revenue"]);
                        Tuple<DateTime, int> record = new Tuple<DateTime, int>(date, revenue);
                        result.Add(record);
                    }
                    reader.Close();
                    connection.Close();
                    return result;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    connection.Close();
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

    }
}
