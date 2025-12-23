import { Page } from '@playwright/test';
import { NewsListPage } from './NewsListPage';
import { AddSourcePage } from './AddSourcePage';
import { SourcesListPage } from './SourcesListPage';

export class AppPage {
  readonly page: Page;
  readonly newsListPage: NewsListPage;
  readonly addSourcePage: AddSourcePage;
  readonly sourcesListPage: SourcesListPage;

  constructor(page: Page) {
    this.page = page;
    this.newsListPage = new NewsListPage(page);
    this.addSourcePage = new AddSourcePage(page);
    this.sourcesListPage = new SourcesListPage(page);
  }

  get sidebar() {
    return this.page.locator('.bg-white').first();
  }

  get telegramButton() {
    return this.page.locator('button:has-text("Telegram")');
  }

  get vkButton() {
    return this.page.locator('button:has-text("ВКонтакте")');
  }

  get rssButton() {
    return this.page.locator('button:has-text("RSS каналы")');
  }

  get settingsButton() {
    return this.page.locator('button:has-text("Настройки")');
  }

  get logoButton() {
    return this.page.locator('button:has-text("Svodka")');
  }

  // Методы навигации
  async navigateToSources() {
    await this.settingsButton.click();
  }

  async navigateToAddSource() {
    await this.navigateToSources();
    const addSourceButton = this.page.locator('button:has-text("Добавить источник")');
    await addSourceButton.click();
  }

  async navigateToNewsList() {
    await this.logoButton.click();
  }

  async goTo() {
    await this.page.goto('/'); 
  }

  async isSidebarVisible() {
    return await this.sidebar.isVisible();
  }
}