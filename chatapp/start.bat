@echo off
echo Starting ChatApp...
echo.

echo [1/2] Starting backend (ASP.NET Core)...
start "ChatApp Backend" cmd /k "cd ChatApp.Api && dotnet run"

echo Waiting for backend to start...
timeout /t 5 /nobreak >nul

echo [2/2] Starting frontend (Vite)...
start "ChatApp Frontend" cmd /k "cd client && npm install && npm run dev"

echo.
echo ✓ ChatApp starting up!
echo   Backend:  http://localhost:5000
echo   Frontend: http://localhost:5173
echo.
pause
