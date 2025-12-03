using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Svodka.Domain.Interfaces;
using Svodka.Domain.Models;

namespace Svodka.Infrastructure.Services
{
    /// <summary>
    /// Опции для службы агрегации новостей
    /// </summary>
    public class NewsAggregationOptions
    {
        /// <summary>
        /// Имя секции конфигурации
        /// </summary>
        public const string SectionName = "NewsAggregation";

        /// <summary>
        /// Интервал опроса источников в минутах
        /// </summary>
        public int PollingIntervalMinutes { get; set; } = 5;

        /// <summary>
        /// Максимальное количество новостей для получения с каждого источника
        /// </summary>
        public int NewsLimitPerSource { get; set; } = 10;
    }

    /// <summary>
    /// Фоновая служба для агрегации новостей из источников
    /// </summary>
    public class NewsAggregationBackgroundService : BackgroundService
    {
        private readonly ILogger<NewsAggregationBackgroundService> _logger;
        private readonly NewsAggregationOptions _options;
        private readonly INewsAggregationJob _aggregationJob;

        /// <summary>
        /// Конструктор фоновой службы агрегации
        /// </summary>
        /// <param name="logger">Логгер</param>
        /// <param name="aggregationJob">Служба агрегации новостей</param>
        /// <param name="options">Опции агрегации</param>
        public NewsAggregationBackgroundService(
            ILogger<NewsAggregationBackgroundService> logger,
            INewsAggregationJob aggregationJob,
            IOptions<NewsAggregationOptions> options)
        {
            _logger = logger;
            _aggregationJob = aggregationJob;
            _options = options.Value;
        }

        /// <summary>
        /// Асинхронное выполнение фоновой службы
        /// </summary>
        /// <param name="stoppingToken">Токен отмены</param>
        /// <returns>Задача выполнения службы</returns>
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