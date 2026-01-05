using Microsoft.Extensions.DependencyInjection;
using Svodka.Domain.Interfaces;
using Svodka.Infrastructure.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svodka.Infrastructure.Services
{
    /// <summary>
    /// Фабрика для получения экземпляров INewsProvider по типу провайдера
    /// </summary>
    public class NewsProviderFactory : INewsProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Конструктор фабрики провайдеров новостей
        /// </summary>
        /// <param name="serviceProvider">Поставщик сервисов</param>
        public NewsProviderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Возвращает провайдер (поставщик) новостей для указанного типа
        /// </summary>
        /// <param name="providerType">Тип провайдера (например, "rss")</param>
        /// <returns>Экземпляр INewsProvider</returns>
        public INewsProvider GetProvider(string providerType)
        {
            switch (providerType.ToLower())
            {
                case "rss":
                {
                    return _serviceProvider.GetRequiredService<RssNewsProvider>();
                }
                case "github":
                {
                    return _serviceProvider.GetRequiredService<GitHubNewsProvider>();
                }
                // case "reddit":
                // {
                //     return _serviceProvider.GetRequiredService<RedditNewsProvider>();
                // }
                default:
                {
                    throw new ArgumentException($"Неизвестный тип провайдера: {providerType}", nameof(providerType));
                }
            }

        }
    }
}
