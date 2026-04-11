using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Svodka.Domain.Entities;
using Svodka.Domain.Enums;
using Svodka.Domain.Interfaces;
using Svodka.Domain.Models;
using Svodka.Infrastructure.Services;

namespace Svodka.Infrastructure.Providers
{
    /// <summary>
    /// Провайдер для получения новостей из GitHub репозиториев
    /// </summary>
    public class GitHubNewsProvider : INewsProvider
    {
        private readonly IGitHubService _gitHubService;
        private readonly ILogger<GitHubNewsProvider> _logger;

        public GitHubNewsProvider(IGitHubService gitHubService, ILogger<GitHubNewsProvider> logger)
        {
            _gitHubService = gitHubService;
            _logger = logger;
        }

        public SourceType Type => SourceType.GitHub;

        public async Task<IEnumerable<NewsItem>> GetNewsAsync(object configuration)
        {
            if (configuration is not GitHubSourceConfiguration gitHubConfig)
            {
                _logger.LogError("Неправильный тип конфигурации для GitHubNewsProvider: {Type}", configuration?.GetType().FullName);
                throw new ArgumentException("Ожидается GitHubSourceConfiguration.", nameof(configuration));
            }

            _logger.LogInformation("Загрузка новостей из GitHub: {Owner}/{Repo}", gitHubConfig.RepositoryOwner, gitHubConfig.RepositoryName);

            try
            {
                var items = await _gitHubService.FetchRepositoryEventsAsync(
                    gitHubConfig.RepositoryOwner,
                    gitHubConfig.RepositoryName,
                    gitHubConfig.Token,
                    gitHubConfig.Limit,
                    gitHubConfig.EventTypes);

                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении новостей из GitHub: {Owner}/{Repo}", 
                    gitHubConfig.RepositoryOwner, gitHubConfig.RepositoryName);
                throw;
            }
        }

        public string ValidateAndNormalize(JsonElement json)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var githubConfig = JsonSerializer.Deserialize<GitHubSourceConfiguration>(json.GetRawText(), options);

            if (githubConfig == null || string.IsNullOrWhiteSpace(githubConfig.RepositoryOwner) ||
                string.IsNullOrWhiteSpace(githubConfig.RepositoryName))
            {
                throw new ArgumentException("GitHub configuration must include RepositoryOwner and RepositoryName.");
            }

            return JsonSerializer.Serialize(githubConfig);
        }

        public object DeserializeConfiguration(string json, int defaultLimit)
        {
            var config = JsonSerializer.Deserialize<GitHubSourceConfiguration>(json)
                ?? throw new ArgumentException("Failed to deserialize GitHub configuration.");

            if (config.Limit == 0)
            {
                config.Limit = defaultLimit;
            }
            return config;
        }

        public string? GetCategory(string json)
        {
            var config = JsonSerializer.Deserialize<GitHubSourceConfiguration>(json);
            return config?.Category ?? "GitHub";
        }
    }
}
