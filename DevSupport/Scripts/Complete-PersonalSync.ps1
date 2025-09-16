<#
.SYNOPSIS
    Script PowerShell pentru sincronizarea complet? între valid?ri FluentValidation ?i structura tabelei Personal
.DESCRIPTION
    Verific? diferen?ele între valid?ri ?i baza de date. Poate genera script-uri SQL pentru ad?ugarea de coloane
    sau sugerarea de valid?ri noi pentru FluentValidation.
.PARAMETER Server
    Numele serverului SQL (default: TS1828\ERP)
.PARAMETER Database
    Numele bazei de date (default: ValyanMed)
.PARAMETER GenerateAlterScripts
    Genereaz? script-uri ALTER TABLE pentru coloane lips?
.PARAMETER ValidatorFile
    Calea c?tre fi?ierul PersonalValidator.cs pentru analiz?
.EXAMPLE
    .\Complete-PersonalSync.ps1 -GenerateAlterScripts
.EXAMPLE
    .\Complete-PersonalSync.ps1 -ValidatorFile "..\..\ValyanClinic.Domain\Validators\PersonalValidator.cs"
#>

param(
    [Parameter(Mandatory=$false)]
    [string]$Server = "TS1828\ERP",
    
    [Parameter(Mandatory=$false)]
    [string]$Database = "ValyanMed",
    
    [Parameter(Mandatory=$false)]
    [switch]$GenerateAlterScripts = $false,
    
    [Parameter(Mandatory=$false)]
    [string]$ValidatorFile = "..\..\ValyanClinic.Domain\Validators\PersonalValidator.cs"
)

# Func?ie pentru ob?inerea structurii din baza de date
function Get-DatabaseColumns {
    param([string]$ConnectionString)
    
    $query = @"
    SELECT 
        COLUMN_NAME, 
        DATA_TYPE, 
        IS_NULLABLE, 
        CHARACTER_MAXIMUM_LENGTH,
        NUMERIC_PRECISION,
        COLUMN_DEFAULT,
        ORDINAL_POSITION
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Personal' 
    ORDER BY ORDINAL_POSITION
"@
    
    $connection = New-Object System.Data.SqlClient.SqlConnection $ConnectionString
    $connection.Open()
    
    $command = $connection.CreateCommand()
    $command.CommandText = $query
    
    $columns = @{}
    $reader = $command.ExecuteReader()
    
    while ($reader.Read()) {
        $columnName = $reader["COLUMN_NAME"]
        $columns[$columnName] = @{
            DataType = $reader["DATA_TYPE"]
            IsNullable = $reader["IS_NULLABLE"] -eq "YES"
            MaxLength = if ($reader["CHARACTER_MAXIMUM_LENGTH"] -is [DBNull]) { $null } else { $reader["CHARACTER_MAXIMUM_LENGTH"] }
            DefaultValue = if ($reader["COLUMN_DEFAULT"] -is [DBNull]) { $null } else { $reader["COLUMN_DEFAULT"] }
            Position = $reader["ORDINAL_POSITION"]
        }
    }
    
    $reader.Close()
    $connection.Close()
    
    return $columns
}

# Func?ie pentru parsarea validatorului FluentValidation
function Get-ValidatorRules {
    param([string]$ValidatorFilePath)
    
    if (-not (Test-Path $ValidatorFilePath)) {
        Write-Warning "Fi?ierul validator nu a fost g?sit: $ValidatorFilePath"
        return @{}
    }
    
    $content = Get-Content $ValidatorFilePath -Raw
    $rules = @{}
    
    # Regex pentru identificarea regulilor RuleFor
    $rulePattern = 'RuleFor\(x\s*=>\s*x\.(\w+)\)'
    $matches = [regex]::Matches($content, $rulePattern)
    
    foreach ($match in $matches) {
        $fieldName = $match.Groups[1].Value
        
        # Extragere informa?ii despre valid?ri pentru acest câmp
        $fieldRules = @{
            HasNotEmpty = $content.Contains("RuleFor(x => x.$fieldName)") -and $content -match "\.NotEmpty\(\)"
            HasLength = $content.Contains(".Length(")
            HasMaxLength = $content.Contains(".MaximumLength(")
            HasEmail = $content.Contains(".EmailAddress()")
            HasPattern = $content.Contains(".Matches(")
            HasRequired = $content.Contains(".NotNull()")
        }
        
        # Extragere limite de lungime dac? exist?
        $lengthPattern = "\.Length\((\d+),\s*(\d+)\)"
        $lengthMatch = [regex]::Match($content, $lengthPattern)
        if ($lengthMatch.Success) {
            $fieldRules.MinLength = [int]$lengthMatch.Groups[1].Value
            $fieldRules.MaxLength = [int]$lengthMatch.Groups[2].Value
        }
        
        $maxLengthPattern = "\.MaximumLength\((\d+)\)"
        $maxLengthMatch = [regex]::Match($content, $maxLengthPattern)
        if ($maxLengthMatch.Success) {
            $fieldRules.MaxLengthValue = [int]$maxLengthMatch.Groups[1].Value
        }
        
        $rules[$fieldName] = $fieldRules
    }
    
    return $rules
}

# Func?ie pentru generarea script-urilor ALTER TABLE
function Generate-AlterScripts {
    param($MissingColumns, $DatabaseColumns)
    
    $alterScripts = @()
    
    foreach ($column in $MissingColumns) {
        $columnName = $column.Name
        $dataType = $column.SuggestedType
        $nullable = if ($column.IsNullable) { "NULL" } else { "NOT NULL" }
        $defaultValue = if ($column.DefaultValue) { "DEFAULT $($column.DefaultValue)" } else { "" }
        
        $alterScript = @"
-- Ad?ugare coloan? lips?: $columnName
ALTER TABLE Personal 
ADD $columnName $dataType $nullable $defaultValue;
GO

"@
        $alterScripts += $alterScript
    }
    
    return $alterScripts -join "`n"
}

# Func?ie pentru generarea suger?rilor de valid?ri
function Generate-ValidationSuggestions {
    param($MissingValidations)
    
    $suggestions = @()
    
    foreach ($validation in $MissingValidations) {
        $suggestion = @"
// Validare sugerat? pentru coloana: $($validation.ColumnName)
RuleFor(x => x.$($validation.ColumnName))
"@
        
        if (-not $validation.IsNullable) {
            $suggestion += "`n    .NotEmpty()"
            $suggestion += "`n    .WithMessage(`"$($validation.ColumnName) este obligatoriu`")"
        }
        
        if ($validation.MaxLength -and $validation.MaxLength -ne -1) {
            $suggestion += "`n    .MaximumLength($($validation.MaxLength))"
            $suggestion += "`n    .WithMessage(`"$($validation.ColumnName) nu poate dep??i $($validation.MaxLength) caractere`")"
        }
        
        if ($validation.ColumnName -like "*Email*") {
            $suggestion += "`n    .EmailAddress()"
            $suggestion += "`n    .WithMessage(`"Formatul email-ului nu este valid`")"
        }
        
        $suggestion += "`n    .When(x => !string.IsNullOrEmpty(x.$($validation.ColumnName)));`n"
        
        $suggestions += $suggestion
    }
    
    return $suggestions -join "`n`n"
}

try {
    Write-Host "?? SINCRONIZARE COMPLET? PERSONAL - VALID?RI ?I BAZA DE DATE" -ForegroundColor Cyan
    Write-Host "=============================================================" -ForegroundColor Cyan
    Write-Host ""
    
    # Conexiune la baza de date
    $connectionString = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;"
    
    Write-Host "?? Ob?inere structur? din baza de date..." -ForegroundColor Yellow
    $dbColumns = Get-DatabaseColumns -ConnectionString $connectionString
    
    Write-Host "?? Analiz? valid?ri FluentValidation..." -ForegroundColor Yellow
    $validatorRules = Get-ValidatorRules -ValidatorFilePath $ValidatorFile
    
    Write-Host ""
    Write-Host "?? ANALIZA DIFEREN?ELOR:" -ForegroundColor Green
    Write-Host ""
    
    # Verificare coloane din DB care nu au valid?ri
    $missingValidations = @()
    foreach ($columnName in $dbColumns.Keys) {
        if (-not $validatorRules.ContainsKey($columnName)) {
            $column = $dbColumns[$columnName]
            $missingValidations += @{
                ColumnName = $columnName
                DataType = $column.DataType
                IsNullable = $column.IsNullable
                MaxLength = $column.MaxLength
            }
            Write-Host "   ??  Coloana '$columnName' din DB nu are valid?ri FluentValidation" -ForegroundColor Yellow
        } else {
            Write-Host "   ? Coloana '$columnName' este validat?" -ForegroundColor Green
        }
    }
    
    # Verificare valid?ri care nu au coloane în DB
    $missingColumns = @()
    foreach ($fieldName in $validatorRules.Keys) {
        if (-not $dbColumns.ContainsKey($fieldName)) {
            $rule = $validatorRules[$fieldName]
            $suggestedType = "NVARCHAR(100)" # Tip implicit
            
            if ($rule.ContainsKey('MaxLengthValue')) {
                $maxLen = $rule.MaxLengthValue
                $suggestedType = "NVARCHAR($maxLen)"
            } elseif ($rule.ContainsKey('MaxLength')) {
                $maxLen = $rule.MaxLength
                $suggestedType = "NVARCHAR($maxLen)"
            }
            
            $isNullable = -not $rule.HasNotEmpty -and -not $rule.HasRequired
            
            $missingColumns += @{
                Name = $fieldName
                SuggestedType = $suggestedType
                IsNullable = $isNullable
                DefaultValue = $null
            }
            Write-Host "   ??  Validarea pentru '$fieldName' nu are coloan? corespunz?toare în DB" -ForegroundColor Yellow
        }
    }
    
    Write-Host ""
    Write-Host "?? STATISTICI:" -ForegroundColor Blue
    Write-Host "   • Coloane în DB: $($dbColumns.Count)"
    Write-Host "   • Valid?ri definite: $($validatorRules.Count)"
    Write-Host "   • Valid?ri lips?: $($missingValidations.Count)"
    Write-Host "   • Coloane lips?: $($missingColumns.Count)"
    
    # Generare script-uri ALTER dac? este solicitat
    if ($GenerateAlterScripts -and $missingColumns.Count -gt 0) {
        Write-Host ""
        Write-Host "???  GENERAT SCRIPT-URI ALTER TABLE:" -ForegroundColor Magenta
        Write-Host ""
        
        $alterScript = Generate-AlterScripts -MissingColumns $missingColumns -DatabaseColumns $dbColumns
        Write-Host $alterScript -ForegroundColor Gray
        
        # Salvare script în fi?ier
        $scriptFile = "Personal_AlterTable_$(Get-Date -Format 'yyyyMMdd_HHmmss').sql"
        $alterScript | Out-File -FilePath $scriptFile -Encoding UTF8
        Write-Host "?? Script salvat în: $scriptFile" -ForegroundColor Green
    }
    
    # Generare suger?ri de valid?ri
    if ($missingValidations.Count -gt 0) {
        Write-Host ""
        Write-Host "?? SUGER?RI VALID?RI FLUENTVALIDATION:" -ForegroundColor Magenta
        Write-Host ""
        
        $validationSuggestions = Generate-ValidationSuggestions -MissingValidations $missingValidations
        Write-Host $validationSuggestions -ForegroundColor Gray
        
        # Salvare suger?ri în fi?ier
        $suggestionFile = "Personal_ValidationSuggestions_$(Get-Date -Format 'yyyyMMdd_HHmmss').cs"
        $validationSuggestions | Out-File -FilePath $suggestionFile -Encoding UTF8
        Write-Host "?? Suger?ri salvate în: $suggestionFile" -ForegroundColor Green
    }
    
    if ($missingValidations.Count -eq 0 -and $missingColumns.Count -eq 0) {
        Write-Host ""
        Write-Host "?? PERFECT! Valid?rile ?i structura bazei de date sunt complet sincronizate!" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "? Analiza complet?!" -ForegroundColor Green
    
    exit 0
}
catch {
    Write-Host ""
    Write-Host "?? EROARE: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Stack trace: $($_.ScriptStackTrace)" -ForegroundColor DarkRed
    exit 1
}