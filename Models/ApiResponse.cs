namespace JobAppHR.Models
{
    public class ApiResponse
    {
        public object result { get; set; }
        public string message { get; set; }
        public bool isSuccess {get; set; } = false;
    }
}
