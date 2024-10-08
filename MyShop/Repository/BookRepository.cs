﻿using Accessibility;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Abstractions;
using MyShop.Model;
using MyShop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Storage;
using static System.Reflection.Metadata.BlobBuilder;

namespace MyShop.Repository
{
    class BookRepository : RepositoryBase, IBookRepository
    {
        public async Task<bool> Add(Book book)
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
                    string sql = "insert into BOOK (title,author,description,genre_id,price,quantity,published_date,image)" +
                        "values (@title,@author,@description,@genre_id,@price,@quantity,@published_date, @image)";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@title", SqlDbType.NVarChar).Value = book.Title;
                    command.Parameters.Add("@author", SqlDbType.NVarChar).Value = book.Author ?? "";
                    command.Parameters.Add("@description", SqlDbType.NVarChar).Value = book.Description ?? "";
                    command.Parameters.Add("@image", SqlDbType.NVarChar).Value = book.Image ?? "";
                    command.Parameters.Add("@genre_id", SqlDbType.Int).Value = book.GenreId;
                    command.Parameters.Add("@price", SqlDbType.Int).Value = book.Price;
                    command.Parameters.Add("@quantity", SqlDbType.Int).Value = book.Quantity;
                    command.Parameters.Add("@published_date", SqlDbType.Date).Value = book.PublishedDate;

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0) { isSuccessful = true; }
                    else { isSuccessful = false; }
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                MessageBox.Show(ex.Message);
            }
            finally
            { 
                connection?.Close();
            }

            return isSuccessful;
        }

        public async Task<bool> AddGenre(Genre genre)
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
                    string sql = "insert into Genre (name) values (@name)";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@name", SqlDbType.NVarChar).Value = genre.Name;

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0) { isSuccessful = true; }
                    else { isSuccessful = false; }
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection?.Close();
            }

         
            return isSuccessful;
        }

        public async Task<bool> Edit(Book book)
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
                    string sql = "update BOOK set title=@title,author=@author,description=@description,genre_id=@genre_id," +
                        "price=@price,quantity=@quantity,published_date=@published_date,image=@image " +
                        "where id = @id";

                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@title", SqlDbType.NVarChar).Value = book.Title;
                    command.Parameters.Add("@author", SqlDbType.NVarChar).Value = book.Author ?? "";
                    command.Parameters.Add("@description", SqlDbType.NVarChar).Value = book.Description ?? "";
                    command.Parameters.Add("@image", SqlDbType.NVarChar).Value = book.Image ?? "";
                    command.Parameters.Add("@genre_id", SqlDbType.Int).Value = book.GenreId;
                    command.Parameters.Add("@price", SqlDbType.Int).Value = book.Price;
                    command.Parameters.Add("@quantity", SqlDbType.Int).Value = book.Quantity;
                    command.Parameters.Add("@published_date", SqlDbType.Date).Value = book.PublishedDate;
                    command.Parameters.Add("@id", SqlDbType.Int).Value = book.Id;
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0) { isSuccessful = true; }
                    else { isSuccessful = false; }
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection?.Close();
            }


            return isSuccessful;
        }

        public async Task<bool> EditGenre(Genre genre)
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
                    string sql = "update GENRE set name=@name where id = @id";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@name", SqlDbType.NVarChar).Value = genre.Name;
                    command.Parameters.Add("@id", SqlDbType.Int).Value = genre.Id;
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0) { isSuccessful = true; }
                    else { isSuccessful = false; }

                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection?.Close();
            }


            return isSuccessful;
        }

        public List<Book> Filter(List<Book> booksList, int startPrice = 0, int endPrice = Int32.MaxValue, string keyword = "", int genre = 0)
        {
            var filteredBooks = booksList.Where(book =>
                book.Price >= startPrice &&
                book.Price <= endPrice &&
                (string.IsNullOrEmpty(keyword) || book.Title.ToLower().Contains(keyword.ToLower())) &&
                (genre == 0 || book.GenreId == genre)
            ).ToList();

            return filteredBooks;
        }

        public async Task<List<Book>> GetAll()
        {
            List<Book> books = new List<Book>();
            var connection = GetConnection();

            try
            {
                await Task.Run(() =>
                {
                    connection.Open();
                }).ConfigureAwait(false);

                if (connection != null && connection.State == ConnectionState.Open)
                {
                    string sql = "select id,title,image,author,description,genre_id,price,quantity,published_date from BOOK order by id desc";
                    var command = new SqlCommand(sql, connection);
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["id"]);
                        string title = Convert.ToString(reader["title"]);
                        string author = Convert.ToString(reader["author"]);
                        string description = Convert.ToString(reader["description"]);
                        string image = Convert.ToString(reader["image"]);
                        int genre_id = Convert.ToInt32(reader["genre_id"]);
                        int price = Convert.ToInt32(reader["price"]);
                        int quantity = Convert.ToInt32(reader["quantity"]);
                        object obj = reader["published_date"];
                        DateOnly published_date = obj == null || obj == DBNull.Value ? default(DateOnly) : DateOnly.FromDateTime(Convert.ToDateTime(obj));

                        books.Add(new Book
                        {
                            Id = id,
                            Title = title,
                            Author = author,
                            Description = description,
                            Image = image,
                            GenreId = genre_id,
                            Price = price,
                            Quantity = quantity,
                            PublishedDate = published_date
                        });
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                books = null;
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection?.Close();
            }

            return books;
        }

        public async Task<Book> GetById(int id)
        {
            Book newBook = null;
            var connection = GetConnection();

            try 
            {
                await Task.Run(() =>
                {
                    connection.Open();
                }).ConfigureAwait(false);

                if (connection != null && connection.State == ConnectionState.Open)
                {
                    string sql = "select title,author,description,image,genre_id,price,quantity,published_date from BOOK" +
                        "where id = @id";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string title = Convert.ToString(reader["title"]);
                        string author = Convert.ToString(reader["author"]);
                        string description = Convert.ToString(reader["description"]);
                        string image = Convert.ToString(reader["image"]);
                        int genre_id = Convert.ToInt32(reader["genre_id"]);
                        int price = Convert.ToInt32(reader["price"]);
                        int quantity = Convert.ToInt32(reader["quantity"]);
                        DateOnly published_date = DateOnly.FromDateTime(Convert.ToDateTime(reader["published_date"]));

                        newBook = new Book
                        {
                            Id = id,
                            Title = title,
                            Author = author,
                            Description = description,
                            Image = image,
                            GenreId = genre_id,
                            Price = price,
                            Quantity = quantity,
                            PublishedDate = published_date
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                newBook = null;
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection?.Close();
            }
        
            return newBook;
        }

        public async Task<List<Genre>> GetGenres()
        {
            List<Genre> genres = new List<Genre>();
            var connection = GetConnection();

            try
            {
                await Task.Run(() =>
                {
                    connection.Open();
                }).ConfigureAwait(false);

                if (connection != null && connection.State == ConnectionState.Open)
                {
                    string sql = "select id,name from GENRE";
                    var command = new SqlCommand(sql, connection);
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["id"]);
                        string name = Convert.ToString(reader["name"]);

                        genres.Add(new Genre
                        {
                            Id = id,
                            Name = name
                        });
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                genres = null;
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection?.Close();
            }

            return genres;
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
                    string sql = "delete from BOOK where id = @id";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0) { isSuccessful = true; }
                    else { isSuccessful = false; }
                }   
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection?.Close();
            }

            return isSuccessful;
        }

        public async Task<bool> RemoveGenre(int id)
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
                    string sql = "delete from GENRE where id = @id";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0) { isSuccessful = true; }
                    else { isSuccessful = false; }
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection?.Close();
            }

            return isSuccessful;
        }

        public async Task<bool> EditBookQuantity(int id, int quantity)
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
                    string sql = "update BOOK set quantity=@quantity where id=@id";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    command.Parameters.Add("@quantity", SqlDbType.Int).Value = quantity;
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0) { isSuccessful = true; }
                    else { isSuccessful = false; }
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection?.Close();
            }
           
            return isSuccessful;
        }

        public async void Refresh(List<Book> books, List<Genre> genres)
        {
            var connection = GetConnection();
            await Task.Run(() =>
            {
                connection.Open();
            }).ConfigureAwait(false);


            SqlCommand command = connection.CreateCommand();
            SqlTransaction transaction = connection.BeginTransaction();
            command.Transaction = transaction;

            try
            {
                // Remove all records related to book from database
                command.CommandText = "DELETE FROM BOOK_PROMOTION";
                command.ExecuteNonQuery();
                command.CommandText = "DELETE FROM DETAILED_BILL";
                command.ExecuteNonQuery();
                command.CommandText = "DELETE FROM BILL";
                command.ExecuteNonQuery();
                command.CommandText = "DELETE FROM BOOK";
                command.ExecuteNonQuery();
                command.CommandText = "DELETE FROM GENRE";
                command.ExecuteNonQuery();
                command.CommandText = "DBCC CHECKIDENT (BOOK, RESEED, 0)";
                command.ExecuteNonQuery();
                command.CommandText = "DBCC CHECKIDENT (GENRE, RESEED, 0)";
                command.ExecuteNonQuery();
                command.CommandText = "DBCC CHECKIDENT (BILL, RESEED, 0)";
                command.ExecuteNonQuery();

                // Prepare the INSERT statement with parameters
                command.CommandText = "INSERT INTO GENRE (name) VALUES (@name)";
                command.Parameters.Add("@name", SqlDbType.NVarChar);
                // Insert all records from List<Genre> instance
                foreach (Genre genre in genres)
                {
                    command.Parameters["@name"].Value = genre.Name;
                    command.ExecuteNonQuery();
                }

                // Prepare the INSERT statement with parameters
                command.CommandText = "insert into BOOK(title,author,description,genre_id,price,quantity,published_date,image) " +
                    "values (@title,@author,@description,@genre_id,@price,@quantity,@published_date,@image)";
                command.Parameters.Add("@title", SqlDbType.NVarChar);
                command.Parameters.Add("@author", SqlDbType.NVarChar);
                command.Parameters.Add("@description", SqlDbType.NVarChar);
                command.Parameters.Add("@image", SqlDbType.NVarChar);
                command.Parameters.Add("@genre_id", SqlDbType.Int);
                command.Parameters.Add("@price", SqlDbType.Int);
                command.Parameters.Add("@quantity", SqlDbType.Int);
                command.Parameters.Add("@published_date", SqlDbType.Date);
                // Insert all records from List<Book> instance
                foreach (Book book in books)
                {
                    command.Parameters["@title"].Value = book.Title;
                    command.Parameters["@author"].Value = book.Author ?? "";
                    command.Parameters["@description"].Value = book.Description ?? "";
                    command.Parameters["@image"].Value = book.Image ?? "";
                    command.Parameters["@genre_id"].Value = book.GenreId;
                    command.Parameters["@price"].Value = book.Price;
                    command.Parameters["@quantity"].Value = book.Quantity;
                    command.Parameters["@published_date"].Value = book.PublishedDate;
                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw ex;
            }
        }

        public async Task<List<Book>> ReadBookDataFromExcelFile(StorageFile file)
        {
            var _books = new List<Book>();

            try
            {
                // If you use EPPlus in a noncommercial context
                // according to the Polyform Noncommercial license:
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                var stream = await file.OpenStreamForReadAsync();
                var excelPackage = new OfficeOpenXml.ExcelPackage(stream);

                string message = file.Name + "\n";
                foreach (var excelWorksheet in excelPackage.Workbook.Worksheets)
                {
                    message += "found sheet: " + excelWorksheet.Name + "\n";
                }
                var worksheet = excelPackage.Workbook.Worksheets["BOOK"];
                int rows = worksheet.Dimension.Rows;
                int columns = worksheet.Dimension.Columns;

                for (int row = 2; row <= rows; row++) // Assuming data starts from row 2
                {
                    // Read data from Excel cells and create a data model
                    Book dataModel = new Book
                    {
                        Id = Convert.ToInt32(worksheet.Cells[row, 1].Value),
                        Title = worksheet.Cells[row, 2].Value?.ToString(),
                        Author = worksheet.Cells[row, 3].Value?.ToString(),
                        Image = worksheet.Cells[row, 4].Value?.ToString(),
                        GenreId = Convert.ToInt32(worksheet.Cells[row, 5].Value?.ToString()),
                        Description = worksheet.Cells[row, 6].Value?.ToString(),
                        PublishedDate = DateOnly.Parse(worksheet.Cells[row, 7].Value?.ToString()),
                        Price = Convert.ToInt32(worksheet.Cells[row, 8].Value),
                        Quantity = Convert.ToInt32(worksheet.Cells[row, 9].Value),
                        // Add more properties for other columns as needed
                    };
                    _books.Add(dataModel); // Add data model to the collection
                }
            }
            catch (Exception ex)
            {
                _books = null;
                MessageBox.Show(ex.Message);
            }

            return _books;
        }

        public async Task<List<Genre>> ReadBookGenreFromExcelFile(StorageFile file)
        {
            var _genres = new List<Genre>();

            try
            {
                // If you use EPPlus in a noncommercial context
                // according to the Polyform Noncommercial license:
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                using (var stream = await file.OpenStreamForReadAsync())
                {
                    using (var excelPackage = new OfficeOpenXml.ExcelPackage(stream))
                    {
                        var worksheet = excelPackage.Workbook.Worksheets["GENRE"];
                        int rows = worksheet.Dimension.Rows;
                        int columns = worksheet.Dimension.Columns;

                        for (int row = 2; row <= rows; row++) // Assuming data starts from row 2
                        {
                            // Read data from Excel cells and create a data model
                            Genre dataModel = new Genre
                            {
                                Id = Convert.ToInt32(worksheet.Cells[row, 1].Value),
                                Name = worksheet.Cells[row, 2].Value?.ToString(),
                                // Add more properties for other columns as needed
                            };
                            _genres.Add(dataModel); // Add data model to the collection
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                _genres = null;
            }
            return _genres;
        }

        public async Task<List<Book>> ReadBookDataFromAccessFile(StorageFile file)
        {
            var books = new List<Book>();

            try
            {
                // Set up the query to retrieve genre data from the file
                var query = "SELECT id,title,author,description,genre_id,price,quantity,published_date,image FROM BOOK";

                // Open a connection to the Access file
                using (var connection = GetOleSqlConnection(file))
                {
                    await connection.OpenAsync();

                    // Create a command to execute the query
                    using (var command = new OleDbCommand(query, connection))
                    {
                        // Execute the query and get a reader to read the results
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            // Read the results into genre objects and add them to the collection
                            while (await reader.ReadAsync())
                            {
                                object obj = reader["published_date"];
                                DateOnly published_date = obj == null || obj == DBNull.Value ? default(DateOnly) : DateOnly.FromDateTime(Convert.ToDateTime(obj));
                                var newBook = new Book
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    Title = reader["title"].ToString(),
                                    Author = reader["author"].ToString(),
                                    Description = reader["description"].ToString(),
                                    GenreId = Convert.ToInt32(reader["genre_id"]),
                                    Price = Convert.ToInt32(reader["price"]),
                                    Quantity = Convert.ToInt32(reader["quantity"]),
                                    PublishedDate = published_date,
                                    Image = reader["image"].ToString(),
                                    // Add more properties for other columns as needed
                                };
                                books.Add(newBook);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                books = null;
            }

            return books;
        }

        public async Task<List<Genre>> ReadBookGenreFromAccessFile(StorageFile file)
        {
            var genres = new List<Genre>();

            try
            {
                // Set up the query to retrieve genre data from the file
                var query = "SELECT id,name FROM GENRE";

                // Open a connection to the Access file
                using (var connection = GetOleSqlConnection(file))
                {
                    await connection.OpenAsync();

                    // Create a command to execute the query
                    using (var command = new OleDbCommand(query, connection))
                    {
                        // Execute the query and get a reader to read the results
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            // Read the results into genre objects and add them to the collection
                            while (await reader.ReadAsync())
                            {
                                var genre = new Genre
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    Name = reader["name"].ToString(),
                                    // Add more properties for other columns as needed
                                };
                                genres.Add(genre);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                genres = null;
                MessageBox.Show(ex.Message);
            }

            return genres;
        }

    }
}
