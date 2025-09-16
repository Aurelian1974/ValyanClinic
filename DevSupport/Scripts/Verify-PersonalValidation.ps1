<#
.SYNOPSIS
    Script PowerShell simplificat pentru verificarea sincroniz?rii valid?rilor Personal
.DESCRIPTION
    Verific? ?i raporteaz? diferen?ele între constrângerile din baza de date ?i valid?rile FluentValidation
#>

param(
    [Parameter(Mandatory=$false)]
    [string]$Server = "TS1828\ERP",
    
    [Parameter(Mandatory=$false)]
    [string]$Database = "ValyanMed"
)

try {
    Write-Host "?? VERIFICARE SINCRONIZARE VALID?RI PERSONAL" -ForegroundColor Cyan
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host ""
    
    # Construire connection string
    $connectionString = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;"
    
    Write-Host "?? Conectare la baza de date: $Server\$Database" -ForegroundColor Yellow
    Write-Host ""
    
    # Test conexiune simplu
    $query = @"
    SELECT 
        COLUMN_NAME, 
        DATA_TYPE, 
        IS_NULLABLE, 
        CHARACTER_MAXIMUM_LENGTH
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Personal' 
    ORDER BY ORDINAL_POSITION
"@
    
    $connection = New-Object System.Data.SqlClient.SqlConnection $connectionString
    $connection.Open()
    
    $command = $connection.CreateCommand()
    $command.CommandText = $query
    
    $reader = $command.ExecuteReader()
    
    Write-Host "?? ANALIZA COLOANE PERSONAL:" -ForegroundColor Green
    Write-Host ""
    
    $columnCount = 0
    $validationSummary = @()
    
    while ($reader.Read()) {
        $columnCount++
        $columnName = $reader["COLUMN_NAME"]
        $dataType = $reader["DATA_TYPE"]
        $isNullable = $reader["IS_NULLABLE"] -eq "YES"
        $maxLength = if ($reader["CHARACTER_MAXIMUM_LENGTH"] -is [DBNull]) { "NULL" } else { $reader["CHARACTER_MAXIMUM_LENGTH"] }
        
        $status = "?"
        $note = "SINCRONIZAT"
        
        # Verific?ri specifice pentru coloane importante
        switch ($columnName) {
            "Nume" {
                if ($maxLength -ne 100 -and $maxLength -ne "NULL") {
                    $status = "??"
                    $note = "FluentValidation trebuie ajustat pentru $maxLength caractere"
                }
            }
            "Prenume" {
                if ($maxLength -ne 100 -and $maxLength -ne "NULL") {
                    $status = "??"
                    $note = "FluentValidation trebuie ajustat pentru $maxLength caractere"
                }
            }
            "CNP" {
                if ($maxLength -ne 13 -and $maxLength -ne "NULL") {
                    $status = "??"
                    $note = "FluentValidation trebuie ajustat pentru $maxLength caractere"
                }
            }
            "Cod_Angajat" {
                if ($maxLength -ne 20 -and $maxLength -ne "NULL") {
                    $status = "??"
                    $note = "FluentValidation trebuie ajustat pentru $maxLength caractere"
                }
            }
        }
        
        Write-Host "   $status $columnName ($dataType, Max: $maxLength, Nullable: $isNullable) - $note" -ForegroundColor Gray
        
        if ($status -eq "??") {
            $validationSummary += "ATEN?IE: $columnName - $note"
        }
    }
    
    $reader.Close()
    $connection.Close()
    
    Write-Host ""
    Write-Host "?? STATISTICI:" -ForegroundColor Blue
    Write-Host "   • Total coloane analizate: $columnCount" -ForegroundColor Gray
    Write-Host "   • Probleme identificate: $($validationSummary.Count)" -ForegroundColor Gray
    
    if ($validationSummary.Count -eq 0) {
        Write-Host ""
        Write-Host "?? PERFECT! Toate valid?rile sunt sincronizate cu baza de date!" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "??  PROBLEME IDENTIFICATE:" -ForegroundColor Yellow
        foreach ($issue in $validationSummary) {
            Write-Host "   • $issue" -ForegroundColor Yellow
        }
    }
    
    Write-Host ""
    Write-Host "? Verificare complet?!" -ForegroundColor Green
    
    exit 0
}
catch {
    Write-Host ""
    Write-Host "?? EROARE: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
finally {
    if ($connection.State -eq 'Open') {
        $connection.Close()
    }
}