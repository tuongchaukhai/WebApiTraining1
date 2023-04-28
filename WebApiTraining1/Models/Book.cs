using System;
using System.Collections.Generic;

namespace WebApiTraining1.Models;

public partial class Book
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Author { get; set; }

    public string? Ibsn { get; set; }

    public DateTime? LastModified { get; set; }
}
