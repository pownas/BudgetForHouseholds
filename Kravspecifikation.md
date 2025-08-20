# Kravspecifikation – Privatekonomiapp
Mobile-first applikation med stöd för webbimport av banktransaktioner och delning inom sambofamilj/hushåll.

## 1. Översikt
- Syfte: Ge privatpersoner och sambofamiljer ett enkelt sätt att se, kategorisera och dela sina utgifter/inkomster, sätta budgetar och bokföra hushållsposter i en mobilanpassad lösning med kompletterande webbgränssnitt.
- Målgrupp: Svensk privatmarknad; individer och sambopar/familjer med gemensam eller delvis gemensam ekonomi.
- Mål:
  - Snabb överblick av ekonomi och hushållskostnader.
  - Enkel import av transaktioner via webbläsare och/eller bankkoppling.
  - Smidig delning av transaktionsdata inom hushåll med tydlig fördelning och utjämning.
  - Säker, integritetsfokuserad hantering av person- och transaktionsdata.

## 2. Omfattning
- In scope:
  - Mobilapp (mobile-first UX) och PWA/desktop-webb för import, granskning och rapportering.
  - Import av banktransaktioner via fil (CSV/OFX/CAMT/MT940 där möjligt) och/eller PSD2/Open Banking-anslutning via aggregator.
  - Kategorisering, regler, delning i hushåll, utjämning av gemensamma utgifter, budget, rapporter.
- Out of scope (v1):
  - Automatiska betalningar.
  - Skattedeklarationsstöd.
  - Full dubbla bokföringsprinciper; fokus på personal finance (ej företag).
  - Investeringsportfölj-analys (kan komma senare).

## 3. Definitioner och förkortningar
- Hushåll: Delad grupp (t.ex. sambopar/familj) som delar ekonomi helt/delvis.
- Hushållsposter: Utgifts-/intäktsposter kopplade till boende/hem och andra gemensamma kostnader (exempel: hyra/bolån, el, mat, försäkring, bil).
- PWA: Progressive Web App.
- PSD2/Open Banking: Regelverk och API:er för bankdataåtkomst via tredjepart (ex. Tink, GoCardless/Nordigen).
- OFX/MT940/CAMT.053: Standardiserade filformat för banktransaktioner.
- GDPR: Dataskyddsförordningen.

## 4. Användarroller och behörigheter
- Individ (standard): Hanterar egna konton/transaktioner.
- Hushållsmedlem: Delar transaktioner/kategorier/rapporter inom ett hushåll.
- Hushållsadministratör: Kan bjuda in/ta bort medlemmar, konfigurera hushållets regler, standardkategorier, delningspolicy.
- Observatör (valfritt v2): Läsrättigheter (ex. rådgivare).

## 5. Personas (exempel)
- Alex (28): Vill få koll på mat och transport, använder mobilen dagligen.
- Bea (33): Delar ekonomi med partner, vill se rättvis kostnadsfördelning samt månadsvy över hushållsposter.
- Chris (41): Importerar CSV från banken på datorn, vill snabbt kategorisera och exportera till Excel.

## 6. Användningsfall (urval)
- Som användare vill jag:
  - Koppla mina bankkonton eller importera fil så att jag ser aktuella transaktioner.
  - Kategorisera transaktioner automatiskt via regler så att det går snabbt.
  - Dela utvalda konton/transaktioner med mitt hushåll så att vi ser helheten.
  - Splitta en transaktion mellan hushållsmedlemmar så att kostnader fördelas korrekt.
  - Sätta budget per kategori och få notiser när vi närmar oss gränsen.
  - Se rapporter per period, kategori och person, samt hur mycket som ska utjämnas.
  - Exportera data till CSV/Excel för vidare analys.
  - Ladda upp kvitton/foton till transaktioner för dokumentation.

## 7. Funktionella krav
### 7.1 Konto- och identitetshantering
- Registrering och inloggning via e-post/lösenord samt stöd för 2FA; BankID-inloggning (bör).
- Hantering av profil, språk (svenska v1), valuta (SEK v1).
- Återställning av lösenord, sessionshantering, enhetsöversikt.

### 7.2 Hushåll och delning
- Skapa/ansluta till hushåll.
- Bjuda in medlemmar via e-post/länk; roll: medlem/administratör.
- Delningspolicy per konto och/eller per transaktion:
  - Privat (endast ägare).
  - Delad i hushåll.
- Splittning:
  - Fast fördelning (50/50 eller valfri procent).
  - Per transaktion eller per kategori.
  - Möjlighet att markera en transaktion som helt privat även om kontot delas.
- Utjämning:
  - Beräkning av saldo mellan medlemmar.
  - Förslag på utjämningsbetalning (Swish-länk; informativ, ingen faktisk betalning v1).
  - Historik/logg över utjämningar.

### 7.3 Datakällor och import
- Filuppladdning via webbapp (desktop och mobil):
  - Stöd v1: CSV (konfigurerbara kolumnmappar), OFX (bör).
  - Stöd v2: CAMT.053, MT940 (beroende på bankernas export).
- Guidat importflöde:
  - Mappning av kolumner (datum, belopp, valuta, text, saldo, referens).
  - Förhandsgranskning, dubblettdetektion (baserat på belopp, datum, referens/ID, hashning).
  - Import till valt konto, med möjlighet att skapa nytt konto.
- PSD2/Open Banking-integration (bör v1, måste v2):
  - Via aggregator (ex. Tink/GoCardless/Nordigen) för konto- och transaktionsdata.
  - Hantering av samtycke, uppdatering av token, felhantering och återauktorisering.
- Swish-specifika referenser (bör): Identifiera och labela swish-betalningar.

### 7.4 Konton och saldon
- Skapa och hantera konton (bankkonto, kreditkort, kontant, sparkonto, skuld).
- Valuta SEK v1; kursstöd fler valutor v2.
- Visning av aktuellt saldo och historik (för importerade konton).

### 7.5 Transaktionshantering
- Visa lista och detaljvy; sök och filter (datum, belopp, text, kategori, taggar, delningsstatus).
- Kategorisering:
  - Standardkategorier (ex. Boende, Mat, Transport, Försäkring, Barn, Nöje, Hälsa, Övrigt).
  - Egna kategorier och underkategorier (bör).
  - Regler: Om text innehåller X och belopp inom Y → tilldela kategori Z, splitta, lägg tagg.
  - Manuell omklassificering med historik.
- Splittning och taggar:
  - Splitta per person/procent.
  - Taggar för projekt/renovering/bil.
- Bilagor:
  - Ladda upp kvitto/bild/pdf; maxstorlek och filtyper; förhandsvisning.
- Anteckningar:
  - Fria kommentarer.
- Massåtgärder:
  - Markera flera och tillämpa kategori/regler i bulk.

### 7.6 Budget och mål
- Budget per kategori och per hushåll/individ.
- Periodisering (månad/vecka/år).
- Prognos baserad på historik (bör).
- Notiser vid 75/100/120% av budget (konfigurerbart).

### 7.7 Rapporter och insikter
- Översikt: inkomster, utgifter, sparande, netto per period.
- Nedbrytning per kategori/underkategori, per person, per konto.
- Hushållsutjämning: vem är skyldig vem.
- Trendgrafer, topphandlare, regelträffar.
- Export: CSV/Excel för valda urval och perioder.

### 7.8 Hushållsposter (boende/hem) – förenklad bokföring
- Fördefinierad kategoriuppsättning för hushållsposter.
- Snabbregistrering av återkommande kostnader (hyra/bolån, el, bredband, försäkring).
- Markera kostnader som fasta eller rörliga.
- Föreslagna splittregler per hushållspost (ex. boende 50/50).
- Månadsrapport “Hushållsöversikt”.

### 7.9 Notiser och kommunikation
- Pushnotiser/e-post vid:
  - Ny import klar.
  - Regelträff och kategorisering utförd.
  - Budgettrösklar passerade.
  - Utjämningsförslag genererat.
- In-app center för alla händelser (med tyst läge).

### 7.10 Offline och synk
- PWA med offline-läge för visning av senaste data och väntande ändringar.
- Konflikthantering vid samtidig redigering (senaste vinner + logg, bör: merge för splitt/kategori).

### 7.11 Hjälp och support
- Inbyggda guider, FAQ.
- Kontaktformulär, felrapportering med logg-ID.

## 8. Icke-funktionella krav
- Användbarhet:
  - Mobile-first, WCAG 2.1 AA (bör).
  - Touch-optimerad kategorisering och massåtgärder.
- Prestanda:
  - Start < 2s på modern mobil, lista 1000 transaktioner < 1s filtrering lokalt.
- Säkerhet:
  - Kryptering i transit (TLS 1.2+) och i vila.
  - Åtkomstkontroller per hushåll/konto/transaktion.
  - Sekretess mellan hushållsmedlemmar för markerade privata poster.
  - Skydd mot vanliga attacker (OWASP Top 10, MASVS-baskrav).
  - 2FA; BankID-inloggning (bör).
- Integritet och GDPR:
  - Samtycke för bankdata.
  - Rätt till radering/export (Data Subject Rights).
  - Dataminimering; retention-policy (standard 24 mån med möjlighet till justering).
  - Personuppgiftsbiträdesavtal och registerförteckning.
- Tillgänglighet och driftsäkerhet:
  - SLO: 99.5% månatlig tillgänglighet v1.
  - Säkerhetskopior dagligen; RPO ≤ 24h, RTO ≤ 4h.
- Skalbarhet:
  - Horisontell skalning av API; asynkrona jobb för import/parsing.
- Observability:
  - Strukturerad logg, spår-ID, metriker (latens, fel), larm.
- Kompatibilitet:
  - iOS 15+, Android 9+, Browser: senaste 2 versioner av Chrome/Edge/Safari/Firefox.
- Lokalisation:
  - Språk: Svenska v1; fler språk v2.

## 9. Data- och informationsmodell (översikt)
- Entiteter:
  - User: id, profil, auth, inställningar.
  - Household: id, namn, medlemmar, roller, delningspolicy.
  - Account: id, ägare, hushållsdelning, typ, valuta, saldo.
  - Transaction: id, accountId, datum, belopp, valuta, text, motpart, kategoriId, taggar, delningsstatus, split (lista av {userId, andel}).
  - Category: id, namn, parentId, hushålls- eller användaromfattning.
  - Rule: id, villkor (textmatch, beloppsintervall, motpart), åtgärd (kategori, split, tagg), prioritet.
  - Budget: id, period, kategoriId, belopp, hushåll/individ.
  - Settlement: id, fromUser, toUser, belopp, datum, status, referens.
  - Attachment: id, transactionId, url/metadatapointer, hash, typ.
  - ImportJob: id, källa (fil/PSD2), status, mappning, dubblettpolicy, logg.
- Integritetsprincip:
  - Fälten för privata transaktioner markeras och undanhålls för övriga medlemmar.
  - Audit-logg över visningar/ändringar (begränsas för PII).

## 10. Filformat och importspecifikation
- CSV (v1):
  - Obligatoriska fält: Date, Amount, Description.
  - Valfria fält: Balance, Currency, TransactionId/Reference.
  - Datumformat: auto-detektion (YYYY-MM-DD, DD/MM/YYYY); tidszon: Europe/Stockholm.
  - Avgränsare: komma/semikolon; citationstecken stöd.
  - Mappningsguide inklusive förhandsgranskning och 5–10 provrader.
- OFX (v1-bör), CAMT.053/MT940 (v2):
  - Parser med normalisering till intern modell.
  - Säker hantering av specialtecken och motpartsnamn.
- Dubblettdetektion:
  - Hash av (belopp, datum±1d, normaliserad text, konto) + ev. ref.
  - Val att ignorera/merge vid kollisioner.

## 11. Integrationer
- PSD2/Open Banking via aggregator (v1-bör/v2-måste):
  - Kontolista, transaktioner, balans.
  - Samtyckesflöde och förnyelsevarningar.
- BankID (v1-bör):
  - Inlogg/step-up för känsliga åtgärder (t.ex. ändra delningspolicy).
- Lagring av bilagor:
  - Krypterad objektlagring; signade URL:er; antivirus-skanning.

## 12. Arkitektur och tekniska krav
- Frontend:
  - Mobilapp (hybrid eller React Native/Flutter) + PWA-webb.
  - State management med offline-stöd, delta-synk.
- Backend:
  - REST/GraphQL API; OAuth2/OIDC för sessioner.
  - Jobbkön för import/parsers/regler.
  - Relationsdatabas för kärndata; objektlagring för bilagor.
- Säkerhet:
  - Rotationspolicy för nycklar; hemlighetshantering.
  - Rate limiting och bot-skydd.
- CI/CD:
  - Enhetstester, e2e för kritiska flöden (import, delning, utjämning).
  - Statisk analys och SCA.

## 13. Telemetri och privacy-by-design
- Opt-in telemetri med anonymisering.
- Separat toggel för kraschrapporter.
- Ingen delning av PII till tredjepart utan explicit samtycke.

## 14. Acceptanskriterier (exempel per epic)
- Import (CSV):
  - Kan ladda upp CSV, mappa fält, se förhandsgranskning, importera utan fel.
  - Dubbletter upptäcks och hanteras enligt vald policy.
- Kategorisering:
  - Minst 90% av återkommande transaktioner kategoriseras korrekt efter att regler satts.
- Hushållsdelning:
  - Medlem kan dela konto, markera privata transaktioner, och dessa är osynliga för andra.
  - Splittning fungerar och påverkar utjämningsrapport.
- Budget:
  - Budget per kategori kan sättas; notiser skickas vid trösklar.
- Rapporter:
  - Periodrapporter kan exporteras till CSV med samma siffror som UI.

## 15. Prioritering (MoSCoW) och milstolpar
- Must have (MVP):
  - Konto, transaktioner, CSV-import, manuell kategorisering, regler, hushållsdelning, splittning, budget, grundrapporter, export CSV, grundläggande säkerhet/GDPR.
- Should have:
  - OFX-stöd, PWA offline, notiser, bilagor, utjämningsförslag, BankID-inlogg, PSD2 via en aggregator.
- Could have:
  - CAMT/MT940, avancerad prognos, observatörsroll, fler språk/valutor.
- Won’t have (v1):
  - Automatiska betalningar, avancerad investerings- och skattemodul.

- Milstolpar:
  - M1: MVP mobil + webbinport CSV, delning, budget, rapporter.
  - M2: PSD2-integration + notiser + bilagor + PWA offline.
  - M3: OFX/CAMT-stöd, prognos, BankID.

## 16. Risker och beroenden
- Beroende av tredjepartsaggregatorer (tillgänglighet, kostnader).
- Variation i bankernas exportformat (CSV/OFX/CAMT).
- GDPR/BankID-efterlevnad och säkerhetskrav ökar komplexiteten.
- Användarförtroende: måste ha tydlig och transparent hantering av data.

## 17. Öppna frågor
- Vilka banker/aggregatorer prioriteras för PSD2?
- Exakt lista på standardkategorier och ikonografi.
- Behövs SIE-export för användare som vill använda enklare bokföringssystem?
- Nivå för end-to-end-kryptering inom hushållsdelning (balans mellan UX och säkerhet).
- Hur ska kvitto-tolkning (OCR) prioriteras (v2/v3)?

## 18. Spårbarhet
- Varje krav märks med ID (t.ex. FR-1, NFR-3) och knyts till user stories, testfall och releaser i backloggssystem.

---
Kontaktperson: Produktägare - Jonas A.
Version: 1.0 (utkast)
Datum: 2025-08-20
