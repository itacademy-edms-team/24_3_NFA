import { test, expect } from '@playwright/test';
import { AddSourcePage } from '../tests/pageObjects/AddSourcePage';
import { SourcesListPage } from '../tests/pageObjects/SourcesListPage';

test.describe('Add Source Functionality', () => {
  let addSourcePage: AddSourcePage;
  let sourcesListPage: SourcesListPage;

  test.beforeEach(async ({ page }) => {
    addSourcePage = new AddSourcePage(page);
    sourcesListPage = new SourcesListPage(page);
  });

  test('should add a new RSS source successfully', async ({ page }) => {
    // Переходим на страницу добавления источника
    await page.goto('/sources');
    await page.locator('button:has-text("Добавить источник")').click();

    // Заполняем форму добавления источника
    await addSourcePage.fillSourceDetails(
      'Test RSS Source',
      'https://rss.example.com/feed',
      10,
      'Test Category'
    );

    // Отправляем форму
    await addSourcePage.submitForm();

    // Проверяем, что появилось сообщение об успехе или произошел редирект
    // В реальном приложении может появиться модальное окно или алерт
    await page.waitForTimeout(1000); // даем время на обработку запроса

    // Возвращаемся к списку источников
    await page.goto('/sources');

    // Проверяем, что источник появился в списке
    await expect(page.locator('td:has-text("Test RSS Source")')).toBeVisible();
  });

  test('should show validation errors for invalid input', async ({ page }) => {
    // Переходим на страницу добавления источника
    await page.goto('/add-source');

    // Пытаемся отправить пустую форму
    await addSourcePage.submitForm();

    // Проверяем, что появилась ошибка валидации (ожидаем ошибку на поле имени)
    // Так как в приложении используются стандартные HTML5 валидации
    const nameInput = page.locator('#name');
    await expect(nameInput).toBeFocused(); // поле с ошибкой будет в фокусе

    // Проверяем, что форма не отправилась
    await expect(page).toHaveURL('/add-source');
  });

  test('should handle invalid URL error', async ({ page }) => {
    // Переходим на страницу добавления источника
    await page.goto('/add-source');

    // Заполняем форму с неверным URL
    await page.locator('#name').fill('Test Source');
    // Убеждаемся, что выбран тип RSS
    await page.locator('#sourceType').selectOption('rss');
    await page.locator('input[placeholder="https://example.com/rss"]').fill('invalid-url');

    // Отправляем форму
    await page.locator('button:has-text("Добавить источник")').click();

    // Ждем появления сообщения об ошибке
    await page.waitForSelector('.bg-red-100', { state: 'visible' });

    // Проверяем, что сообщение об ошибке отображается
    const errorDiv = page.locator('.bg-red-100');
    await expect(errorDiv).toBeVisible();
  });
});