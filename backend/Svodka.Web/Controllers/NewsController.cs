using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Svodka.Domain.Entities;
using Svodka.Domain.Enums;
using Svodka.Domain.Interfaces;
using System.Security.Claims;

namespace Svodka.Web.Controllers
{
    /// <summary>
    /// Контроллер для работы с новостями
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NewsController : ControllerBase
    {
        private readonly INewsItemRepository _newsItemRepository;
        private readonly INewsSourceRepository _newsSourceRepository;

        /// <summary>
        /// Конструктор контроллера NewsController
        /// </summary>
        /// <param name="newsItemRepository">Репозиторий новостей</param>
        /// <param name="newsSourceRepository">Репозиторий источников</param>
        public NewsController(INewsItemRepository newsItemRepository, INewsSourceRepository newsSourceRepository)
        {
            _newsItemRepository = newsItemRepository;
            _newsSourceRepository = newsSourceRepository;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

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
        public async Task<ActionResult> GetLatestNews(
            int offset = 0,
            int limit = 10,
            string? q = null,
            string? period = null,
            [FromQuery] int[]? sources = null,
            [FromQuery] string[]? categories = null,
            string? sourceType = null)
        {
            var userId = GetUserId();
            var userSources = await _newsSourceRepository.GetAllSourcesByUserIdAsync(userId);
            var userSourceIds = userSources.Select(s => s.Id).ToList();

            // Если запрошены источники, фильтруем только те, что принадлежат пользователю
            var sourceIdsToQuery = (sources != null && sources.Length > 0)
                ? sources.Intersect(userSourceIds).ToList()
                : userSourceIds;

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

            var categoriesList = categories?.ToList();

            SourceType? st = null;
            if (!string.IsNullOrWhiteSpace(sourceType) && Enum.TryParse<SourceType>(sourceType, true, out var parsedType))
            {
                st = parsedType;
            }

            var news = await _newsItemRepository.GetLatestNewsAsync(limit, q, fromDateUtc, sourceIdsToQuery, categoriesList, offset, st);
            bool hasMore = news.Count() == limit;

            return Ok(new { items = news, hasMore = hasMore, offset = offset, limit = limit });
        }
    }
}
