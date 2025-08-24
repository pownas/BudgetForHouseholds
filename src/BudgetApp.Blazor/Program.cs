using BudgetApp.Blazor.Components;
using BudgetApp.Blazor.Models;
using BudgetApp.Blazor.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add MudBlazor services
builder.Services.AddMudServices();

// Add HttpClient for API calls
builder.Services.AddHttpClient();

// Add API service configuration
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
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
