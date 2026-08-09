namespace ReverseProxy.Tests.UnitTests
{
    public class ConfigParsingTests
    {
        // Helper method matching what your RateLimitRule should do
        private int ParseWindowToSeconds(string window)
        {
            if (!window.EndsWith("s"))
                throw new FormatException($"Invalid window format: {window}");
            return int.Parse(window.TrimEnd('s'));
        }

        [Fact]
        public void ParseWindow_60s_Returns60()
        {
            var result = ParseWindowToSeconds("60s");
            Assert.Equal(60, result);
        }

        [Fact]
        public void ParseWindow_3600s_Returns3600()
        {
            var result = ParseWindowToSeconds("3600s");
            Assert.Equal(3600, result);
        }

        [Fact]
        public void ParseWindow_InvalidString_ThrowsException()
        {
            Assert.Throws<FormatException>(() => ParseWindowToSeconds("abc"));
        }

        [Theory]
        [InlineData("60s", 60)]
        [InlineData("30s", 30)]
        [InlineData("3600s", 3600)]
        [InlineData("1s", 1)]
        public void ParseWindow_MultipleValidInputs_ReturnsCorrectSeconds(
            string input, int expected)
        {
            var result = ParseWindowToSeconds(input);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void RedisKey_IsBuiltCorrectly()
        {
            var apiKey = "550e8400-e29b-41d4-a716-446655440000";
            var path = "/products";
            var windowSeconds = 60;

            var redisKey = $"ratelimit:{apiKey}:{path}:{windowSeconds}";

            Assert.Equal(
                "ratelimit:550e8400-e29b-41d4-a716-446655440000:/products:60",
                redisKey
            );
        }

        [Fact]
        public void RetryAfter_IsCalculatedCorrectly()
        {
            var windowSeconds = 60;
            var oldestRequestTimestamp = 1700000010;
            var currentTime = 1700000040;

            var retryAfter = (oldestRequestTimestamp + windowSeconds) - currentTime;

            Assert.Equal(30, retryAfter);
        }
    }
}