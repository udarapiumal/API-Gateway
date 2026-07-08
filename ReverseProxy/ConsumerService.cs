using ReverseProxy.Database;
using ReverseProxy.Models;
using StackExchange.Redis;

namespace ReverseProxy
{
    public class ConsumerService : BackgroundService
    {
        private readonly StackExchange.Redis.IDatabase _db;
        private readonly IServiceScopeFactory _scopefactory;
        public ConsumerService(IConnectionMultiplexer muxer, IServiceScopeFactory scopeFactory)
        {
            _db = muxer.GetDatabase();
            _scopefactory = scopeFactory;
        }


        Dictionary<string, string> ParseResult(StreamEntry entry) => entry.Values.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
          
            const string streamName = "gateway:logs";
            const string groupName = "loggers";
            if (!(await _db.KeyExistsAsync(streamName)) ||
    (await _db.StreamGroupInfoAsync(streamName)).All(x => x.Name != groupName))
            {
                await _db.StreamCreateConsumerGroupAsync(streamName, groupName, "$", true);
            }

            string id = string.Empty;

            while (!stoppingToken.IsCancellationRequested)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    await _db.StreamAcknowledgeAsync(streamName, groupName,id);
                    id = string.Empty;
                }

                var result = await _db.StreamReadGroupAsync(streamName, groupName, "logger-1", ">", 1);
                if (result.Any())
                {
                    id = result.First().Id;
                   
                    var dict = ParseResult(result.First());

                    var scope = _scopefactory.CreateScope();

                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var log = new RequestLog
                    {
                        ApiKey = dict["apikey"],
                        Path = dict["path"],
                        Method = dict["method"],
                        StatusCode = int.Parse(dict["statuscode"]),
                        ResponseTimeMs = long.Parse(dict["ms"]),
                        Timestamp = DateTime.UtcNow

                    };
                    await dbContext.RequestLogs.AddAsync(log);
                    await dbContext.SaveChangesAsync();

                }
               
                await Task.Delay(1000, stoppingToken);

            }
        }
    }
}
