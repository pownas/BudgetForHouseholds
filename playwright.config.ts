import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright configuration for BudgetForHouseholds smoke tests
 * See https://playwright.dev/docs/test-configuration.
 */
export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'html',
  
  use: {
    baseURL: 'http://localhost:5109',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],

  /* Uncomment this if you want Playwright to auto-start the server
  webServer: {
    command: 'dotnet run --project src/BudgetApp.Blazor',
    url: 'http://localhost:5109',
    reuseExistingServer: !process.env.CI,
    timeout: 120 * 1000,
  },
  */
});