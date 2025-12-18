using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Svodka.Infrastructure.Data;
using System.Net.Http.Json;
using Svodka.Domain.Entities;
using Svodka.Infrastructure.Services;
using Svodka.Domain.Interfaces;

namespace Svodka.IntegrationTests
{
    public class IntegrationTestBase : IDisposable
    {
        protected readonly WebApplicationFactory<Program> _factory;
        protected readonly HttpClient _httpClient;

        public IntegrationTestBase()
        {
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Удаляем все регистрации, связанные с базой данных
                        var descriptorsToRemove = services
                            .Where(d => d.ServiceType == typeof(NewsAggregatorDbContext) ||
                                       d.ServiceType == typeof(DbContextOptions<NewsAggregatorDbContext>) ||
                                       d.ServiceType == typeof(DbContextOptions))
                            .ToList();

                        foreach (var descriptor in descriptorsToRemove)
                        {
                            services.Remove(descriptor);
                        }

                        // Удаляем все зарегистрированные IHostedService, чтобы избежать конфликта провайдеров
                        var hostedServiceDescriptors = services
                            .Where(d => d.ServiceType == typeof(IHostedService))
                            .ToList();

                        foreach (var descriptor in hostedServiceDescriptors)
                        {
                            services.Remove(descriptor);
                        }

                        // Регистрируем DbContext с InMemory провайдером
                        services.AddDbContext<NewsAggregatorDbContext>(options =>
                        {
                            options.UseInMemoryDatabase("TestDb");
                            // Отключаем резервирование значений для Identity, если используется
                            options.UseInternalServiceProvider(new ServiceCollection()
                                .AddEntityFrameworkInMemoryDatabase()
                                .BuildServiceProvider());
                        });
                    });
                });

            _httpClient = _factory.CreateClient();
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            _factory?.Dispose();
        }
    }
}