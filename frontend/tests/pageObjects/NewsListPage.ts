import { Page } from '@playwright/test';

export class NewsListPage {
  readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async goto() {
    await this.page.goto('/');  
  }

  get searchInput() {
    return this.page.locator('input[placeholder="Поиск новостей..."]');
  }

  get dayFilterButton() {
    return this.page.locator('button:has-text("ЗА ДЕНЬ")');
  }

  get weekFilterButton() {
    return this.page.locator('button:has-text("ЗА НЕДЕЛЮ")');
  }

  get monthFilterButton() {
    return this.page.locator('button:has-text("ЗА МЕСЯЦ")');
  }

  get filterButton() {
    return this.page.locator('button').filter({ has: this.page.locator('svg') }).nth(0);
  }

  get newsCards() {
    return this.page.locator('article');
  }

  get emptyStateButton() {
    return this.page.locator('button:has-text("Добавить канал")');
  }

  async searchNews(query: string) {
    await this.searchInput.fill(query);
    await this.page.waitForResponse(response => 
      response.url().includes('/api/news') && response.status() === 200
    );
  }

  async clickDayFilter() {
    await this.dayFilterButton.click();
    await this.page.waitForResponse(response => 
      response.url().includes('/api/news') && response.status() === 200
    );
  }

  async clickWeekFilter() {
    await this.weekFilterButton.click();
    await this.page.waitForResponse(response => 
      response.url().includes('/api/news') && response.status() === 200
    );
  }

  async clickMonthFilter() {
    await this.monthFilterButton.click();
    await this.page.waitForResponse(response => 
      response.url().includes('/api/news') && response.status() === 200
    );
  }

  async openFilters() {
    await this.filterButton.click();
  }

  async waitForNewsCards() {
    await this.page.waitForSelector('article', { state: 'visible' });
  }

  async getNewsCardCount() {
    return await this.newsCards.count();
  }

  async clickAddChannelButton() {
    await this.emptyStateButton.click();
  }
}