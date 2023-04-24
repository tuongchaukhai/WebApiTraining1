using Microsoft.AspNetCore.Mvc;
using System.Transactions;
using WebApiTraining1.Models;

namespace WebApiTraining1.Services
{
    public interface IBookRepository
    {
        List<Book> GetAll([FromQuery] int? id, [FromQuery] string? title, [FromQuery] string? author, string? sortBy, int page = 1);

        Book Create(Book book);
    }
}
