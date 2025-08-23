#!/bin/bash
# Startar backend och Blazor-frontend parallellt

# Starta backend
echo "Startar backend..."
dotnet run --project BudgetApp.Api &
BACKEND_PID=$!

# Vänta kort så backend hinner starta
sleep 3

# Starta Blazor-frontend
cd BudgetApp.Blazor
echo "Startar Blazor-frontend..."
dotnet run &
FRONTEND_PID=$!
cd ..

# Info
echo "Backend PID: $BACKEND_PID"
echo "Frontend PID: $FRONTEND_PID"
echo "Backend kör på http://localhost:5300"
echo "Blazor-frontend kör på http://localhost:5000"

# Vänta på att båda processer avslutas
wait $BACKEND_PID
wait $FRONTEND_PID
