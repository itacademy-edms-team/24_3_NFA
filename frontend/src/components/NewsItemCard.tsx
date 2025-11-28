import React from 'react';
import { type NewsItem } from '../types/NewsItem';

interface NewsItemCardProps {
  newsItem: NewsItem;
}

const NewsItemCard: React.FC<NewsItemCardProps> = ({ newsItem }) => {
  const formattedDate = new Date(newsItem.publishedAtUtc).toLocaleString();

  return (
    <div className="bg-white rounded-2xl p-5 border border-slate-100 shadow-sm hover:shadow-md transition-shadow duration-200">
      <h3 className="text-base font-semibold text-slate-900 mb-1">
        <a
          href={newsItem.link}
          target="_blank"
          rel="noopener noreferrer"
          className="hover:text-indigo-600"
        >
          {newsItem.title}
        </a>
      </h3>
      {newsItem.description && (
        <p className="text-slate-600 text-sm mb-3 line-clamp-3">{newsItem.description}</p>
      )}
      <div className="text-[11px] text-slate-500 flex justify-between items-center">
        <span>Источник ID: {newsItem.sourceId}</span>
        <span>Опубликовано: {formattedDate}</span>
      </div>
      {newsItem.category && (
        <span className="inline-block mt-2 px-2 py-1 text-[11px] bg-slate-100 text-slate-700 rounded-full">
          {newsItem.category}
        </span>
      )}
      {newsItem.author && (
        <p className="text-[11px] text-slate-500 mt-1">Автор: {newsItem.author}</p>
      )}
    </div>
  );
};

export default NewsItemCard;