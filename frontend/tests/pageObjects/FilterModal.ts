import { Page } from '@playwright/test';

export class FilterModal {
  readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  get modal() {
    return this.page.locator('div').filter({ has: this.page.locator('text=Период публикации') });
  }

  get categoryAccordion() {
    return this.page.locator('button:has-text("Категории")');
  }

  get sourceAccordion() {
    return this.page.locator('button:has-text("Каналы")');
  }

  get applyButton() {
    return this.page.locator('button:has-text("Применить")');
  }

  get resetButton() {
    return this.page.locator('button:has-text("Сброс")');
  }

  async clickCategoryAccordion() {
    await this.categoryAccordion.click();
  }

  async clickSourceAccordion() {
    await this.sourceAccordion.click();
  }

  async selectCategory(categoryName: string) {
    await this.clickCategoryAccordion();
    const categoryCheckbox = this.page.locator(`label:has-text("${categoryName}") input[type="checkbox"]`);
    await categoryCheckbox.check();
  }

  async selectSource(sourceName: string) {
    await this.clickSourceAccordion();
    const sourceCheckbox = this.page.locator(`label:has-text("${sourceName}") input[type="checkbox"]`);
    await sourceCheckbox.check();
  }

  async applyFilters() {
    await this.applyButton.click();
    await this.modal.waitFor({ state: 'hidden' });
  }

  async resetFilters() {
    await this.resetButton.click();
  }

  async isVisible() {
    return await this.modal.isVisible();
  }
}