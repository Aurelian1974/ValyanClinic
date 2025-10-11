# ========================================
# Script pentru Compararea Schemei DB cu Entity Models
# ValyanClinic - Schema vs Code Comparison
# ========================================

param(
    [Parameter(Mandatory=$false)]
    [string]$ConfigPath = "..\..\..\ValyanClinic\appsettings.json",
    [Parameter(Mandatory=$false)]
    [string]$DomainPath = "..\..\..\ValyanClinic.Domain"
)

$ErrorActionPreference = "Continue"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "COMPARARE SCHEMA DB vs ENTITY MODELS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# ========================================
# HELPER FUNCTIONS
# ========================================
function Invoke-DbQuery {
    param(
        [string]$Query,
        [string]$ConnectionString,
        [int]$CommandTimeout = 30
    )
    
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $ConnectionString
    
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = $Query
        $command.CommandTimeout = $CommandTimeout
        
        $adapter = New-Object System.Data.SqlClient.SqlDataAdapter $command
        $dataset = New-Object System.Data.DataSet
        [void]$adapter.Fill($dataset)
        
        return $dataset.Tables[0]
    }
    finally {
        if ($connection.State -eq 'Open') {
            $connection.Close()
        }
    }
}

function Parse-CSharpEntity {
    param([string]$FilePath)
    
    if (-not (Test-Path $FilePath)) {
        return $null
    }
    
    $content = Get-Content $FilePath -Raw
    $properties = @()
    
    # Regex pentru a gasi proprietatile C#
    $propertyPattern = '(?m)^\s*public\s+([^\s]+(?:\?)?)\s+(\w+)\s*\{\s*get;\s*set;\s*\}'
    $matches = [regex]::Matches($content, $propertyPattern)
    
    foreach ($match in $matches) {
        $type = $match.Groups[1].Value
        $name = $match.Groups[2].Value
        
        $properties += @{
            Name = $name
            Type = $type
            IsNullable = $type.EndsWith('?')
        }
    }
    
    return $properties
}

function Convert-SqlTypeToCSharp {
    param([string]$SqlType, [bool]$IsNullable)
    
    $csharpType = switch ($SqlType.ToLower()) {
        'int' { 'int' }
        'bigint' { 'long' }
        'smallint' { 'short' }
        'tinyint' { 'byte' }
        'bit' { 'bool' }
        'decimal' { 'decimal' }
        'numeric' { 'decimal' }
        'money' { 'decimal' }
        'smallmoney' { 'decimal' }
        'float' { 'double' }
        'real' { 'float' }
        'datetime' { 'DateTime' }
        'datetime2' { 'DateTime' }
        'smalldatetime' { 'DateTime' }
        'date' { 'DateTime' }
        'time' { 'TimeSpan' }
        'char' { 'string' }
        'nchar' { 'string' }
        'varchar' { 'string' }
        'nvarchar' { 'string' }
        'text' { 'string' }
        'ntext' { 'string' }
        'uniqueidentifier' { 'Guid' }
        'varbinary' { 'byte[]' }
        'binary' { 'byte[]' }
        'image' { 'byte[]' }
        default { 'object' }
    }
    
    if ($IsNullable -and $csharpType -ne 'string' -and $csharpType -ne 'byte[]') {
        $csharpType += '?'
    }
    
    return $csharpType
}

# ========================================
# CONFIGURARE
# ========================================
Write-Host "`n[1/4] Citire configuratie..." -ForegroundColor Yellow

try {
    $config = Get-Content $ConfigPath -Raw | ConvertFrom-Json
    $connectionString = $config.ConnectionStrings.DefaultConnection
    Write-Host "? Connection string incarcat" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare la citirea configuratiei: $_" -ForegroundColor Red
    exit 1
}

# ========================================
# EXTRAGERE SCHEMA DB
# ========================================
Write-Host "`n[2/4] Extragere schema din baza de date..." -ForegroundColor Yellow

$targetTables = @('Personal', 'PersonalMedical', 'Judete', 'Localitati', 'Departamente')
$dbSchema = @{}

foreach ($tableName in $targetTables) {
    Write-Host "  Procesez tabel: $tableName" -ForegroundColor Gray
    
    $schemaQuery = @"
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length,
    c.precision,
    c.scale,
    c.is_nullable AS IsNullable,
    c.is_identity AS IsIdentity,
    ISNULL(pk.is_primary_key, 0) AS IsPrimaryKey
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
LEFT JOIN (
    SELECT ic.object_id, ic.column_id, 1 as is_primary_key
    FROM sys.index_columns ic
    INNER JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id
    WHERE i.is_primary_key = 1
) pk ON c.object_id = pk.object_id AND c.column_id = pk.column_id
WHERE c.object_id = OBJECT_ID('dbo.$tableName')
ORDER BY c.column_id
"@
    
    try {
        $columns = Invoke-DbQuery -Query $schemaQuery -ConnectionString $connectionString
        if ($columns.Rows.Count -gt 0) {
            $dbSchema[$tableName] = $columns
            Write-Host "    ? $($columns.Rows.Count) coloane gasite" -ForegroundColor Green
        } else {
            Write-Host "    ? Tabel nu exista in DB" -ForegroundColor Yellow
            $dbSchema[$tableName] = $null
        }
    }
    catch {
        Write-Host "    ? Eroare: $_" -ForegroundColor Red
        $dbSchema[$tableName] = $null
    }
}

# ========================================
# EXTRAGERE ENTITY MODELS
# ========================================
Write-Host "`n[3/4] Analiza entity models..." -ForegroundColor Yellow

$entityModels = @{}
$entitiesPath = "$DomainPath\Entities"

if (-not (Test-Path $entitiesPath)) {
    Write-Host "? Director Entities nu exista: $entitiesPath" -ForegroundColor Yellow
    $entitiesPath = $DomainPath
}

foreach ($tableName in $targetTables) {
    $entityFile = "$entitiesPath\$tableName.cs"
    Write-Host "  Caut fisier: $entityFile" -ForegroundColor Gray
    
    if (Test-Path $entityFile) {
        $properties = Parse-CSharpEntity -FilePath $entityFile
        $entityModels[$tableName] = $properties
        Write-Host "    ? $($properties.Count) proprietati gasite" -ForegroundColor Green
    } else {
        Write-Host "    ? Fisier nu exista" -ForegroundColor Yellow
        $entityModels[$tableName] = $null
    }
}

# ========================================
# COMPARARE SI RAPORT
# ========================================
Write-Host "`n[4/4] Generare raport de comparare..." -ForegroundColor Yellow

$report = @"
# RAPORT COMPARARE SCHEMA DB vs ENTITY MODELS
**Generat:** $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

## Sumar Comparare

"@

$totalTables = $targetTables.Count
$tablesInDb = ($dbSchema.Values | Where-Object { $_ -ne $null }).Count
$tablesInCode = ($entityModels.Values | Where-Object { $_ -ne $null }).Count

$report += @"
- **Tabele analizate:** $totalTables
- **Tabele in DB:** $tablesInDb
- **Entity models in cod:** $tablesInCode

"@

foreach ($tableName in $targetTables) {
    $report += @"

## Tabel: $tableName

"@
    
    $dbColumns = $dbSchema[$tableName]
    $codeProperties = $entityModels[$tableName]
    
    if ($dbColumns -eq $null -and $codeProperties -eq $null) {
        $report += "? **CRITIC:** Nici in DB, nici in cod`n"
        continue
    }
    
    if ($dbColumns -eq $null) {
        $report += "?? **ATENTIE:** Entity model exista in cod dar tabel nu exista in DB`n"
        $report += "**Proprietati in cod:** $($codeProperties.Count)`n"
        continue
    }
    
    if ($codeProperties -eq $null) {
        $report += "?? **ATENTIE:** Tabel exista in DB dar entity model nu exista in cod`n"
        $report += "**Coloane in DB:** $($dbColumns.Rows.Count)`n"
        continue
    }
    
    # Comparare detaliata
    $report += "? **OK:** Ambele exista`n`n"
    
    $report += "### Comparare Coloane/Proprietati`n`n"
    $report += "| Coloana DB | Tip DB | Nullable | Proprietate Cod | Tip Cod | Match |`n"
    $report += "|------------|--------|----------|-----------------|---------|-------|`n"
    
    # Verifica fiecare coloana din DB
    foreach ($column in $dbColumns.Rows) {
        $columnName = $column['ColumnName']
        $dbType = $column['DataType']
        $isNullable = $column['IsNullable']
        $expectedCSharpType = Convert-SqlTypeToCSharp -SqlType $dbType -IsNullable $isNullable
        
        $codeProperty = $codeProperties | Where-Object { $_.Name -eq $columnName }
        
        if ($codeProperty) {
            $match = if ($codeProperty.Type -eq $expectedCSharpType) { "?" } else { "?" }
            $report += "| $columnName | $dbType | $isNullable | $($codeProperty.Name) | $($codeProperty.Type) | $match |`n"
        } else {
            $report += "| $columnName | $dbType | $isNullable | *LIPSA* | - | ? |`n"
        }
    }
    
    # Verifica proprietati care sunt in cod dar nu in DB
    foreach ($property in $codeProperties) {
        $dbColumn = $dbColumns.Rows | Where-Object { $_['ColumnName'] -eq $property.Name }
        if (-not $dbColumn) {
            $report += "| *LIPSA* | - | - | $($property.Name) | $($property.Type) | ? |`n"
        }
    }
    
    # Statistici pentru tabel
    $matchingCount = 0
    $totalDbColumns = $dbColumns.Rows.Count
    $totalCodeProperties = $codeProperties.Count
    
    foreach ($column in $dbColumns.Rows) {
        $columnName = $column['ColumnName']
        $codeProperty = $codeProperties | Where-Object { $_.Name -eq $columnName }
        if ($codeProperty) {
            $dbType = $column['DataType']
            $isNullable = $column['IsNullable']
            $expectedType = Convert-SqlTypeToCSharp -SqlType $dbType -IsNullable $isNullable
            if ($codeProperty.Type -eq $expectedType) {
                $matchingCount++
            }
        }
    }
    
    $matchPercentage = if ($totalDbColumns -gt 0) { [math]::Round(($matchingCount / $totalDbColumns) * 100, 2) } else { 0 }
    
    $report += "`n**Statistici:**`n"
    $report += "- Coloane in DB: $totalDbColumns`n"
    $report += "- Proprietati in cod: $totalCodeProperties`n"
    $report += "- Potriviri perfecte: $matchingCount`n"
    $report += "- Rata de potrivire: $matchPercentage%`n"
}

$report += @"

## Recomandari

### Actiuni Prioritare:
1. **Creaza entity models** pentru tabelele care exista doar in DB
2. **Actualizeaza tipurile de date** pentru proprietatile care nu se potrivesc
3. **Adauga coloane lipsa** in entity models
4. **Sterge proprietati** care nu au corespondent in DB

### Script-uri Utile:
- Ruleaza ``Extract-SpecificTables.ps1`` pentru a extrage structura exacta
- Foloseste raportul de mai sus pentru a actualiza manual entity models
- Verifica foreign key relationships dupa actualizare

---
*Generat automat de Schema Comparison Script*
"@

# Salveaza raportul
$reportPath = "..\Database\Schema_Comparison_Report.md"
$report | Out-File -FilePath $reportPath -Encoding UTF8

Write-Host "? Raport generat: $reportPath" -ForegroundColor Green

# Afiseaza si un sumar rapid in consola
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "SUMAR COMPARARE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

foreach ($tableName in $targetTables) {
    $dbExists = $dbSchema[$tableName] -ne $null
    $codeExists = $entityModels[$tableName] -ne $null
    
    $status = if ($dbExists -and $codeExists) { 
        "? AMBELE EXISTA" 
    } elseif ($dbExists -and -not $codeExists) { 
        "?? DOAR IN DB" 
    } elseif (-not $dbExists -and $codeExists) { 
        "?? DOAR IN COD" 
    } else { 
        "? NICIUNUL" 
    }
    
    Write-Host "$tableName : $status" -ForegroundColor $(if ($dbExists -and $codeExists) { 'Green' } elseif ($dbExists -or $codeExists) { 'Yellow' } else { 'Red' })
}

Write-Host "`nPentru detalii complete, verifica raportul: $reportPath" -ForegroundColor Cyan