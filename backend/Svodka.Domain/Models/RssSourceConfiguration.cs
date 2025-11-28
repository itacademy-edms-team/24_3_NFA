using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svodka.Domain.Models
{
    /// <summary>
    /// Конфигурация RSS-источника
    /// </summary>
    public class RssSourceConfiguration
    {
        public string Url { get; set; } = string.Empty;
        public int Limit { get; set; } = 10;
        public string? Category {  get; set; }
    }
}
