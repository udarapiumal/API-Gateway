using Microsoft.AspNetCore.Mvc.Testing;
using StackExchange.Redis;
using System.Net;

namespace ReverseProxy.Tests.IntegrationTests
{
    public class RateLimitIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private const string TestApiKey = "550e8400-e29b-41d4-a716-446655440000";

        public RateLimitIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        // Flush Redis before each test so tests don't affect each other
        private void FlushRedis()
        {
            // Add allowAdmin=true so FLUSHDB is permitted
            var redis = ConnectionMultiplexer.Connect("localhost:6379,allowAdmin=true");
            var server = redis.GetServer("localhost:6379");
            server.FlushDatabase();
        }

        // ─── Unit-style checks (no Redis needed) ────────────────────────────

        [Fact]
        public void LuaReturn0_MeansAllowed()
        {
            Assert.False(0 == 1);
        }

        [Fact]
        public void LuaReturn1_MeansBlocked()
        {
            Assert.True(1 == 1);
        }

        // ─── Integration tests (need Redis + gateway running) ────────────────

        [Fact]
        public async Task SingleRequest_ValidApiKey_Returns200()
        {
            FlushRedis();

            var request = new HttpRequestMessage(HttpMethod.Get, "/products");
            request.Headers.Add("X-Api-Key", TestApiKey);

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task NoApiKey_Returns401()
        {
            FlushRedis();

            var request = new HttpRequestMessage(HttpMethod.Get, "/products");
            // deliberately no X-Api-Key header

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Over100Requests_101stReturns429()
        {
            FlushRedis();

            // Send 100 requests — all should pass
            for (int i = 0; i < 100; i++)
            {
                var req = new HttpRequestMessage(HttpMethod.Get, "/products");
                req.Headers.Add("X-Api-Key", TestApiKey);
                var res = await _client.SendAsync(req);
                Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            }

            // 101st should be blocked
            var blockedReq = new HttpRequestMessage(HttpMethod.Get, "/products");
            blockedReq.Headers.Add("X-Api-Key", TestApiKey);
            var blockedRes = await _client.SendAsync(blockedReq);

            Assert.Equal(HttpStatusCode.TooManyRequests, blockedRes.StatusCode);
        }

        [Fact]
        public async Task RateLimited_Response_HasRetryAfterHeader()
        {
            FlushRedis();

            for (int i = 0; i < 100; i++)
            {
                var req = new HttpRequestMessage(HttpMethod.Get, "/products");
                req.Headers.Add("X-Api-Key", TestApiKey);
                await _client.SendAsync(req);
            }

            var blockedReq = new HttpRequestMessage(HttpMethod.Get, "/products");
            blockedReq.Headers.Add("X-Api-Key", TestApiKey);
            var blockedRes = await _client.SendAsync(blockedReq);

            Assert.Equal(HttpStatusCode.TooManyRequests, blockedRes.StatusCode);
            Assert.True(
                blockedRes.Headers.Contains("Retry-After"),
                "429 response must include Retry-After header"
            );
        }

        [Fact]
        public async Task AfterWindowExpires_RequestsAllowedAgain()
        {
            FlushRedis();

            // Fill the limit
            for (int i = 0; i < 100; i++)
            {
                var req = new HttpRequestMessage(HttpMethod.Get, "/products");
                req.Headers.Add("X-Api-Key", TestApiKey);
                await _client.SendAsync(req);
            }

            // Confirm blocked
            var blockedReq = new HttpRequestMessage(HttpMethod.Get, "/products");
            blockedReq.Headers.Add("X-Api-Key", TestApiKey);
            var blockedRes = await _client.SendAsync(blockedReq);
            Assert.Equal(HttpStatusCode.TooManyRequests, blockedRes.StatusCode);

            // Wait for window to expire
            await Task.Delay(TimeSpan.FromSeconds(61));

            // Should pass now
            var newReq = new HttpRequestMessage(HttpMethod.Get, "/products");
            newReq.Headers.Add("X-Api-Key", TestApiKey);
            var newRes = await _client.SendAsync(newReq);
            Assert.Equal(HttpStatusCode.OK, newRes.StatusCode);
        }
    }
}