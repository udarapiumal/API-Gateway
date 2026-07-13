using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReverseProxy.Database;
using ReverseProxy.Dto;

namespace ReverseProxy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public DashboardController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("summary")]
        public async Task<IActionResult> Getsummary()
        {
            var totalRequests = await _dbContext.RequestLogs.CountAsync();
            var avgMs = await _dbContext.RequestLogs.AverageAsync(x => x.ResponseTimeMs);
            var noOferrors = await _dbContext.RequestLogs.CountAsync(x => x.StatusCode != 200);
            var activeApiKeys = await _dbContext.RequestLogs.Select(x => x.ApiKey).Distinct().CountAsync();

            var summaryDto = new SummaryDto
            {
                totalRequests = totalRequests,
                avgResponseTimeMs = avgMs,
                errorRate = (noOferrors / totalRequests) * 100,
                activeApikeys = activeApiKeys

            };

            return Ok(summaryDto);
        }

        [HttpGet]
        [Route("timeseries")]
        public async Task<IActionResult> GetTimeSeries()
        {
            var results = await _dbContext.RequestLogs.
                Where(x => x.Timestamp >= DateTime.UtcNow.AddMinutes(-60))
                .GroupBy(x => new DateTime(x.Timestamp.Year, x.Timestamp.Month,
    x.Timestamp.Day, x.Timestamp.Hour, x.Timestamp.Minute, 0))
                .Select(g => new TimeSeriesDto
                {
                    time = g.Key,
                    requests = g.Count()
                })
                .OrderBy(x => x.time)
                .ToListAsync();


            return Ok(results);
        }



    }
}
