# BudgetForHouseholds - GitHub Copilot Instructions

**ALWAYS follow these instructions first and fallback to additional search and context gathering ONLY if the information here is incomplete or found to be in error.**

BudgetForHouseholds is a Swedish household budget management application designed for mobile-first usage with web import capabilities. The project enables transaction import, categorization, and cost splitting between household members.

## Current Project State

**IMPORTANT: This is currently a specification-only repository with no runnable code.**

- The repository contains requirements specification (Kravspecifikation.md) in Swedish
- No build processes, dependencies, or executable code exists yet
- No CI/CD pipelines are configured
- Development environment is not yet established

## Repository Structure

```
/
├── .github/               # GitHub configuration (this directory)
├── .gitignore            # Visual Studio template gitignore
├── Kravspecifikation.md  # Comprehensive requirements specification (Swedish)
├── LICENSE               # MIT License
└── README.md             # Basic project description (2 lines)
```

## Working Effectively

### Current Limitations
- **DO NOT** attempt to build, test, or run the application - no code exists yet
- **DO NOT** look for package.json, *.csproj, Dockerfile, or other build files - they don't exist
- **DO NOT** expect any npm, dotnet, or other platform commands to work

### What You CAN Do
- Read and analyze the comprehensive requirements in `Kravspecifikation.md`
- Understand the planned architecture and technical requirements
- Create initial project structure based on the specifications
- Set up development environment according to planned tech stack

### Essential Reading
- **ALWAYS** start by reading `Kravspecifikation.md` for complete project understanding
- The specification is in Swedish but contains detailed technical architecture
- Contains technology stack decisions, functional requirements, and milestones

## Planned Architecture (from Specifications)

### Technology Stack
- **Frontend**: Mobile app (React Native/Flutter hybrid) + PWA web application
- **Backend**: REST/GraphQL API with OAuth2/OIDC authentication
- **Database**: Relational database for core data + object storage for attachments
- **Infrastructure**: Job queue for import/parsing/rules processing
- **Security**: Key rotation policy, rate limiting, bot protection

### Key Features (Planned)
- CSV/OFX/CAMT/MT940 bank transaction import
- PSD2/Open Banking integration via aggregator
- Transaction categorization with automatic rules
- Household cost sharing and splitting
- Budget management and notifications
- Offline PWA support with conflict resolution
- BankID integration for authentication

## Development Guidance

### Before Starting Development
1. **Choose primary technology stack** based on team expertise:
   - React Native + TypeScript for mobile-first approach
   - Flutter + Dart for cross-platform development
   - Or start with PWA using React/Vue/Angular

2. **Set up project structure** according to chosen stack:
   - Mobile app directory structure
   - Backend API structure
   - Shared types/models
   - CI/CD pipeline configuration

3. **Review Swedish requirements** in `Kravspecifikation.md`:
   - Understand functional requirements (Section 7)
   - Note non-functional requirements (Section 8)
   - Check architecture requirements (Section 12)
   - Review prioritization (Section 15)

### Development Priorities (MoSCoW from Specs)
- **Must have (MVP)**: Accounts, transactions, CSV import, manual categorization, rules, household sharing, splitting, budget, basic reports, CSV export, basic security/GDPR
- **Should have**: OFX support, PWA offline, notifications, attachments, equalization suggestions, BankID login, PSD2 via aggregator
- **Could have**: CAMT/MT940, advanced forecasting, observer role, multiple languages/currencies

### Security Considerations
- Handle sensitive financial data with appropriate encryption
- Implement GDPR compliance from day one
- Use secure authentication (OAuth2/OIDC)
- Plan for key rotation and secrets management

## Common Tasks

### Repository Analysis
```bash
# List all files (current state)
ls -la
# Output: .git/ .github/ .gitignore Kravspecifikation.md LICENSE README.md

# Check for any build configurations
find . -name "*.json" -o -name "*.yml" -o -name "*.yaml" -o -name "*.config.*"
# Output: (none - no build files exist)

# View requirements specification
cat Kravspecifikation.md
# Contains comprehensive Swedish requirements specification
```

### Initial Development Setup (when starting development)
```bash
# For React Native approach:
npx react-native init BudgetForHouseholds --template typescript
# TIMING: Initial setup takes 2-3 minutes

# For Flutter approach:
flutter create budget_for_households
# TIMING: Initial setup takes 1-2 minutes

# For Node.js backend:
npm init -y
npm install express cors helmet dotenv
# TIMING: npm install takes 30-60 seconds
```

## Validation and Testing Strategy

### When Code Exists (Future)
- **NEVER CANCEL** build processes - allow full completion
- Mobile app builds can take 15-30 minutes - set timeouts to 45+ minutes
- End-to-end testing should cover critical flows: import, sharing, equalization
- Always run linting and static analysis before commits

### Manual Testing Scenarios (Future)
1. **Transaction Import Flow**:
   - Upload CSV file with bank transactions
   - Verify transactions appear correctly
   - Test categorization rules application

2. **Household Sharing Flow**:
   - Create household group
   - Invite member
   - Share transactions
   - Test cost splitting

3. **Budget Management Flow**:
   - Set category budgets
   - Add transactions that exceed budget
   - Verify notifications work

## Key Documentation

### Requirements Specification (Kravspecifikation.md)
- Section 7: Functional requirements (Swedish)
- Section 8: Non-functional requirements
- Section 12: Architecture and technical requirements
- Section 15: Prioritization (MoSCoW) and milestones

### Development Notes
- Primary language: Swedish (UI will be Swedish v1, English v2)
- Target market: Swedish individuals and cohabiting couples/families
- Mobile-first design philosophy
- Privacy-by-design approach

## Immediate Next Steps

1. **Choose technology stack** based on team capabilities
2. **Create initial project structure** for chosen stack
3. **Set up CI/CD pipeline** with build and test stages
4. **Implement basic authentication** (OAuth2/OIDC foundation)
5. **Create initial data models** based on requirements
6. **Start with MVP features** listed in prioritization section

## Important Reminders

- **This repository currently contains NO executable code**
- **All build/test/run instructions are for FUTURE development**
- **Start by reading Kravspecifikation.md for complete context**
- **Swedish language requirements - consider internationalization from start**
- **Financial data requires extra security considerations**
- **Mobile-first approach is a key architectural principle**