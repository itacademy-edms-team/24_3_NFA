import React, { useState, useEffect, useCallback, useRef } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import NewsItemCard from './NewsItemCard';
import type { NewsItem } from '../types/NewsItem';
import { fetchLatestNews, type PeriodFilter, type FilterParams } from '../services/newsService';

interface NewsListProps {
  timeFilter: PeriodFilter;
  searchQuery: string;
  sourceType?: string;
  appliedFilters?: {
    sources: number[];
    categories: string[];
    period: string;
  };
}

const NewsList: React.FC<NewsListProps> = ({ timeFilter, searchQuery, sourceType, appliedFilters }) => {
  const location = useLocation();
  const [news, setNews] = useState<NewsItem[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [loadingMore, setLoadingMore] = useState<boolean>(false);
  const [hasMore, setHasMore] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();
  const offset = useRef(0);
  const [filters, setFilters] = useState({
    sources: appliedFilters?.sources || [] as number[],
    categories: appliedFilters?.categories || [] as string[],
    period: appliedFilters?.period || timeFilter || 'week'
  });

  const loadNews = useCallback(async (reset = false) => {
    if (reset) {
      setLoading(true);
      offset.current = 0;
    } else {
      if (offset.current > 0 && !hasMore) return; // Если больше нет новостей, выходим
      setLoadingMore(true);
    }

    try {
      const params: FilterParams = {
        offset: offset.current,
        limit: 10,
        searchQuery: searchQuery,
        period: filters.period as PeriodFilter,
        sources: filters.sources.length > 0 ? filters.sources : undefined,
        categories: filters.categories.length > 0 ? filters.categories : undefined,
        sourceType: sourceType
      };

      const newsData = await fetchLatestNews(params);

      if (reset) {
        setNews(newsData);
      } else {
        setNews(prev => [...prev, ...newsData]);
      }

      // Если получили меньше 10 новостей, значит больше нет
      setHasMore(newsData.length === 10);
      offset.current += 10;
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Произошла ошибка при загрузке новостей.');
      console.error(err);
    } finally {
      if (reset) {
        setLoading(false);
      } else {
        setLoadingMore(false);
      }
    }
  }, [searchQuery, filters, sourceType]);

  useEffect(() => {
    if (appliedFilters) {
      setFilters({
        sources: appliedFilters.sources,
        categories: appliedFilters.categories,
        period: appliedFilters.period
      });
    }
  }, [appliedFilters]);

  useEffect(() => {
    const state = location.state as { sourceFilter?: number } | null;
    const sourceId = state?.sourceFilter;
    if (typeof sourceId === 'number') {
      setFilters(prev => ({
        ...prev,
        sources: [sourceId],
      }));
      setNews([]);
      setHasMore(true);
      offset.current = 0;
      navigate(location.pathname, { replace: true, state: {} });
    }
  }, [location, navigate]);

  useEffect(() => {
    loadNews(true); // Загрузить с начала при изменении фильтров
  }, [filters, searchQuery, timeFilter, sourceType, loadNews]);

  // Обработчик прокрутки для бесконечной ленты
  useEffect(() => {
    const handleScroll = () => {
      if (
        window.innerHeight + document.documentElement.scrollTop
        >= document.documentElement.offsetHeight - 1000 // Загружаем за 1000px до конца
        && !loadingMore
        && hasMore
      ) {
        loadNews(false); // Загрузить следующую порцию
      }
    };

    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, [loadingMore, hasMore, loadNews]);

  if (loading) {
    return <div className="text-center text-slate-500 mt-10 text-sm">Загрузка новостей...</div>;
  }

  const showEmptyState = !!error || (news.length === 0 && !loadingMore);

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
      <div className="flex justify-between items-center">
        <h2 className="text-lg font-semibold">Новости</h2>
      </div>

      {news.map((item) => (
        <NewsItemCard key={item.id} newsItem={item} />
      ))}

      {loadingMore && (
        <div className="text-center text-slate-500 py-4 text-sm">Загрузка новостей...</div>
      )}
    </div>
  );
}

export default NewsList;