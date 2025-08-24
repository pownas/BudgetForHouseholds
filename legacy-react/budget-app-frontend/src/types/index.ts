export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
}

export interface AuthResult {
  success: boolean;
  token?: string;
  user?: User;
  error?: string;
}

export interface LoginDto {
  email: string;
  password: string;
}

export interface RegisterDto {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface Account {
  id: number;
  name: string;
  type: AccountType;
  ownerId: string;
  householdId?: number;
  sharingType: SharingType;
  currentBalance: number;
  currency: string;
  description?: string;
  createdAt: string;
  updatedAt: string;
}

export enum AccountType {
  BankAccount = 0,
  CreditCard = 1,
  Cash = 2,
  SavingsAccount = 3,
  Debt = 4
}

export enum SharingType {
  Private = 0,
  SharedInHousehold = 1
}

export interface Transaction {
  id: number;
  accountId: number;
  date: string;
  amount: number;
  currency: string;
  description: string;
  counterpart?: string;
  categoryId?: number;
  notes?: string;
  sharingStatus: SharingStatus;
  externalId?: string;
  importHash?: string;
  createdAt: string;
  updatedAt: string;
  account?: Account;
  category?: Category;
  splits?: TransactionSplit[];
  tags?: TransactionTag[];
}

export enum SharingStatus {
  Private = 0,
  SharedInHousehold = 1
}

export interface TransactionSplit {
  id: number;
  transactionId: number;
  userId: string;
  percentage: number;
  amount: number;
  notes?: string;
  user?: User;
}

export interface TransactionTag {
  id: number;
  transactionId: number;
  tag: string;
}

export interface Category {
  id: number;
  name: string;
  description?: string;
  parentId?: number;
  scope: CategoryScope;
  userId?: string;
  householdId?: number;
  color: string;
  icon?: string;
  createdAt: string;
}

export enum CategoryScope {
  User = 0,
  Household = 1,
  System = 2
}

export interface Household {
  id: number;
  name: string;
  description?: string;
  createdAt: string;
  updatedAt: string;
  members?: HouseholdMember[];
}

export interface HouseholdMember {
  id: number;
  householdId: number;
  userId: string;
  role: HouseholdRole;
  joinedAt: string;
  user?: User;
}

export enum HouseholdRole {
  Member = 0,
  Administrator = 1
}

export interface CreateAccountDto {
  name: string;
  type: AccountType;
  sharingType: SharingType;
  householdId?: number;
  currentBalance: number;
  description?: string;
}

export interface CreateTransactionDto {
  accountId: number;
  date: string;
  amount: number;
  description: string;
  counterpart?: string;
  categoryId?: number;
  notes?: string;
  sharingStatus: SharingStatus;
  splits?: TransactionSplitDto[];
  tags?: string[];
}

export interface TransactionSplitDto {
  userId: string;
  percentage: number;
  notes?: string;
}

export interface CreateHouseholdDto {
  name: string;
  description?: string;
}

export interface ImportResult {
  success: boolean;
  error?: string;
  message?: string;
  importedCount: number;
  skippedCount: number;
  errorCount: number;
}

export interface CsvPreviewRow {
  rowNumber: number;
  data: { [key: string]: string };
}

// PSD2/Open Banking types
export interface BankConnection {
  id: number;
  bankName: string;
  bankId: string;
  status: ConnectionStatus;
  consentGivenAt: string;
  consentExpiresAt?: string;
  lastSyncAt?: string;
  errorMessage?: string;
  accountCount: number;
}

export enum ConnectionStatus {
  Pending = 0,
  Active = 1,
  ConsentExpired = 2,
  Error = 3,
  Disconnected = 4
}

export interface CreateBankConnectionDto {
  bankId: string;
  redirectUrl: string;
}

export interface BankConnectionResult {
  success: boolean;
  error?: string;
  authorizationUrl?: string;
  connectionId?: string;
}

export interface ExternalAccount {
  id: number;
  externalAccountId: string;
  accountName: string;
  accountNumber: string;
  accountType: ExternalAccountType;
  currentBalance: number;
  availableBalance?: number;
  currency: string;
  isActive: boolean;
  lastUpdated: string;
  linkedAccountId?: number;
  linkedAccountName?: string;
}

export enum ExternalAccountType {
  Current = 0,
  Savings = 1,
  CreditCard = 2,
  Loan = 3,
  Other = 4
}

export interface ExternalTransaction {
  id: number;
  externalTransactionId: string;
  date: string;
  amount: number;
  description: string;
  counterpart?: string;
  reference?: string;
  currency: string;
  isImported: boolean;
}

export interface ImportExternalTransactionsDto {
  externalAccountId: number;
  accountId: number;
  fromDate?: string;
  toDate?: string;
}

export interface LinkAccountDto {
  externalAccountId: number;
  accountId: number;
}

export interface Bank {
  id: string;
  name: string;
  logoUrl: string;
}