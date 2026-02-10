import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useFavorites } from '../contexts/FavoritesContext';
import NewsItemCard from './NewsItemCard';
import type { NewsItem } from '../types/NewsItem';

const FavoritesPage: React.FC = () => {
  const ctx = useFavorites();
  const favorites = ctx?.favorites ?? [];
  const navigate = useNavigate();

  if (favorites.length === 0) {
    return (
      <div className="max-w-4xl mx-auto mt-16 flex flex-col items-center justify-center px-4">
        <div className="text-center space-y-4">
          <h2 className="text-xl font-semibold text-slate-900">Избранное пусто</h2>
          <p className="text-sm text-slate-500">
            Отмечайте понравившиеся посты сердечком, и они появятся здесь.
          </p>
          <button
            type="button"
            onClick={() => navigate('/')}
            className="mt-2 inline-flex items-center px-4 py-2 rounded-full bg-slate-900 text-white text-sm font-medium hover:bg-slate-800 transition-colors"
          >
            Перейти к новостям
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto space-y-4">
      <h2 className="text-lg font-semibold text-slate-900">Избранное ({favorites.length})</h2>
      {favorites.map((item: NewsItem) => (
        <NewsItemCard key={item.id} newsItem={item} />
      ))}
    </div>
  );
};

export default FavoritesPage;
