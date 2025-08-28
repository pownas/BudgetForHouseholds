if [ -n "$LOGIN_URL" ]; then
	echo "Login-URL: $LOGIN_URL"
	# Försök öppna i webbläsaren
		if [ -n "$CODESPACES" ] || [ -n "$GITHUB_CODESPACE_TOKEN" ]; then
			# GitHub Codespaces
			gp open "$LOGIN_URL"
		elif [ "$(uname)" = "Darwin" ]; then
			# macOS
			open "$LOGIN_URL"
		elif grep -qEi "cygwin|mingw|msys" <<< "$(uname)"; then
			# Windows (Git Bash, WSL)
			cmd.exe /C start "$LOGIN_URL"
		elif [ -n "$BROWSER" ]; then
			"$BROWSER" "$LOGIN_URL"
		else
			xdg-open "$LOGIN_URL" >/dev/null 2>&1 &
		fi
else
	echo "Ingen login-url hittades automatiskt. Kolla aspire.log om du behöver den."
fi
#!/bin/bash

echo "Bygger hela lösningen..."
dotnet build

echo "Startar Aspire AppHost (dashboard)..."
# Kör AppHost och extrahera login-url från utdata
dotnet run --project src/BudgetApp.AppHost | tee aspire.log &
APPHOST_PID=$!

# Vänta några sekunder så att AppHost hinner starta
sleep 5

# Försök hitta login-url i loggen
LOGIN_URL=$(grep -Eo 'http://localhost:[0-9]+/login\?code=[A-Za-z0-9]+' aspire.log | head -n1)

echo "Aspire dashboard: http://localhost:18888"
if [ -n "$LOGIN_URL" ]; then
	echo "Login-URL: $LOGIN_URL"
else
	echo "Ingen login-url hittades automatiskt. Kolla aspire.log om du behöver den."
fi
echo "Frontend PID: $FRONTEND_PID"
echo "Backend kör på http://localhost:5300"
echo "Blazor-frontend kör på http://localhost:5000"

# Vänta på att båda processer avslutas
wait $BACKEND_PID
wait $FRONTEND_PID
