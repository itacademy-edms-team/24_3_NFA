import React from 'react';
import { type NewsItem } from '../../types/NewsItem';
import { FaGithub, FaReddit, FaRss, FaChevronRight } from 'react-icons/fa';

interface NewsCardListProps {
  newsItem: NewsItem;
  onClick?: () => void;
}

const NewsCardList: React.FC<NewsCardListProps> = ({ newsItem, onClick }) => {
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
    return text.substring(0, maxLength) + '...';
  };

  const formatTime = (dateString: string) => {
    const date = new Date(dateString);
    const now = new Date();
    const diffMinutes = Math.floor((now.getTime() - date.getTime()) / (1000 * 60));

    if (diffMinutes < 60) return `${diffMinutes} min`;
    const diffHours = Math.floor(diffMinutes / 60);
    if (diffHours < 24) return `${diffHours} hr`;
    const diffDays = Math.floor(diffHours / 24);
    return `${diffDays} day`;
  };

  return (
    <div
      onClick={onClick}
      className="bg-white rounded-xl p-3 shadow-sm flex gap-3 cursor-pointer active:bg-[#F5F5F7] transition-colors"
    >
      <div className="w-20 h-20 bg-[#F5F5F7] rounded-lg flex-shrink-0 flex items-center justify-center overflow-hidden">
        {newsItem.imageUrl ? (
          <img
            src={newsItem.imageUrl}
            alt={newsItem.title}
            className="w-full h-full object-cover"
          />
        ) : (
          <div className="text-[#8E8E93]">
            {SourceIcon && <SourceIcon className="w-8 h-8" />}
          </div>
        )}
      </div>

      <div className="flex-1 min-w-0 flex flex-col justify-center">
        <h3 className="text-[14px] font-semibold text-[#1A1A1A] line-clamp-2 mb-1">
          {newsItem.title}
        </h3>
        {newsItem.description && (
          <p className="text-[12px] text-[#6B6B6B] line-clamp-2 mb-2">
            {truncateText(newsItem.description, 80)}
          </p>
        )}
        <div className="flex items-center gap-2 text-[11px] text-[#8E8E93]">
          <span>{formatTime(newsItem.publishedAtUtc)}</span>
          <span>•</span>
          {SourceIcon && <SourceIcon className="w-3 h-3" />}
        </div>
      </div>

      <div className="flex items-center">
        <FaChevronRight className="w-4 h-4 text-[#8E8E93]" />
      </div>
    </div>
  );
};

export default NewsCardList;
