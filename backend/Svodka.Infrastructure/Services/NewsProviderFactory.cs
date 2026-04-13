using Svodka.Domain.Enums;
using Svodka.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Svodka.Infrastructure.Services
{
    /// <summary>
    /// Фабрика для получения экземпляров INewsProvider на основе зарегистрированных провайдеров
    /// </summary>
    public class NewsProviderFactory : INewsProviderFactory
    {
        private readonly Dictionary<SourceType, INewsProvider> _providers;

        /// <summary>
        /// Конструктор фабрики провайдеров новостей
        /// </summary>
        /// <param name="providers">Список всех зарегистрированных провайдеров</param>
        public NewsProviderFactory(IEnumerable<INewsProvider> providers)
        {
            _providers = providers.ToDictionary(p => p.Type);
        }

        /// <summary>
        /// Возвращает провайдер (поставщик) новостей для указанного типа
        /// </summary>
        /// <param name="sourceType">Тип источника</param>
        /// <returns>Экземпляр INewsProvider</returns>
        public INewsProvider GetProvider(SourceType sourceType)
        {
            if (_providers.TryGetValue(sourceType, out var provider))
            {
                return provider;
            }

            throw new ArgumentException($"Неизвестный тип провайдера: {sourceType}", nameof(sourceType));
        }
    }
}
