using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Caching.Memory;

namespace AspNetHandons.Services
{
    public class BackgroundworkerService : BackgroundService
    {
        private readonly ILogger<BackgroundworkerService> _logger;
        private readonly IMemoryCache _cache;

        private const string CACHE_KEY = "all";

        public BackgroundworkerService(
            ILogger<BackgroundworkerService> logger,
            IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
                    _cache.Remove(CACHE_KEY);
                    _logger.LogInformation("Cache '{key}' cleared at {time}",
                        CACHE_KEY, DateTime.Now);
                }
                catch (TaskCanceledException)
                {
                }
            }
        }
    }
}