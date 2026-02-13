import React, { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { FaBars, FaSearch, FaFilter, FaUserCircle, FaChevronDown, FaPlus } from 'react-icons/fa';
import { fetchFilterOptions, SOURCES_CHANGED_EVENT } from '../services/newsService';
import { createPortal } from 'react-dom';

export type TopBarPeriod = 'day' | 'week' | 'month' | '';

interface TopBarProps {
  searchQuery: string;
  onSearchChange: (value: string) => void;
  period: TopBarPeriod;
  onPeriodChange: (value: TopBarPeriod) => void;
  onFiltersChange?: (filters: {
    sources: number[];
    categories: string[];
    period: TopBarPeriod;
  }) => void;
}

const TopBar: React.FC<TopBarProps> = ({
  searchQuery,
  onSearchChange,
  period,
  onPeriodChange,
  onFiltersChange,
}) => {
  const navigate = useNavigate();
  const [filterMenuOpen, setFilterMenuOpen] = useState(false);
  const [categoriesOpen, setCategoriesOpen] = useState(false);
  const [sourcesOpen, setSourcesOpen] = useState(false);
  const [filterOptions, setFilterOptions] = useState<{
    sources: Array<{ id: number, name: string }>,
    categories: string[]
  } | null>(null);
  const [selectedSources, setSelectedSources] = useState<number[]>([]);
  const [selectedCategories, setSelectedCategories] = useState<string[]>([]);
  const filterRef = useRef<HTMLDivElement>(null);
  const [filterPosition, setFilterPosition] = useState({ top: 0, left: 0 });
  const [filterOptionsVersion, setFilterOptionsVersion] = useState(0);

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
  }, [filterOptionsVersion]);

  useEffect(() => {
    const handleSourcesChanged = () => {
      setFilterOptionsVersion((prev) => prev + 1);
    };

    if (typeof window === 'undefined') {
      return;
    }

    window.addEventListener(SOURCES_CHANGED_EVENT, handleSourcesChanged);
    return () => window.removeEventListener(SOURCES_CHANGED_EVENT, handleSourcesChanged);
  }, []);

  const menuRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (filterRef.current && !filterRef.current.contains(event.target as Node) &&
          menuRef.current && !menuRef.current.contains(event.target as Node)) {
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
    onFiltersChange?.({
      sources: selectedSources,
      categories: selectedCategories,
      period: period
    });
    setFilterMenuOpen(false);
  };

  const resetFilters = () => {
    setSelectedSources([]);
    setSelectedCategories([]);
    onFiltersChange?.({
      sources: [],
      categories: [],
      period: '' as any
    });
    onPeriodChange('' as any);
    setFilterMenuOpen(false);
  };

  return (
    <header className="h-20 border-b border-slate-200 flex items-center px-8 justify-between bg-white/80 backdrop-blur relative">
      <div className="flex items-center space-x-2" />

      <div className="flex-1 flex flex-col items-center">
        <div className="w-3/4 max-w-xl flex items-center bg-slate-50 border border-slate-200 rounded-full px-4 py-1 shadow-sm">
          <button className="mr-2 text-slate-400 hover:text-slate-600">
            <FaBars />
          </button>
          <input
            type="text"
            placeholder="Поиск новостей..."
            className="flex-1 bg-transparent outline-none text-sm text-slate-800 placeholder:text-slate-400"
            value={searchQuery}
            onChange={(e) => onSearchChange(e.target.value)}
          />
          <FaSearch className="text-slate-400" />
        </div>
        <div className="mt-2 flex items-center space-x-2 text-[11px] uppercase tracking-wide relative" ref={filterRef}>
          <button
            className={`px-3 py-1 rounded-full border transition-colors ${
              period === 'day'
                ? 'bg-slate-900 text-white border-slate-900'
                : 'bg-white text-slate-700 border-slate-300'
            }`}
            onClick={() => onPeriodChange('day')}
          >
            ЗА ДЕНЬ
          </button>
          <button
            className={`px-3 py-1 rounded-full border transition-colors ${
              period === 'week'
                ? 'bg-slate-900 text-white border-slate-900'
                : 'bg-white text-slate-700 border-slate-300'
            }`}
            onClick={() => onPeriodChange('week')}
          >
            ЗА НЕДЕЛЮ
          </button>
          <button
            className={`px-3 py-1 rounded-full border transition-colors ${
              period === 'month'
                ? 'bg-slate-900 text-white border-slate-900'
                : 'bg-white text-slate-700 border-slate-300'
            }`}
            onClick={() => onPeriodChange('month')}
          >
            ЗА МЕСЯЦ
          </button>
          <button
            className="ml-2 text-slate-500 hover:text-slate-700 relative"
            onClick={() => {
              if (filterRef.current) {
                const rect = filterRef.current.getBoundingClientRect();
                setFilterPosition({
                  top: rect.bottom + window.scrollY,
                  left: rect.left + window.scrollX
                });
              }
              setFilterMenuOpen(!filterMenuOpen);
            }}
          >
            <FaFilter />
          </button>
          <button
            onClick={() => navigate('/add-source')}
            className="ml-2 px-3 py-1.5 bg-indigo-600 text-white text-xs rounded-full hover:bg-indigo-700 transition-colors flex items-center space-x-1"
          >
            <FaPlus className="w-3 h-3" />
            <span>Добавить источник</span>
          </button>

          {filterMenuOpen && createPortal(
            <div
              ref={menuRef}
              className="fixed w-80 bg-white rounded-lg shadow-xl border border-slate-200 z-[99999] max-h-[70vh] overflow-y-auto"
              style={{
                top: `${filterPosition.top + 8}px`, // добавляем небольшой отступ
                left: `${filterPosition.left}px`
              }}
            >
              <div className="p-4 space-y-4">
                {/* Период */}
                <div>
                  <h3 className="font-semibold text-sm mb-2 text-slate-900">Период публикации</h3>
                  <div className="flex flex-wrap gap-2">
                    {(['day', 'week', 'month'] as TopBarPeriod[]).map((p) => (
                      <button
                        key={p}
                        onClick={() => onPeriodChange(p)}
                        className={`px-3 py-1 text-xs rounded-full transition-colors ${
                          period === p
                            ? 'bg-slate-900 text-white'
                            : 'bg-slate-100 text-slate-700 hover:bg-slate-200'
                        }`}
                      >
                        {p === 'day' ? 'День' : p === 'week' ? 'Неделя' : 'Месяц'}
                      </button>
                    ))}
                  </div>
                </div>

                {/* Категории - Аккордеон */}
                {filterOptions?.categories && filterOptions.categories.length > 0 && (
                  <div>
                    <button
                      onClick={() => setCategoriesOpen(!categoriesOpen)}
                      className="w-full flex items-center justify-between font-semibold text-sm text-slate-900 mb-2"
                    >
                      <span>Категории</span>
                      <FaChevronDown className={`text-xs transition-transform ${categoriesOpen ? 'rotate-180' : ''}`} />
                    </button>
                    {categoriesOpen && (
                      <div className="space-y-1 max-h-40 overflow-y-auto pl-2">
                        {filterOptions.categories.map((category) => (
                          <label key={category} className="flex items-center space-x-2 text-sm cursor-pointer">
                            <input
                              type="checkbox"
                              checked={selectedCategories.includes(category)}
                              onChange={() => handleCategoryToggle(category)}
                              className="rounded text-indigo-600"
                            />
                            <span className="text-slate-700">{category}</span>
                          </label>
                        ))}
                      </div>
                    )}
                  </div>
                )}

                {/* Каналы - Аккордеон */}
                {filterOptions?.sources && filterOptions.sources.length > 0 && (
                  <div>
                    <button
                      onClick={() => setSourcesOpen(!sourcesOpen)}
                      className="w-full flex items-center justify-between font-semibold text-sm text-slate-900 mb-2"
                    >
                      <span>Каналы</span>
                      <FaChevronDown className={`text-xs transition-transform ${sourcesOpen ? 'rotate-180' : ''}`} />
                    </button>
                    {sourcesOpen && (
                      <div className="space-y-1 max-h-40 overflow-y-auto pl-2">
                        {filterOptions.sources.map((source) => (
                          <label key={source.id} className="flex items-center space-x-2 text-sm cursor-pointer">
                            <input
                              type="checkbox"
                              checked={selectedSources.includes(source.id)}
                              onChange={() => handleSourceToggle(source.id)}
                              className="rounded text-indigo-600"
                            />
                            <span className="text-slate-700">{source.name}</span>
                          </label>
                        ))}
                      </div>
                    )}
                  </div>
                )}

                <div className="flex space-x-2 pt-2 border-t border-slate-200">
                  <button
                    onClick={applyFilters}
                    className="flex-1 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 transition-colors text-sm font-medium"
                  >
                    Применить
                  </button>
                  <button
                    onClick={resetFilters}
                    className="py-2 px-4 bg-slate-200 rounded-md hover:bg-slate-300 transition-colors text-sm font-medium"
                  >
                    Сброс
                  </button>
                </div>
              </div>
            </div>,
            document.body
          )}
        </div>
      </div>

      <div>
        <FaUserCircle className="w-9 h-9 text-slate-500" />
      </div>

      
    </header>
  );
};

export default TopBar;
