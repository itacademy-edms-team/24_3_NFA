import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import NewsItemCard from './NewsItemCard';
import type { NewsItem } from '../types/NewsItem';
import { fetchLatestNews, type PeriodFilter } from '../services/newsService';

interface NewsListProps {
  timeFilter: PeriodFilter;
  searchQuery: string;
}

const NewsList: React.FC<NewsListProps> = ({ timeFilter, searchQuery }) => {
  const [news, setNews] = useState<NewsItem[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    const getNews = async () => {
      try {
        setLoading(true);
        setError(null);
        const newsData = await fetchLatestNews(10, searchQuery, timeFilter ?? undefined);
        setNews(newsData);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Произошла ошибка при загрузке новостей.');
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    getNews();
  }, [timeFilter, searchQuery]);

  if (loading) {
    return <div className="text-center text-slate-500 mt-10 text-sm">Загрузка новостей...</div>;
  }

  const showEmptyState = !!error || news.length === 0;

  if (showEmptyState) {
    return (
      <div className="mt-16 flex flex-col items-center justify-center">
        <div className="max-w-md text-center space-y-4">
          <h2 className="text-xl font-semibold text-slate-900">У вас ещё нет новостных каналов</h2>
          <p className="text-sm text-slate-500">
            Добавьте первый RSS-канал, чтобы начать получать новости в вашу ленту.
          </p>
          <button
            type="button"
            onClick={() => navigate('/add-source')}
            className="mt-2 inline-flex items-center px-4 py-2 rounded-full bg-slate-900 text-white text-sm font-medium hover:bg-slate-800 transition-colors"
          >
            Добавить канал
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {news.map((item) => (
        <NewsItemCard key={item.id} newsItem={item} />
      ))}
    </div>
  );
};

export default NewsList;