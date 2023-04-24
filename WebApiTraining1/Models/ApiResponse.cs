using Microsoft.AspNetCore.Mvc;

namespace WebApiTraining1.Models
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public SerializableError ModelState { get; set; }
    }
}
