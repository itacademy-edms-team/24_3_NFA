import React from 'react';
import { FaBars, FaSearch, FaFilter, FaUserCircle } from 'react-icons/fa';

export type TopBarPeriod = 'day' | 'week' | 'month';

interface TopBarProps {
  searchQuery: string;
  onSearchChange: (value: string) => void;
  period: TopBarPeriod;
  onPeriodChange: (value: TopBarPeriod) => void;
}

const TopBar: React.FC<TopBarProps> = ({
  searchQuery,
  onSearchChange,
  period,
  onPeriodChange,
}) => {
  return (
    <header className="h-20 border-b border-slate-200 flex items-center px-8 justify-between bg-white/80 backdrop-blur">
      <div className="flex items-center space-x-2" />

      <div className="flex-1 flex flex-col items-center">
        <div className="w-3/4 max-w-xl flex items-center bg-slate-50 border border-slate-200 rounded-full px-4 py-1 shadow-sm">
          <button className="mr-2 text-slate-400 hover:text-slate-600">
            <FaBars />
          </button>
          <input
            type="text"
            placeholder="Hinted search text"
            className="flex-1 bg-transparent outline-none text-sm text-slate-800 placeholder:text-slate-400"
            value={searchQuery}
            onChange={(e) => onSearchChange(e.target.value)}
          />
          <FaSearch className="text-slate-400" />
        </div>
        <div className="mt-2 flex items-center space-x-2 text-[11px] uppercase tracking-wide">
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
          <button className="ml-2 text-slate-500 hover:text-slate-700">
            <FaFilter />
          </button>
        </div>
      </div>

      <div>
        <FaUserCircle className="w-9 h-9 text-slate-500" />
      </div>
    </header>
  );
};

export default TopBar;
