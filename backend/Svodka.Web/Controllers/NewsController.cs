using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Svodka.Domain.Entities;
using Svodka.Domain.Interfaces;

namespace Svodka.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly INewsItemRepository _newsItemRepository;

        public NewsController(INewsItemRepository newsItemRepository) 
        {
            _newsItemRepository = newsItemRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NewsItem>>> GetLatestNews(
            int limit = 10,
            string? q = null,
            string? period = null)
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

            var news = await _newsItemRepository.GetLatestNewsAsync(limit, q, fromDateUtc);
            return Ok(news);
        }
    }
}
