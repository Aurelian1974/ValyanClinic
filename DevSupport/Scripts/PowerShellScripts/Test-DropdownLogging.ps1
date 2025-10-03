# SCRIPT PENTRU TESTAREA DROPDOWN-URILOR CU SERILOG
# Acest script porne?te aplica?ia ?i urm?re?te logurile pentru debugging

Write-Host "?? Starting ValyanClinic with detailed Serilog logging..." -ForegroundColor Green
Write-Host "?? Monitor logs for LocationDependentGridDropdowns debugging" -ForegroundColor Yellow
Write-Host ""

# Navigheaz? la directorul aplica?iei
Set-Location "D:\Projects\CMS\ValyanClinic"

# Porne?te aplica?ia cu logging detaliat
Write-Host "?? Starting application..." -ForegroundColor Cyan
dotnet run --environment Development

Write-Host ""
Write-Host "? Application stopped" -ForegroundColor Green