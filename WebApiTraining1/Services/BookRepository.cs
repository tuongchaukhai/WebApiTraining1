﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using WebApiTraining1.Models;
using WebApiTraining1.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Reflection.Metadata.BlobBuilder;

namespace WebApiTraining1.Services
{
    public class BookRepository : IBookRepository
    {
        private MyDbContext _context;
        private IConfiguration _configuration;

        public BookRepository(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public object GetAll([FromQuery] int? id, [FromQuery] string? title, [FromQuery] string? author, string? sortBy, int page = 1, int rows = 5)
        {
            var book = _context.Books.AsQueryable();

            //Filtering
            if (id.HasValue)
            {
                book = _context.Books.Where(x => x.Id == id);
            }
            if (!string.IsNullOrEmpty(title))
            {
                //book = _context.Books.Where(x => x.Title.Contains(title));
                book = _context.Books.Where(x => x.Title == title);
            }
            if (!string.IsNullOrEmpty(author))
            {
                //book = _context.Books.Where(x => x.Author.Contains(author));
                book = _context.Books.Where(x => x.Author == author);
            }

            //Sorting
            if (book != null)
            {
                //Default sort by title
                book = book.OrderBy(x => x.Title);

                switch (sortBy)
                {
                    case "title_desc": book = book.OrderByDescending(x => x.Title); break;
                    case "author_asc": book = book.OrderBy(x => x.Author); break;
                    case "author_desc": book = book.OrderByDescending(x => x.Author); break;

                }
            }

            //Paging
            book = book.Skip((page - 1) * rows).Take(rows);
            var totalBooks = _context.Books.Count();

            return new { books = book.ToList(), totalBooks };
        }

        public object GetAllWithSP([FromQuery] int? id, [FromQuery] string? title, [FromQuery] string? author, string? sortBy, int page = 1, int rows = 5)
        {
     
            var books = _context.Books.FromSql($"EXEC sp_RetrieveBooks {title}, {author}").ToList();

            var bookSkip = books.Skip((page - 1) * rows).Take(rows);
            var totalBooks = books.Count();

            return new { bookSkip, totalBooks};
        }

        #region ADO.NET func
        //public object GetAllWithSP([FromQuery] int? id, [FromQuery] string? title, [FromQuery] string? author, string? sortBy, int page = 1, int rows = 5)
        //{
        //    List<Book> books = new List<Book>();
        //    using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("MyDbConnection")))
        //    {
        //        using (SqlCommand cmd = new SqlCommand("sp_RetrieveBooks", connection))
        //        {
        //            cmd.CommandType = CommandType.StoredProcedure;
        //            cmd.Parameters.AddWithValue("@title", title);
        //            cmd.Parameters.AddWithValue("@author", author);
        //            connection.Open();

        //            using (var reader = cmd.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    var book = new Book
        //                    {
        //                        Id = reader.GetInt32(0),
        //                        Title = reader.GetString(1),
        //                        Author = reader.GetString(2),
        //                        Ibsn = reader.GetString(3)
        //                    };
        //                    books.Add(book);
        //                }
        //            }
        //        }
        //    }

        //    //Paging
        //    var bookSkip = books.Skip((page - 1) * rows).Take(rows);
        //    var totalBooks = books.Count;
        //    return new { bookSkip, totalBooks };
        //}
        #endregion

        public Book Create(Book book)
        {
            try
            {
                var newBook = new Book
                {
                    Title = book.Title,
                    Author = book.Author,
                    Ibsn = book.Ibsn
                };
                _context.Add(newBook);
                _context.SaveChanges();

                return newBook;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void Update(int id, Book book)
        {
            try
            {
                var targetBook = _context.Books.SingleOrDefault(x => x.Id == id);
                if (targetBook != null)
                {
                    targetBook.Title = book.Title;
                    targetBook.Author = book.Author;
                    targetBook.Ibsn = book.Ibsn;
                    _context.Books.Update(targetBook);
                    _context.SaveChanges();
                }
                else
                {
                    throw new Exception("This book id isn't exist");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Delete(int id)
        {
            try
            {
                var book = _context.Books.SingleOrDefault(x => x.Id == id);
                if (book != null)
                {
                    _context.Books.Remove(book);
                    _context.SaveChanges();
                }
                else
                { throw new Exception("This book doesn't exist"); }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Book Details(int id)
        {
            try
            {
                var book = _context.Books.FirstOrDefault(x => x.Id == id);
                if (book != null)
                {
                    return book;
                }
                else { throw new Exception("This book id doesn't exist"); }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
