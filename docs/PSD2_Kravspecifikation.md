# Kravspecifikation för PSD2/Open Banking Implementation

## 1. Översikt

Denna kravspecifikation beskriver implementeringen av PSD2/Open Banking-funktionalitet för BudgetForHouseholds-applikationen. Syftet är att möjliggöra säker och automatiserad hantering av bankanslutningar enligt europeiska regelverk och standarder.

## 2. Omfattning

### 2.1 Funktioner som ingår
- Lista tillgängliga banker via aggregator
- Upprätta säkra anslutningar till banker
- Hantera användarmedgivanden (consent management)
- Automatisk och manuell synkronisering av konton
- Import av transaktioner från externa källor
- Övervakning av anslutningsstatus
- Händelseloggning för audit och felsökning
- GDPR-kompatibel datahantering

### 2.2 Funktioner som inte ingår (v1)
- Direkta PSD2-anslutningar utan aggregator
- Betalningsinitieringstjänster (PIS)
- Avancerad kontoinformationsanalys
- Multi-valuta stöd (endast SEK i v1)

## 3. Tekniska Krav

### 3.1 Säkerhetskrav

#### 3.1.1 Kryptering och Kommunikation
- **TLS 1.2+**: All kommunikation mellan applikation och aggregator ska ske över TLS 1.2 eller senare
- **Token-säkerhet**: OAuth2-tokens ska lagras krypterat i databasen
- **Key rotation**: Implementera rutiner för regelbunden rotation av kryptografiska nycklar
- **Rate limiting**: Implementera begränsningar för API-anrop för att förhindra missbruk

#### 3.1.2 Autentisering och Auktorisering
- **OAuth2/OIDC**: Använd OAuth2 med OIDC för säker autentisering mot aggregator
- **Scope management**: Begränsa åtkomst till endast nödvändiga banktjänster
- **Token refresh**: Automatisk förnyelse av access tokens före utgång
- **Consent tracking**: Spåra och dokumentera alla användarmedgivanden

### 3.2 Prestanda och Tillgänglighet

#### 3.2.1 Synkronisering
- **Automatisk synkronisering**: Utförs var 6:e timme för aktiva anslutningar
- **Manuell synkronisering**: Tillgänglig on-demand via användargränssnitt
- **Batch processing**: Hantera multipla konton effektivt under synkronisering
- **Retry logic**: Implementera exponential backoff för misslyckade API-anrop

#### 3.2.2 Skalbarhet
- **Background services**: Asynkron bearbetning av synkroniseringsuppgifter
- **Connection pooling**: Effektiv hantering av databasanslutningar
- **Caching**: Cachning av banklistor och metadata för förbättrad prestanda

### 3.3 Datahantering

#### 3.3.1 Datamodell
```sql
-- Bank Connection (Bankanslutning)
BankConnection {
    Id: int (PK)
    UserId: string (FK)
    BankName: string
    BankId: string
    ExternalConnectionId: string (unik)
    Status: ConnectionStatus (enum)
    ConsentGivenAt: DateTime
    ConsentExpiresAt: DateTime?
    LastSyncAt: DateTime?
    ErrorMessage: string?
    CreatedAt: DateTime
    UpdatedAt: DateTime
}

-- External Account (Externt konto)
ExternalAccount {
    Id: int (PK)
    BankConnectionId: int (FK)
    ExternalAccountId: string
    AccountName: string
    AccountNumber: string
    AccountType: ExternalAccountType (enum)
    CurrentBalance: decimal
    AvailableBalance: decimal?
    Currency: string
    IsActive: bool
    LastUpdated: DateTime
    LinkedAccountId: int? (FK till intern Account)
}

-- Event Log (Händelselogg)
Psd2EventLog {
    Id: int (PK)
    UserId: string (FK)
    BankConnectionId: int? (FK)
    EventType: string
    EventDescription: string
    EventData: string? (JSON)
    Timestamp: DateTime
    IsSuccess: bool
    ErrorMessage: string?
    IpAddress: string?
    UserAgent: string?
}
```

#### 3.3.2 Standardiserade händelsetyper
- `CONNECTION_CREATED`: Ny bankanslutning skapad
- `CONNECTION_COMPLETED`: Anslutning slutförd och aktiv
- `CONNECTION_FAILED`: Anslutning misslyckades
- `CONNECTION_DISCONNECTED`: Anslutning frånkopplad
- `SYNC_STARTED`: Synkronisering påbörjad
- `SYNC_COMPLETED`: Synkronisering slutförd
- `SYNC_FAILED`: Synkronisering misslyckades
- `CONSENT_EXPIRED`: Medgivande har upphört
- `CONSENT_RENEWED`: Medgivande förnyat
- `TRANSACTIONS_IMPORTED`: Transaktioner importerade
- `ACCOUNT_LINKED`: Konto länkat till intern account
- `ERROR_OCCURRED`: Allmänt fel inträffat

## 4. API-specifikation

### 4.1 Bank Connection API

#### 4.1.1 Lista tillgängliga banker
```http
GET /api/psd2/banks
Authorization: Bearer {token}

Response: 200 OK
[
  {
    "id": "SWEDSESS",
    "name": "Swedbank",
    "logoUrl": "/images/swedbank.png"
  }
]
```

#### 4.1.2 Skapa bankanslutning
```http
POST /api/psd2/connections
Authorization: Bearer {token}
Content-Type: application/json

{
  "bankId": "SWEDSESS",
  "redirectUrl": "https://app.example.com/callback"
}

Response: 200 OK
{
  "success": true,
  "authorizationUrl": "https://aggregator.com/auth?...",
  "connectionId": "uuid"
}
```

#### 4.1.3 Slutför bankanslutning
```http
POST /api/psd2/connections/{connectionId}/complete
Authorization: Bearer {token}
Content-Type: application/json

"authorization_code_from_bank"

Response: 200 OK
{
  "message": "Bank connection completed successfully"
}
```

#### 4.1.4 Synkronisera anslutning
```http
POST /api/psd2/connections/{connectionId}/sync
Authorization: Bearer {token}

Response: 200 OK
{
  "message": "Bank connection synced successfully"
}
```

### 4.2 Event Logging API

#### 4.2.1 Hämta händelseloggar
```http
GET /api/psd2/event-logs?page=1&pageSize=50&fromDate=2024-01-01
Authorization: Bearer {token}

Response: 200 OK
[
  {
    "id": 123,
    "eventType": "SYNC_COMPLETED",
    "eventDescription": "Successfully synchronized Swedbank",
    "timestamp": "2024-01-15T10:30:00Z",
    "isSuccess": true,
    "bankConnectionId": 1,
    "bankName": "Swedbank"
  }
]
```

## 5. Implementeringsdetaljer

### 5.1 Felhantering

#### 5.1.1 Retry-strategier
- **Connection failures**: 3 försök med exponential backoff (1s, 2s, 4s)
- **Timeout hantering**: 30 sekunder för API-anrop, 5 minuter för auth flows
- **Rate limit hantering**: Implementera backoff när rate limits träffas

#### 5.1.2 Felkategorier
```csharp
public enum Psd2ErrorCategory
{
    NetworkError,           // Nätverksproblem
    AuthenticationError,    // Autentiseringsfel
    AuthorizationError,     // Behörighetsfel
    ConsentExpired,        // Medgivande upphört
    BankError,             // Fel från bank
    AggregatorError,       // Fel från aggregator
    ValidationError,       // Valideringsfel
    SystemError            // Systemfel
}
```

### 5.2 Monitorering och Logging

#### 5.2.1 Metrics som ska spåras
- Antal aktiva anslutningar per bank
- Synkroniseringsfrekvens och framgång
- API response times mot aggregator
- Fel-frekvens per bank och operation
- Consent expiry rate

#### 5.2.2 Alerting
- Consent som upphör inom 7 dagar
- Synkroniseringsfel som överstiger 10% för en bank
- API response times över 10 sekunder
- Systemfel som påverkar >5% av användarna

## 6. GDPR och Compliance

### 6.1 Databehandling
- **Lawful basis**: Användarens explicit samtycke enligt Art. 6(1)(a)
- **Data minimization**: Endast nödvändig data hämtas och lagras
- **Storage limitation**: Transaktionsdata lagras max 7 år, metadata 3 år
- **Right to erasure**: Implementera rutiner för datatborttagning på begäran

### 6.2 Användarrättigheter
```http
GET /api/users/gdpr/export     # Exportera all PSD2-relaterad data
DELETE /api/users/gdpr/delete  # Ta bort all PSD2-relaterad data
GET /api/users/gdpr/consents   # Visa alla aktiva medgivanden
```

### 6.3 Audit Trail
- Alla PSD2-operationer loggas i Psd2EventLog
- Inkludera IP-adress och User-Agent för alla operationer
- Bevara loggar i minimum 3 år för compliance

## 7. Säkerhetskontroller

### 7.1 Input Validation
- Validera alla API-parametrar enligt definierade scheman
- Sanitera användarinput för att förhindra injection-attacker
- Implementera rate limiting per användare och IP

### 7.2 Output Security
- Ingen känslig information i felmeddelanden
- Maskera kontonummer i loggar (visa endast sista 4 siffror)
- Kryptera känslig data i databas (tokens, kontonummer)

### 7.3 Transport Security
```csharp
// Exempel på säker HTTP-klient konfiguration
services.AddHttpClient("AggregatorClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
});
```

## 8. Testing

### 8.1 Enhets-tester
- Validera affärslogik för alla PSD2-operationer
- Mocka externa beroenden (aggregator API)
- Testa felhantering och edge cases

### 8.2 Integration-tester
- Testa med mock aggregator
- Validera dataflöden från bank till intern databas
- Testa GDPR-funktionalitet end-to-end

### 8.3 Säkerhetstester
- Penetrationstester av API-endpoints
- Validera token-säkerhet och rotation
- Testa rate limiting och DoS-skydd

## 9. Deployment och Configuration

### 9.1 Miljöspecifika inställningar
```json
{
  "Psd2Settings": {
    "AggregatorBaseUrl": "https://api.aggregator.com",
    "ClientId": "your-client-id",
    "ClientSecret": "encrypted-secret",
    "SyncIntervalHours": 6,
    "ConsentCheckIntervalHours": 24,
    "RetryAttempts": 3,
    "TimeoutSeconds": 30
  }
}
```

### 9.2 Databas-migration
```bash
# Lägg till nya tabeller för PSD2
dotnet ef migrations add "AddPsd2EventLogging"
dotnet ef database update
```

## 10. Maintenance och Support

### 10.1 Regelbundet underhåll
- **Dagligen**: Kontrollera consent expiry och error rates
- **Veckovis**: Analysera synkroniseringsstatistik
- **Månadsvis**: Granska säkerhetsloggar och prestanda
- **Kvartalsvis**: Uppdatera banklistor och konfiguration

### 10.2 Incident Response
1. **Upptäckt**: Automatisk alerting eller användarrapport
2. **Triage**: Klassificera allvarlighetsgrad inom 15 minuter
3. **Response**: Påbörja åtgärder inom 1 timme för kritiska fel
4. **Resolution**: Dokumentera lösning och förbättringar
5. **Post-mortem**: Utvärdera incident och förbättra processer

---

*Dokumentversion: 1.0*  
*Senast uppdaterad: 2024-01-15*  
*Författare: System Architect*