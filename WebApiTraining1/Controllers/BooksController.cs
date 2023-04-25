using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiTraining1.Models;
using WebApiTraining1.Services;
using WebApiTraining1.ViewModels;
using static System.Reflection.Metadata.BlobBuilder;

namespace WebApiTraining1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : Controller
    {
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;

        public BooksController(IBookRepository bookRepository, IMapper mapper)
        {
            _bookRepository = bookRepository;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetAll([FromQuery] int? id, [FromQuery] string? title, [FromQuery] string? author, string? sortBy, int page = 1)
        {
            try
            {
                var result = _bookRepository.GetAll(id, title, author, sortBy, page);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest("Can't get the book");
            }
        }

        [HttpPost]
        public IActionResult Create(BookViewModel bookVM)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var book = _mapper.Map<Book>(bookVM);
            var newBook = _bookRepository.Create(book);

            if (newBook == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok(newBook);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, BookViewModel bookVM)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var book = _mapper.Map<Book>(bookVM);
                _bookRepository.Update(id, book);
                return NoContent();
            }
           catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

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
