import React, { useState } from 'react';
import { type NewsItem } from '../types/NewsItem';
import { FaRss, FaGithub, FaReddit } from 'react-icons/fa';

interface NewsItemCardProps {
  newsItem: NewsItem;
}

const NewsItemCard: React.FC<NewsItemCardProps> = ({ newsItem }) => {
  const [showFullText, setShowFullText] = useState(false);
  const formattedDate = new Date(newsItem.publishedAtUtc).toLocaleString('ru-RU', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });

  const description = newsItem.description || '';
  const shouldTruncate = description.length > 300;
  const displayText = showFullText || !shouldTruncate 
    ? description 
    : description.substring(0, 300) + '...';

  const getSourceIcon = () => {
    const sourceType = newsItem.sourceType?.toLowerCase();
    
    const iconWrapperClass = "flex items-center justify-center w-8 h-8 bg-white rounded-full"; 

    if (sourceType === 'rss') {
        return (
            <div className={iconWrapperClass}>
                <FaRss className="w-5 h-5 text-orange-500" />
            </div>
        );
    }
    if (sourceType === 'github') {
        return (
            <div className={iconWrapperClass}>
                <FaGithub className="w-5 h-5 text-gray-800" />
            </div>
        );
    }
    if (sourceType === 'reddit') {
        return (
            <div className={iconWrapperClass}>
                <FaReddit className="w-5 h-5 text-orange-600" />
            </div>
        );
    }
    return null;
  };

  return (
    <article className="bg-white rounded-2xl p-6 border border-slate-100  relative">
      {newsItem.sourceType && (
        <div className="absolute top-4 right-4 z-10">
          {getSourceIcon()}
        </div>
      )}
      {newsItem.imageUrl && (
        <div className="mb-4 rounded-xl overflow-hidden">
          <img 
            src={newsItem.imageUrl} 
            alt={newsItem.title}
            className="w-full h-64 object-cover"
            onError={(e) => {
              (e.target as HTMLImageElement).style.display = 'none';
            }}
          />
        </div>
      )}
      
      <h2 className="text-xl font-bold text-slate-900 mb-3 leading-tight">
        {newsItem.title}
      </h2>
      
      {description && (
        <div className="mb-4">
          <p className="text-slate-700 text-sm leading-relaxed whitespace-pre-wrap">
            {displayText}
          </p>
          {shouldTruncate && (
            <button
              onClick={() => setShowFullText(!showFullText)}
              className="mt-2 text-sm text-indigo-600 hover:text-indigo-800 font-medium"
            >
              {showFullText ? 'Свернуть' : 'Читать полностью'}
            </button>
          )}
        </div>
      )}

      <div className="flex flex-wrap items-center gap-3 mt-4 pt-4 border-t border-slate-100">
        {newsItem.category && (
          <span className="inline-block px-3 py-1 text-xs font-medium bg-indigo-100 text-indigo-700 rounded-full">
            {newsItem.category}
          </span>
        )}
        {newsItem.author && (
          <span className="text-xs text-slate-500">Автор: {newsItem.author}</span>
        )}
        <span className="text-xs text-slate-500">{formattedDate}</span>
        <a
          href={newsItem.link}
          target="_blank"
          rel="noopener noreferrer"
          className="ml-auto px-4 py-2 text-xs font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 transition-colors"
        >
          Перейти к источнику →
        </a>
      </div>
    </article>
  );
};

export default NewsItemCard;
