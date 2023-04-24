using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiTraining1.Models;
using WebApiTraining1.Services;
using static System.Reflection.Metadata.BlobBuilder;

namespace WebApiTraining1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : Controller
    {
        private IBookRepository _bookRepository;


        public BooksController(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetAll([FromQuery] int? id, [FromQuery] string? title, [FromQuery] string? author, string? sortBy, int page = 1)
        {
            try
            {
                var result = _bookRepository.GetAll(id, title, author, sortBy);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest("Can't get the book");
            }
        }

        [HttpPost]
        public IActionResult Create(Book book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newBook = _bookRepository.Create(book);

            if (newBook == null)
            {
                return BadRequest();
            }

            return Ok(newBook);
        }

        //[HttpPut("{id}")]
        //public IActionResult Update(int id, Book book)
        //{
        //    var targetBook = _context.Books.SingleOrDefault(x => x.Id == id);
        //    if (targetBook != null)
        //    {
        //        targetBook.Title = book.Title;
        //        targetBook.Author = book.Author;
        //        targetBook.Ibsn = book.Ibsn;
        //        _context.SaveChanges();
        //        return NoContent();
        //    }
        //    else
        //    {
        //        return NotFound();
        //    }
        //}

        //[HttpDelete]
        //public IActionResult Delete(int id)
        //{
        //    var book = _context.Books.SingleOrDefault(x => x.Id == id);
        //    if (book != null)
        //    {
        //        _context.Books.Remove(book);
        //        _context.SaveChanges();
        //        return NoContent();
        //    }
        //    else
        //    {
        //        return NotFound();
        //    }
        //}


        ////////////////////////// ////////////////////////
        //[Authorize(Roles = "staff,admin,customer")]
        //[HttpGet]
        //public IActionResult Get([FromQuery] int? id, [FromQuery] string? title, [FromQuery] string? author)
        //{
        //    var book = _context.Books.AsQueryable();

        //    if(id != null)
        //    {
        //        book = _context.Books.Where(x => x.Id == id);

        //    }
        //    else if(title != null)
        //    {
        //        book = _context.Books.Where(x=>x.Title == title);
        //    }
        //    else if(author != null)
        //    {
        //        book = _context.Books.Where(x => x.Author == author);
        //    }

        //    return Ok(book.ToList());
        //}

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


    }
}
