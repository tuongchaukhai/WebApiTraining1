using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiTraining1.Models;
using static System.Reflection.Metadata.BlobBuilder;

namespace WebApiTraining1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : Controller
    {
        private MyDbContext _context;
        public BooksController(MyDbContext context)
        {

            _context = context;
        }

        [Authorize(Roles = "staff,admin,customer")]
        [HttpGet]
        public IActionResult Get([FromQuery] int? id, [FromQuery] string? title, [FromQuery] string? author)
        {
            var book = _context.Books.AsQueryable();

            if(id != null)
            {
                book = _context.Books.Where(x => x.Id == id);

            }
            else if(title != null)
            {
                book = _context.Books.Where(x=>x.Title == title);
            }
            else if(author != null)
            {
                book = _context.Books.Where(x => x.Author == author);
            }

            return Ok(book.ToList());
        }

        //[HttpGet("GetById")]
        //public IActionResult GetById([FromQuery] int id)
        //{
        //    var book = _context.Books.Where(x => x.Id == id);
        //    if (book != null)
        //    {
        //        return Ok(book);
        //    }
        //    else
        //    {
        //        return NotFound();
        //    }
        //}

        //[HttpGet("GetByTitle")]
        //public IActionResult GetByTitle([FromQuery] string title)
        //{
        //    var book = _context.Books.SingleOrDefault(x => x.Title == title);
        //    if (book != null)
        //    {
        //        return Ok(book);
        //    }
        //    else
        //    {
        //        return NotFound();
        //    }
        //}

        [HttpPost]
        public IActionResult Create(Book book)
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
                return Ok(newBook);
            }
            catch
            { 
                return BadRequest();
            }
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, Book book)
        {
            var targetBook = _context.Books.SingleOrDefault(x => x.Id == id);
            if(targetBook != null)
            {
                targetBook.Title = book.Title;
                targetBook.Author = book.Author;
                targetBook.Ibsn = book.Ibsn;
                _context.SaveChanges();
                return NoContent(); 
            }
            else
            {
                return NotFound();
            }
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var book = _context.Books.SingleOrDefault( x => x.Id == id);
            if(book != null)
            {
                _context.Books.Remove(book);
                _context.SaveChanges();
                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }
    }
}
