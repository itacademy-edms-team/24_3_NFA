using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Svodka.Domain.Entities;
using Svodka.Domain.Interfaces;
using Svodka.Domain.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Threading;

namespace Svodka.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SourcesController : ControllerBase
    {
        private readonly INewsSourceRepository _newsSourceRepository;
        private readonly ILogger<SourcesController> _logger;
        private readonly INewsAggregationJob _newsAggregationJob;

        public SourcesController(
            INewsSourceRepository newsSourceRepository,
            ILogger<SourcesController> logger,
            INewsAggregationJob newsAggregationJob)
        {
            _newsSourceRepository = newsSourceRepository;
            _logger = logger;
            _newsAggregationJob = newsAggregationJob;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSource([FromBody] CreateSourceRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Получен некорректный запрос для создания источника: {@Request}", request);
                return BadRequest(ModelState);
            }

            if (request.Type != "rss")
            {
                _logger.LogWarning("Попытка создать источник с неподдерживаемым типом: {Type}", request.Type);
                return BadRequest(new ProblemDetails
                {
                    Title = "Unsupported source type",
                    Detail = "Currently, only 'rss' type is supported."
                });
            }

            var rssConfig = request.Configuration;

            _logger.LogInformation("Получена конфигурация: {@Configuration}", rssConfig);

            // Нормализуем URL перед проверкой
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

            // Проверяем, начинается ли URL с http:// или https://, и добавляем при необходимости
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

            // Обновляем URL в конфиге
            rssConfig.Url = normalizedUrl;

            // Сериализуем конфиг в строку для хранения в сущности
            var configurationJson = JsonSerializer.Serialize(rssConfig);
            _logger.LogInformation("Обновленная конфигурация: {UpdatedConfiguration}", configurationJson);

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

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateSource(int id, [FromBody] CreateSourceRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingSource = await _newsSourceRepository.GetByIdAsync(id);
            if (existingSource == null) return NotFound();

            _logger.LogInformation("Получен запрос на обновление источника {Id} с конфигурацией {@Configuration}", id, request.Configuration);

            var rssConfig = request.Configuration;

            // Проверяем, что конфигурация не пустая
            if (rssConfig == null)
            {
                _logger.LogWarning("Конфигурация источника пуста при обновлении источника {Id}", id);
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid configuration",
                    Detail = "Configuration is null."
                });
            }

            // Нормализуем URL при апдейте
            var normalizedUrl = rssConfig.Url?.Trim();
            _logger.LogInformation("Нормализованный URL до проверки: {NormalizedUrl} для источника {Id}", normalizedUrl, id);

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
                // Пробуем добавить https:// как префикс
                _logger.LogInformation("Добавляем префикс https:// к URL: {OriginalUrl} для источника {Id}", normalizedUrl, id);
                normalizedUrl = "https://" + normalizedUrl;
            }

            if (!Uri.IsWellFormedUriString(normalizedUrl, UriKind.Absolute))
            {
                _logger.LogWarning("Предоставленный URL не является корректным: {Url} для источника {Id}", normalizedUrl, id);
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid URL",
                    Detail = "The provided URL is not valid."
                });
            }

            rssConfig.Url = normalizedUrl;
            var configurationJson = JsonSerializer.Serialize(rssConfig);
            _logger.LogInformation("Обновленная конфигурация: {UpdatedConfiguration} для источника {Id}", configurationJson, id);

            // Обновляем поля
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
                _logger.LogError(ex, "Ошибка при удалении источника {SourceName} (ID: {SourceId})", source.Name, id);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Database Error",
                    Detail = "An error occurred while deleting the source."
                });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetSourceById(int id)
        {
            var source = await _newsSourceRepository.GetByIdAsync(id);
            if (source == null) return NotFound();
            return Ok(source);
        }

        [HttpGet]
        public async Task<IActionResult> GetSources()
        {
            var sources = await _newsSourceRepository.GetAllSourcesAsync();
            return Ok(sources);
        }

        [HttpGet("filter-options")]
        public async Task<IActionResult> GetFilterOptions()
        {
            var sources = await _newsSourceRepository.GetAllSourcesAsync();

            var categories = new List<string>();
            foreach (var source in sources)
            {
                var config = JsonSerializer.Deserialize<RssSourceConfiguration>(source.Configuration);
                if (!string.IsNullOrEmpty(config?.Category) && !categories.Contains(config.Category))
                {
                    categories.Add(config.Category);
                }
            }

            var filterOptions = new
            {
                sources = sources.Select(s => new { s.Id, s.Name }).ToList(),
                categories
            };

            return Ok(filterOptions);
        }
    }

    public class CreateSourceRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [RegularExpression("rss", ErrorMessage = "Only 'rss' type is supported for now.")]
        public string Type { get; set; } = string.Empty;

        [Required]
        public RssSourceConfiguration Configuration { get; set; } = new();

        public bool IsActive { get; set; } = true;
    }
}
