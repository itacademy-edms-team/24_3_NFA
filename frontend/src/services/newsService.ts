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
  tags?: string[];
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

export const fetchFilterOptions = async (): Promise<{ sources: Array<{ id: number, name: string }>, tags: string[] }> => {
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
  configuration: RssSourceConfiguration | GitHubSourceConfiguration | RedditSourceConfiguration;
  isActive: boolean;
  tags?: string[];
}

export interface RssSourceConfiguration {
  url: string;
  limit: number;
  category?: string;
}

export interface GitHubSourceConfiguration {
  repositoryOwner: string;
  repositoryName: string;
  token?: string;
  limit: number;
  eventTypes?: string[];
  category?: string;
}

export interface RedditSourceConfiguration {
  subreddit: string;
  sortType: string;
  limit: number;
  category?: string;
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
      tags: sourceData.tags ?? [],
    });
    emitSourcesChanged();
  } catch (error) {
    console.error("Error creating source:", error);
    throw error;
  }
};