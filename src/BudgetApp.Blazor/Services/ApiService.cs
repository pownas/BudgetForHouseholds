using BudgetApp.Blazor.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;

namespace BudgetApp.Blazor.Services;

public interface IApiService
{
    // Auth methods
    Task<AuthResult> LoginAsync(LoginDto loginDto);
    Task<AuthResult> RegisterAsync(RegisterDto registerDto);
    void Logout();

    // Account methods
    Task<List<Account>> GetAccountsAsync();
    Task<Account> GetAccountAsync(int id);
    Task<Account> CreateAccountAsync(CreateAccountDto accountDto);
    Task<Account> UpdateAccountAsync(int id, CreateAccountDto accountDto);
    Task DeleteAccountAsync(int id);

    // Transaction methods
    Task<List<Transaction>> GetTransactionsAsync();
    Task<Transaction> GetTransactionAsync(int id);
    Task<Transaction> CreateTransactionAsync(CreateTransactionDto transactionDto);
    Task<Transaction> UpdateTransactionAsync(int id, CreateTransactionDto transactionDto);
    Task DeleteTransactionAsync(int id);

    // Household methods
    Task<List<Household>> GetHouseholdsAsync();
    Task<Household> GetHouseholdAsync(int id);
    Task<Household> CreateHouseholdAsync(CreateHouseholdDto householdDto);
    Task<Household> UpdateHouseholdAsync(int id, CreateHouseholdDto householdDto);
    Task DeleteHouseholdAsync(int id);

    // Category methods
    Task<List<Category>> GetCategoriesAsync();

    // Import methods
    Task<ImportResult> ImportCsvAsync(Stream fileStream, string fileName);

    // PSD2 methods
    Task<List<BankConnection>> GetBankConnectionsAsync();
    Task<List<Bank>> GetBanksAsync();
    Task<BankConnectionResult> CreateBankConnectionAsync(CreateBankConnectionDto dto);
    Task CompleteBankConnectionAsync(string connectionId, string authorizationCode);
    Task DisconnectBankAsync(int connectionId);
    Task SyncBankConnectionAsync(int connectionId);
    Task<List<ExternalAccount>> GetExternalAccountsAsync(int connectionId);
    Task<List<ExternalTransaction>> GetExternalTransactionsAsync(int externalAccountId);
    Task<ImportResult> ImportExternalTransactionsAsync(ImportExternalTransactionsDto dto);
    Task LinkAccountAsync(LinkAccountDto dto);
    Task<ConsentCheckResult> CheckConsentExpiryAsync();
}

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly ApiConfiguration _config;
    private readonly JsonSerializerOptions _jsonOptions;
    private string? _token;

    public ApiService(HttpClient httpClient, IOptions<ApiConfiguration> config)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        // Check for existing token
        // Note: In a real application, you'd want to use a more secure storage mechanism
        // This is simplified for the prototype
    }

    private void SetAuthHeader(string token)
    {
        _token = token;
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    private void RemoveAuthHeader()
    {
        _token = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    private async Task<T> GetAsync<T>(string endpoint)
    {
        var response = await _httpClient.GetAsync($"{_config.BaseUrl}/{endpoint}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, _jsonOptions)!;
    }

    private async Task<T> PostAsync<T>(string endpoint, object data)
    {
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{_config.BaseUrl}/{endpoint}", content);
        response.EnsureSuccessStatusCode();
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(responseJson, _jsonOptions)!;
    }

    private async Task<T> PutAsync<T>(string endpoint, object data)
    {
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"{_config.BaseUrl}/{endpoint}", content);
        response.EnsureSuccessStatusCode();
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(responseJson, _jsonOptions)!;
    }

    private async Task DeleteAsync(string endpoint)
    {
        var response = await _httpClient.DeleteAsync($"{_config.BaseUrl}/{endpoint}");
        response.EnsureSuccessStatusCode();
    }

    // Auth methods
    public async Task<AuthResult> LoginAsync(LoginDto loginDto)
    {
        try
        {
            var result = await PostAsync<AuthResult>("auth/login", loginDto);
            if (result.Success && !string.IsNullOrEmpty(result.Token))
            {
                SetAuthHeader(result.Token);
            }
            return result;
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<AuthResult> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            var result = await PostAsync<AuthResult>("auth/register", registerDto);
            if (result.Success && !string.IsNullOrEmpty(result.Token))
            {
                SetAuthHeader(result.Token);
            }
            return result;
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, Error = ex.Message };
        }
    }

    public void Logout()
    {
        RemoveAuthHeader();
    }

    // Account methods
    public async Task<List<Account>> GetAccountsAsync()
    {
        return await GetAsync<List<Account>>("accounts");
    }

    public async Task<Account> GetAccountAsync(int id)
    {
        return await GetAsync<Account>($"accounts/{id}");
    }

    public async Task<Account> CreateAccountAsync(CreateAccountDto accountDto)
    {
        return await PostAsync<Account>("accounts", accountDto);
    }

    public async Task<Account> UpdateAccountAsync(int id, CreateAccountDto accountDto)
    {
        return await PutAsync<Account>($"accounts/{id}", accountDto);
    }

    public async Task DeleteAccountAsync(int id)
    {
        await DeleteAsync($"accounts/{id}");
    }

    // Transaction methods
    public async Task<List<Transaction>> GetTransactionsAsync()
    {
        return await GetAsync<List<Transaction>>("transactions");
    }

    public async Task<Transaction> GetTransactionAsync(int id)
    {
        return await GetAsync<Transaction>($"transactions/{id}");
    }

    public async Task<Transaction> CreateTransactionAsync(CreateTransactionDto transactionDto)
    {
        return await PostAsync<Transaction>("transactions", transactionDto);
    }

    public async Task<Transaction> UpdateTransactionAsync(int id, CreateTransactionDto transactionDto)
    {
        return await PutAsync<Transaction>($"transactions/{id}", transactionDto);
    }

    public async Task DeleteTransactionAsync(int id)
    {
        await DeleteAsync($"transactions/{id}");
    }

    // Household methods
    public async Task<List<Household>> GetHouseholdsAsync()
    {
        return await GetAsync<List<Household>>("households");
    }

    public async Task<Household> GetHouseholdAsync(int id)
    {
        return await GetAsync<Household>($"households/{id}");
    }

    public async Task<Household> CreateHouseholdAsync(CreateHouseholdDto householdDto)
    {
        return await PostAsync<Household>("households", householdDto);
    }

    public async Task<Household> UpdateHouseholdAsync(int id, CreateHouseholdDto householdDto)
    {
        return await PutAsync<Household>($"households/{id}", householdDto);
    }

    public async Task DeleteHouseholdAsync(int id)
    {
        await DeleteAsync($"households/{id}");
    }

    // Category methods
    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await GetAsync<List<Category>>("categories");
    }

    // Import methods
    public async Task<ImportResult> ImportCsvAsync(Stream fileStream, string fileName)
    {
        using var formData = new MultipartFormDataContent();
        var streamContent = new StreamContent(fileStream);
        formData.Add(streamContent, "file", fileName);

        var response = await _httpClient.PostAsync($"{_config.BaseUrl}/import/csv", formData);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ImportResult>(json, _jsonOptions)!;
    }

    // PSD2 methods
    public async Task<List<BankConnection>> GetBankConnectionsAsync()
    {
        return await GetAsync<List<BankConnection>>("psd2/connections");
    }

    public async Task<List<Bank>> GetBanksAsync()
    {
        return await GetAsync<List<Bank>>("psd2/banks");
    }

    public async Task<BankConnectionResult> CreateBankConnectionAsync(CreateBankConnectionDto dto)
    {
        return await PostAsync<BankConnectionResult>("psd2/connections", dto);
    }

    public async Task<List<ExternalAccount>> GetExternalAccountsAsync(int connectionId)
    {
        return await GetAsync<List<ExternalAccount>>($"psd2/connections/{connectionId}/accounts");
    }

    public async Task<List<ExternalTransaction>> GetExternalTransactionsAsync(int externalAccountId)
    {
        return await GetAsync<List<ExternalTransaction>>($"psd2/accounts/{externalAccountId}/transactions");
    }

    public async Task<ImportResult> ImportExternalTransactionsAsync(ImportExternalTransactionsDto dto)
    {
        return await PostAsync<ImportResult>("psd2/import", dto);
    }

    public async Task LinkAccountAsync(LinkAccountDto dto)
    {
        await PostAsync<object>("psd2/link", dto);
    }

    public async Task CompleteBankConnectionAsync(string connectionId, string authorizationCode)
    {
        await PostAsync<object>($"psd2/connections/{connectionId}/complete", authorizationCode);
    }

    public async Task DisconnectBankAsync(int connectionId)
    {
        await DeleteAsync($"psd2/connections/{connectionId}");
    }

    public async Task SyncBankConnectionAsync(int connectionId)
    {
        await PostAsync<object>($"psd2/connections/{connectionId}/sync", new { });
    }

    public async Task<ConsentCheckResult> CheckConsentExpiryAsync()
    {
        return await GetAsync<ConsentCheckResult>("psd2/consent-check");
    }
}