import React, { createContext, useCallback, useContext, useMemo, useState } from 'react';
import type { NewsItem } from '../types/NewsItem';

const STORAGE_KEY = 'svodka_favorites';

interface FavoritesContextValue {
  favorites: NewsItem[];
  isFavorite: (id: number) => boolean;
  toggleFavorite: (item: NewsItem) => void;
  refresh: () => void;
}

const FavoritesContext = createContext<FavoritesContextValue | null>(null);

function loadFromStorage(): NewsItem[] {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return [];
    const parsed = JSON.parse(raw);
    return Array.isArray(parsed) ? parsed : [];
  } catch {
    return [];
  }
}

function saveToStorage(items: NewsItem[]) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(items));
}

export function FavoritesProvider({ children }: { children: React.ReactNode }) {
  const [favorites, setFavorites] = useState<NewsItem[]>(loadFromStorage);

  const refresh = useCallback(() => {
    setFavorites(loadFromStorage());
  }, []);

  const isFavorite = useCallback(
    (id: number) => favorites.some((f) => f.id === id),
    [favorites]
  );

  const toggleFavorite = useCallback((item: NewsItem) => {
    setFavorites((prev) => {
      const exists = prev.some((f) => f.id === item.id);
      const next = exists ? prev.filter((f) => f.id !== item.id) : [...prev, item];
      saveToStorage(next);
      return next;
    });
  }, []);

  const value = useMemo(
    () => ({ favorites, isFavorite, toggleFavorite, refresh }),
    [favorites, isFavorite, toggleFavorite, refresh]
  );

  return (
    <FavoritesContext.Provider value={value}>{children}</FavoritesContext.Provider>
  );
}

export function useFavorites(): FavoritesContextValue | null {
  return useContext(FavoritesContext);
}
