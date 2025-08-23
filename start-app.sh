#!/bin/bash
# Startar både backend och frontend för BudgetForHouseholds

# Starta backend (API)
echo "Startar backend..."
dotnet run --project BudgetApp.Api &
BACKEND_PID=$!

# Vänta kort så backend hinner starta
sleep 3

# Starta frontend
cd budget-app-frontend
echo "Startar frontend..."
npm start &
FRONTEND_PID=$!

# Info
echo "Backend PID: $BACKEND_PID"
echo "Frontend PID: $FRONTEND_PID"
echo "Backend kör på http://localhost:5000 eller enligt launchSettings.json"
echo "Frontend kör på http://localhost:3000"

# Vänta på att båda processer avslutas
wait $BACKEND_PID
wait $FRONTEND_PID
