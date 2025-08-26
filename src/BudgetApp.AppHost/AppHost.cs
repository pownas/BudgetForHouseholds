using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

//builder.AddProject<Projects.BudgetApp_Blazor>("budgetapp-blazor");


//TODO : Add a proper database for production use

// Sqlite database resource (file-based for dev)
//var db = builder.AddSqlite("budgetdb", builder.Environment.IsDevelopment() ? "./data/budget.db" : "budget.db");

// API project
var api = builder.AddProject<Projects.BudgetApp_Api>("api");
    //TODO: Enable when using a shared database              
    // .WithReference(db);

// Blazor server project (web)
var web = builder.AddProject<Projects.BudgetApp_Blazor>("web")
                 .WithReference(api);

builder.Build().Run();
