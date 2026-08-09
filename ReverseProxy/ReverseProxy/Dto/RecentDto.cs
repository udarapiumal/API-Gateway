namespace ReverseProxy.Dto
{
    public class RecentDto
    {
        public string apiKey { get; set; }
        public string path { get; set; }
        public string method { get; set; }
        public int statusCode { get; set; }
        public long ResponseTimeMs { get; set; }
        public DateTime Timestamp { get; set; }

    }
}
