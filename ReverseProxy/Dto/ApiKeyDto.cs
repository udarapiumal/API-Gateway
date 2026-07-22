namespace ReverseProxy.Dto
{
    public class ApiKeyDto
    {
        public string apikey { get; set; }
        public int requests { get; set; }
        public double avgMs { get; set; }
        public DateTime lastSeen { get; set; }
    }
}
