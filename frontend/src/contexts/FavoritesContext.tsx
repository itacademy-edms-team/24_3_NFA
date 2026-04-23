import React, { createContext, useCallback, useContext, useMemo, useState, useEffect } from 'react';
import type { NewsItem } from '../types/NewsItem';
import { useAuth } from './AuthContext';

interface FavoritesContextValue {
  favorites: NewsItem[];
  isFavorite: (id: number) => boolean;
  toggleFavorite: (item: NewsItem) => void;
  refresh: () => void;
}

const FavoritesContext = createContext<FavoritesContextValue | null>(null);

export function FavoritesProvider({ children }: { children: React.ReactNode }) {
  const { user } = useAuth();
  const STORAGE_KEY = useMemo(() => user?.email ? `svodka_favorites_${user.email}` : 'svodka_favorites_guest', [user]);
  
  const [favorites, setFavorites] = useState<NewsItem[]>(() => {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (!raw) return [];
      const parsed = JSON.parse(raw);
      return Array.isArray(parsed) ? parsed : [];
    } catch {
      return [];
    }
  });

  useEffect(() => {
      const raw = localStorage.getItem(STORAGE_KEY);
      setFavorites(raw ? JSON.parse(raw) : []);
  }, [STORAGE_KEY]);

  const saveToStorage = useCallback((items: NewsItem[]) => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(items));
  }, [STORAGE_KEY]);

  const refresh = useCallback(() => {
    const raw = localStorage.getItem(STORAGE_KEY);
    setFavorites(raw ? JSON.parse(raw) : []);
  }, [STORAGE_KEY]);

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
  }, [saveToStorage]);

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
