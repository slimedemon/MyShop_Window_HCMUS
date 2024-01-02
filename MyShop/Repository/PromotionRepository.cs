using Microsoft.Data.SqlClient;
using MyShop.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace MyShop.Repository
{
    public class PromotionRepository : RepositoryBase, IPromotionRepository
    {
        public async Task<int> AddPromotion(Promotion promotion)
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
                    string sql = "insert into Promotion (name,discount,start_date,end_date) " +
                        "values (@name,@discount,@start_date,@end_date); " +
                        "select ident_current('Promotion');";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@name", SqlDbType.NVarChar).Value = promotion.Name;
                    command.Parameters.Add("@discount", SqlDbType.Int).Value = promotion.Discount;
                    command.Parameters.Add("@start_date", SqlDbType.Date).Value = promotion.StartDate;
                    command.Parameters.Add("@end_date", SqlDbType.Date).Value = promotion.EndDate;
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

 

        public async Task<bool> EditPromotion(Promotion promotion)
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
                    string sql = "update PROMOTION set name=@name,discount=@discount,start_date=@start_date,end_date=@end_date " +
                        "where id=@id";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@id", SqlDbType.Int).Value = promotion.Id;
                    command.Parameters.Add("@name", SqlDbType.NVarChar).Value = promotion.Name;
                    command.Parameters.Add("@discount", SqlDbType.Int).Value = promotion.Discount;
                    command.Parameters.Add("@start_date", SqlDbType.Date).Value = promotion.StartDate;
                    command.Parameters.Add("@end_date", SqlDbType.Date).Value = promotion.EndDate;
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

        public async Task<bool> RemovePromotion(int id)
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
                    string sql = "delete from PROMOTION where id=@id";
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
        public async Task<List<Promotion>> GetAllPromotions(DateOnly? dateFrom, DateOnly? dateTo)
        {
            List<Promotion> promotionList = new List<Promotion>();
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
                        sql = "select id,name,discount,start_date,end_date from PROMOTION";
                        command = new SqlCommand(sql, connection);
                    }
                    else
                    {
                        sql = "select id,name,discount,start_date, end_date from PROMOTION " +
                        "where start_date >= @date_from and end_date <= @date_to";
                        command = new SqlCommand(sql, connection);
                        command.Parameters.Add("@date_from", SqlDbType.Date).Value = dateFrom;
                        command.Parameters.Add("@date_to", SqlDbType.Date).Value = dateTo;
                    }

                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int id = reader["id"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["id"]);
                        string name = reader["name"].Equals(DBNull.Value) ? "" : Convert.ToString(reader["name"]);
                        int discount = reader["discount"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["discount"]);

                        object obj1 = reader["start_date"];
                        DateOnly startDate = obj1 == null || obj1 == DBNull.Value ? default(DateOnly) : DateOnly.FromDateTime(Convert.ToDateTime(obj1));

                        object obj2 = reader["end_date"];
                        DateOnly endDate = obj2 == null || obj2 == DBNull.Value ? default(DateOnly) : DateOnly.FromDateTime(Convert.ToDateTime(obj2));

                        promotionList.Add(new Promotion
                        {
                            Id = id,
                            Name = name,
                            Discount = discount,
                            StartDate = startDate,
                            EndDate = endDate
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                promotionList = null;
            }
            finally
            {
                connection?.Close();
            }

            return promotionList;
        }

        public async Task<Promotion> GetPromotionById(int promotionId)
        {
            Promotion newPromotion = null;
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
                    string sql = "select id,name,discount,start_date,end_date from PROMOTION " +
                        "where id=@id";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@id", SqlDbType.Int).Value = promotionId;
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int id = reader["id"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["id"]);
                        string name = reader["name"].Equals(DBNull.Value) ? "" : Convert.ToString(reader["name"]);
                        int discount = reader["discount"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["discount"]);

                        object obj1 = reader["start_date"];
                        DateOnly startDate = obj1 == null || obj1 == DBNull.Value ? default(DateOnly) : DateOnly.FromDateTime(Convert.ToDateTime(obj1));

                        object obj2 = reader["end_date"];
                        DateOnly endDate = obj2 == null || obj2 == DBNull.Value ? default(DateOnly) : DateOnly.FromDateTime(Convert.ToDateTime(obj2));

                        newPromotion = new Promotion
                        {
                            Id = id,
                            Name = name,
                            Discount = discount,
                            StartDate = startDate,
                            EndDate = endDate
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                newPromotion = null;
            }
            finally
            {
                connection?.Close();
            }

            return newPromotion;
        }

        public async Task<List<BookPromotion>> GetAllBookPromotionsByPromotionId(int promotionId)
        {
            List<BookPromotion> bookPromotions = new List<BookPromotion>();
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
                    string sql = "select bp.promotion_id,bp.book_id,b.title from BOOK_PROMOTION as bp join BOOK as b on bp.book_id=b.id " +
                        "where bp.promotion_id=@promotion_id";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@promotion_id", SqlDbType.Int).Value = promotionId;
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int bookId = reader["book_id"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(reader["book_id"]);
                        string bookName = Convert.ToString(reader["title"]);

                        bookPromotions.Add(new BookPromotion
                        {
                            PromotionId = promotionId,
                            BookId = bookId,
                            BookName = bookName,
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                bookPromotions = null;
            }
            finally
            {
                connection?.Close();
            }

            return bookPromotions;
        }

        public async Task<bool> AddBookPromotion(BookPromotion bookPromotion)
        {
            bool isSuccessful = true;
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
                    string sql = "insert into BOOK_PROMOTION (promotion_id, book_id) " + // revise this
                        "values (@promotion_id, @book_id); ";

                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@promotion_id", SqlDbType.Int).Value = bookPromotion.PromotionId;
                    command.Parameters.Add("@book_id", SqlDbType.Int).Value = bookPromotion.BookId;
                    var result = command.ExecuteNonQuery();

                    if (result >= 1)
                    {
                        isSuccessful = true;
                    }
                    else {
                        isSuccessful = false;
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

            return isSuccessful;
        }

        public async Task<bool> RemoveBookPromotion(int promotionId, int bookId)
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
                    string sql = "delete from BOOK_PROMOTION where promotion_id=@promotion_id and book_id=@book_id";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@promotion_id", SqlDbType.Int).Value = promotionId;
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
