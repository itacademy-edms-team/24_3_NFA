using Microsoft.Extensions.Logging;
using Svodka.Application.DTOs;
using Svodka.Application.Interfaces;
using Svodka.Domain.Entities;
using Svodka.Domain.Enums;
using Svodka.Domain.Interfaces;
using Svodka.Domain.Models;
using System.Text.Json;

namespace Svodka.Application.Services
{
    public class SourceService : ISourceService
    {
        private readonly INewsSourceRepository _newsSourceRepository;
        private readonly INewsAggregationJob _newsAggregationJob;
        private readonly INewsProviderFactory _newsProviderFactory;
        private readonly ILogger<SourceService> _logger;

        public SourceService(
            INewsSourceRepository newsSourceRepository,
            INewsAggregationJob newsAggregationJob,
            INewsProviderFactory newsProviderFactory,
            ILogger<SourceService> logger)
        {
            _newsSourceRepository = newsSourceRepository;
            _newsAggregationJob = newsAggregationJob;
            _newsProviderFactory = newsProviderFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<NewsSource>> GetAllSourcesByUserIdAsync(int userId)
        {
            return await _newsSourceRepository.GetAllSourcesByUserIdAsync(userId);
        }

        public async Task<NewsSource?> GetSourceByIdAndUserIdAsync(int id, int userId)
        {
            return await _newsSourceRepository.GetByIdAndUserIdAsync(id, userId);
        }

        public async Task<NewsSource> CreateSourceAsync(int userId, SourceDto dto, CancellationToken ct)
        {
            var configurationJson = ValidateAndNormalizeConfiguration(dto);

            var newsSource = new NewsSource
            {
                Name = dto.Name,
                Type = dto.Type,
                Configuration = configurationJson,
                IsActive = dto.IsActive,
                UserId = userId
            };

            await _newsSourceRepository.AddNewsSourceAsync(newsSource);
            await _newsSourceRepository.SaveChangesAsync();

            try
            {
                await _newsAggregationJob.ExecuteAsync(newsSource.Id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при принудительной агрегации источника {SourceId}", newsSource.Id);
            }

            return newsSource;
        }

        public async Task<NewsSource?> UpdateSourceAsync(int id, int userId, SourceDto dto, CancellationToken ct)
        {
            var existingSource = await _newsSourceRepository.GetByIdAndUserIdAsync(id, userId);
            if (existingSource == null) return null;

            var configurationJson = ValidateAndNormalizeConfiguration(dto);

            existingSource.Name = dto.Name;
            existingSource.Type = dto.Type;
            existingSource.Configuration = configurationJson;
            existingSource.IsActive = dto.IsActive;

            await _newsSourceRepository.SaveChangesAsync();

            try
            {
                await _newsAggregationJob.ExecuteAsync(existingSource.Id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при принудительной агрегации после обновления источника {SourceId}", existingSource.Id);
            }

            return existingSource;
        }

        public async Task<bool> DeleteSourceAsync(int id, int userId)
        {
            var deleted = await _newsSourceRepository.DeleteNewsSourceAsync(id, userId);
            if (deleted)
            {
                await _newsSourceRepository.SaveChangesAsync();
            }
            return deleted;
        }

        public async Task<object> GetFilterOptionsAsync(int userId)
        {
            var sources = await _newsSourceRepository.GetAllSourcesByUserIdAsync(userId);
            var categories = new List<string>();

            foreach (var source in sources)
            {
                try
                {
                    var provider = _newsProviderFactory.GetProvider(source.Type);
                    var category = provider.GetCategory(source.Configuration);
                    if (!string.IsNullOrEmpty(category) && !categories.Contains(category))
                    {
                        categories.Add(category);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Ошибка при получении категории источника {SourceId} для фильтров", source.Id);
                }
            }

            return new
            {
                sources = sources.Select(s => new { s.Id, s.Name, Type = s.Type.ToString().ToLower() }).ToList(),
                categories
            };
        }

        private string ValidateAndNormalizeConfiguration(SourceDto dto)
        {
            var provider = _newsProviderFactory.GetProvider(dto.Type);
            return provider.ValidateAndNormalize(dto.Configuration);
        }
    }
}
