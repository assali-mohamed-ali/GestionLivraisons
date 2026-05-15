# GestionLivraisons — Start All Microservices
# Run this script from the solution root directory

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  GestionLivraisons — Starting Services" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Start each service in a new PowerShell window
Write-Host "[1/7] Starting Identity.API (port 5001)..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\src\Services\Identity.API'; dotnet run"

Start-Sleep -Seconds 3

Write-Host "[2/7] Starting Colis.API (port 5002)..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\src\Services\Colis.API'; dotnet run"

Write-Host "[3/7] Starting Livreur.API (port 5003)..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\src\Services\Livreur.API'; dotnet run"

Write-Host "[4/7] Starting Client.API (port 5004)..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\src\Services\Client.API'; dotnet run"

Start-Sleep -Seconds 3

Write-Host "[5/7] Starting Dashboard.API (port 5005)..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\src\Services\Dashboard.API'; dotnet run"

Write-Host "[6/7] Starting Gateway (port 5000)..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\src\Gateway'; dotnet run"

Start-Sleep -Seconds 2

Write-Host "[7/7] Starting Web Frontend (port 5006)..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\src\Frontend\GestionLivraisons.Web'; dotnet run"

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  All services started!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "  Web App:     http://localhost:5006" -ForegroundColor White
Write-Host "  Gateway:     http://localhost:5000" -ForegroundColor White
Write-Host "  Identity:    http://localhost:5001/swagger" -ForegroundColor DarkGray
Write-Host "  Colis:       http://localhost:5002/swagger" -ForegroundColor DarkGray
Write-Host "  Livreur:     http://localhost:5003/swagger" -ForegroundColor DarkGray
Write-Host "  Client:      http://localhost:5004/swagger" -ForegroundColor DarkGray
Write-Host "  Dashboard:   http://localhost:5005/swagger" -ForegroundColor DarkGray
Write-Host ""
Write-Host "  Login Credentials:" -ForegroundColor Cyan
Write-Host "  Admin: admin@livraisons.com / Admin123!" -ForegroundColor White
Write-Host "  User:  user@livraisons.com  / User123!" -ForegroundColor White
Write-Host ""
