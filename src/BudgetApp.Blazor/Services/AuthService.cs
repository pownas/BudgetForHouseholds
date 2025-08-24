using BudgetApp.Blazor.Models;

namespace BudgetApp.Blazor.Services;

public interface IAuthService
{
    event Action<User?> AuthStateChanged;
    User? CurrentUser { get; }
    bool IsAuthenticated { get; }
    Task<AuthResult> LoginAsync(string email, string password);
    Task<AuthResult> RegisterAsync(string firstName, string lastName, string email, string password);
    Task LogoutAsync();
    Task InitializeAsync();
}

public class AuthService : IAuthService
{
    private readonly IApiService _apiService;
    private User? _currentUser;

    public event Action<User?>? AuthStateChanged;
    public User? CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser != null;

    public AuthService(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task InitializeAsync()
    {
        // In a real app, you'd check for stored tokens and validate them
        // For now, we'll just set the user as not authenticated
        await Task.CompletedTask;
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        var result = await _apiService.LoginAsync(new LoginDto { Email = email, Password = password });
        
        if (result.Success && result.User != null)
        {
            _currentUser = result.User;
            AuthStateChanged?.Invoke(_currentUser);
        }

        return result;
    }

    public async Task<AuthResult> RegisterAsync(string firstName, string lastName, string email, string password)
    {
        var result = await _apiService.RegisterAsync(new RegisterDto 
        { 
            FirstName = firstName, 
            LastName = lastName, 
            Email = email, 
            Password = password 
        });
        
        if (result.Success && result.User != null)
        {
            _currentUser = result.User;
            AuthStateChanged?.Invoke(_currentUser);
        }

        return result;
    }

    public async Task LogoutAsync()
    {
        _apiService.Logout();
        _currentUser = null;
        AuthStateChanged?.Invoke(null);
        await Task.CompletedTask;
    }
}