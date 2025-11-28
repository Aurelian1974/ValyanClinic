# =============================================
# GET ICD10_Codes TABLE STRUCTURE
# Afi?eaz? toate coloanele din tabelul ICD10_Codes
# =============================================

param(
    [string]$AppSettingsPath = "..\..\ValyanClinic\appsettings.json"
)

$ColorSuccess = "Green"
$ColorInfo = "Cyan"
$ColorWarning = "Yellow"

Write-Host "========================================" -ForegroundColor $ColorInfo
Write-Host "STRUCTURA TABEL ICD10_Codes" -ForegroundColor $ColorInfo
Write-Host "========================================" -ForegroundColor $ColorInfo
Write-Host ""

# Read connection string
$appSettingsFullPath = Join-Path $PSScriptRoot $AppSettingsPath
$appSettings = Get-Content $appSettingsFullPath -Raw | ConvertFrom-Json
$connectionString = $appSettings.ConnectionStrings.DefaultConnection

# Connect
$connection = New-Object System.Data.SqlClient.SqlConnection
$connection.ConnectionString = $connectionString
$connection.Open()

# Get columns
$query = @"
SELECT 
    COLUMN_NAME as ColumnName,
    DATA_TYPE as DataType,
    CHARACTER_MAXIMUM_LENGTH as MaxLength,
    IS_NULLABLE as IsNullable,
    COLUMN_DEFAULT as DefaultValue
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ICD10_Codes'
ORDER BY ORDINAL_POSITION
"@

$command = $connection.CreateCommand()
$command.CommandText = $query
$adapter = New-Object System.Data.SqlClient.SqlDataAdapter $command
$dataTable = New-Object System.Data.DataTable
$adapter.Fill($dataTable) | Out-Null

Write-Host "Coloane din tabelul ICD10_Codes:" -ForegroundColor $ColorSuccess
Write-Host ""
Write-Host ("{0,-30} {1,-20} {2,-10} {3,-10}" -f "Column Name", "Data Type", "Max Length", "Nullable") -ForegroundColor $ColorInfo
Write-Host ("-" * 75) -ForegroundColor $ColorInfo

foreach ($row in $dataTable.Rows) {
    $columnName = $row["ColumnName"]
    $dataType = $row["DataType"]
    $maxLength = if ($row["MaxLength"] -eq [DBNull]::Value) { "-" } else { $row["MaxLength"] }
    $isNullable = $row["IsNullable"]
    
    $color = if ($columnName -like "*Date*" -or $columnName -like "*At*") { $ColorWarning } else { "White" }
    
    Write-Host ("{0,-30} {1,-20} {2,-10} {3,-10}" -f $columnName, $dataType, $maxLength, $isNullable) -ForegroundColor $color
}

$connection.Close()

Write-Host ""
Write-Host "Coloanele audit (Date/At) sunt marcate cu galben" -ForegroundColor $ColorWarning
Write-Host ""
