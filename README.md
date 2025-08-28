## Installation och setup

1. Installera .NET 8 SDK (eller senare):
   https://dotnet.microsoft.com/download

2. Klona repot:
   ```bash
   git clone https://github.com/pownas/BudgetForHouseholds.git
   cd BudgetForHouseholds
   ```

3. (Valfritt) Installera Aspire workload:
   ```bash
   dotnet workload install aspire
   ```

4. Bygg och starta AppHost:
   ```bash
   dotnet build
   dotnet run --project src/BudgetApp.AppHost
   ```

5. Öppna Aspire dashboard i webbläsaren:
   http://localhost:18888

6. Starta Blazor-appen via dashboarden ("BudgetApp.Blazor").

SQLite används som databas och skapas automatiskt vid första körning.
Aspire dashboard:
- När AppHost körs startas Aspire dashboard automatiskt.
- Dashboarden är normalt tillgänglig på http://localhost:18888
- Blazor-appen och API-tjänster nås via dashboardens tjänsteöversikt.

Exempel:
1. Starta AppHost:
   ```bash
   dotnet run --project src/BudgetApp.AppHost
   ```
2. Öppna Aspire dashboard i webbläsaren:
   http://localhost:18888
3. Klicka på "BudgetApp.Blazor" för att öppna Blazor-appen.
Förutsättningar:
- .NET 8 SDK eller senare
- SQLite (för lokal databas)
- Aspire (AppHost) för lokal orkestrering och dashboard

Bygg och kör:
1. Klona repot
2. Bygg och starta med .NET:
   ```bash
   dotnet build
   dotnet run --project src/BudgetApp.AppHost
   ```
3. Blazor-appen körs via AppHost och är tillgänglig via dashboarden.

Node.js, npm och React behövs inte längre. All frontend är nu Blazor WebAssembly.
BudgetForHouseholds
===================

Swedish household budget management app. Mobile-first, import transactions, categorize, split costs, manage household budgets.

Frontend: Blazor WebAssembly (with mobile-first design)
Backend: REST API (.NET Core)
Database: SQLite
Infrastructure: Aspire dashboard and AppHost for orchestration and local development

See Kravspecifikation.md for requirements (Swedish).

Aspire integration:
- The solution includes an Aspire AppHost for local orchestration, service discovery, and dashboarding.
- To run locally, use the AppHost project for a unified developer experience.

Blazor app:
- The Blazor frontend replaces the previous React-based implementation.
- All UI and client logic is now in `src/BudgetApp.Blazor/`.

React frontend info has been removed. For legacy code, see previous commits or branches.
# BudgetForHouseholds - MVP

En privatekonomi app för budget och kostnadssplit mellan sambos. Denna MVP implementerar grundfunktionaliteten enligt kravspecifikationen.

## 🚀 Funktioner i MVP

### Must Have (Implementerat)
- ✅ **Användarregistrering och inloggning** med JWT-autentisering
- ✅ **Responsiv design** med Material UI (mobile-first)
- ✅ **Dashboard** med översikt av ekonomisk status
- ✅ **Datamodeller** för alla kärnentiteter (User, Account, Transaction, Household, Category, Budget, etc.)
- ✅ **API-endpoints** för konto- och transaktionshantering
- ✅ **Databas** med SQLite och Entity Framework
- ✅ **CSV-import** stöd med förhandsgranskning och dubblettdetektion
- ✅ **Kategorisering** med fördefinierade svenska kategorier
- ✅ **Hushållsdelning** grundstruktur
- ✅ **Säkerhet** med CORS, JWT-autentisering och datavalidering

### Teknisk Stack
- **Backend**: .NET 8 Web API
- **Frontend**: React 18 med TypeScript
- **UI Framework**: Material UI v5
- **Database**: SQLite med Entity Framework Core
- **Authentication**: JWT Bearer tokens
- **Import**: CSV med CsvHelper
- **API Communication**: Axios

## 📱 Screenshots

### Inloggningssida
![Login](https://github.com/user-attachments/assets/c3dfb1ee-c1ed-4085-bbde-12825de63e23)

### Registrering
![Register](https://github.com/user-attachments/assets/fcb1d453-1522-48d4-af43-e4ebc8d5de39)

### Dashboard
![Dashboard](https://github.com/user-attachments/assets/c9d13eca-cf40-42d1-b07a-6703cca79b53)

## 🛠️ Installation och Setup

### Förutsättningar
- .NET 8 SDK
- Node.js 18+
- npm eller yarn

### Backend (API)
```bash
cd BudgetApp.Api
dotnet restore
dotnet run
```
API:t kommer att köras på `http://localhost:5291`

### Frontend
```bash
cd budget-app-frontend
npm install
npm start
```
Frontend kommer att köras på `http://localhost:3000`

## 🧪 Testning

### Skapa testkonto
1. Navigera till `http://localhost:3000`
2. Klicka på "Registrera dig här"
3. Fyll i formuläret med:
   - Förnamn: Test
   - Efternamn: Användare  
   - E-post: test@example.com
   - Lösenord: Password123 (måste innehålla versaler, gemener och siffror)
4. Du loggas automatiskt in efter registrering

### API-dokumentation
API-dokumentation finns tillgänglig via Swagger på `http://localhost:5291/swagger` när backend körs.

## 📊 Datamodell

Applikationen använder följande huvudentiteter:
- **User** - Användare med autentisering
- **Household** - Hushåll för delning
- **Account** - Bankkonton och andra konton
- **Transaction** - Transaktioner med kategorisering och delning
- **Category** - Kategorier för transaktioner
- **Budget** - Budgetar per kategori
- **CategoryRule** - Regler för automatisk kategorisering
- **Settlement** - Utjämningar mellan hushållsmedlemmar

## 🔄 Nästa steg (Should Have)

- OFX-filstöd för import
- PWA offline-funktionalitet
- Push-notifikationer
- Bifogade kvitton
- Utjämningsförslag
- BankID-inloggning
- PSD2/Open Banking integration

## 🛡️ Säkerhet

- JWT Bearer token autentisering
- Lösenordsvalidering med krav på komplexitet
- CORS-konfiguration för frontend
- SQL injection-skydd via Entity Framework
- Input-validering på API-nivå

## 📝 Licens

MIT License - se LICENSE filen för detaljer.
