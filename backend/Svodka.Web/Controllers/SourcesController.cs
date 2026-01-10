using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Svodka.Domain.Entities;
using Svodka.Domain.Interfaces;
using Svodka.Domain.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace Svodka.Web.Controllers
{
    /// <summary>
    /// Контроллер для управления источниками новостей
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SourcesController : ControllerBase
    {
        private readonly INewsSourceRepository _newsSourceRepository;
        private readonly ILogger<SourcesController> _logger;
        private readonly INewsAggregationJob _newsAggregationJob;

        /// <summary>
        /// Конструктор контроллера SourcesController
        /// </summary>
        /// <param name="newsSourceRepository">Репозиторий источников новостей</param>
        /// <param name="logger">Логгер</param>
        /// <param name="newsAggregationJob">Служба агрегации новостей</param>
        public SourcesController(
            INewsSourceRepository newsSourceRepository,
            ILogger<SourcesController> logger,
            INewsAggregationJob newsAggregationJob)
        {
            _newsSourceRepository = newsSourceRepository;
            _logger = logger;
            _newsAggregationJob = newsAggregationJob;
        }

        /// <summary>
        /// Создает новый источник новостей
        /// </summary>
        /// <param name="request">Запрос на создание источника</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат создания источника</returns>
        [HttpPost]
        public async Task<IActionResult> CreateSource([FromBody] CreateSourceRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Получен некорректный запрос для создания источника: {@Request}", request);
                return BadRequest(ModelState);
            }

            var sourceType = request.Type.ToLower();
            if (sourceType != "rss" && sourceType != "github" && sourceType != "reddit")
            {
                _logger.LogWarning("Попытка создать источник с неподдерживаемым типом: {Type}", request.Type);
                return BadRequest(new ProblemDetails
                {
                    Title = "Unsupported source type",
                    Detail = "Currently, only 'rss', 'github', and 'reddit' types are supported."
                });
            }

            string configurationJson;
            
            if (sourceType == "rss")
            {
                var rssConfig = JsonSerializer.Deserialize<RssSourceConfiguration>(request.Configuration.GetRawText());
                
                if (rssConfig == null)
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Configuration",
                        Detail = "Invalid RSS configuration."
                    });
                }

            _logger.LogInformation("Получена конфигурация: {@Configuration}", rssConfig);

            var normalizedUrl = rssConfig.Url?.Trim() ?? string.Empty;
            _logger.LogInformation("Нормализованный URL до проверки: {NormalizedUrl}", normalizedUrl);

            if (string.IsNullOrEmpty(normalizedUrl))
            {
                _logger.LogWarning("Предоставлен пустой или null URL. Оригинальный URL: {OriginalUrl}", rssConfig.Url);
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid URL",
                    Detail = "The provided URL is empty."
                });
            }

            if (!normalizedUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !normalizedUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Добавляем префикс https:// к URL: {OriginalUrl}", normalizedUrl);
                normalizedUrl = "https://" + normalizedUrl;
            }

            if (!Uri.IsWellFormedUriString(normalizedUrl, UriKind.Absolute))
            {
                _logger.LogWarning("Предоставленный URL не является корректным: {Url}", normalizedUrl);
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid URL",
                    Detail = "The provided URL is not valid."
                });
            }

                rssConfig.Url = normalizedUrl;

                configurationJson = JsonSerializer.Serialize(rssConfig);
                _logger.LogInformation("Обновленная конфигурация: {UpdatedConfiguration}", configurationJson);
            }
            else if (sourceType == "github")
            {
                var rawConfig = request.Configuration.GetRawText();
                _logger.LogInformation("Получена GitHub конфигурация (raw): {RawConfig}", rawConfig);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var githubConfig = JsonSerializer.Deserialize<GitHubSourceConfiguration>(rawConfig, options);
                _logger.LogInformation("Десериализованная GitHub конфигурация: RepositoryOwner={RepositoryOwner}, RepositoryName={RepositoryName}", 
                    githubConfig?.RepositoryOwner, githubConfig?.RepositoryName);
                
                if (githubConfig == null || string.IsNullOrWhiteSpace(githubConfig.RepositoryOwner) || 
                    string.IsNullOrWhiteSpace(githubConfig.RepositoryName))
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Configuration",
                        Detail = "GitHub configuration must include RepositoryOwner and RepositoryName."
                    });
                }

                configurationJson = JsonSerializer.Serialize(githubConfig);
                _logger.LogInformation("GitHub конфигурация: {UpdatedConfiguration}", configurationJson);
            }
            else if (sourceType == "reddit")
            {
                var rawConfig = request.Configuration.GetRawText();
                _logger.LogInformation("Получена Reddit конфигурация (raw): {RawConfig}", rawConfig);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var redditConfig = JsonSerializer.Deserialize<RedditSourceConfiguration>(rawConfig, options);
                _logger.LogInformation("Десериализованная Reddit конфигурация: Subreddit={Subreddit}", redditConfig?.Subreddit);
                
                if (redditConfig == null || string.IsNullOrWhiteSpace(redditConfig.Subreddit))
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Configuration",
                        Detail = "Reddit configuration must include Subreddit."
                    });
                }

                var validSortTypes = new[] { "hot", "new", "top" };
                if (!validSortTypes.Contains(redditConfig.SortType?.ToLower() ?? ""))
                {
                    redditConfig.SortType = "hot";
                }

                configurationJson = JsonSerializer.Serialize(redditConfig);
                _logger.LogInformation("Reddit конфигурация: {UpdatedConfiguration}", configurationJson);
            }
            else
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Source Type",
                    Detail = $"Unsupported source type: {sourceType}"
                });
            }

            var newsSource = new NewsSource
            {
                Name = request.Name,
                Type = request.Type,
                Configuration = configurationJson,
                IsActive = request.IsActive
            };

            try
            {
                await _newsSourceRepository.AddNewsSourceAsync(newsSource);
                await _newsSourceRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении источника в базу данных для: {Name}", request.Name);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Database Error",
                    Detail = "An error occurred while saving the source."
                });
            }

            try
            {
                await _newsAggregationJob.ExecuteAsync(newsSource.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при принудительной агрегации источника {SourceId}", newsSource.Id);
            }

            return CreatedAtAction(nameof(GetSourceById),
                new { id = newsSource.Id },
                new { id = newsSource.Id, name = newsSource.Name });
        }

        /// <summary>
        /// Обновляет существующий источник новостей
        /// </summary>
        /// <param name="id">Идентификатор источника</param>
        /// <param name="request">Запрос на обновление источника</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат обновления источника</returns>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateSource(int id, [FromBody] CreateSourceRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingSource = await _newsSourceRepository.GetByIdAsync(id);
            if (existingSource == null) return NotFound();

            var sourceType = request.Type.ToLower();
            if (sourceType != "rss" && sourceType != "github" && sourceType != "reddit")
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Unsupported source type",
                    Detail = "Currently, only 'rss', 'github', and 'reddit' types are supported."
                });
            }

            _logger.LogInformation("Получен запрос на обновление источника {Id} типа {Type} с конфигурацией {@Configuration}", id, sourceType, request.Configuration);

            string configurationJson;

            if (sourceType == "rss")
            {
                var rssConfig = JsonSerializer.Deserialize<RssSourceConfiguration>(request.Configuration.GetRawText());
                
                if (rssConfig == null)
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Configuration",
                        Detail = "Invalid RSS configuration."
                    });
                }

                var normalizedUrl = rssConfig.Url?.Trim() ?? string.Empty;
                
                if (string.IsNullOrEmpty(normalizedUrl))
                {
                    _logger.LogWarning("Предоставлен пустой или null URL при обновлении источника {Id}. Оригинальный URL: {OriginalUrl}", id, rssConfig.Url);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid URL",
                        Detail = "The provided URL is empty."
                    });
                }

                if (!normalizedUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !normalizedUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    normalizedUrl = "https://" + normalizedUrl;
                }

                if (!Uri.IsWellFormedUriString(normalizedUrl, UriKind.Absolute))
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid URL",
                        Detail = "The provided URL is not valid."
                    });
                }

                rssConfig.Url = normalizedUrl;
                configurationJson = JsonSerializer.Serialize(rssConfig);
            }
            else if (sourceType == "github")
            {
                var githubConfig = JsonSerializer.Deserialize<GitHubSourceConfiguration>(request.Configuration.GetRawText());
                
                if (githubConfig == null || string.IsNullOrWhiteSpace(githubConfig.RepositoryOwner) || 
                    string.IsNullOrWhiteSpace(githubConfig.RepositoryName))
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Configuration",
                        Detail = "GitHub configuration must include RepositoryOwner and RepositoryName."
                    });
                }

                configurationJson = JsonSerializer.Serialize(githubConfig);
            }
            else if (sourceType == "reddit")
            {
                var redditConfig = JsonSerializer.Deserialize<RedditSourceConfiguration>(request.Configuration.GetRawText());
                
                if (redditConfig == null || string.IsNullOrWhiteSpace(redditConfig.Subreddit))
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Configuration",
                        Detail = "Reddit configuration must include Subreddit."
                    });
                }

                var validSortTypes = new[] { "hot", "new", "top" };
                if (!validSortTypes.Contains(redditConfig.SortType?.ToLower() ?? ""))
                {
                    redditConfig.SortType = "hot";
                }

                configurationJson = JsonSerializer.Serialize(redditConfig);
            }
            else
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Source Type",
                    Detail = $"Unsupported source type: {sourceType}"
                });
            }

            _logger.LogInformation("Обновленная конфигурация: {UpdatedConfiguration} для источника {Id}", configurationJson, id);

            existingSource.Name = request.Name;
            existingSource.Type = request.Type;
            existingSource.Configuration = configurationJson;
            existingSource.IsActive = request.IsActive;

            try
            {
                await _newsSourceRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении источника в базу данных для: {Name} (ID: {Id})", request.Name, id);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Database Error",
                    Detail = "An error occurred while updating the source."
                });
            }

            try
            {
                await _newsAggregationJob.ExecuteAsync(existingSource.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при принудительной агрегации после обновления источника {SourceId}", existingSource.Id);
            }

            return Ok(existingSource);
        }

        /// <summary>
        /// Удаляет источник новостей по идентификатору с каскадным удалением новостей
        /// </summary>
        /// <param name="id">Идентификатор источника</param>
        /// <returns>Результат удаления источника</returns>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSource(int id)
        {
            var source = await _newsSourceRepository.GetByIdAsync(id);
            if (source == null) return NotFound();

            try
            {
                var sourceName = source.Name;

                var deleted = await _newsSourceRepository.DeleteNewsSourceAsync(id);
                if (!deleted)
                {
                    return NotFound();
                }

                await _newsSourceRepository.SaveChangesAsync();

                _logger.LogInformation("Источник {SourceName} (ID: {SourceId}) был удален", sourceName, id);

                return Ok(new { message = "Источник успешно удален" });
            }
            catch (Exception ex)
            {
                var sourceNameForLog = source?.Name ?? "Unknown";
                _logger.LogError(ex, "Ошибка при удалении источника {SourceName} (ID: {SourceId}). Детали: {ExceptionMessage}", sourceNameForLog, id, ex.Message);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Database Error",
                    Detail = $"An error occurred while deleting the source. Error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Получает источник новостей по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор источника</param>
        /// <returns>Информация об источнике</returns>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetSourceById(int id)
        {
            var source = await _newsSourceRepository.GetByIdAsync(id);
            if (source == null) return NotFound();
            return Ok(source);
        }

        /// <summary>
        /// Получает все источники новостей
        /// </summary>
        /// <returns>Список всех источников</returns>
        [HttpGet]
        public async Task<IActionResult> GetSources()
        {
            var sources = await _newsSourceRepository.GetAllSourcesAsync();
            return Ok(sources);
        }

        /// <summary>
        /// Получает опции фильтрации (источники и категории) для построения фильтра
        /// </summary>
        /// <returns>Опции фильтрации</returns>
        [HttpGet("filter-options")]
        public async Task<IActionResult> GetFilterOptions()
        {
            var sources = await _newsSourceRepository.GetAllSourcesAsync();

            var categories = new List<string>();
            foreach (var source in sources)
            {
                try
                {
                    if (source.Type.ToLower() == "rss")
                    {
                        var config = JsonSerializer.Deserialize<RssSourceConfiguration>(source.Configuration);
                        if (!string.IsNullOrEmpty(config?.Category) && !categories.Contains(config.Category))
                        {
                            categories.Add(config.Category);
                        }
                    }
                    else if (source.Type.ToLower() == "reddit")
                    {
                        var config = JsonSerializer.Deserialize<RedditSourceConfiguration>(source.Configuration);
                        if (!string.IsNullOrEmpty(config?.Category) && !categories.Contains(config.Category))
                        {
                            categories.Add(config.Category);
                        }
                    }
                    else if (source.Type.ToLower() == "github")
                    {
                        if (!categories.Contains("GitHub"))
                        {
                            categories.Add("GitHub");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Ошибка при десериализации конфигурации источника {SourceId} для фильтров", source.Id);
                }
            }

            var filterOptions = new
            {
                sources = sources.Select(s => new { s.Id, s.Name, s.Type }).ToList(),
                categories
            };

            return Ok(filterOptions);
        }
    }

    /// <summary>
    /// Класс запроса для создания или обновления источника новостей
    /// </summary>
    public class CreateSourceRequest
    {
        /// <summary>
        /// Название источника
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Тип источника (rss, github, reddit)
        /// </summary>
        [Required]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Конфигурация источника (может быть RssSourceConfiguration, GitHubSourceConfiguration или RedditSourceConfiguration)
        /// </summary>
        [Required]
        public JsonElement Configuration { get; set; }

        /// <summary>
        /// Флаг активности источника
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
