using BudgetApp.Blazor.Components;
using BudgetApp.Blazor.Models;
using BudgetApp.Blazor.Services;
using MudBlazor.Services;
using BudgetApp.Aspire.ServiceDefaults; // added

var builder = WebApplication.CreateBuilder(args);

// Shared defaults
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add MudBlazor services
builder.Services.AddMudServices();

// Add HttpClient for API calls
builder.Services.AddHttpClient();

// Add API service configuration (Aspire can override via service discovery later)
builder.Services.Configure<ApiConfiguration>(options =>
{
    options.BaseUrl = builder.Configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "http://localhost:5300/api";
});

// Add application services
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHealthChecks("/health");

app.Run();
