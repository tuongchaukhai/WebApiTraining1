using System.ComponentModel.DataAnnotations;

namespace WebApiTraining1.ViewModels
{
    public class BookViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title must be less than 100 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Author is required")]
        [StringLength(50, ErrorMessage = "Author must be less than 50 characters")]
        public string Author { get; set; }

        [Required(ErrorMessage = "ISBN is required")]
        [RegularExpression("^(97(8|9))?\\d{9}(\\d|X)$", ErrorMessage = "Invalid ISBN")]
        public string Ibsn { get; set; }
    }
}
