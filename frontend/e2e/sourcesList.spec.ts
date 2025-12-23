import { test, expect } from '@playwright/test';
import { SourcesListPage } from '../tests/pageObjects/SourcesListPage';

test.describe('Sources List Functionality', () => {
  let sourcesListPage: SourcesListPage;

  test.beforeEach(async ({ page }) => {
    sourcesListPage = new SourcesListPage(page);
    await page.goto('/sources');
  });

  test('should display sources list page', async ({ page }) => {
    // Проверяем, что заголовок страницы отображается
    await expect(page.locator('h2:has-text("Источники новостей")')).toBeVisible();
    
    // Проверяем, что кнопка добавления источника отображается
    const addButton = page.locator('button:has-text("Добавить источник")');
    await expect(addButton).toBeVisible();
  });

  test('should show message when no sources exist', async ({ page }) => {
    // Для этого теста предполагаем, что в тестовой среде может не быть источников
    const noSourcesMessage = page.locator('text=Нет добавленных источников');
    const sourcesExist = await page.locator('tbody tr').count() > 0;
    
    if (!sourcesExist) {
      await expect(noSourcesMessage).toBeVisible();
    }
  });

  test('should allow navigation to add source page', async ({ page }) => {
    // Кликаем по кнопке добавления источника
    await page.locator('button:has-text("Добавить источник")').click();
    
    // Проверяем, что произошел переход на страницу добавления
    await expect(page).toHaveURL(/.*\/add-source/);
  });

  test('should display source information in table', async ({ page }) => {
    // Проверяем, что если источники существуют, они правильно отображаются в таблице
    const sourceRows = page.locator('tbody tr');
    const rowCount = await sourceRows.count();
    
    if (rowCount > 0) {
      // Проверяем, что первая строка таблицы содержит ожидаемые столбцы
      const firstRowCells = sourceRows.first().locator('td');
      await expect(firstRowCells).toHaveCount(5); // Имя, Тип, Статус, Последняя проверка, Действия
      
      // Проверяем, что хотя бы одно имя источника отображается
      const sourceName = firstRowCells.nth(0).locator('div');
      await expect(sourceName).toBeVisible();
    }
  });
});