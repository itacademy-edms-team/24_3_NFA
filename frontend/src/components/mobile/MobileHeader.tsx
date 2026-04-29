import React, { useState } from 'react';
import { FaSearch, FaEllipsisV, FaTimes } from 'react-icons/fa';

interface MobileHeaderProps {
  title?: string;
  showSearch?: boolean;
  showMenu?: boolean;
  onSearch?: (query: string) => void;
  onMenuClick?: () => void;
}

const MobileHeader: React.FC<MobileHeaderProps> = ({
  title = 'Svodka',
  showSearch = true,
  showMenu = true,
  onSearch,
  onMenuClick,
}) => {
  const [searchOpen, setSearchOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSearch?.(searchQuery);
    setSearchOpen(false);
  };

  const handleSearchClose = () => {
    setSearchOpen(false);
    setSearchQuery('');
  };

  if (searchOpen) {
    return (
      <header className="sticky top-0 z-50 bg-slate-200">
        <form onSubmit={handleSearchSubmit} className="px-4 h-14 flex items-center gap-2">
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder="Поиск..."
            className="flex-1 px-3 py-2 bg-white rounded-lg text-[14px] border border-slate-300 focus:outline-none focus:border-slate-900"
            autoFocus
          />
          <button
            type="submit"
            className="px-3 py-2 bg-slate-900 text-white rounded-lg text-[13px] font-medium"
          >
            Найти
          </button>
          <button
            type="button"
            onClick={handleSearchClose}
            className="p-2 text-slate-500"
          >
            <FaTimes className="w-5 h-5" />
          </button>
        </form>
      </header>
    );
  }

  return (
    <header className="sticky top-0 z-50 bg-slate-200 backdrop-blur-sm bg-opacity-90">
      <div className="px-4 h-14 flex items-center justify-between">
        <h1 className="text-[20px] font-bold text-slate-900">{title}</h1>
        <div className="flex items-center gap-3">
          {showSearch && (
            <button
              onClick={() => setSearchOpen(true)}
              className="p-2 text-slate-500 hover:text-slate-900 active:bg-slate-300 rounded-lg transition-colors"
            >
              <FaSearch className="w-5 h-5" />
            </button>
          )}
          {showMenu && (
            <button
              onClick={onMenuClick}
              className="p-2 text-slate-500 hover:text-slate-900 active:bg-slate-300 rounded-lg transition-colors"
              aria-label="Меню"
            >
              <FaEllipsisV className="w-4 h-4" />
            </button>
          )}
        </div>
      </div>
    </header>
  );
};

export default MobileHeader;
