using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
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
        private readonly ILogger<BooksController> _logger;

        public BooksController(IBookRepository bookRepository, IMapper mapper, ILogger<BooksController> logger)
        {
            _bookRepository = bookRepository;
            _mapper = mapper;
            _logger = logger;
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

        [Authorize(Roles = "staff")]
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
                var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";
                _logger.LogInformation("Book \"{id}\" is created successfully by {email}", book.Id, email);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok(newBook);
        }

        [Authorize(Roles = "staff")]
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
                var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";
                _logger.LogInformation("Book \"{id}\" is updated successfully by {email}",book.Id, email);
                return NoContent();
            }
           catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating book with id {id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _bookRepository.Delete(id);
                var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";
                _logger.LogInformation("Book \"{id}\" is updated successfully by {email}", id, email);
                return NoContent();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error deleting book with id {id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

    }
}
