namespace ReverseProxy.Dto
{
    public class DistributionDto
    {
        public int under10 { get; set; }
        public int under50 { get; set; }
        public int under100 { get; set; }
        public int over100 { get; set; }
    }
}
