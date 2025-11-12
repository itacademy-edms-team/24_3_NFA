using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svodka.Domain.Interfaces
{
    /// <summary>
    /// Фабрика для получения экземпляров INewsProvider строкой
    /// </summary>
    public interface INewsProviderFactory
    {
        /// <summary>
        /// Возвращает провайдер(поставщик) новостей для указанного источника.
        /// </summary>
        /// <param name="providerType">Тип провайдера (например, "rss")</param>
        /// <returns>Экземпляр INewsProvider.</returns>
        INewsProvider GetProvider(string providerType);
    }
}
