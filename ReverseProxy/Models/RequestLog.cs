namespace ReverseProxy.Models
{
    public class RequestLog
    {
        public int Id { get; set; }
        public string ApiKey { get; set; }
        public string Path { get; set; }
        public string Method { get; set; }
        public int StatusCode { get; set; }
        public long ResponseTimeMs { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
