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

export interface NewsSourceListItem {
  id: number;
  name: string;
  type: string;
  configuration: string;
  isActive: boolean;
  lastPolledAtUtc: string | null;
  lastErrorAtUtc: string | null;
  lastError: string | null;
}

export interface SourceSyncResult {
  sourceId: number;
  itemsAdded: number;
  lastPolledAtUtc: string | null;
  lastError: string | null;
  lastErrorAtUtc: string | null;
}

export const fetchLatestNews = async (params: FilterParams = {}): Promise<NewsResponse> => {
  try {
    const queryParams = new URLSearchParams();

    if (params.offset !== undefined) queryParams.append('offset', String(params.offset));
    if (params.limit !== undefined) queryParams.append('limit', String(params.limit));
    if (params.searchQuery) queryParams.append('q', params.searchQuery);
    if (params.period) queryParams.append('period', params.period);
    if (params.sourceType) queryParams.append('sourceType', params.sourceType);
    params.sources?.forEach((sourceId) => queryParams.append('sources', String(sourceId)));
    params.tags?.forEach((tag) => queryParams.append('tags', tag));

    const response = await api.get('/api/news', { params: queryParams });
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
  const response = await api.get('/api/sources/filter-options');
  return response.data;
};

export const fetchSources = async (): Promise<NewsSourceListItem[]> => {
  const response = await api.get<NewsSourceListItem[]>('/api/sources');
  return response.data;
};

export const fetchSourceById = async (id: number): Promise<NewsSourceListItem> => {
  const response = await api.get<NewsSourceListItem>(`/api/sources/${id}`);
  return response.data;
};

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

export interface TumblrSourceConfiguration {
  blogName: string;
  limit: number;
  category?: string;
}

export interface VkSourceConfiguration {
  domain?: string;
  ownerId?: number;
  limit: number;
  category?: string;
}

export type SourceConfiguration =
  | RssSourceConfiguration
  | GitHubSourceConfiguration
  | RedditSourceConfiguration
  | TumblrSourceConfiguration
  | VkSourceConfiguration;

export interface SourceData {
  name: string;
  type: string;
  configuration: SourceConfiguration;
  isActive: boolean;
  tags?: string[];
}

export const updateSource = async (id: number, sourceData: SourceData): Promise<void> => {
  await api.put(`/api/sources/${id}`, sourceData);
  emitSourcesChanged();
};

export const createSource = async (sourceData: SourceData): Promise<void> => {
  await api.post('/api/sources', {
    name: sourceData.name,
    type: sourceData.type,
    configuration: sourceData.configuration,
    isActive: sourceData.isActive,
    tags: sourceData.tags ?? [],
  });
  emitSourcesChanged();
};

export const deleteSource = async (id: number): Promise<void> => {
  await api.delete(`/api/sources/${id}`);
  emitSourcesChanged();
};

export const setSourceActive = async (id: number, isActive: boolean): Promise<NewsSourceListItem> => {
  const response = await api.patch<NewsSourceListItem>(`/api/sources/${id}/active`, { isActive });
  emitSourcesChanged();
  return response.data;
};

export const syncSource = async (id: number): Promise<SourceSyncResult> => {
  const response = await api.post<SourceSyncResult>(`/api/sources/${id}/sync`);
  emitSourcesChanged();
  return response.data;
};
