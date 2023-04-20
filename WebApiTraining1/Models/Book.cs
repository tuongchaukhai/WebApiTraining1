using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiTraining1.Models
{
    [Table("Book")]
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string IBSN { get; set; }
    }
}
