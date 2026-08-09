namespace ReverseProxy.Dto
{
    public class SummaryDto
    {
        public int totalRequests { get; set; }
        public double avgResponseTimeMs { get; set; }
        public double errorRate { get; set; }
        public int activeApikeys { get; set; }
    }
}
