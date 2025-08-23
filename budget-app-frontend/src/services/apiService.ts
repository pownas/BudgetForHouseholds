import axios from 'axios';
import { 
  LoginDto, 
  RegisterDto, 
  AuthResult, 
  Account, 
  CreateAccountDto, 
  Transaction, 
  CreateTransactionDto, 
  Household, 
  CreateHouseholdDto,
  ImportResult,
  CsvPreviewRow,
  Category,
  BankConnection,
  CreateBankConnectionDto,
  BankConnectionResult,
  ExternalAccount,
  ExternalTransaction,
  ImportExternalTransactionsDto,
  LinkAccountDto,
  Bank
} from '../types';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5300/api';

class ApiService {
  private token: string | null = null;

  constructor() {
    this.token = localStorage.getItem('token');
    if (this.token) {
      this.setAuthHeader(this.token);
    }
  }

  private setAuthHeader(token: string) {
    axios.defaults.headers.common['Authorization'] = `Bearer ${token}`;
  }

  private removeAuthHeader() {
    delete axios.defaults.headers.common['Authorization'];
  }

  // Auth methods
  async login(loginDto: LoginDto): Promise<AuthResult> {
    try {
      const response = await axios.post(`${API_BASE_URL}/auth/login`, loginDto);
      const { token, user } = response.data;
      
      this.token = token;
      localStorage.setItem('token', token);
      this.setAuthHeader(token);
      
      return { success: true, token, user };
    } catch (error: any) {
      return { 
        success: false, 
        error: error.response?.data?.error || 'Login failed' 
      };
    }
  }

  async register(registerDto: RegisterDto): Promise<AuthResult> {
    try {
      const response = await axios.post(`${API_BASE_URL}/auth/register`, registerDto);
      const { token, user } = response.data;
      
      this.token = token;
      localStorage.setItem('token', token);
      this.setAuthHeader(token);
      
      return { success: true, token, user };
    } catch (error: any) {
      return { 
        success: false, 
        error: error.response?.data?.error || 'Registration failed' 
      };
    }
  }

  logout() {
    this.token = null;
    localStorage.removeItem('token');
    this.removeAuthHeader();
  }

  // Account methods
  async getAccounts(): Promise<Account[]> {
    const response = await axios.get(`${API_BASE_URL}/accounts`);
    return response.data;
  }

  async getAccount(id: number): Promise<Account> {
    const response = await axios.get(`${API_BASE_URL}/accounts/${id}`);
    return response.data;
  }

  async createAccount(accountDto: CreateAccountDto): Promise<Account> {
    const response = await axios.post(`${API_BASE_URL}/accounts`, accountDto);
    return response.data;
  }

  async updateAccount(id: number, accountDto: CreateAccountDto): Promise<Account> {
    const response = await axios.put(`${API_BASE_URL}/accounts/${id}`, accountDto);
    return response.data;
  }

  async deleteAccount(id: number): Promise<void> {
    await axios.delete(`${API_BASE_URL}/accounts/${id}`);
  }

  async getAccountBalance(id: number): Promise<{ accountId: number; currentBalance: number; lastUpdated: string }> {
    const response = await axios.get(`${API_BASE_URL}/accounts/${id}/balance`);
    return response.data;
  }

  // Transaction methods
  async getTransactions(accountId?: number, householdId?: number): Promise<Transaction[]> {
    const params = new URLSearchParams();
    if (accountId) params.append('accountId', accountId.toString());
    if (householdId) params.append('householdId', householdId.toString());
    
    const response = await axios.get(`${API_BASE_URL}/transactions?${params}`);
    return response.data;
  }

  async getTransaction(id: number): Promise<Transaction> {
    const response = await axios.get(`${API_BASE_URL}/transactions/${id}`);
    return response.data;
  }

  async createTransaction(transactionDto: CreateTransactionDto): Promise<Transaction> {
    const response = await axios.post(`${API_BASE_URL}/transactions`, transactionDto);
    return response.data;
  }

  async updateTransaction(id: number, transactionDto: Partial<CreateTransactionDto>): Promise<Transaction> {
    const response = await axios.put(`${API_BASE_URL}/transactions/${id}`, transactionDto);
    return response.data;
  }

  async deleteTransaction(id: number): Promise<void> {
    await axios.delete(`${API_BASE_URL}/transactions/${id}`);
  }

  // Household methods
  async getHouseholds(): Promise<Household[]> {
    const response = await axios.get(`${API_BASE_URL}/households`);
    return response.data;
  }

  async getHousehold(id: number): Promise<Household> {
    const response = await axios.get(`${API_BASE_URL}/households/${id}`);
    return response.data;
  }

  async createHousehold(householdDto: CreateHouseholdDto): Promise<Household> {
    const response = await axios.post(`${API_BASE_URL}/households`, householdDto);
    return response.data;
  }

  async addHouseholdMember(householdId: number, email: string): Promise<void> {
    await axios.post(`${API_BASE_URL}/households/${householdId}/members`, JSON.stringify(email), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  async removeHouseholdMember(householdId: number, memberUserId: string): Promise<void> {
    await axios.delete(`${API_BASE_URL}/households/${householdId}/members/${memberUserId}`);
  }

  // Import methods
  async previewCsv(file: File): Promise<CsvPreviewRow[]> {
    const formData = new FormData();
    formData.append('file', file);
    
    const response = await axios.post(`${API_BASE_URL}/import/csv/preview`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
    return response.data;
  }

  async importCsv(file: File, accountId: number): Promise<ImportResult> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('accountId', accountId.toString());
    
    const response = await axios.post(`${API_BASE_URL}/import/csv`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
    return response.data;
  }

  // Categories (assuming we'll add this endpoint later)
  async getCategories(): Promise<Category[]> {
    try {
      const response = await axios.get(`${API_BASE_URL}/categories`);
      return response.data;
    } catch (error) {
      // For now, return the default categories from the types
      return [
        { id: 1, name: 'Boende', color: '#FF6B6B', icon: 'home', scope: 2, createdAt: '' },
        { id: 2, name: 'Mat', color: '#4ECDC4', icon: 'restaurant', scope: 2, createdAt: '' },
        { id: 3, name: 'Transport', color: '#45B7D1', icon: 'directions_car', scope: 2, createdAt: '' },
        { id: 4, name: 'Försäkring', color: '#96CEB4', icon: 'security', scope: 2, createdAt: '' },
        { id: 5, name: 'Barn', color: '#FFEAA7', icon: 'child_care', scope: 2, createdAt: '' },
        { id: 6, name: 'Nöje', color: '#DDA0DD', icon: 'movie', scope: 2, createdAt: '' },
        { id: 7, name: 'Hälsa', color: '#98D8C8', icon: 'local_hospital', scope: 2, createdAt: '' },
        { id: 8, name: 'Inkomst', color: '#81C784', icon: 'account_balance_wallet', scope: 2, createdAt: '' },
        { id: 9, name: 'Övrigt', color: '#B0BEC5', icon: 'category', scope: 2, createdAt: '' }
      ];
    }
  }

  // PSD2/Open Banking methods
  async getBankConnections(): Promise<BankConnection[]> {
    const response = await axios.get(`${API_BASE_URL}/psd2/connections`);
    return response.data;
  }

  async createBankConnection(dto: CreateBankConnectionDto): Promise<BankConnectionResult> {
    const response = await axios.post(`${API_BASE_URL}/psd2/connections`, dto);
    return response.data;
  }

  async completeBankConnection(connectionId: string, authorizationCode: string): Promise<void> {
    await axios.post(`${API_BASE_URL}/psd2/connections/${connectionId}/complete`, authorizationCode, {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  async disconnectBank(connectionId: number): Promise<void> {
    await axios.delete(`${API_BASE_URL}/psd2/connections/${connectionId}`);
  }

  async getExternalAccounts(connectionId: number): Promise<ExternalAccount[]> {
    const response = await axios.get(`${API_BASE_URL}/psd2/connections/${connectionId}/accounts`);
    return response.data;
  }

  async getExternalTransactions(externalAccountId: number, fromDate?: string, toDate?: string): Promise<ExternalTransaction[]> {
    const params = new URLSearchParams();
    if (fromDate) params.append('fromDate', fromDate);
    if (toDate) params.append('toDate', toDate);
    
    const response = await axios.get(`${API_BASE_URL}/psd2/accounts/${externalAccountId}/transactions?${params}`);
    return response.data;
  }

  async importExternalTransactions(dto: ImportExternalTransactionsDto): Promise<ImportResult> {
    const response = await axios.post(`${API_BASE_URL}/psd2/import`, dto);
    return response.data;
  }

  async syncBankConnection(connectionId: number): Promise<void> {
    await axios.post(`${API_BASE_URL}/psd2/connections/${connectionId}/sync`);
  }

  async linkAccount(dto: LinkAccountDto): Promise<void> {
    await axios.post(`${API_BASE_URL}/psd2/accounts/link`, dto);
  }

  async checkConsentExpiry(): Promise<{ hasExpiringConsents: boolean }> {
    const response = await axios.get(`${API_BASE_URL}/psd2/consent-check`);
    return response.data;
  }

  async getAvailableBanks(): Promise<Bank[]> {
    const response = await axios.get(`${API_BASE_URL}/psd2/banks`);
    return response.data;
  }
}

export const apiService = new ApiService();