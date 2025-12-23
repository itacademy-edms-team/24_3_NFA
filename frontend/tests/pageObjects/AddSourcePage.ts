import { Page } from '@playwright/test';

export class AddSourcePage {
  readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async goto() {
    await this.page.goto('/add-source');  
  }

  get nameInput() {
    return this.page.locator('input[placeholder="Например, Хабрахабр"]');
  }

  get sourceTypeSelect() {
    return this.page.locator('#sourceType');
  }

  get urlInput() {
    return this.page.locator('input[placeholder="https://example.com/rss"]');
  }

  get rssLimitInput() {
    return this.page.locator('#rssLimit');
  }

  get limitInput() {
    return this.page.locator('#rssLimit, #githubLimit, #redditLimit').first();
  }

  get categoryInput() {
    return this.page.locator('input[placeholder="Например, Технологии"]');
  }

  get submitButton() {
    return this.page.locator('button:has-text("Добавить источник")');
  }

  get errorDiv() {
    return this.page.locator('.bg-red-100');
  }

  async fillSourceDetails(name: string, url: string, limit?: number, category?: string, sourceType: string = 'rss') {
    if (name) await this.nameInput.fill(name);

    await this.sourceTypeSelect.selectOption(sourceType);

    await this.page.waitForTimeout(200);
    if (url && sourceType === 'rss') await this.urlInput.fill(url);
    if (limit !== undefined) await this.limitInput.fill(limit.toString());
    if (category) await this.categoryInput.fill(category);
  }

  async submitForm() {
    await this.submitButton.click();
  }

  async addSource(name: string, url: string, limit: number = 10, category?: string) {
    await this.fillSourceDetails(name, url, limit, category);
    await this.submitForm();

    await this.page.waitForResponse(response => response.url().includes('/api/sources') && response.status() === 200 );
  }

  async hasError() {
    return await this.errorDiv.isVisible();
  }

  async getErrorMessage() {
    return await this.errorDiv.textContent();
  }
}