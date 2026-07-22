namespace ReverseProxy.Dto
{
    public class PathsDto
    {
        public string path { get; set; }
        public int requests { get; set; }
        public double avgMs { get; set; }
        public double errorRate { get; set; }
    }
}
