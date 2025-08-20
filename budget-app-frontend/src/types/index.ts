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
  attachments?: TransactionAttachment[];
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

export interface TransactionAttachment {
  id: number;
  transactionId: number;
  fileName: string;
  filePath: string;
  contentType?: string;
  fileSize: number;
  fileHash?: string;
  uploadedAt: string;
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