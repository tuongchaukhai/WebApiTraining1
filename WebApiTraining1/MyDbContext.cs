using Microsoft.EntityFrameworkCore;
using WebApiTraining1.Models;

namespace WebApiTraining1
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

        public DbSet<Book> Books { get; set; } 

    }
}
