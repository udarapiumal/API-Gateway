namespace ReverseProxy.Models
{
    public class ApiKey
    {
        public Guid id { get; set; }

        public string? owner { get; set; }

        public string? tier { get; set; }
    }
}
