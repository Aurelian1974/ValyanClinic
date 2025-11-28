# =============================================
# RUN SQL MIGRATION - Add CoduriICD10Secundare
# =============================================

param(
    [string]$AppSettingsPath = "..\..\ValyanClinic\appsettings.json",
    [string]$SqlScriptPath = "..\Database\Migrations\Add-CoduriICD10Secundare-Column.sql"
)

$ColorSuccess = "Green"
$ColorError = "Red"
$ColorInfo = "Cyan"

Write-Host "========================================" -ForegroundColor $ColorInfo
Write-Host "MIGRATION: Add CoduriICD10Secundare" -ForegroundColor $ColorInfo
Write-Host "========================================" -ForegroundColor $ColorInfo
Write-Host ""

# Read connection string
$appSettingsFullPath = Join-Path $PSScriptRoot $AppSettingsPath
$appSettings = Get-Content $appSettingsFullPath -Raw | ConvertFrom-Json
$connectionString = $appSettings.ConnectionStrings.DefaultConnection

# Connect to database
$connection = New-Object System.Data.SqlClient.SqlConnection
$connection.ConnectionString = $connectionString
$connection.Open()

Write-Host "? Conectat la database" -ForegroundColor $ColorSuccess
Write-Host ""

# Read and execute SQL script
$sqlScriptFullPath = Join-Path $PSScriptRoot $SqlScriptPath
$sqlScript = Get-Content $sqlScriptFullPath -Raw

try {
    $command = $connection.CreateCommand()
    $command.CommandText = $sqlScript
    $command.ExecuteNonQuery() | Out-Null
    
    Write-Host "? Migration executat cu succes!" -ForegroundColor $ColorSuccess
}
catch {
    Write-Host "? EROARE la executarea migration-ului: $_" -ForegroundColor $ColorError
}
finally {
    $connection.Close()
}

Write-Host ""
