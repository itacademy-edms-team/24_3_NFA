using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Svodka.Domain.Entities;
using Svodka.Domain.Interfaces;

namespace Svodka.Web.Controllers
{
    /// <summary>
    /// Контроллер для работы с новостями
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly INewsItemRepository _newsItemRepository;

        /// <summary>
        /// Конструктор контроллера NewsController
        /// </summary>
        /// <param name="newsItemRepository">Репозиторий новостей</param>
        public NewsController(INewsItemRepository newsItemRepository)
        {
            _newsItemRepository = newsItemRepository;
        }

        /// <summary>
        /// Получает последние новости с возможностью фильтрации и пагинации
        /// </summary>
        /// <param name="offset">Смещение для пагинации</param>
        /// <param name="limit">Количество новостей для получения</param>
        /// <param name="q">Поисковый запрос</param>
        /// <param name="period">Период публикации (day, week, month)</param>
        /// <param name="sources">Фильтр по идентификаторам источников</param>
        /// <param name="categories">Фильтр по категориям</param>
        /// <param name="sourceType">Тип источника</param>
        /// <returns>Список новостей</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NewsItem>>> GetLatestNews(
            int offset = 0,
            int limit = 10,
            string? q = null,
            string? period = null,
            [FromQuery] int[]? sources = null,
            [FromQuery] string[]? categories = null,
            string? sourceType = null)
        {
            DateTime? fromDateUtc = null;

            if (!string.IsNullOrWhiteSpace(period))
            {
                var now = DateTime.UtcNow;
                fromDateUtc = period.ToLower() switch
                {
                    "day" => now.AddDays(-1),
                    "week" => now.AddDays(-7),
                    "month" => now.AddMonths(-1),
                    _ => null
                };
            }

            // Преобразуем массивы в списки для использования в репозитории
            var sourcesList = sources?.ToList();
            var categoriesList = categories?.ToList();

            var news = await _newsItemRepository.GetLatestNewsAsync(limit, q, fromDateUtc, sourcesList, categoriesList, offset, sourceType);
            return Ok(news);
        }
    }
}
