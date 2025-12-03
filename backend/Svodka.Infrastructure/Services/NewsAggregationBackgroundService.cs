using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Svodka.Domain.Interfaces;
using Svodka.Domain.Models;

namespace Svodka.Infrastructure.Services
{
    public class NewsAggregationOptions
    {
        public const string SectionName = "NewsAggregation";
        public int PollingIntervalMinutes { get; set; } = 5; 
        public int NewsLimitPerSource { get; set; } = 10; 
    }

    public class NewsAggregationBackgroundService : BackgroundService
    {
        private readonly ILogger<NewsAggregationBackgroundService> _logger;
        private readonly NewsAggregationOptions _options;
        private readonly INewsAggregationJob _aggregationJob;

        public NewsAggregationBackgroundService(
            ILogger<NewsAggregationBackgroundService> logger,
            INewsAggregationJob aggregationJob,
            IOptions<NewsAggregationOptions> options)
        {
            _logger = logger;
            _aggregationJob = aggregationJob;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Служба агрегации новостей запущена. Интервал: {Interval} минут", _options.PollingIntervalMinutes);

            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_options.PollingIntervalMinutes));

            try
            {
                await _aggregationJob.ExecuteAsync(null, stoppingToken);

                while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await _aggregationJob.ExecuteAsync(null, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Служба агрегации новостей остановлена.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Непредвиденная ошибка в фоновой службе агрегации.");
            }
        }

    }
}