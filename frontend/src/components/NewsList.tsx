import React, { useState, useEffect, useCallback, useRef } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import NewsItemCard from './NewsItemCard';
import type { NewsItem } from '../types/NewsItem';
import { fetchLatestNews, type PeriodFilter, type FilterParams } from '../services/newsService';
import { Virtuoso, type VirtuosoHandle } from 'react-virtuoso';

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

const MAX_NEWS_IN_MEMORY = 100;

const NewsList: React.FC<NewsListProps> = ({ timeFilter, searchQuery, sourceType, appliedFilters }) => {
  const location = useLocation();
  const [news, setNews] = useState<NewsItem[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [loadingMore, setLoadingMore] = useState<boolean>(false);
  const [hasMore, setHasMore] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [showHeader, setShowHeader] = useState<boolean>(true);
  const navigate = useNavigate();
  const virtuosoRef = useRef<VirtuosoHandle>(null);
  const offset = useRef(0);
  const hasMoreRef = useRef(hasMore);
  const loadingMoreRef = useRef(loadingMore);

  hasMoreRef.current = hasMore;
  loadingMoreRef.current = loadingMore;

  const [filters, setFilters] = useState({
    sources: appliedFilters?.sources || [] as number[],
    categories: appliedFilters?.categories || [] as string[],
    period: appliedFilters?.period || timeFilter || ''
  });

  const loadNews = useCallback(async (reset = false) => {
    console.log('loadNews called', { reset, offset: offset.current, hasMore: hasMoreRef.current });
    
    if (reset) {
      setLoading(true);
      offset.current = 0;
      setHasMore(true);
    } else {
      if (loadingMoreRef.current || !hasMoreRef.current) {
        console.log('Skipping load:', { loadingMore: loadingMoreRef.current, hasMore: hasMoreRef.current });
        return;
      }
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

      console.log('Server response:', { rawResponse, params });

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

      console.log('Parsed news:', { newsItemsCount: newsItems.length, serverHasMore });

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
            const merged = [...prev, ...uniqueNewItems];
            return merged.length > MAX_NEWS_IN_MEMORY
              ? merged.slice(0, MAX_NEWS_IN_MEMORY)
              : merged;
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
  }, [searchQuery, filters, sourceType]);

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

  // Обработка конца скролла для Virtuoso
  const handleEndReached = useCallback(() => {
    console.log('endReached triggered', { loading, loadingMore: loadingMoreRef.current, hasMore: hasMoreRef.current });
    if (!loading && !loadingMoreRef.current && hasMoreRef.current) {
      loadNews(false);
    }
  }, [loading, loadNews]);

  // Скрытие/показ заголовка при скролле внутри Virtuoso
  const handleScroll = useCallback((scrollTop: number) => {
    // Показываем заголовок только когда в самом верху (< 50px)
    if (scrollTop < 50) {
      setShowHeader(true);
    } else {
      setShowHeader(false);
    }
  }, []);

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
    <div className="min-w-0 max-w-[844px] mx-auto">
      {showHeader && (
        <div className="flex justify-between items-center mb-4">
          <h2 className="text-lg font-semibold text-slate-900">Новости</h2>
        </div>
      )}

      <Virtuoso
        ref={virtuosoRef}
        style={{ height: 'calc(100vh - 250px)' }}
        data={news}
        overscan={500}
        onScroll={(e) => handleScroll((e.target as HTMLElement).scrollTop)}
        itemContent={(_index, item) => (
          <div className="mb-4">
            <NewsItemCard key={item.id} newsItem={item} />
          </div>
        )}
        endReached={handleEndReached}
        components={{
          Footer: () => (
            <div className="py-6 text-center">
              {loadingMore && (
                <p className="text-slate-500 text-sm">Загрузка еще новостей...</p>
              )}
              {!hasMore && news.length > 0 && (
                <p className="text-slate-400 text-xs">Вы посмотрели все новости</p>
              )}
            </div>
          )
        }}
      />
    </div>
  );
}

export default NewsList;
