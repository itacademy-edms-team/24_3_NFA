using Svodka.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svodka.Domain.Interfaces
{
    /// <summary>
    /// Фабрика для получения экземпляров INewsProvider по типу источника
    /// </summary>
    public interface INewsProviderFactory
    {
        /// <summary>
        /// Возвращает провайдер(поставщик) новостей для указанного источника.
        /// </summary>
        /// <param name="sourceType">Тип источника</param>
        /// <returns>Экземпляр INewsProvider.</returns>
        INewsProvider GetProvider(SourceType sourceType);
    }
}
