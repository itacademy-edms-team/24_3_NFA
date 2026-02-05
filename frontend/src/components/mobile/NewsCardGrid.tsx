import React from 'react';
import { type NewsItem } from '../../types/NewsItem';
import { FaGithub, FaReddit, FaRss } from 'react-icons/fa';

interface NewsCardGridProps {
  newsItem: NewsItem;
  onClick?: () => void;
}

const NewsCardGrid: React.FC<NewsCardGridProps> = ({ newsItem, onClick }) => {
  const getSourceIcon = () => {
    const sourceType = newsItem.sourceType?.toLowerCase();
    if (sourceType === 'github') return FaGithub;
    if (sourceType === 'reddit') return FaReddit;
    if (sourceType === 'rss') return FaRss;
    return null;
  };

  const SourceIcon = getSourceIcon();

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    const now = new Date();
    const diff = now.getTime() - date.getTime();
    const days = Math.floor(diff / (1000 * 60 * 60 * 24));

    if (days === 0) return 'Updated today';
    if (days === 1) return 'Updated yesterday';
    return `Updated ${days} days ago`;
  };

  return (
    <div
      onClick={onClick}
      className="bg-white rounded-xl overflow-hidden shadow-sm active:shadow-md transition-shadow cursor-pointer"
    >
      <div className="aspect-square bg-[#F5F5F7] flex items-center justify-center">
        {newsItem.imageUrl ? (
          <img
            src={newsItem.imageUrl}
            alt={newsItem.title}
            className="w-full h-full object-cover"
          />
        ) : (
          <div className="text-[#8E8E93]">
            {SourceIcon && <SourceIcon className="w-12 h-12" />}
          </div>
        )}
      </div>
      <div className="p-3">
        <h3 className="text-[14px] font-semibold text-[#1A1A1A] line-clamp-2 mb-1">
          {newsItem.title}
        </h3>
        <p className="text-[11px] text-[#8E8E93]">{formatDate(newsItem.publishedAtUtc)}</p>
      </div>
    </div>
  );
};

export default NewsCardGrid;
