using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Svodka.Domain.Entities;
using Svodka.Domain.Interfaces;
using Svodka.Domain.Models;
using Svodka.Infrastructure.Services;


namespace Svodka.Infrastructure.Providers
{
    /// <summary>
    /// Провайдер для получения новостей из RSS-ленты
    /// </summary>
    public class RssNewsProvider : INewsProvider
    {
        private readonly IRssService _rssService;
        private readonly ILogger<RssNewsProvider> _logger;

        /// <summary>
        /// Конструктор провайдера RSS-новостей
        /// </summary>
        /// <param name="rssService">Служба для работы с RSS-лентами</param>
        /// <param name="logger">Логгер</param>
        public RssNewsProvider(IRssService rssService, ILogger<RssNewsProvider> logger)
        {
            _rssService = rssService;
            _logger = logger;
        }

        /// <summary>
        /// Получает список новостей из RSS-ленты
        /// </summary>
        /// <param name="configuration">Конфигурация RSS-источника</param>
        /// <returns>Список новостей</returns>
        public async Task<IEnumerable<NewsItem>> GetNewsAsync(object configuration)
        {
            if (configuration is not RssSourceConfiguration rssConfig)
            {
                _logger.LogError("Неправильный тип конфигурации для RssNewsProvider: {Type}", configuration?.GetType().FullName);
                throw new ArgumentException("Ожидается RssSourceConfiguration.", nameof(configuration));
            }

            _logger.LogInformation("Загрузка новостей из RSS: {Url}", rssConfig.Url);

            try
            {
                var items = await _rssService.FetchRssFeedAsync(rssConfig.Url, rssConfig.Limit);

                var itemsWithCategory = items.Select(item =>
                {
                    item.Category = rssConfig.Category;
                    return item;
                });

                return itemsWithCategory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении новостей из RSS: {Url}", rssConfig.Url);
                throw;
            }
        }
    }
}