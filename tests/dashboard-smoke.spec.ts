import { test, expect, Page } from '@playwright/test';

/**
 * Röktest för BudgetForHouseholds demo-användare och dashboard
 * 
 * Detta test verifierar att:
 * 1. Inloggningssidan laddas korrekt
 * 2. Demo-användaren kan logga in
 * 3. Dashboarden visas korrekt efter inloggning
 * 
 * Inga dataverifieringar görs - endast att gränssnittet visas som förväntat.
 */

test.describe('Dashboard Smoke Test', () => {
  const DEMO_EMAIL = 'demo@demo.se';
  const DEMO_PASSWORD = 'Demo123!';

  test.beforeEach(async ({ page }) => {
    // Navigera till startsidan
    await page.goto('/');
  });

  test('demo-användare kan logga in och visa dashboard', async ({ page }) => {
    // Steg 1: Navigera till inloggningssidan och ta screenshot
    await test.step('Navigera till inloggningssidan', async () => {
      await page.goto('/login');
      await page.waitForLoadState('networkidle');
      
      // Vänta på att inloggningsformuläret ska visas
      await expect(page.locator('text=Logga in på ditt konto')).toBeVisible();
      
      // Ta screenshot av inloggningssidan
      await page.screenshot({ 
        path: 'test-results/login-page.png', 
        fullPage: true 
      });
    });

    // Steg 2: Fyll i demo-användarens uppgifter och logga in
    await test.step('Logga in med demo-användare', async () => {
      // Fyll i e-post
      await page.getByLabel('E-post').fill(DEMO_EMAIL);
      
      // Fyll i lösenord
      await page.getByLabel('Lösenord').fill(DEMO_PASSWORD);
      
      // Ta screenshot före inloggning
      await page.screenshot({ 
        path: 'test-results/login-form-filled.png', 
        fullPage: true 
      });
      
      // Klicka på logga in-knappen
      await page.getByRole('button', { name: 'Logga in' }).click();
      
      // Vänta på att inloggningen ska slutföras och vi omdirigeras
      await page.waitForURL('/', { timeout: 10000 });
    });

    // Steg 3: Verifiera att dashboarden visas korrekt
    await test.step('Verifiera dashboard', async () => {
      await page.waitForLoadState('networkidle');
      
      // Vänta på att dashboard-innehåll ska laddas
      // Kolla efter typiska dashboard-element (kan behöva justeras baserat på faktisk implementation)
      await expect(page.locator('text=Dashboard')).toBeVisible({ timeout: 10000 });
      
      // Ta screenshot av dashboarden
      await page.screenshot({ 
        path: 'test-results/dashboard-full.png', 
        fullPage: true 
      });
      
      // Ta en screenshot av bara viewport för snabbare referens
      await page.screenshot({ 
        path: 'test-results/dashboard-viewport.png', 
        fullPage: false 
      });
    });

    // Steg 4: Verifiera att användaren är inloggad (kontrollera UI-element)
    await test.step('Verifiera inloggningsstatus', async () => {
      // Kolla att vi inte är på login-sidan längre
      await expect(page).not.toHaveURL(/.*\/login.*/);
      
      // Om det finns en logout-knapp eller användarnamn, kolla det
      // Detta kanske behöver justeras baserat på faktisk UI
      const logoutButton = page.locator('text=Logga ut').or(page.locator('[data-testid="logout"]'));
      if (await logoutButton.isVisible({ timeout: 2000 })) {
        await expect(logoutButton).toBeVisible();
      }
      
      console.log('Demo-användare inloggning och dashboard-test slutfört!');
    });
  });

  test('dashboard-sidan är tillgänglig för inloggad användare', async ({ page }) => {
    // Alternativ test som går direkt till dashboard (om användaren redan är inloggad via cookies)
    await test.step('Försök komma åt dashboard direkt', async () => {
      await page.goto('/');
      
      // Om vi hamnar på login-sidan, logga in först
      if (page.url().includes('/login')) {
        await page.getByLabel('E-post').fill(DEMO_EMAIL);
        await page.getByLabel('Lösenord').fill(DEMO_PASSWORD);
        await page.getByRole('button', { name: 'Logga in' }).click();
        await page.waitForURL('/', { timeout: 10000 });
      }
      
      // Ta en snapshot av sidan för referens
      await page.screenshot({ 
        path: 'test-results/dashboard-direct-access.png', 
        fullPage: true 
      });
      
      // Verifiera att sidan inte visar ett fel
      await expect(page.locator('text=Fel')).not.toBeVisible();
      await expect(page.locator('text=Error')).not.toBeVisible();
    });
  });
});