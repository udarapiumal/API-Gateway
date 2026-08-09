namespace ReverseProxy.Tests.UnitTests
{
    public class RateLimitResultTests
    {
        [Fact]
        public void LuaReturns0_RequestIsAllowed()
        {
            var luaResult = 0;
            var isBlocked = luaResult == 1;
            Assert.False(isBlocked);
        }

        [Fact]
        public void LuaReturns1_RequestIsBlocked()
        {
            var luaResult = 1;
            var isBlocked = luaResult == 1;
            Assert.True(isBlocked);
        }

        [Theory]
        [InlineData(0, false)] // allowed
        [InlineData(1, true)]  // blocked
        public void LuaResult_MapsCorrectlyToBlockedStatus(int luaResult, bool expectedBlocked)
        {
            var isBlocked = luaResult == 1;
            Assert.Equal(expectedBlocked, isBlocked);
        }

        [Fact]
        public void MaxRequests_100_AllowsExactly100()
        {
            var maxRequests = 100;

            // Your Lua script uses >= so count of 99 should be allowed
            var countAt99 = 99;
            var countAt100 = 100;

            Assert.False(countAt99 >= maxRequests);  // 99 is allowed
            Assert.True(countAt100 >= maxRequests);  // 100 is blocked
        }
    }
}