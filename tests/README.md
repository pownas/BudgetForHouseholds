# Playwright Tests för BudgetForHouseholds

Detta projekt innehåller Playwright end-to-end tester för BudgetForHouseholds applikationen.

## Röktester (Smoke Tests)

Röktesterna verifierar grundläggande funktionalitet och att användargränssnittet laddas korrekt.

### Dashboard Smoke Test

Testet `dashboard-smoke.spec.ts` verifierar:

1. **Inloggningssida** - Att login-sidan laddas korrekt
2. **Demo-inloggning** - Att demo-användaren kan logga in
3. **Dashboard** - Att dashboarden visas efter inloggning
4. **Screenshots** - Tar screenshots för manuell verifiering

#### Demo-användare
- **E-post:** `demo@demo.se`
- **Lösenord:** `Demo123!`

## Kör tester

### Förutsättningar

1. Installera dependencies:
```bash
npm install
```

2. Starta applikationen (i separat terminal):
```bash
dotnet run --project src/BudgetApp.Blazor
```
Applikationen kommer att köras på `http://localhost:5109`

### Köra tester

```bash
# Kör alla tester
npm test

# Kör endast smoke test
npm run test:smoke

# Kör med synlig webbläsare (för debug)
npm run test:headed

# Kör med Playwright UI
npm run test:ui

# Debug-läge
npm run test:debug
```

### Test-resultat

Screenshots sparas i `test-results/` mappen:
- `login-page.png` - Inloggningssidan
- `login-form-filled.png` - Ifyllt inloggningsformulär
- `dashboard-full.png` - Komplett dashboard (fullpage)
- `dashboard-viewport.png` - Dashboard (synligt område)
- `dashboard-direct-access.png` - Direkt åtkomst till dashboard

### Trouble Shooting

1. **Applikationen startar inte**
   - Kontrollera att .NET 8 SDK är installerat
   - Kontrollera att inga andra processer använder port 5109

2. **Tester misslyckas**
   - Kontrollera att applikationen körs på `http://localhost:5109`
   - Kontrollera att demo-användaren finns (skapas automatiskt vid start)
   - Kontrollera nätverksanslutning

3. **Browsers saknas**
   ```bash
   npx playwright install chromium
   ```

## Test-struktur

- `tests/` - Katalog för alla test-filer
- `playwright.config.ts` - Playwright konfiguration
- `test-results/` - Screenshots och rapporter (skapas automatiskt)

## Tillägg av nya tester

För att lägga till nya tester, skapa `.spec.ts` filer i `tests/` katalogen och följ Playwright testmönster.