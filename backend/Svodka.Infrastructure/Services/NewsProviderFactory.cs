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
    public class NewsProviderFactory : INewsProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public NewsProviderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public INewsProvider GetProvider(string providerType)
        {
            switch (providerType)
            {
                case "rss":
                {
                    return _serviceProvider.GetRequiredService<RssNewsProvider>();
                }
                default:
                {
                    throw new ArgumentException($"Неизвестный тип провайдера: {providerType}", nameof(providerType));
                }
            }

        }
    }
}
