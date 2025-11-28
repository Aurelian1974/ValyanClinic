# =============================================
# VERIFY ICD-10 DATABASE SETUP
# Verific? existen?a tabelului ?i stored procedures pentru ICD-10
# =============================================

param(
    [string]$AppSettingsPath = "..\..\ValyanClinic\appsettings.json"
)

# Colors for output
$ColorSuccess = "Green"
$ColorError = "Red"
$ColorWarning = "Yellow"
$ColorInfo = "Cyan"

Write-Host "========================================" -ForegroundColor $ColorInfo
Write-Host "VERIFICARE SETUP ICD-10 IN DATABASE" -ForegroundColor $ColorInfo
Write-Host "========================================" -ForegroundColor $ColorInfo
Write-Host ""

# =============================================
# 1. READ CONNECTION STRING FROM APPSETTINGS.JSON
# =============================================
Write-Host "1. Citire connection string din appsettings.json..." -ForegroundColor $ColorInfo

$appSettingsFullPath = Join-Path $PSScriptRoot $AppSettingsPath
if (-not (Test-Path $appSettingsFullPath)) {
    Write-Host "   ? EROARE: appsettings.json nu a fost gasit la: $appSettingsFullPath" -ForegroundColor $ColorError
    Write-Host "   Foloseste parametrul -AppSettingsPath pentru a specifica locatia corecta" -ForegroundColor $ColorWarning
    exit 1
}

try {
    $appSettings = Get-Content $appSettingsFullPath -Raw | ConvertFrom-Json
    $connectionString = $appSettings.ConnectionStrings.DefaultConnection
    
    if ([string]::IsNullOrEmpty($connectionString)) {
        Write-Host "   ? EROARE: Connection string 'DefaultConnection' nu a fost gasit in appsettings.json" -ForegroundColor $ColorError
        exit 1
    }
    
    # Extract database name for display
    if ($connectionString -match "Database=([^;]+)") {
        $databaseName = $matches[1]
        Write-Host "   ? Connection string citit cu succes" -ForegroundColor $ColorSuccess
        Write-Host "   Database: $databaseName" -ForegroundColor $ColorInfo
    } else {
        Write-Host "   ? EROARE: Nu s-a putut extrage numele bazei de date din connection string" -ForegroundColor $ColorError
        exit 1
    }
}
catch {
    Write-Host "   ? EROARE la citirea appsettings.json: $_" -ForegroundColor $ColorError
    exit 1
}

Write-Host ""

# =============================================
# 2. CONNECT TO SQL SERVER
# =============================================
Write-Host "2. Conectare la SQL Server..." -ForegroundColor $ColorInfo

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()
    
    Write-Host "   ? Conectare reusita la: $databaseName" -ForegroundColor $ColorSuccess
}
catch {
    Write-Host "   ? EROARE la conectarea la SQL Server: $_" -ForegroundColor $ColorError
    Write-Host "   Connection string: $connectionString" -ForegroundColor $ColorWarning
    exit 1
}

Write-Host ""

# =============================================
# 3. SEARCH FOR ICD10 TABLES (WITH LIKE)
# =============================================
Write-Host "3. Cautare tabele ICD10 (cu LIKE '%ICD10%')..." -ForegroundColor $ColorInfo

$searchTablesQuery = @"
SELECT TABLE_NAME, 
       (SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME)) as ColumnCount
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'dbo' 
AND TABLE_NAME LIKE '%ICD10%'
ORDER BY TABLE_NAME
"@

try {
    $command = $connection.CreateCommand()
    $command.CommandText = $searchTablesQuery
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter $command
    $dataTable = New-Object System.Data.DataTable
    $adapter.Fill($dataTable) | Out-Null
    
    if ($dataTable.Rows.Count -gt 0) {
        Write-Host "   ? Gasit $($dataTable.Rows.Count) tabel(e) ICD10:" -ForegroundColor $ColorSuccess
        
        $icd10TableName = $null
        $rowCount = 0
        
        foreach ($row in $dataTable.Rows) {
            $tableName = $row["TABLE_NAME"]
            $colCount = $row["ColumnCount"]
            
            # Get row count for this table
            $countQuery = "SELECT COUNT(*) FROM [$tableName]"
            $command.CommandText = $countQuery
            $tableRowCount = [int]$command.ExecuteScalar()
            
            Write-Host "     - $tableName ($colCount coloane, $tableRowCount randuri)" -ForegroundColor $ColorInfo
            
            # Use first found table as the main ICD10 table
            if ($null -eq $icd10TableName) {
                $icd10TableName = $tableName
                $rowCount = $tableRowCount
            }
        }
        
        if ($rowCount -eq 0) {
            Write-Host "   ? ATENTIE: Tabelul $icd10TableName este gol - trebuie populat cu date ICD-10" -ForegroundColor $ColorWarning
        }
    } else {
        Write-Host "   ? NU s-a gasit niciun tabel care contine 'ICD10' in nume" -ForegroundColor $ColorError
        Write-Host "   Trebuie creat tabelul inainte de a putea folosi functionalitatea ICD-10" -ForegroundColor $ColorWarning
        $icd10TableName = $null
        $rowCount = 0
    }
}
catch {
    Write-Host "   ? EROARE la cautarea tabelelor: $_" -ForegroundColor $ColorError
    $icd10TableName = $null
    $rowCount = 0
}

Write-Host ""

# =============================================
# 4. SEARCH FOR ICD10 STORED PROCEDURES (WITH LIKE)
# =============================================
Write-Host "4. Cautare Stored Procedures ICD10 (cu LIKE '%ICD10%')..." -ForegroundColor $ColorInfo

$searchSPQuery = @"
SELECT ROUTINE_NAME, 
       CREATED as DateCreated,
       LAST_ALTERED as DateModified
FROM INFORMATION_SCHEMA.ROUTINES 
WHERE ROUTINE_TYPE = 'PROCEDURE' 
AND ROUTINE_SCHEMA = 'dbo'
AND ROUTINE_NAME LIKE '%ICD10%'
ORDER BY ROUTINE_NAME
"@

$foundProcedures = @()

try {
    $command.CommandText = $searchSPQuery
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter $command
    $dataTable = New-Object System.Data.DataTable
    $adapter.Fill($dataTable) | Out-Null
    
    if ($dataTable.Rows.Count -gt 0) {
        Write-Host "   ? Gasit $($dataTable.Rows.Count) stored procedure(s) ICD10:" -ForegroundColor $ColorSuccess
        
        foreach ($row in $dataTable.Rows) {
            $spName = $row["ROUTINE_NAME"]
            $foundProcedures += $spName
            Write-Host "     - $spName" -ForegroundColor $ColorSuccess
        }
    } else {
        Write-Host "   ? NU s-a gasit niciun stored procedure care contine 'ICD10' in nume" -ForegroundColor $ColorError
    }
}
catch {
    Write-Host "   ? EROARE la cautarea stored procedures: $_" -ForegroundColor $ColorError
}

Write-Host ""

# =============================================
# 5. CHECK REQUIRED STORED PROCEDURES
# =============================================
Write-Host "5. Verificare Stored Procedures NECESARE pentru ICD10AutocompleteComponent..." -ForegroundColor $ColorInfo

$requiredStoredProcedures = @(
    "sp_ICD10_Search",
    "sp_ICD10_GetById", 
    "sp_ICD10_GetByCode",
    "sp_ICD10_GetCategories",
    "sp_ICD10_GetCommon",
    "sp_ICD10_GetChildren",
    "sp_ICD10_GetStatistics",
    "sp_ICD10_ValidateCode",
    "sp_ICD10_Insert"
)

$missingProcedures = @()
$existingRequired = @()

foreach ($spName in $requiredStoredProcedures) {
    if ($foundProcedures -contains $spName) {
        $existingRequired += $spName
        Write-Host "   ? $spName" -ForegroundColor $ColorSuccess
    } else {
        $missingProcedures += $spName
        Write-Host "   ? $spName (LIPSESTE)" -ForegroundColor $ColorError
    }
}

Write-Host ""

# =============================================
# 6. SUMMARY
# =============================================
Write-Host "========================================" -ForegroundColor $ColorInfo
Write-Host "SUMAR VERIFICARE" -ForegroundColor $ColorInfo
Write-Host "========================================" -ForegroundColor $ColorInfo
Write-Host ""

Write-Host "Database: $databaseName" -ForegroundColor $ColorInfo

if ($null -ne $icd10TableName) {
    Write-Host "Tabel ICD10: ? EXISTA ($icd10TableName cu $rowCount inregistrari)" -ForegroundColor $ColorSuccess
} else {
    Write-Host "Tabel ICD10: ? NU EXISTA" -ForegroundColor $ColorError
}

Write-Host ""
Write-Host "Stored Procedures GASITE: $($foundProcedures.Count)" -ForegroundColor $(if ($foundProcedures.Count -gt 0) { $ColorSuccess } else { $ColorWarning })
Write-Host "Stored Procedures NECESARE: $($existingRequired.Count)/$($requiredStoredProcedures.Count)" -ForegroundColor $(if ($existingRequired.Count -eq $requiredStoredProcedures.Count) { $ColorSuccess } else { $ColorWarning })

if ($missingProcedures.Count -gt 0) {
    Write-Host ""
    Write-Host "Stored Procedures LIPSA (necesare pentru component):" -ForegroundColor $ColorWarning
    foreach ($sp in $missingProcedures) {
        Write-Host "  - $sp" -ForegroundColor $ColorError
    }
}

Write-Host ""

# =============================================
# 7. RECOMMENDATIONS
# =============================================
Write-Host "========================================" -ForegroundColor $ColorInfo
Write-Host "RECOMANDARI" -ForegroundColor $ColorInfo
Write-Host "========================================" -ForegroundColor $ColorInfo
Write-Host ""

$hasIssues = $false

if ($null -eq $icd10TableName) {
    $hasIssues = $true
    Write-Host "? ACTIUNE NECESARA: Creaza tabelul pentru coduri ICD-10" -ForegroundColor $ColorWarning
    Write-Host "  Tabel recomandat: ICD10Code sau ICD10Codes" -ForegroundColor $ColorInfo
    Write-Host "  Ruleaza: DevSupport\Database\Create-ICD10Table.sql" -ForegroundColor $ColorInfo
    Write-Host ""
}

if ($null -ne $icd10TableName -and $rowCount -eq 0) {
    $hasIssues = $true
    Write-Host "? ACTIUNE NECESARA: Populeaza tabelul $icd10TableName cu date" -ForegroundColor $ColorWarning
    Write-Host "  Ruleaza: DevSupport\Database\Import-ICD10Data.sql" -ForegroundColor $ColorInfo
    Write-Host "  SAU: DevSupport\Scripts\Import-ICD10FromCSV.ps1" -ForegroundColor $ColorInfo
    Write-Host ""
}

if ($missingProcedures.Count -gt 0) {
    $hasIssues = $true
    Write-Host "? ACTIUNE NECESARA: Creaza stored procedures lipsa ($($missingProcedures.Count) buc)" -ForegroundColor $ColorWarning
    Write-Host "  Ruleaza: DevSupport\Database\Create-ICD10StoredProcedures.sql" -ForegroundColor $ColorInfo
    Write-Host ""
}

if ($foundProcedures.Count -gt 0 -and $missingProcedures.Count -eq 0) {
    Write-Host "? ATENTIE: Exista $($foundProcedures.Count) SP-uri ICD10, dar NU toate cele necesare!" -ForegroundColor $ColorWarning
    Write-Host "  Posibil aveti o implementare custom sau cu nume diferite." -ForegroundColor $ColorInfo
    Write-Host ""
}

if (-not $hasIssues -and $null -ne $icd10TableName -and $rowCount -gt 0) {
    Write-Host "? SETUP COMPLET! Toate componentele ICD-10 sunt prezente." -ForegroundColor $ColorSuccess
    Write-Host "  Tabel: $icd10TableName ($rowCount coduri)" -ForegroundColor $ColorSuccess
    Write-Host "  SP-uri: $($foundProcedures.Count) gasite" -ForegroundColor $ColorSuccess
    Write-Host "  Componenta ICD10AutocompleteComponent este gata de utilizare!" -ForegroundColor $ColorSuccess
} else {
    Write-Host "? SETUP INCOMPLET! Urmeaza recomandarile de mai sus." -ForegroundColor $ColorWarning
}

Write-Host ""

# =============================================
# 8. CLEANUP
# =============================================
$connection.Close()
$connection.Dispose()

Write-Host "Verificare finalizata!" -ForegroundColor $ColorInfo
Write-Host ""

# Return exit code
if ($hasIssues) {
    exit 1  # Issues found
} else {
    exit 0  # All OK
}
