import React, { useState, useEffect } from 'react';
import { fetchFilterOptions } from '../services/newsService';

interface FilterPanelProps {
  onFilterChange: (filters: {
    sources: number[];
    categories: string[];
    period: string;
  }) => void;
  currentFilters: {
    sources: number[];
    categories: string[];
    period: string;
  };
}

const FilterPanel: React.FC<FilterPanelProps> = ({ onFilterChange, currentFilters }) => {
  const [isOpen, setIsOpen] = useState(false);
  const [filterOptions, setFilterOptions] = useState<{ 
    sources: Array<{ id: number, name: string }>, 
    categories: string[] 
  } | null>(null);
  const [selectedSources, setSelectedSources] = useState<number[]>(currentFilters.sources);
  const [selectedCategories, setSelectedCategories] = useState<string[]>(currentFilters.categories);
  const [selectedPeriod, setSelectedPeriod] = useState<string>(currentFilters.period);

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
    setSelectedSources(currentFilters.sources);
    setSelectedCategories(currentFilters.categories);
    setSelectedPeriod(currentFilters.period);
  }, [currentFilters]);

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
    onFilterChange({
      sources: selectedSources,
      categories: selectedCategories,
      period: selectedPeriod
    });
    setIsOpen(false);
  };

  const resetFilters = () => {
    setSelectedSources([]);
    setSelectedCategories([]);
    setSelectedPeriod('week');
    onFilterChange({
      sources: [],
      categories: [],
      period: 'week'
    });
    setIsOpen(false);
  };

  return (
    <div className="relative">
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="px-4 py-2 bg-gray-200 rounded-md hover:bg-gray-300 transition-colors flex items-center"
      >
        <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 mr-1" viewBox="0 0 20 20" fill="currentColor">
          <path fillRule="evenodd" d="M3 3a1 1 0 011-1h12a1 1 0 011 1v3a1 1 0 01-.293.707L12 11.414V15a1 1 0 01-.293.707l-2 2A1 1 0 018 17v-5.586L3.293 6.707A1 1 0 013 6V3z" clipRule="evenodd" />
        </svg>
        Фильтры
      </button>

      {isOpen && (
        <div className="absolute top-full right-0 mt-2 w-80 bg-white rounded-lg shadow-xl border border-gray-200 z-50 max-h-[70vh] overflow-y-auto">
          <div className="p-4">
            <div className="mb-4">
              <h3 className="font-medium mb-2">Период</h3>
              <div className="flex flex-wrap gap-2">
                {['day', 'week', 'month'].map((period) => (
                  <button
                    key={period}
                    onClick={() => setSelectedPeriod(period)}
                    className={`px-3 py-1 text-sm rounded-full ${
                      selectedPeriod === period
                        ? 'bg-blue-600 text-white'
                        : 'bg-gray-100 hover:bg-gray-200'
                    }`}
                  >
                    {period === 'day' ? 'День' : period === 'week' ? 'Неделя' : 'Месяц'}
                  </button>
                ))}
              </div>
            </div>

            {filterOptions?.sources && filterOptions.sources.length > 0 && (
              <div className="mb-4">
                <h3 className="font-medium mb-2">Источники</h3>
                <div className="space-y-1 max-h-40 overflow-y-auto">
                  {filterOptions.sources.map((source) => (
                    <label key={source.id} className="flex items-center space-x-2 text-sm">
                      <input
                        type="checkbox"
                        checked={selectedSources.includes(source.id)}
                        onChange={() => handleSourceToggle(source.id)}
                        className="rounded text-blue-600"
                      />
                      <span>{source.name}</span>
                    </label>
                  ))}
                </div>
              </div>
            )}

            {filterOptions?.categories && filterOptions.categories.length > 0 && (
              <div className="mb-4">
                <h3 className="font-medium mb-2">Категории</h3>
                <div className="space-y-1 max-h-40 overflow-y-auto">
                  {filterOptions.categories.map((category) => (
                    <label key={category} className="flex items-center space-x-2 text-sm">
                      <input
                        type="checkbox"
                        checked={selectedCategories.includes(category)}
                        onChange={() => handleCategoryToggle(category)}
                        className="rounded text-blue-600"
                      />
                      <span>{category}</span>
                    </label>
                  ))}
                </div>
              </div>
            )}

            <div className="flex space-x-2 pt-2 border-t">
              <button
                onClick={applyFilters}
                className="flex-1 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
              >
                Применить
              </button>
              <button
                onClick={resetFilters}
                className="py-2 px-4 bg-gray-200 rounded-md hover:bg-gray-300 transition-colors"
              >
                Сброс
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default FilterPanel;