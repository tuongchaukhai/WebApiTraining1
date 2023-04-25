using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiTraining1.Models;

namespace WebApiTraining1.Services
{
    public class BookRepository : IBookRepository
    {
        private MyDbContext _context;
        public static int PAGE_SIZE { get; set; } = 2;

        public BookRepository(MyDbContext context)
        {
            _context = context;
        }

        public List<Book> GetAll([FromQuery] int? id, [FromQuery] string? title, [FromQuery] string? author, string? sortBy, int page = 1)
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
            book = book.Skip((page - 1) * PAGE_SIZE).Take(PAGE_SIZE);

            return book.ToList();
        }



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
    }
}
