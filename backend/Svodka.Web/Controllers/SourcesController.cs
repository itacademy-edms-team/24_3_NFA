using Microsoft.AspNetCore.Mvc;
using Svodka.Application.DTOs;
using Svodka.Application.Interfaces;
using Svodka.Domain.Entities;
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
        private readonly ISourceService _sourceService;
        private readonly ILogger<SourcesController> _logger;

        /// <summary>
        /// Конструктор контроллера SourcesController
        /// </summary>
        /// <param name="sourceService">Сервис источников новостей</param>
        /// <param name="logger">Логгер</param>
        public SourcesController(
            ISourceService sourceService,
            ILogger<SourcesController> logger)
        {
            _sourceService = sourceService;
            _logger = logger;
        }

        /// <summary>
        /// Создает новый источник новостей
        /// </summary>
        /// <param name="request">Запрос на создание источника</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат создания источника</returns>
        [HttpPost]
        public async Task<IActionResult> CreateSource([FromBody] SourceDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Получен некорректный запрос для создания источника: {@Request}", request);
                return BadRequest(ModelState);
            }

            try
            {
                var newsSource = await _sourceService.CreateSourceAsync(request, cancellationToken);
                return CreatedAtAction(nameof(GetSourceById),
                    new { id = newsSource.Id },
                    new { id = newsSource.Id, name = newsSource.Name });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Ошибка валидации при создании источника");
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании источника: {Name}", request.Name);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while creating the source."
                });
            }
        }

        /// <summary>
        /// Обновляет существующий источник новостей
        /// </summary>
        /// <param name="id">Идентификатор источника</param>
        /// <param name="request">Запрос на обновление источника</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат обновления источника</returns>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateSource(int id, [FromBody] SourceDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedSource = await _sourceService.UpdateSourceAsync(id, request, cancellationToken);
                if (updatedSource == null) return NotFound();

                return Ok(updatedSource);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Ошибка валидации при обновлении источника {Id}", id);
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении источника {Id}", id);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while updating the source."
                });
            }
        }

        /// <summary>
        /// Удаляет источник новостей по идентификатору с каскадным удалением новостей
        /// </summary>
        /// <param name="id">Идентификатор источника</param>
        /// <returns>Результат удаления источника</returns>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSource(int id)
        {
            try
            {
                var deleted = await _sourceService.DeleteSourceAsync(id);
                if (!deleted) return NotFound();

                return Ok(new { message = "Источник успешно удален" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении источника {SourceId}", id);
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
            var source = await _sourceService.GetSourceByIdAsync(id);
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
            var sources = await _sourceService.GetAllSourcesAsync();
            return Ok(sources);
        }

        /// <summary>
        /// Получает опции фильтрации (источники и категории) для построения фильтра
        /// </summary>
        /// <returns>Опции фильтрации</returns>
        [HttpGet("filter-options")]
        public async Task<IActionResult> GetFilterOptions()
        {
            var filterOptions = await _sourceService.GetFilterOptionsAsync();
            return Ok(filterOptions);
        }
    }
}
