import { type NewsItem } from '../types/NewsItem';
import api from './api';

export const SOURCES_CHANGED_EVENT = 'sources:changed';

export const emitSourcesChanged = () => {
  if (typeof window !== 'undefined') {
    window.dispatchEvent(new Event(SOURCES_CHANGED_EVENT));
  }
};

export type PeriodFilter = 'day' | 'week' | 'month' | '';

export interface FilterParams {
  offset?: number;
  limit?: number;
  searchQuery?: string;
  period?: PeriodFilter;
  sources?: number[];
  categories?: string[];
  sourceType?: string;
}

export interface NewsResponse {
    items: NewsItem[];
    hasMore: boolean;
    offset: number;
    limit: number;
}

export const fetchLatestNews = async (params: FilterParams = {}): Promise<NewsResponse> => {
  try {
    const response = await api.get('/api/news', { params });
    const data = response.data;

    if (Array.isArray(data)) {
        return {
            items: data,
            hasMore: data.length === (params.limit || 10),
            offset: params.offset || 0,
            limit: params.limit || 10
        };
    }

    return data as NewsResponse;
  } catch (error) {
    console.error("Error fetching news:", error);
    throw error;
  }
};

export const fetchFilterOptions = async (): Promise<{ sources: Array<{ id: number, name: string }>, categories: string[] }> => {
  try {
    const response = await api.get('/api/sources/filter-options');
    return response.data;
  } catch (error) {
    console.error("Error fetching filter options:", error);
    throw error;
  }
};

export interface SourceData {
  name: string;
  type: string; 
  configuration: any;
  isActive: boolean;
}

export const updateSource = async (id: number, sourceData: SourceData): Promise<void> => {
  try {
    await api.put(`/api/sources/${id}`, sourceData);
    emitSourcesChanged();
  } catch (error) {
    console.error("Error updating source:", error);
    throw error;
  }
};

export const createSource = async (sourceData: SourceData): Promise<void> => {
  try {
    await api.post('/api/sources', {
      name: sourceData.name,
      type: sourceData.type,
      configuration: sourceData.configuration, 
      isActive: sourceData.isActive,
    });
    emitSourcesChanged();
  } catch (error) {
    console.error("Error creating source:", error);
    throw error;
  }
};