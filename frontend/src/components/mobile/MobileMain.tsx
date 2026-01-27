import React, { useState, useEffect, useCallback, useRef } from 'react';
import { useLocation } from 'react-router-dom';
import NewsCardFeed from './NewsCardFeed';
import MobileHeader from './MobileHeader';
import { fetchLatestNews, fetchFilterOptions, type PeriodFilter, type FilterParams } from '../../services/newsService';
import { type NewsItem } from '../../types/NewsItem';
import FilterChips from './FilterChips';
import { FaSearch, FaFilter, FaTimes } from 'react-icons/fa';
import { Virtuoso, type VirtuosoHandle } from 'react-virtuoso';

const MobileMain: React.FC = () => {
  const location = useLocation();
  const [news, setNews] = useState<NewsItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);
  const [hasMore, setHasMore] = useState(true);
  const [offset, setOffset] = useState(0);
  const [searchQuery, setSearchQuery] = useState('');
  const [searchOpen, setSearchOpen] = useState(false);
  const virtuosoRef = useRef<VirtuosoHandle>(null);

  const [selectedPeriod, setSelectedPeriod] = useState<string | number>('');
  const [selectedSources, setSelectedSources] = useState<number[]>([]);
  const [selectedCategories, setSelectedCategories] = useState<string[]>([]);
  const [filterMenuOpen, setFilterMenuOpen] = useState(false);
  const [filterOptions, setFilterOptions] = useState<{
    sources: Array<{ id: number; name: string }>;
    categories: string[];
  } | null>(null);

  useEffect(() => {
    const state = location.state as { sourceFilter?: number } | null;
    if (state?.sourceFilter) {
      setSelectedSources([state.sourceFilter]);
      window.history.replaceState({}, document.title);
    }
  }, [location]);

  const filterMenuRef = useRef<HTMLDivElement>(null);

  const periods = [
    { id: '', label: 'Все' },
    { id: 'day', label: 'День' },
    { id: 'week', label: 'Неделя' },
    { id: 'month', label: 'Месяц' },
  ];

  useEffect(() => {
    const loadFilterOptions = async () => {
      try {
        const options = await fetchFilterOptions();
        setFilterOptions(options);
      } catch (error) {
        console.error('Error loading filter options:', error);
      }
    };
    loadFilterOptions();
  }, []);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (filterMenuRef.current && !filterMenuRef.current.contains(event.target as Node)) {
        setFilterMenuOpen(false);
      }
    };

    if (filterMenuOpen) {
      document.addEventListener('mousedown', handleClickOutside);
    }

    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [filterMenuOpen]);

  const loadNews = useCallback(async (reset = false) => {
    if (reset) {
      setLoading(true);
      setOffset(0);
      setHasMore(true);
    } else {
      if (loadingMore || !hasMore) return;
      setLoadingMore(true);
    }

    try {
      const currentOffset = reset ? 0 : offset;
      const params: FilterParams = {
        offset: currentOffset,
        limit: 10,
        period: String(selectedPeriod) as PeriodFilter,
        searchQuery: searchQuery || undefined,
        sources: selectedSources.length > 0 ? selectedSources : undefined,
        categories: selectedCategories.length > 0 ? selectedCategories : undefined,
      };

      const response = await fetchLatestNews(params) as any;
      const newsItems: NewsItem[] = response?.items || response || [];
      const serverHasMore = newsItems.length === 10;

      if (reset) {
        setNews(newsItems);
      } else {
        setNews(prev => {
          const existingIds = new Set(prev.map(i => i.id));
          const uniqueItems = newsItems.filter(i => !existingIds.has(i.id));
          return [...prev, ...uniqueItems];
        });
      }

      setHasMore(serverHasMore);
      setOffset(currentOffset + 10);
    } catch (error) {
      console.error('Error loading news:', error);
    } finally {
      setLoading(false);
      setLoadingMore(false);
    }
  }, [offset, selectedPeriod, searchQuery, selectedSources, selectedCategories]);

  useEffect(() => {
    loadNews(true);
  }, [selectedPeriod, searchQuery, selectedSources, selectedCategories]);

  const handleSourceToggle = (sourceId: number) => {
    setSelectedSources(prev =>
      prev.includes(sourceId)
        ? prev.filter(id => id !== sourceId)
        : [...prev, sourceId]
    );
  };

  const handleCategoryToggle = (category: string) => {
    setSelectedCategories(prev =>
      prev.includes(category)
        ? prev.filter(cat => cat !== category)
        : [...prev, category]
    );
  };

  const applyFilters = () => {
    setFilterMenuOpen(false);
    loadNews(true);
  };

  const clearFilters = () => {
    setSelectedSources([]);
    setSelectedCategories([]);
    setSelectedPeriod('');
    setFilterMenuOpen(false);
    loadNews(true);
  };

  // Обработка конца скролла для Virtuoso
  const handleEndReached = useCallback(() => {
    if (!loadingMore && hasMore) {
      loadNews(false);
    }
  }, [loadingMore, hasMore, loadNews]);

  if (loading) {
    return (
      <div>
        <MobileHeader showSearch={false} showMenu={false} />
        <div className="flex items-center justify-center py-20">
          <p className="text-[14px] text-[#6B6B6B]">Загрузка новостей...</p>
        </div>
      </div>
    );
  }

  if (news.length === 0) {
    return (
      <div>
        <MobileHeader showSearch={false} showMenu={false} />
        <div className="flex flex-col items-center justify-center py-20 px-4">
          <p className="text-[16px] font-semibold text-[#1A1A1A] mb-2">Новости не найдены</p>
          <p className="text-[14px] text-[#6B6B6B] text-center">
            Попробуйте изменить параметры фильтров
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="h-[calc(100vh-56px)] flex flex-col">
      <MobileHeader showSearch={false} showMenu={false} />

      {/* Filter Bar с периодами, поиском и фильтром */}
      <div className="flex-shrink-0 bg-[#F5F5F7]">
        {/* Строка с периодами */}
        <div className="px-4 py-2 flex items-center gap-2">
          <div className="flex-1 overflow-x-auto">
            <FilterChips
              chips={periods}
              selectedId={selectedPeriod}
              onSelect={setSelectedPeriod}
            />
          </div>

          {/* Поиск и фильтр справа */}
          <div className="flex items-center gap-2 flex-shrink-0">
            {searchOpen ? (
              <div className="flex items-center gap-1">
                <input
                  type="text"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  placeholder="Поиск..."
                  className="w-32 px-2 py-1.5 bg-white rounded-lg text-[13px] border border-[#E5E5EA] focus:outline-none focus:border-[#6B5B95]"
                  autoFocus
                />
                <button
                  onClick={() => {
                    setSearchOpen(false);
                    setSearchQuery('');
                  }}
                  className="p-2 text-[#6B6B6B] active:bg-[#E5E5EA] rounded-lg"
                >
                  <FaTimes className="w-4 h-4" />
                </button>
              </div>
            ) : (
              <button
                onClick={() => setSearchOpen(true)}
                className="p-2 text-[#6B6B6B] active:bg-[#E5E5EA] rounded-lg transition-colors"
              >
                <FaSearch className="w-4 h-4" />
              </button>
            )}

            <button
              onClick={() => setFilterMenuOpen(true)}
              className={`p-2 rounded-lg transition-colors ${
                selectedSources.length > 0 || selectedCategories.length > 0
                  ? 'text-[#6B5B95] bg-[#6B5B95]/10'
                  : 'text-[#6B6B6B] active:bg-[#E5E5EA]'
              }`}
            >
              <FaFilter className="w-4 h-4" />
            </button>
          </div>
        </div>
      </div>

      {/* Virtuoso список новостей */}
      <div className="flex-1 overflow-hidden">
        <Virtuoso
          ref={virtuosoRef}
          style={{ height: '100%' }}
          data={news}
          overscan={500}
          itemContent={(_index, item) => (
            <div className="px-4 py-2">
              <NewsCardFeed newsItem={item} />
            </div>
          )}
          endReached={handleEndReached}
          components={{
            Footer: () => (
              <div className="py-6 text-center">
                {loadingMore && (
                  <p className="text-[13px] text-[#6B6B6B]">Загрузка еще новостей...</p>
                )}
                {!hasMore && news.length > 0 && (
                  <p className="text-[12px] text-[#8E8E93]">Вы посмотрели все новости</p>
                )}
              </div>
            )
          }}
        />
      </div>

      {/* Filter Menu Modal */}
      {filterMenuOpen && (
        <div className="fixed inset-0 z-50 flex items-end justify-center bg-black/50" onClick={() => setFilterMenuOpen(false)}>
          <div
            ref={filterMenuRef}
            className="bg-white rounded-t-2xl w-full max-h-[70vh] overflow-y-auto mb-0"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="p-4 border-b border-[#E5E5EA] flex items-center justify-between sticky top-0 bg-white">
              <h3 className="text-[16px] font-semibold text-[#1A1A1A]">Фильтры</h3>
              <button
                onClick={() => setFilterMenuOpen(false)}
                className="p-2 text-[#6B6B6B] active:bg-[#E5E5EA] rounded-lg"
              >
                <FaTimes className="w-4 h-4" />
              </button>
            </div>

            <div className="p-4 space-y-4">
              {/* Фильтр по источникам */}
              <div>
                <h4 className="text-[14px] font-semibold text-[#1A1A1A] mb-2">Источники</h4>
                <div className="space-y-2">
                  {filterOptions?.sources.map((source) => (
                    <label
                      key={source.id}
                      className="flex items-center gap-3 p-3 bg-[#F5F5F7] rounded-lg cursor-pointer active:bg-[#E5E5EA]"
                    >
                      <input
                        type="checkbox"
                        checked={selectedSources.includes(source.id)}
                        onChange={() => handleSourceToggle(source.id)}
                        className="w-5 h-5 rounded border-[#E5E5EA] text-[#6B5B95] focus:ring-[#6B5B95]"
                      />
                      <span className="text-[14px] text-[#1A1A1A]">{source.name}</span>
                    </label>
                  ))}
                </div>
              </div>

              {/* Фильтр по категориям */}
              <div>
                <h4 className="text-[14px] font-semibold text-[#1A1A1A] mb-2">Категории</h4>
                <div className="flex flex-wrap gap-2">
                  {filterOptions?.categories.map((category) => (
                    <button
                      key={category}
                      onClick={() => handleCategoryToggle(category)}
                      className={`px-3 py-1.5 rounded-full text-[13px] font-medium transition-colors ${
                        selectedCategories.includes(category)
                          ? 'bg-[#6B5B95] text-white'
                          : 'bg-[#F5F5F7] text-[#6B6B6B]'
                      }`}
                    >
                      {category}
                    </button>
                  ))}
                </div>
              </div>
            </div>

            {/* Кнопки действий */}
            <div className="p-4 border-t border-[#E5E5EA] flex gap-2 sticky bottom-0 bg-white">
              <button
                onClick={clearFilters}
                className="flex-1 px-4 py-3 bg-[#F5F5F7] text-[#6B6B6B] rounded-lg text-[14px] font-medium"
              >
                Сбросить
              </button>
              <button
                onClick={applyFilters}
                className="flex-1 px-4 py-3 bg-[#6B5B95] text-white rounded-lg text-[14px] font-medium"
              >
                Применить
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default MobileMain;
