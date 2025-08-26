using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BudgetApp.Api.Data;
using BudgetApp.Api.Models;
using BudgetApp.Api.Services;
using BudgetApp.Aspire.ServiceDefaults; // added

var builder = WebApplication.CreateBuilder(args);

// Shared defaults (health checks, telemetry, resilience)
builder.AddServiceDefaults();

// Add services to the container.
// Database (will be overridden by Aspire connection string named "budgetdb" if present)
var connectionString = builder.Configuration.GetConnectionString("budgetdb")
                       ?? builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Data Source=budgetapp.db";
builder.Services.AddDbContext<BudgetAppDbContext>(options =>
    options.UseSqlite(connectionString));

// Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<BudgetAppDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "BudgetApp_SecretKey_ForDevelopment_Only_2025!";
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICsvImportService, CsvImportService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IHouseholdService, HouseholdService>();
builder.Services.AddScoped<IPsd2Service, Psd2Service>();
builder.Services.AddScoped<IPsd2EventLogService, Psd2EventLogService>();
builder.Services.AddScoped<IReceiptAttachmentService, ReceiptAttachmentService>();

// Background Services
builder.Services.AddHostedService<BudgetApp.Api.BackgroundServices.Psd2SyncBackgroundService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Budget App API", Version = "v1" });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BudgetAppDbContext>();
    context.Database.EnsureCreated();

    // Skapa demoanvändare om den inte finns
    var userManager = scope.ServiceProvider.GetService<UserManager<User>>();
    if (userManager != null)
    {
        var demoUser = await userManager.FindByEmailAsync("demo@demo.se");
        if (demoUser == null)
        {
            var newUser = new User
            {
                UserName = "demo@demo.se",
                Email = "demo@demo.se",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(newUser, "Demo123!");
        }
    }
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
