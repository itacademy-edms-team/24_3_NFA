import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import {
  FaGithub,
  FaReddit,
  FaRss,
  FaHeart,
  FaBars,
  FaCog,
  FaEllipsisV,
} from 'react-icons/fa';
import { SOURCES_CHANGED_EVENT } from '../services/newsService';

interface SidebarItem {
  title: string;
  icon: React.ComponentType<{ className?: string }>;
  type?: string;
  id?: number;
  route?: string;
}

interface SidebarGroup {
  name: string;
  items: SidebarItem[];
}

interface SidebarProps {
  collapsed?: boolean;
  onToggle?: () => void;
  onSourceTypeChange?: (type: string | undefined) => void;
  onLogoClick?: () => void;
}

interface Source {
  id: number;
  name: string;
  type: string;
  configuration: string;
  isActive: boolean;
}

const Sidebar: React.FC<SidebarProps> = ({ 
  collapsed = false, 
  onToggle,
  onSourceTypeChange,
  onLogoClick 
}) => {
  const navigate = useNavigate();
  const location = useLocation();
  const [rssSources, setRssSources] = useState<Source[]>([]);
  const [kebabMenuOpen, setKebabMenuOpen] = useState<number | null>(null);
  const [rssMenuOpen, setRssMenuOpen] = useState(false);

  const loadRssSources = useCallback(async () => {
    try {
      const response = await fetch('http://localhost:5043/api/sources');
      if (response.ok) {
        const sources: Source[] = await response.json();
        setRssSources(sources.filter(s => s.type.toLowerCase() === 'rss'));
      }
    } catch (error) {
      console.error('Error loading RSS sources:', error);
    }
  }, []);

  useEffect(() => {
    loadRssSources();
  }, [loadRssSources, location.pathname]);

  useEffect(() => {
    if (typeof window === 'undefined') {
      return;
    }

    window.addEventListener(SOURCES_CHANGED_EVENT, loadRssSources);
    return () => window.removeEventListener(SOURCES_CHANGED_EVENT, loadRssSources);
  }, [loadRssSources]);

  const handleItemClick = (item: SidebarItem) => {
    if (item.route === '/favorites') {
      onSourceTypeChange?.(undefined);
      navigate('/favorites');
      return;
    }
    if (item.type && (item.type === 'rss' || item.type === 'github' || item.type === 'reddit')) {
      onSourceTypeChange?.(item.type);
      navigate('/');
    } else {
      onSourceTypeChange?.(undefined);
      navigate('/');
    }
  };

  const handleRssKebabClick = (e: React.MouseEvent | React.KeyboardEvent) => {
    e.stopPropagation();
    setRssMenuOpen(!rssMenuOpen);
  };

  const handleSourceKebabClick = (e: React.MouseEvent | React.KeyboardEvent, sourceId: number) => {
    e.stopPropagation();
    setKebabMenuOpen(kebabMenuOpen === sourceId ? null : sourceId);
  };

  const handleModalBackdropClick = (e: React.MouseEvent<HTMLDivElement>) => {
    if (e.target === e.currentTarget) {
      setRssMenuOpen(false);
      setKebabMenuOpen(null);
    }
  };

  useEffect(() => {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        setRssMenuOpen(false);
        setKebabMenuOpen(null);
      }
    };

    if (rssMenuOpen) {
      document.addEventListener('keydown', handleEscape);
    }

    return () => {
      document.removeEventListener('keydown', handleEscape);
    };
  }, [rssMenuOpen]);

  const handleEditSource = (sourceId: number) => {
    navigate(`/edit-source/${sourceId}`);
    setKebabMenuOpen(null);
  };

  const sidebarData: SidebarGroup[] = [
    {
      name: 'Источники',
      items: [
        { title: 'GitHub источники', icon: FaGithub, type: 'github' },
        { title: 'Reddit Источники', icon: FaReddit, type: 'reddit' },
        { title: 'RSS каналы', icon: FaRss, type: 'rss' },
      ],
    },
    {
      name: 'Избранное',
      items: [
        { title: 'Избранное', icon: FaHeart, route: '/favorites' },
      ],
    },
  ];

  return (
    <div
      className={`h-full bg-white text-slate-900 flex flex-col border-r border-slate-200 transition-[width] duration-300 ${
        collapsed ? 'w-14' : 'w-56'
      } shadow-sm`}
    >
      <div className="h-20 px-4 flex items-center gap-2 border-b border-slate-200">
        {onToggle && (
          <button
            onClick={onToggle}
            className="p-2 rounded-lg hover:bg-slate-100 focus:outline-none transition-colors text-slate-600"
            aria-label="Toggle sidebar"
          >
            <FaBars />
          </button>
        )}
        {!collapsed && (
          <button
            onClick={onLogoClick}
            className="text-xl font-black tracking-tight hover:text-indigo-600 transition-colors cursor-pointer"
          >
            Svodka
          </button>
        )}
      </div>

      <div className="flex-grow p-3 overflow-y-auto space-y-4 bg-white">
        {sidebarData.map((group) => (
          <div key={group.name} className="space-y-2">
            <p
              className={`text-xs font-semibold text-slate-400 uppercase transition-opacity duration-200 ${
                collapsed ? 'opacity-0' : 'opacity-100'
              }`}
            >
              {group.name}
            </p>
            <ul className="space-y-1">
              {group.items.map((item) => (
                <li key={item.title} className="relative group">
                  <button
                    onClick={() => handleItemClick(item)}
                    className="flex items-center justify-between w-full p-2 text-sm font-medium text-slate-600 rounded-xl hover:bg-slate-100 transition-colors"
                  >
                    <div className="flex items-center">
                      <item.icon className="text-lg text-slate-500" />
                      <span
                        className={`transition-all duration-200 ${
                          collapsed ? 'opacity-0 w-0' : 'opacity-100 w-auto'
                        }`}
                        style={{ marginLeft: collapsed ? 0 : '0.75rem' }}
                      >
                        {item.title}
                      </span>
                    </div>
                    {!collapsed && item.type === 'rss' && (
                      <span
                        role="button"
                        tabIndex={0}
                        onClick={handleRssKebabClick}
                        onKeyDown={(e) => {
                          if (e.key === 'Enter' || e.key === ' ') {
                            e.preventDefault();
                            handleRssKebabClick(e);
                          }
                        }}
                        className="opacity-0 group-hover:opacity-100 p-1 hover:bg-slate-200 rounded transition-opacity cursor-pointer"
                      >
                        <FaEllipsisV className="text-xs text-slate-500" />
                      </span>
                    )}
                  </button>
                  
                </li>
              ))}
            </ul>
          </div>
        ))}
      </div>

      <div className="p-3 border-t border-slate-200">
        <button
          onClick={() => navigate('/sources')}
          className="flex items-center w-full p-2 text-sm font-medium text-slate-600 rounded-xl hover:bg-slate-100 transition-colors"
        >
          <FaCog className="text-lg text-slate-500" />
          <span
            className={`ml-3 whitespace-nowrap transition-all duration-200 ${
              collapsed ? 'opacity-0 w-0' : 'opacity-100 w-auto'
            }`}
          >
            Настройки
          </span>
        </button>
      </div>

      {/* Модальное окно RSS каналов */}
      {rssMenuOpen && (
        <div
          className="fixed inset-0 flex items-center justify-center z-[9999] pointer-events-none"
          onClick={handleModalBackdropClick}
        >
          <div
            className="bg-white rounded-2xl shadow-2xl w-full max-w-md mx-4 max-h-[80vh] overflow-hidden flex flex-col pointer-events-auto"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="px-6 py-4 border-b border-slate-200 flex items-center justify-between">
              <h3 className="text-lg font-semibold text-slate-900">RSS каналы</h3>
              <button
                onClick={() => {
                  setRssMenuOpen(false);
                  setKebabMenuOpen(null);
                }}
                className="text-slate-400 hover:text-slate-600 transition-colors"
                aria-label="Закрыть"
              >
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
            <div className="overflow-y-auto flex-1">
              {rssSources.length === 0 ? (
                <div className="px-6 py-8 text-center text-slate-500">
                  <p className="text-sm">Нет добавленных RSS каналов</p>
                  <button
                    onClick={() => {
                      setRssMenuOpen(false);
                      navigate('/add-source');
                    }}
                    className="mt-4 px-4 py-2 text-sm font-medium text-indigo-600 hover:text-indigo-700"
                  >
                    Добавить канал
                  </button>
                </div>
              ) : (
                <div className="py-2">
                  {rssSources.map((source) => (
                    <div key={source.id} className="relative group/item">
                      <button
                        onClick={() => {
                          onSourceTypeChange?.('rss');
                          navigate('/');
                          setRssMenuOpen(false);
                        }}
                        className="w-full text-left px-6 py-3 text-sm text-slate-700 hover:bg-slate-50 flex items-center justify-between transition-colors"
                      >
                        <span className="font-medium">{source.name}</span>
                        <span
                          role="button"
                          tabIndex={0}
                          onClick={(e) => handleSourceKebabClick(e, source.id)}
                          onKeyDown={(e) => {
                            if (e.key === 'Enter' || e.key === ' ') {
                              e.preventDefault();
                              handleSourceKebabClick(e, source.id);
                            }
                          }}
                          className="p-2 hover:bg-slate-200 rounded-full transition-colors cursor-pointer"
                        >
                          <FaEllipsisV className="text-xs text-slate-500" />
                        </span>
                      </button>
                      {kebabMenuOpen === source.id && (
                        <div className="absolute right-0 top-full mt-1 mr-4 bg-white rounded-lg shadow-xl border border-slate-200 z-50 min-w-[140px]">
                          <button
                            onClick={() => {
                              handleEditSource(source.id);
                              setRssMenuOpen(false);
                            }}
                            className="w-full text-left px-4 py-2 text-sm text-slate-700 hover:bg-slate-100 transition-colors"
                          >
                            Редактировать
                          </button>
                        </div>
                      )}
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default Sidebar;

