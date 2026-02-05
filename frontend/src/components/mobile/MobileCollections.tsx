import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useFavorites } from '../../contexts/FavoritesContext';
import NewsCardGrid from './NewsCardGrid';
import FilterChips from './FilterChips';
import MobileHeader from './MobileHeader';
import { FaTimes } from 'react-icons/fa';
import type { NewsItem } from '../../types/NewsItem';
import { FaRss, FaGithub, FaReddit, FaHeart } from 'react-icons/fa';
import SafeImage from '../SafeImage';

const PostModal: React.FC<{ item: NewsItem; onClose: () => void }> = ({ item, onClose }) => {
  const favorites = useFavorites();
  const isFav = favorites?.isFavorite(item.id) ?? false;
  const formattedDate = new Date(item.publishedAtUtc).toLocaleString('ru-RU', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });

  const getSourceIcon = () => {
    const sourceType = item.sourceType?.toLowerCase();
    if (sourceType === 'github') return FaGithub;
    if (sourceType === 'reddit') return FaReddit;
    if (sourceType === 'rss') return FaRss;
    return null;
  };

  const SourceIcon = getSourceIcon();

  return (
    <div className="fixed inset-0 z-50 flex items-end justify-center bg-black/50" onClick={onClose}>
      <div 
        className="bg-white rounded-t-2xl w-full max-h-[85vh] overflow-y-auto"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="sticky top-0 bg-white border-b border-slate-200 px-4 py-3 flex items-center justify-between">
          <h3 className="text-[16px] font-semibold text-slate-900">Пост</h3>
          <button onClick={onClose} className="p-2 text-slate-600 active:bg-slate-100 rounded-lg">
            <FaTimes className="w-5 h-5" />
          </button>
        </div>

        <div className="p-4">
          {item.imageUrl && (
            <div className="mb-4 rounded-xl overflow-hidden bg-slate-50">
              <SafeImage
                src={item.imageUrl}
                alt={item.title}
                className="w-full h-auto max-h-[300px] object-contain"
              />
            </div>
          )}

          <h2 className="text-[18px] font-bold text-slate-900 mb-3">{item.title}</h2>

          <div className="flex items-center gap-2 mb-4">
            {SourceIcon && <SourceIcon className="w-4 h-4 text-slate-500" />}
            <span className="text-xs text-slate-500">{formattedDate}</span>
          </div>

          {item.description && (
            <div className="text-slate-700 text-sm leading-relaxed whitespace-pre-wrap mb-4">
              {item.description.length > 500 
                ? item.description.substring(0, 500) + '...' 
                : item.description}
            </div>
          )}

          <div className="flex items-center gap-3 pt-4 border-t border-slate-200">
            {favorites && (
              <button
                onClick={() => favorites.toggleFavorite(item)}
                className="p-2 rounded-full hover:bg-slate-100"
              >
                <FaHeart className={`w-5 h-5 ${isFav ? 'text-red-500 fill-red-500' : 'text-slate-400'}`} />
              </button>
            )}
            <a
              href={item.link}
              target="_blank"
              rel="noopener noreferrer"
              className="ml-auto px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700"
            >
              Открыть источник
            </a>
          </div>
        </div>
      </div>
    </div>
  );
};

const MobileCollections: React.FC = () => {
  const favorites = useFavorites();
  const navigate = useNavigate();
  const [selectedFilter, setSelectedFilter] = useState<string | number | undefined>();
  const [selectedPost, setSelectedPost] = useState<NewsItem | null>(null);

  const favoritesList = favorites?.favorites ?? [];

  const filters = [
    { id: 'all', label: 'Все' },
    { id: 'github', label: 'GitHub' },
    { id: 'reddit', label: 'Reddit' },
    { id: 'rss', label: 'RSS' },
  ];

  const filteredFavorites = selectedFilter && selectedFilter !== 'all'
    ? favoritesList.filter(item => item.sourceType?.toLowerCase() === selectedFilter)
    : favoritesList;

  const handleCardClick = (item: NewsItem) => {
    setSelectedPost(item);
  };

  if (favoritesList.length === 0) {
    return (
      <div>
        <MobileHeader title="Избранное" showSearch={false} />
        <div className="flex flex-col items-center justify-center pt-20 px-4">
          <p className="text-[16px] font-semibold text-[#1A1A1A] mb-2">Избранное пусто</p>
          <p className="text-[14px] text-[#6B6B6B] text-center mb-4">
            Отмечайте понравившиеся посты сердечком
          </p>
          <button
            onClick={() => navigate('/')}
            className="px-6 py-2.5 bg-[#6B5B95] text-white text-[14px] font-medium rounded-full"
          >
            Перейти к новостям
          </button>
        </div>
      </div>
    );
  }

  return (
    <div>
      <MobileHeader title="Избранное" showSearch={false} />
      <FilterChips
        chips={filters}
        selectedId={selectedFilter}
        onSelect={setSelectedFilter}
      />

      <div className="px-4 py-3">
        <div className="grid grid-cols-2 gap-3">
          {filteredFavorites.map((item) => (
            <NewsCardGrid
              key={item.id}
              newsItem={item}
              onClick={() => handleCardClick(item)}
            />
          ))}
        </div>
      </div>

      {selectedPost && <PostModal item={selectedPost} onClose={() => setSelectedPost(null)} />}
    </div>
  );
};

export default MobileCollections;
