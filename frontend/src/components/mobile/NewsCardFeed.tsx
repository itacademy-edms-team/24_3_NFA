import React, { useState } from 'react';
import { type NewsItem } from '../../types/NewsItem';
import { FaGithub, FaReddit, FaRss, FaHeart } from 'react-icons/fa';
import { useFavorites } from '../../contexts/FavoritesContext';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import rehypeRaw from 'rehype-raw';

interface NewsCardFeedProps {
  newsItem: NewsItem;
}

const NewsCardFeed: React.FC<NewsCardFeedProps> = ({ newsItem }) => {
  const favorites = useFavorites();
  const isFav = favorites?.isFavorite(newsItem.id) ?? false;
  const [showFullText, setShowFullText] = useState(false);

  const getSourceIcon = () => {
    const sourceType = newsItem.sourceType?.toLowerCase();
    if (sourceType === 'github') return FaGithub;
    if (sourceType === 'reddit') return FaReddit;
    if (sourceType === 'rss') return FaRss;
    return null;
  };

  const SourceIcon = getSourceIcon();

  const truncateText = (text: string, maxLength: number) => {
    if (text.length <= maxLength) return text;
    return text.substring(0, maxLength) + ' ...';
  };

  const displayText = showFullText ? newsItem.description : truncateText(newsItem.description || '', 300);

  return (
    <article className="bg-white rounded-2xl overflow-hidden shadow-sm mb-4">
      {newsItem.imageUrl && (
        <div className="w-full bg-[#F5F5F7]">
          <img
            src={newsItem.imageUrl}
            alt={newsItem.title}
            className="w-full h-auto max-w-full"
          />
        </div>
      )}

      <div className="p-4">
        <div className="flex items-start justify-between mb-3">
          <h2 className="text-[16px] font-semibold text-[#1A1A1A] leading-tight flex-1">
            {newsItem.title}
          </h2>
          {favorites && (
            <button
              onClick={() => favorites.toggleFavorite(newsItem)}
              className="ml-2 p-2 -mr-2"
            >
              <FaHeart
                className={`w-5 h-5 ${
                  isFav ? 'text-red-500 fill-red-500' : 'text-[#8E8E93]'
                }`}
              />
            </button>
          )}
        </div>

        {newsItem.description && (
          <div className="mb-4">
            {newsItem.sourceType?.toLowerCase() === 'github' ||
            newsItem.sourceType?.toLowerCase() === 'reddit' ? (
              <div className="text-[14px] text-[#6B6B6B] leading-relaxed">
                <ReactMarkdown
                  remarkPlugins={[remarkGfm]}
                  rehypePlugins={[rehypeRaw]}
                >
                  {displayText}
                </ReactMarkdown>
              </div>
            ) : (
              <p className="text-[14px] text-[#6B6B6B] leading-relaxed whitespace-pre-wrap">
                {displayText}
              </p>
            )}
            {newsItem.description.length > 300 && (
              <button
                onClick={() => setShowFullText(!showFullText)}
                className="mt-2 text-[14px] font-medium text-[#6B5B95]"
              >
                {showFullText ? 'Свернуть' : 'Читать полностью'}
              </button>
            )}
          </div>
        )}

        <div className="flex items-center justify-between pt-3 border-t border-[#E5E5EA]">
          <div className="flex items-center gap-2">
            {SourceIcon && (
              <div className="text-[#8E8E93]">
                <SourceIcon className="w-4 h-4" />
              </div>
            )}
            {newsItem.author && (
              <span className="text-[12px] text-[#8E8E93]">{newsItem.author}</span>
            )}
            <span className="text-[12px] text-[#8E8E93]">
              {new Date(newsItem.publishedAtUtc).toLocaleDateString('ru-RU', {
                day: '2-digit',
                month: '2-digit',
                hour: '2-digit',
                minute: '2-digit',
              })}
            </span>
          </div>
          <a
            href={newsItem.link}
            target="_blank"
            rel="noopener noreferrer"
            className="px-4 py-2 bg-[#6B5B95] text-white text-[13px] font-medium rounded-full hover:bg-[#5A4A85] transition-colors"
          >
            Подробнее
          </a>
        </div>
      </div>
    </article>
  );
};

export default NewsCardFeed;
