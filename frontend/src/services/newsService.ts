import { type NewsItem } from '../types/NewsItem';

const API_BASE_URL = 'http://localhost:7043';

export type PeriodFilter = 'day' | 'week' | 'month' | undefined;

export const fetchLatestNews = async (
  limit: number = 10,
  searchQuery?: string,
  period?: PeriodFilter,
): Promise<NewsItem[]> => {
  try {
    const params = new URLSearchParams();
    params.set('limit', String(limit));

    if (searchQuery && searchQuery.trim().length > 0) {
      params.set('q', searchQuery.trim());
    }

    if (period) {
      params.set('period', period);
    }

    const response = await fetch(`${API_BASE_URL}/api/news?${params.toString()}`);
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    const news: NewsItem[] = await response.json();
    return news;
  } catch (error) {
    console.error("Error fetching news:", error);
    throw error; 
  }
};

export interface SourceData {
  name: string;
  type: string; 
  configuration: string;
  isActive: boolean;
}

export const createSource = async (sourceData: SourceData): Promise<void> => {
  try {
    const response = await fetch(`${API_BASE_URL}/api/sources`, { 
      method: 'POST',
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

  } catch (error) {
    console.error("Error creating source:", error);
    throw error;
  }
};