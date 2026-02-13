export interface NewsItem {
  id: number;
  title: string;
  description: string;
  link: string;
  publishedAtUtc: string;
  sourceId: number;
  sourceItemId: string;
  author?: string;
  imageUrl?: string;
  category?: string;
  indexedAtUtc: string;
  sourceType?: string;
  metadata?: string;
}