import { type NewsItem } from '../types/NewsItem';

const API_BASE_URL = 'http://localhost:5043';
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
    const urlParams = new URLSearchParams();

    urlParams.set('offset', String(params.offset || 0));
    urlParams.set('limit', String(params.limit || 10));

    if (params.searchQuery && params.searchQuery.trim().length > 0) {
      urlParams.set('q', params.searchQuery.trim());
    }

    if (params.period) {
      urlParams.set('period', params.period);
    }

    if (params.sources && params.sources.length > 0) {
      params.sources.forEach(source => urlParams.append('sources', String(source)));
    }

    if (params.categories && params.categories.length > 0) {
      params.categories.forEach(category => urlParams.append('categories', category));
    }

    if (params.sourceType) {
      urlParams.set('sourceType', params.sourceType);
    }

    const response = await fetch(`${API_BASE_URL}/api/news?${urlParams.toString()}`);
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    const data = await response.json();

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
    const response = await fetch(`${API_BASE_URL}/api/sources/filter-options`);
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    const options = await response.json();
    return options;
  } catch (error) {
    console.error("Error fetching filter options:", error);
    throw error;
  }
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
  eventTypes?: string[];
  limit: number;
  category?: string;
}

export interface RedditSourceConfiguration {
  subreddit: string;
  sortType: string;
  limit: number;
  category?: string;
}

export interface SourceData {
  name: string;
  type: string; 
  configuration: RssSourceConfiguration | GitHubSourceConfiguration | RedditSourceConfiguration;
  isActive: boolean;
}

export const updateSource = async (id: number, sourceData: SourceData): Promise<void> => {
  try {
    const response = await fetch(`${API_BASE_URL}/api/sources/${id}`, { 
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(sourceData),
    });

    if (!response.ok) {
      let errorMessage = `HTTP error! status: ${response.status}`;
      try {
        const errorBody = await response.text();
        if (errorBody) {
          errorMessage += `. Details: ${errorBody}`;
        }
      } catch (e) {
        console.error("Could not parse error response:", e);
      }
      throw new Error(errorMessage);
    }

    emitSourcesChanged();
  } catch (error) {
    console.error("Error updating source:", error);
    throw error;
  }
};

export const createSource = async (sourceData: SourceData): Promise<void> => {
  try {

    const requestBody = {
      name: sourceData.name,
      type: sourceData.type,
      configuration: sourceData.configuration, 
      isActive: sourceData.isActive,
    };

    const response = await fetch(`${API_BASE_URL}/api/sources`, { 
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(requestBody),
    });

    if (!response.ok) {
      let errorMessage = `HTTP error! status: ${response.status}`;
      try {
        const errorBody = await response.text();
        if (errorBody) {
          errorMessage += `. Details: ${errorBody}`;
        }
      } catch (e) {
        console.error("Could not parse error response:", e);
      }
      throw new Error(errorMessage);
    }

    emitSourcesChanged();

  } catch (error) {
    console.error("Error creating source:", error);
    throw error;
  }
};