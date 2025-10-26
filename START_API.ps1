# Start the Data Abstraction API
Write-Host "Starting API server on http://localhost:5000" -ForegroundColor Green
Write-Host "Swagger UI will be available at: http://localhost:5000/swagger" -ForegroundColor Green
Write-Host ""
Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Yellow
Write-Host ""

Set-Location DataAbstractionAPI.API
dotnet run --urls "http://localhost:5000"

