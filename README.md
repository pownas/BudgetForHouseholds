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
