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
                errorRate = totalRequests == 0 ? 0 : ((double)noOferrors / totalRequests) * 100,
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

        [HttpGet]
        [Route("statuscodes")]
        public async Task<IActionResult> GetStatusCodes()
        {
            var results = await _dbContext.RequestLogs.
                GroupBy(x => x.StatusCode).
                Select(g => new StatusCodesDto
                {
                    statusCode = g.Key,
                    count = g.Count()
                })
                .ToListAsync();

            return Ok(results);
        }

        [HttpGet]
        [Route("paths")]
        public async Task<IActionResult> GetPaths()
        {
            var results = await _dbContext.RequestLogs
                  .GroupBy(x => x.Path)
                  .Select(g => new PathsDto
                  {
                      path = g.Key,
                      requests = g.Count(),
                      avgMs = g.Average(x => x.ResponseTimeMs),
                      errorRate = (double)g.Count(x => x.StatusCode != 200) / g.Count() * 100
                  })
                  .OrderByDescending(x => x.requests)
                  .ToListAsync();

            return Ok(results);
        }
        [HttpGet]
        [Route("apikeys")]
        public async Task<IActionResult> GetApiKeys()
        {
            var results = await _dbContext.RequestLogs
                 .GroupBy(x => x.ApiKey)
                 .Select(g => new ApiKeyDto
                 {
                     apikey = g.Key,
                     requests = g.Count(),
                     avgMs = g.Average(x => x.ResponseTimeMs),
                     lastSeen = g.Max(x => x.Timestamp)
                 })
                 .OrderByDescending(x => x.requests)
                 .ToListAsync();

            return Ok(results);
        }
        [HttpGet]
        [Route("recent")]
        public async Task<IActionResult> GetRecent()
        {
            var results = await _dbContext.RequestLogs
                 .OrderByDescending(x => x.Timestamp)
                 .Take(20)
                 .ToListAsync();

            return Ok(results);
        }
        [HttpGet]
        [Route("distribution")]
        public async Task<IActionResult> GetDistribution()
        {
            var under10 = await _dbContext.RequestLogs.CountAsync(x => x.ResponseTimeMs < 10);
            var under50 = await _dbContext.RequestLogs.CountAsync(x => x.ResponseTimeMs <= 50);
            var under100 = await _dbContext.RequestLogs.CountAsync(x => x.ResponseTimeMs <= 100);
            var over10 = await _dbContext.RequestLogs.CountAsync(x => x.ResponseTimeMs >= 100);

            var distributionDto = new DistributionDto
            {
                under10 = under10,
                under50 = under50,
                under100 = under100,
                over100 = over10
            };

    
            return Ok(distributionDto);
        }




    }
}
