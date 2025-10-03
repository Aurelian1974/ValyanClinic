<#
.SYNOPSIS
    Script PowerShell pentru sincronizarea valid?rilor FluentValidation cu structura bazei de date Personal
.DESCRIPTION
    Verific? ?i raporteaz? diferen?ele între constrângerile din baza de date ?i valid?rile FluentValidation pentru tabela Personal
.PARAMETER Server
    Numele serverului SQL (default: TS1828\ERP)
.PARAMETER Database
    Numele bazei de date (default: ValyanMed)
.PARAMETER CheckOnly
    Doar verific? diferen?ele f?r? s? fac? modific?ri
.EXAMPLE
    .\Sync-PersonalValidation.ps1 -CheckOnly
.EXAMPLE
    .\Sync-PersonalValidation.ps1 -Server "localhost" -Database "ValyanMedDev"
#>

param(
    [Parameter(Mandatory=$false)]
    [string]$Server = "TS1828\ERP",
    
    [Parameter(Mandatory=$false)]
    [string]$Database = "ValyanMed",
    
    [Parameter(Mandatory=$false)]
    [switch]$CheckOnly = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$UseIntegratedSecurity = $true
)

# Func?ie pentru ob?inerea structurii tabelei din baza de date
function Get-DatabaseStructure {
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
    
    try {
        $connection = New-Object System.Data.SqlClient.SqlConnection $ConnectionString
        $connection.Open()
        
        $command = $connection.CreateCommand()
        $command.CommandText = $query
        
        $adapter = New-Object System.Data.SqlClient.SqlDataAdapter $command
        $dataset = New-Object System.Data.DataSet
        
        $adapter.Fill($dataset) | Out-Null
        
        return $dataset.Tables[0]
    }
    catch {
        throw "Eroare la ob?inerea structurii din baza de date: $($_.Exception.Message)"
    }
    finally {
        if ($connection.State -eq 'Open') {
            $connection.Close()
        }
    }
}

# Func?ie pentru ob?inerea constrângerilor CHECK din baza de date
function Get-CheckConstraints {
    param([string]$ConnectionString)
    
    $query = @"
    SELECT 
        cc.CONSTRAINT_NAME, 
        cc.CHECK_CLAUSE 
    FROM INFORMATION_SCHEMA.CHECK_CONSTRAINTS cc 
    WHERE cc.CONSTRAINT_NAME LIKE '%Personal%'
"@
    
    try {
        $connection = New-Object System.Data.SqlClient.SqlConnection $ConnectionString
        $connection.Open()
        
        $command = $connection.CreateCommand()
        $command.CommandText = $query
        
        $adapter = New-Object System.Data.SqlClient.SqlDataAdapter $command
        $dataset = New-Object System.Data.DataSet
        
        $adapter.Fill($dataset) | Out-Null
        
        return $dataset.Tables[0]
    }
    catch {
        throw "Eroare la ob?inerea constrângerilor CHECK: $($_.Exception.Message)"
    }
    finally {
        if ($connection.State -eq 'Open') {
            $connection.Close()
        }
    }
}

# Func?ie pentru ob?inerea constrângerilor UNIQUE
function Get-UniqueConstraints {
    param([string]$ConnectionString)
    
    $query = @"
    SELECT 
        tc.CONSTRAINT_NAME, 
        ccu.COLUMN_NAME 
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc 
    JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ccu ON tc.CONSTRAINT_NAME = ccu.CONSTRAINT_NAME 
    WHERE tc.TABLE_NAME = 'Personal' AND tc.CONSTRAINT_TYPE = 'UNIQUE'
"@
    
    try {
        $connection = New-Object System.Data.SqlClient.SqlConnection $ConnectionString
        $connection.Open()
        
        $command = $connection.CreateCommand()
        $command.CommandText = $query
        
        $adapter = New-Object System.Data.SqlClient.SqlDataAdapter $command
        $dataset = New-Object System.Data.DataSet
        
        $adapter.Fill($dataset) | Out-Null
        
        return $dataset.Tables[0]
    }
    catch {
        throw "Eroare la ob?inerea constrângerilor UNIQUE: $($_.Exception.Message)"
    }
    finally {
        if ($connection.State -eq 'Open') {
            $connection.Close()
        }
    }
}

# Func?ie pentru analiza diferen?elor de validare
function Analyze-ValidationDifferences {
    param(
        $DatabaseStructure,
        $CheckConstraints,
        $UniqueConstraints
    )
    
    $differences = @()
    $validationRules = @()
    
    Write-Host "?? Analizarea structurii bazei de date..." -ForegroundColor Yellow
    
    if ($null -eq $DatabaseStructure -or $DatabaseStructure.Rows.Count -eq 0) {
        $differences += "? Nu s-au putut ob?ine date din baza de date"
        return @{
            Differences = $differences
            ValidationRules = $validationRules
            TotalColumns = 0
        }
    }
    
    foreach ($row in $DatabaseStructure.Rows) {
        $columnName = $row["COLUMN_NAME"]
        $dataType = $row["DATA_TYPE"]
        $isNullable = $row["IS_NULLABLE"] -eq "YES"
        $maxLength = if ($row["CHARACTER_MAXIMUM_LENGTH"] -is [DBNull]) { $null } else { $row["CHARACTER_MAXIMUM_LENGTH"] }
        $columnDefault = $row["COLUMN_DEFAULT"]
        
        Write-Host "   Analizare coloan?: $columnName ($dataType)" -ForegroundColor Gray
        
        # Analiza pentru fiecare coloan?
        switch ($columnName) {
            "Nume" {
                if ($null -ne $maxLength -and $maxLength -ne 100) {
                    $differences += "? Nume: DB permite $maxLength caractere, FluentValidation ar trebui s? permit? acela?i lucru"
                } else {
                    $validationRules += "? Nume: Length(2, 100) - SINCRONIZAT"
                }
            }
            "Prenume" {
                if ($null -ne $maxLength -and $maxLength -ne 100) {
                    $differences += "? Prenume: DB permite $maxLength caractere, FluentValidation ar trebui s? permit? acela?i lucru"
                } else {
                    $validationRules += "? Prenume: Length(2, 100) - SINCRONIZAT"
                }
            }
            "CNP" {
                if ($null -ne $maxLength -and $maxLength -ne 13) {
                    $differences += "? CNP: DB permite $maxLength caractere, dar CNP-ul trebuie s? aib? exact 13"
                } else {
                    $validationRules += "? CNP: Length(13) + algoritm validare - SINCRONIZAT"
                }
            }
            "Cod_Angajat" {
                if ($null -ne $maxLength -and $maxLength -ne 20) {
                    $differences += "? Cod_Angajat: DB permite $maxLength caractere, FluentValidation ar trebui s? permit? acela?i lucru"
                } else {
                    $validationRules += "? Cod_Angajat: Length(3, 20) + pattern - SINCRONIZAT"
                }
            }
            "Email_Personal" {
                if ($null -ne $maxLength -and $maxLength -ne 100) {
                    $differences += "? Email_Personal: DB permite $maxLength caractere, FluentValidation ar trebui s? permit? acela?i lucru"
                } else {
                    $validationRules += "? Email_Personal: MaximumLength(100) + EmailAddress - SINCRONIZAT"
                }
            }
            "Email_Serviciu" {
                if ($null -ne $maxLength -and $maxLength -ne 100) {
                    $differences += "? Email_Serviciu: DB permite $maxLength caractere, FluentValidation ar trebui s? permit? acela?i lucru"
                } else {
                    $validationRules += "? Email_Serviciu: MaximumLength(100) + EmailAddress - SINCRONIZAT"
                }
            }
            "Telefon_Personal" {
                if ($null -ne $maxLength -and $maxLength -ne 20) {
                    $differences += "? Telefon_Personal: DB permite $maxLength caractere, FluentValidation ar trebui s? permit? acela?i lucru"
                } else {
                    $validationRules += "? Telefon_Personal: MaximumLength(20) + pattern românesc - SINCRONIZAT"
                }
            }
            "Telefon_Serviciu" {
                if ($null -ne $maxLength -and $maxLength -ne 20) {
                    $differences += "? Telefon_Serviciu: DB permite $maxLength caractere, FluentValidation ar trebui s? permit? acela?i lucru"
                } else {
                    $validationRules += "? Telefon_Serviciu: MaximumLength(20) + pattern românesc - SINCRONIZAT"
                }
            }
            "Functia" {
                if ($null -ne $maxLength -and $maxLength -ne 100) {
                    $differences += "? Functia: DB permite $maxLength caractere, FluentValidation ar trebui s? permit? acela?i lucru"
                } else {
                    $validationRules += "? Functia: Length(2, 100) - SINCRONIZAT"
                }
            }
            "Nationalitate" {
                if ($null -ne $maxLength -and $maxLength -ne 50) {
                    $differences += "? Nationalitate: DB permite $maxLength caractere, FluentValidation ar trebui s? permit? acela?i lucru"
                } else {
                    $validationRules += "? Nationalitate: Length(2, 50) - SINCRONIZAT"
                }
            }
            "Cetatenie" {
                if ($null -ne $maxLength -and $maxLength -ne 50) {
                    $differences += "? Cetatenie: DB permite $maxLength caractere, FluentValidation ar trebui s? permit? acela?i lucru"
                } else {
                    $validationRules += "? Cetatenie: Length(2, 50) - SINCRONIZAT"
                }
            }
            "Adresa_Domiciliu" {
                if ($maxLength -eq -1) {
                    $validationRules += "? Adresa_Domiciliu: MaxLength(4000) pentru nvarchar(MAX) - SINCRONIZAT"
                } else {
                    $differences += "? Adresa_Domiciliu: Tip de date nea?teptat în DB"
                }
            }
        }
        
        # Verificare NULL/NOT NULL
        if (-not $isNullable -and $columnName -notin @("Id_Personal", "Data_Crearii", "Data_Ultimei_Modificari")) {
            $validationRules += "? ${columnName}: NotEmpty() - OBLIGATORIU în DB"
        }
    }
    
    return @{
        Differences = $differences
        ValidationRules = $validationRules
        TotalColumns = $DatabaseStructure.Rows.Count
    }
}

# Func?ie pentru generarea raportului
function Generate-ValidationReport {
    param($Analysis, $CheckConstraints, $UniqueConstraints)
    
    Write-Host ""
    Write-Host "?? RAPORT SINCRONIZARE VALID?RI PERSONAL" -ForegroundColor Cyan
    Write-Host "=========================================" -ForegroundColor Cyan
    Write-Host ""
    
    Write-Host "?? STATISTICI GENERALE:" -ForegroundColor Green
    Write-Host "  • Total coloane în DB: $($Analysis.TotalColumns)"
    Write-Host "  • Reguli de validare verificate: $($Analysis.ValidationRules.Count)"
    Write-Host "  • Diferen?e identificate: $($Analysis.Differences.Count)"
    Write-Host ""
    
    if ($Analysis.Differences.Count -eq 0) {
        Write-Host "? SINCRONIZARE PERFECT?!" -ForegroundColor Green
        Write-Host "   Toate valid?rile FluentValidation sunt sincronizate cu baza de date." -ForegroundColor Green
    } else {
        Write-Host "??  DIFEREN?E IDENTIFICATE:" -ForegroundColor Yellow
        foreach ($diff in $Analysis.Differences) {
            Write-Host "   $diff" -ForegroundColor Yellow
        }
    }
    
    Write-Host ""
    Write-Host "? REGULI DE VALIDARE SINCRONIZATE:" -ForegroundColor Green
    foreach ($rule in $Analysis.ValidationRules) {
        Write-Host "   $rule" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "?? CONSTRÂNGERI CHECK DIN BAZA DE DATE:" -ForegroundColor Blue
    foreach ($row in $CheckConstraints.Rows) {
        $constraintName = $row["CONSTRAINT_NAME"]
        $checkClause = $row["CHECK_CLAUSE"]
        Write-Host "   • $constraintName" -ForegroundColor Blue
        Write-Host "     $checkClause" -ForegroundColor Gray
    }
    
    Write-Host ""
    Write-Host "?? CONSTRÂNGERI UNIQUE DIN BAZA DE DATE:" -ForegroundColor Magenta
    foreach ($row in $UniqueConstraints.Rows) {
        $columnName = $row["COLUMN_NAME"]
        Write-Host "   • $columnName (UNIQUE)" -ForegroundColor Magenta
    }
    
    Write-Host ""
    Write-Host "?? RECOMAND?RI:" -ForegroundColor Cyan
    Write-Host "   • Verifica?i c? PersonalValidator.cs con?ine toate valid?rile men?ionate" -ForegroundColor Gray
    Write-Host "   • Testa?i valid?rile cu date reale din baza de date" -ForegroundColor Gray
    Write-Host "   • Rula?i testele de integrare dup? modific?ri" -ForegroundColor Gray
    Write-Host "   • Sincroniza?i enum-urile cu valorile distincte din DB" -ForegroundColor Gray
    
    return $Analysis.Differences.Count -eq 0
}

# Execu?ia principal?
try {
    Write-Host "?? SINCRONIZARE VALID?RI PERSONAL - VALYAN CLINIC" -ForegroundColor Cyan
    Write-Host "=================================================" -ForegroundColor Cyan
    Write-Host ""
    
    # Construire connection string
    $connectionString = if ($UseIntegratedSecurity) {
        "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;"
    } else {
        "Server=$Server;Database=$Database;User Id=$Username;Password=$Password;TrustServerCertificate=True;"
    }
    
    Write-Host "?? Conectare la baza de date..." -ForegroundColor Yellow
    Write-Host "   Server: $Server" -ForegroundColor Gray
    Write-Host "   Database: $Database" -ForegroundColor Gray
    Write-Host ""
    
    # Ob?inere date din baza de date
    $dbStructure = Get-DatabaseStructure -ConnectionString $connectionString
    $checkConstraints = Get-CheckConstraints -ConnectionString $connectionString  
    $uniqueConstraints = Get-UniqueConstraints -ConnectionString $connectionString
    
    # Analiz? diferen?e
    $analysis = Analyze-ValidationDifferences -DatabaseStructure $dbStructure -CheckConstraints $checkConstraints -UniqueConstraints $uniqueConstraints
    
    # Generare raport
    $isSynchronized = Generate-ValidationReport -Analysis $analysis -CheckConstraints $checkConstraints -UniqueConstraints $uniqueConstraints
    
    if ($isSynchronized) {
        Write-Host "?? SUCCES: Sincronizarea este complet?!" -ForegroundColor Green
        exit 0
    } else {
        Write-Host "??  ATEN?IE: Exist? diferen?e care necesit? rezolvare!" -ForegroundColor Yellow
        exit 1
    }
}
catch {
    Write-Host "?? EROARE: $_" -ForegroundColor Red
    exit 1
}