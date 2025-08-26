var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.BudgetApp_Blazor>("budgetapp-blazor");

builder.Build().Run();
