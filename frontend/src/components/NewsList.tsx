import React, { useState, useEffect, useCallback, useRef } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import NewsItemCard from './NewsItemCard';
import type { NewsItem } from '../types/NewsItem';
import { fetchLatestNews, type PeriodFilter, type FilterParams } from '../services/newsService';

// Интерфейс ответа
interface NewsResponse {
  items: NewsItem[];
  hasMore: boolean;
}

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
  
  // Реф для отслеживания текущего смещения
  const offset = useRef(0);
  
  // НОВЫЙ РЕФ: Якорь для бесконечного скролла
  const observerTarget = useRef<HTMLDivElement>(null);

  const [filters, setFilters] = useState({
    sources: appliedFilters?.sources || [] as number[],
    categories: appliedFilters?.categories || [] as string[],
    period: appliedFilters?.period || timeFilter || ''
  });

  const loadNews = useCallback(async (reset = false) => {
    if (reset) {
      setLoading(true);
      offset.current = 0;
      setHasMore(true);
    } else {
      // Если уже грузим или больше нечего грузить - выходим
      if (loadingMore || !hasMore) return;
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

      const rawResponse = await fetchLatestNews(params) as any;
      
      let newsItems: NewsItem[] = [];
      let serverHasMore = false;

      if (Array.isArray(rawResponse)) {
          newsItems = rawResponse;
          serverHasMore = newsItems.length === 10;
      } else if (rawResponse && Array.isArray(rawResponse.items)) {
          newsItems = rawResponse.items;
          serverHasMore = !!rawResponse.hasMore;
      } else {
          newsItems = [];
          serverHasMore = false;
      }

      // Fallback логика
      if (newsItems.length === 0 && params.sources && params.sources.length > 0) {
        const paramsWithoutSources: FilterParams = {
          ...params,
          sources: undefined
        };
        const rawFallbackResponse = await fetchLatestNews(paramsWithoutSources) as any;
        
        if (Array.isArray(rawFallbackResponse)) {
            newsItems = rawFallbackResponse;
            serverHasMore = newsItems.length === 10;
        } else if (rawFallbackResponse && Array.isArray(rawFallbackResponse.items)) {
            newsItems = rawFallbackResponse.items;
            serverHasMore = !!rawFallbackResponse.hasMore;
        }
      }

      if (reset) {
        setNews(newsItems);
      } else {
        setNews(prev => {
            const existingIds = new Set(prev.map(i => i.id));
            const uniqueNewItems = newsItems.filter(i => !existingIds.has(i.id));
            return [...prev, ...uniqueNewItems];
        });
      }

      setHasMore(serverHasMore);
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
  }, [searchQuery, filters, sourceType, hasMore, loadingMore]);

  // Обновление фильтров
  useEffect(() => {
    if (appliedFilters) {
      setFilters({
        sources: appliedFilters.sources,
        categories: appliedFilters.categories,
        period: appliedFilters.period
      });
    }
  }, [appliedFilters]);

  // Сброс при переходе с параметров
  useEffect(() => {
    const state = location.state as { sourceFilter?: number } | null;
    const sourceId = state?.sourceFilter;
    if (typeof sourceId === 'number') {
      setFilters(prev => ({ ...prev, sources: [sourceId] }));
      setNews([]);
      setHasMore(true);
      offset.current = 0;
      navigate(location.pathname, { replace: true, state: {} });
    }
  }, [location, navigate]);

  // Первая загрузка / перезагрузка при смене фильтров
  useEffect(() => {
    loadNews(true);
  }, [filters, searchQuery, timeFilter, sourceType]); 

  // =========================================================
  // НОВАЯ ЛОГИКА СКРОЛЛА: Intersection Observer
  // =========================================================
  useEffect(() => {
    const observer = new IntersectionObserver(
      (entries) => {
        // entries[0] - это наш div внизу списка
        if (entries[0].isIntersecting && hasMore && !loadingMore && !loading) {
          console.log('Observer triggered: loading more news...');
          loadNews(false);
        }
      },
      {
        threshold: 0.1, // Срабатывает, когда хотя бы 10% якоря видно
        rootMargin: '100px', // Начинаем грузить за 100px до появления якоря
      }
    );

    if (observerTarget.current) {
      observer.observe(observerTarget.current);
    }

    return () => {
      if (observerTarget.current) {
        observer.unobserve(observerTarget.current);
      }
    };
  }, [hasMore, loadingMore, loading, loadNews]);


  // Рендер
  if (loading) {
    return <div className="text-center text-slate-500 mt-10 text-sm">Загрузка новостей...</div>;
  }

  const showEmptyState = !!error || (news.length === 0 && !loadingMore);

  if (showEmptyState) {
    return (
      <div className="mt-16 flex flex-col items-center justify-center">
        <div className="max-w-md text-center space-y-4">
          <h2 className="text-xl font-semibold text-slate-900">Новости не найдены</h2>
          <p className="text-sm text-slate-500">Попробуйте изменить параметры поиска или фильтры.</p>
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

      {/* ЯКОРЬ ДЛЯ СКРОЛЛА */}
      {/* Этот элемент невидим, но Observer следит за ним */}
      <div ref={observerTarget} className="h-4 w-full" />

      {loadingMore && (
        <div className="text-center text-slate-500 py-4 text-sm">Загрузка еще новостей...</div>
      )}
      
      {!hasMore && news.length > 0 && (
        <div className="text-center text-slate-400 py-6 text-xs">Вы посмотрели все новости</div>
      )}
    </div>
  );
}

export default NewsList;
