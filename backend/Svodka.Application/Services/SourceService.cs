using Microsoft.Extensions.Logging;
using Svodka.Application.DTOs;
using Svodka.Application.Interfaces;
using Svodka.Domain.Entities;
using Svodka.Domain.Enums;
using Svodka.Domain.Interfaces;
using Svodka.Domain.Models;
using System.Text.RegularExpressions;
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
            var normalizedTags = NormalizeTags(dto.Tags);
            var existingTags = await _newsSourceRepository.GetTagsByNormalizedNamesAsync(normalizedTags.Select(t => t.NormalizedName));
            var existingTagMap = existingTags.ToDictionary(t => t.NormalizedName, t => t);

            var newTags = normalizedTags
                .Where(t => !existingTagMap.ContainsKey(t.NormalizedName))
                .Select(t => new Tag
                {
                    Name = t.Name,
                    NormalizedName = t.NormalizedName
                })
                .ToList();

            if (newTags.Any())
            {
                await _newsSourceRepository.AddTagsAsync(newTags);
            }

            var allTags = existingTags.Concat(newTags).ToList();

            var newsSource = new NewsSource
            {
                Name = dto.Name,
                Type = dto.Type,
                Configuration = configurationJson,
                IsActive = dto.IsActive,
                UserId = userId,
                NewsSourceTags = allTags
                    .Select(tag => new NewsSourceTag
                    {
                        Tag = tag
                    })
                    .ToList()
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
            var tags = new HashSet<string>();

            foreach (var source in sources)
            {
                try
                {
                    var provider = _newsProviderFactory.GetProvider(source.Type);
                    var suggestedTags = provider.GetSuggestedTags(source.Configuration);
                    
                    foreach (var tag in suggestedTags)
                    {
                        tags.Add(tag);
                    }

                    if (source.NewsSourceTags != null)
                    {
                        foreach (var nst in source.NewsSourceTags)
                        {
                            if (nst.Tag != null)
                            {
                                tags.Add(nst.Tag.Name);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Ошибка при получении тегов источника {SourceId} для фильтров", source.Id);
                }
            }

            return new
            {
                sources = sources.Select(s => new { s.Id, s.Name, Type = s.Type.ToString().ToLower() }).ToList(),
                tags = tags.OrderBy(t => t).ToList()
            };
        }

        private string ValidateAndNormalizeConfiguration(SourceDto dto)
        {
            var provider = _newsProviderFactory.GetProvider(dto.Type);
            return provider.ValidateAndNormalize(dto.Configuration);
        }

        private static List<(string Name, string NormalizedName)> NormalizeTags(IEnumerable<string>? tags)
        {
            if (tags == null)
            {
                return new List<(string Name, string NormalizedName)>();
            }

            var seen = new HashSet<string>();
            var result = new List<(string Name, string NormalizedName)>();

            foreach (var rawTag in tags)
            {
                if (string.IsNullOrWhiteSpace(rawTag))
                {
                    continue;
                }

                var trimmed = Regex.Replace(rawTag.Trim(), @"\s+", " ");
                if (trimmed.Length == 0)
                {
                    continue;
                }

                var normalized = trimmed.ToLowerInvariant();
                if (!seen.Add(normalized))
                {
                    continue;
                }

                result.Add((trimmed, normalized));
            }

            return result;
        }
    }
}
