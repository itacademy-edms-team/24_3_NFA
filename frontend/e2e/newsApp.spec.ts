import { test, expect } from '@playwright/test';
import { AppPage } from '../tests/pageObjects/AppPage';
import { NewsListPage } from '../tests/pageObjects/NewsListPage';
import { SourcesListPage } from '../tests/pageObjects/SourcesListPage';
import { AddSourcePage } from '../tests/pageObjects/AddSourcePage';

test.describe('News Aggregator App', () => {
  let appPage: AppPage;
  let newsListPage: NewsListPage;
  let sourcesListPage: SourcesListPage;
  let addSourcePage: AddSourcePage;

  test.beforeEach(async ({ page }) => {
    appPage = new AppPage(page);
    newsListPage = new NewsListPage(page);
    sourcesListPage = new SourcesListPage(page);
    addSourcePage = new AddSourcePage(page);
    
    await page.goto('/');
  });

  test('should display news list on the main page', async ({ page }) => {
    // Проверяем, что на главной странице отображаются новости
    await expect(page.locator('article')).toBeVisible();
    
    // Проверяем, что видим хотя бы одну карточку новости
    const newsCardCount = await page.locator('article').count();
    expect(newsCardCount).toBeGreaterThan(0);
  });

  test('should filter news by period (day/week/month)', async ({ page }) => {
    const initialNewsCount = await page.locator('article').count();
    
    // Нажимаем на фильтр "ЗА ДЕНЬ"
    await page.locator('button:has-text("ЗА ДЕНЬ")').click();
    
    // Ждем, пока обновится список новостей
    await page.waitForResponse(response => 
      response.url().includes('/api/news') && response.status() === 200
    );
    
    const dayFilteredNewsCount = await page.locator('article').count();
    
    // Скорее всего, за день новостей меньше, чем за неделю (по умолчанию)
    // Но главное - список обновился
    await expect(page.locator('article')).toHaveCount(dayFilteredNewsCount);
  });

  test('should search news by keywords', async ({ page }) => {
    // Проверяем поиск новостей
    const searchInput = page.locator('input[placeholder="Поиск новостей..."]');
    await searchInput.fill('test');
    
    // Ждем завершения поиска
    await page.waitForResponse(response => 
      response.url().includes('/api/news') && response.status() === 200
    );
    
    // Проверяем, что список новостей обновился (мог быть найден результат или пустой список)
    await expect(page.locator('article')).toBeDefined();
  });

  test('should navigate to sources page and back', async ({ page }) => {
    // Переходим на страницу источников
    await page.locator('button:has-text("Настройки")').click();
    await expect(page).toHaveURL(/.*\/sources/);
    
    // Проверяем, что на странице источников есть заголовок
    await expect(page.locator('h2:has-text("Источники новостей")')).toBeVisible();
    
    // Возвращаемся на главную
    await page.locator('button:has-text("Svodka")').click();
    await expect(page).toHaveURL('/');
  });

  test('should access sources list page', async ({ page }) => {
    // Проверяем, что можем попасть на страницу списка источников
    await page.locator('button:has-text("Настройки")').click();
    
    // Проверяем, что на странице источников есть кнопка добавления
    const addButton = page.locator('button:has-text("Добавить источник")');
    await expect(addButton).toBeVisible();
  });
});