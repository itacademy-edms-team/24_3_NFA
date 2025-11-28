using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Svodka.Domain.Entities;
using Svodka.Domain.Interfaces;
using Svodka.Domain.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Svodka.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SourcesController : ControllerBase
    {
        private readonly INewsSourceRepository _newsSourceRepository;
        private readonly ILogger<SourcesController> _logger;

        public SourcesController(INewsSourceRepository newsSourceRepository, ILogger<SourcesController> logger)
        {
            _newsSourceRepository = newsSourceRepository;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSource([FromBody] CreateSourceRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Получен некорректный запрос для создания источника: {@Request}", request);
                return BadRequest(ModelState);
            }

            if (request.Type != "rss")
            {
                _logger.LogWarning("Попытка создать источник с неподдерживаемым типом: {Type}", request.Type);
                return BadRequest(new ProblemDetails { Title = "Unsupported source type", Detail = "Currently, only 'rss' type is supported." });
            }

            RssSourceConfiguration? rssConfig;
            try
            {
                rssConfig = JsonSerializer.Deserialize<RssSourceConfiguration>(request.Configuration);
                if (rssConfig == null)
                {
                    _logger.LogWarning("Конфигурация источника не может быть десериализована как RssSourceConfiguration: {Configuration}", request.Configuration);
                    return BadRequest(new ProblemDetails { Title = "Invalid configuration", Detail = "Configuration is not a valid RssSourceConfiguration JSON." });
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Ошибка десериализации конфигурации источника: {Configuration}", request.Configuration);
                return BadRequest(new ProblemDetails { Title = "Invalid configuration", Detail = ex.Message });
            }

            if (!Uri.IsWellFormedUriString(rssConfig.Url, UriKind.Absolute))
            {
                _logger.LogWarning("Предоставленный URL не является корректным: {Url}", rssConfig.Url);
                return BadRequest(new ProblemDetails { Title = "Invalid URL", Detail = "The provided URL is not valid." });
            }

            var newsSource = new NewsSource
            {
                Name = request.Name,
                Type = request.Type,
                Configuration = request.Configuration,
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
                return StatusCode(500, new ProblemDetails { Title = "Database Error", Detail = "An error occurred while saving the source." });
            }

            return CreatedAtAction(nameof(GetSourceById), new { id = newsSource.Id }, new { id = newsSource.Id, name = newsSource.Name });
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
    }

    public class CreateSourceRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [RegularExpression("rss", ErrorMessage = "Only 'rss' type is supported for now.")]
        public string Type { get; set; } = string.Empty;

        [Required]
        public string Configuration { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
