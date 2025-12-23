import { Page } from '@playwright/test';

export class SourcesListPage {
  readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async goto() {
    await this.page.goto('/sources');  
  }

  get addSourceButton() {
    return this.page.locator('button:has-text("Добавить источник")');
  }

  get sourcesTable() {
    return this.page.locator('table');
  }

  get sourceRows() {
    return this.page.locator('tbody tr');
  }

  get deleteButtons() {
    return this.page.locator('button:has-text("Удалить")');
  }

  get editButtons() {
    return this.page.locator('button:has-text("Редактировать")');
  }

  get readNewsButtons() {
    return this.page.locator('button:has-text("Читать новости")');
  }

  get noSourcesMessage() {
    return this.page.locator('text=Нет добавленных источников');
  }

  async clickAddSourceButton() {
    await this.addSourceButton.click();
  }

  async getSourcesCount() {
    return await this.sourceRows.count();
  }

  async deleteFirstSource() {
    const deleteButton = this.deleteButtons.first();
    await deleteButton.click();
    
    const confirmButton = this.page.locator('button:has-text("Удалить")').last();
    await confirmButton.click();
    
    await this.page.waitForResponse(response => 
      response.url().includes('/api/sources') && response.status() === 200
    );
  }

  async editFirstSource() {
    const editButton = this.editButtons.first();
    await editButton.click();
  }

  async readNewsFromFirstSource() {
    const readNewsButton = this.readNewsButtons.first();
    await readNewsButton.click();
  }

  async waitForSources() {
    await this.page.waitForSelector('tbody tr', { state: 'visible' });
  }

  async isNoSourcesMessageVisible() {
    return await this.noSourcesMessage.isVisible();
  }
}