using System;
using System.Collections.Generic;

namespace Svodka.Domain.Entities
{
    /// <summary>
    /// Представляет пользователя системы
    /// </summary>
    public class User
    {
        /// <summary>
        /// Уникальный идентификатор пользователя
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Email пользователя (используется как логин)
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Хэш пароля
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Дата и время создания пользователя
        /// </summary>
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Коллекция источников новостей, принадлежащих пользователю
        /// </summary>
        public virtual ICollection<NewsSource> NewsSources { get; set; } = new List<NewsSource>();
    }
}
