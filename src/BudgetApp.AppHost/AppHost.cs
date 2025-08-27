using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Sqlite database resource (file-based for dev)
var db = builder.AddSqlite("budgetdb", builder.Environment.IsDevelopment() ? "./data/budget.db" : "budget.db");

// API project
var api = builder.AddProject<Projects.BudgetApp_Api>("api")          
                 .WithReference(db);

// Blazor server project (web)
var web = builder.AddProject<Projects.BudgetApp_Blazor>("web")
                 .WithReference(api);

builder.Build().Run();
