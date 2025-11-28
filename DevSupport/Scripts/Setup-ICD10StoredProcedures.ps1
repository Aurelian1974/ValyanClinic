# =============================================
# SETUP ICD-10 STORED PROCEDURES
# Ruleaza automat scriptul SQL pentru creare SP-uri
# =============================================

param(
    [string]$AppSettingsPath = "..\..\ValyanClinic\appsettings.json",
    [string]$SqlScriptPath = "..\Database\StoredProcedures\ICD10\Create-ICD10StoredProcedures.sql"
)

# Colors
$ColorSuccess = "Green"
$ColorError = "Red"
$ColorWarning = "Yellow"
$ColorInfo = "Cyan"

Write-Host "========================================" -ForegroundColor $ColorInfo
Write-Host "SETUP ICD-10 STORED PROCEDURES" -ForegroundColor $ColorInfo
Write-Host "========================================" -ForegroundColor $ColorInfo
Write-Host ""

# =============================================
# 1. READ CONNECTION STRING
# =============================================
Write-Host "1. Citire connection string..." -ForegroundColor $ColorInfo

$appSettingsFullPath = Join-Path $PSScriptRoot $AppSettingsPath
if (-not (Test-Path $appSettingsFullPath)) {
    Write-Host "   ? EROARE: appsettings.json nu a fost gasit" -ForegroundColor $ColorError
    exit 1
}

$appSettings = Get-Content $appSettingsFullPath -Raw | ConvertFrom-Json
$connectionString = $appSettings.ConnectionStrings.DefaultConnection

if ([string]::IsNullOrEmpty($connectionString)) {
    Write-Host "   ? EROARE: Connection string lipseste" -ForegroundColor $ColorError
    exit 1
}

if ($connectionString -match "Database=([^;]+)") {
    $databaseName = $matches[1]
    Write-Host "   ? Database: $databaseName" -ForegroundColor $ColorSuccess
} else {
    Write-Host "   ? EROARE: Nu s-a putut extrage database name" -ForegroundColor $ColorError
    exit 1
}

Write-Host ""

# =============================================
# 2. CHECK SQL SCRIPT EXISTS
# =============================================
Write-Host "2. Verificare script SQL..." -ForegroundColor $ColorInfo

$sqlScriptFullPath = Join-Path $PSScriptRoot $SqlScriptPath
if (-not (Test-Path $sqlScriptFullPath)) {
    Write-Host "   ? EROARE: Script SQL nu a fost gasit la: $sqlScriptFullPath" -ForegroundColor $ColorError
    exit 1
}

Write-Host "   ? Script gasit: $(Split-Path $sqlScriptFullPath -Leaf)" -ForegroundColor $ColorSuccess
Write-Host ""

# =============================================
# 3. EXECUTE SQL SCRIPT
# =============================================
Write-Host "3. Executare script SQL..." -ForegroundColor $ColorInfo
Write-Host "   Se creaza stored procedures ICD-10..." -ForegroundColor $ColorInfo
Write-Host ""

try {
    # Execute SQL script using sqlcmd
    $sqlcmdOutput = & sqlcmd -S "(localdb)\MSSQLLocalDB" -d $databaseName -i $sqlScriptFullPath -b 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ? Script executat cu succes!" -ForegroundColor $ColorSuccess
        
        # Display output
        if ($sqlcmdOutput) {
            Write-Host ""
            Write-Host "   OUTPUT SQL:" -ForegroundColor $ColorInfo
            $sqlcmdOutput | ForEach-Object {
                if ($_ -match "?") {
                    Write-Host "   $_" -ForegroundColor $ColorSuccess
                } elseif ($_ -match "EROARE|ERROR") {
                    Write-Host "   $_" -ForegroundColor $ColorError
                } else {
                    Write-Host "   $_" -ForegroundColor $ColorInfo
                }
            }
        }
    } else {
        Write-Host "   ? EROARE la executarea scriptului SQL!" -ForegroundColor $ColorError
        Write-Host ""
        Write-Host "   OUTPUT:" -ForegroundColor $ColorWarning
        $sqlcmdOutput | ForEach-Object { Write-Host "   $_" -ForegroundColor $ColorWarning }
        exit 1
    }
}
catch {
    Write-Host "   ? EXCEPTIE: $_" -ForegroundColor $ColorError
    exit 1
}

Write-Host ""

# =============================================
# 4. VERIFY STORED PROCEDURES
# =============================================
Write-Host "4. Verificare Stored Procedures create..." -ForegroundColor $ColorInfo

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()
    
    $verifyQuery = @"
SELECT COUNT(*) as TotalProcedures
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_TYPE = 'PROCEDURE'
AND ROUTINE_SCHEMA = 'dbo'
AND ROUTINE_NAME LIKE 'sp_ICD10_%'
"@
    
    $command = $connection.CreateCommand()
    $command.CommandText = $verifyQuery
    $totalProcedures = [int]$command.ExecuteScalar()
    
    Write-Host "   ? Total Stored Procedures ICD-10: $totalProcedures" -ForegroundColor $ColorSuccess
    
    if ($totalProcedures -eq 9) {
        Write-Host "   ? PERFECT! Toate 9 SP-uri au fost create." -ForegroundColor $ColorSuccess
    } elseif ($totalProcedures -gt 0) {
        Write-Host "   ? ATENTIE: S-au creat $totalProcedures SP-uri (asteptate: 9)" -ForegroundColor $ColorWarning
    } else {
        Write-Host "   ? EROARE: Niciun SP nu a fost creat!" -ForegroundColor $ColorError
    }
    
    $connection.Close()
}
catch {
    Write-Host "   ? EROARE la verificare: $_" -ForegroundColor $ColorError
}

Write-Host ""

# =============================================
# 5. RUN VERIFICATION SCRIPT
# =============================================
Write-Host "5. Rulare verificare completa..." -ForegroundColor $ColorInfo
Write-Host ""

$verifyScriptPath = Join-Path $PSScriptRoot "Verify-ICD10Database.ps1"
if (Test-Path $verifyScriptPath) {
    & $verifyScriptPath
} else {
    Write-Host "   ? Script verificare nu a fost gasit (optional)" -ForegroundColor $ColorWarning
}

Write-Host ""
Write-Host "========================================" -ForegroundColor $ColorInfo
Write-Host "SETUP FINALIZAT!" -ForegroundColor $ColorInfo
Write-Host "========================================" -ForegroundColor $ColorInfo
Write-Host ""
Write-Host "Next steps:" -ForegroundColor $ColorInfo
Write-Host "  1. Build aplicatia: dotnet build" -ForegroundColor $ColorInfo
Write-Host "  2. Ruleaza aplicatia: dotnet run --project ValyanClinic" -ForegroundColor $ColorInfo
Write-Host "  3. Testeaza ICD10AutocompleteComponent in Consultatie Modal" -ForegroundColor $ColorInfo
Write-Host ""
