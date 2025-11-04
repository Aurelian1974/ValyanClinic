# Script rapid pentru testare sp_Pacienti_GetAll
$connectionString = "Server=DESKTOP-9H54BCS\SQLSERVER;Database=ValyanMed;Integrated Security=True;TrustServerCertificate=True;"

Write-Host "Testare Stored Procedure: sp_Pacienti_GetAll" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()
    
    Write-Host "? Conexiune SQL reusita" -ForegroundColor Green
    
    $command = $connection.CreateCommand()
    $command.CommandText = "sp_Pacienti_GetAll"
    $command.CommandType = [System.Data.CommandType]::StoredProcedure
    
    # Parametri
    $command.Parameters.AddWithValue("@PageNumber", 1) | Out-Null
    $command.Parameters.AddWithValue("@PageSize", 50) | Out-Null
    $command.Parameters.AddWithValue("@SearchText", [DBNull]::Value) | Out-Null
    $command.Parameters.AddWithValue("@Judet", [DBNull]::Value) | Out-Null
    $command.Parameters.AddWithValue("@Asigurat", [DBNull]::Value) | Out-Null
    $command.Parameters.AddWithValue("@Activ", [DBNull]::Value) | Out-Null
    $command.Parameters.AddWithValue("@SortColumn", "Nume") | Out-Null
    $command.Parameters.AddWithValue("@SortDirection", "ASC") | Out-Null
    
    Write-Host "? Parametri configurati" -ForegroundColor Green
    
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter $command
    $dataset = New-Object System.Data.DataSet
    
    Write-Host "Executare stored procedure..." -ForegroundColor Yellow
    $recordsAffected = $adapter.Fill($dataset)
    
    Write-Host "? Stored procedure executata cu succes" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "Rezultate:" -ForegroundColor Cyan
    Write-Host "  - Nr tabele returnate: $($dataset.Tables.Count)" -ForegroundColor White
    
    if ($dataset.Tables.Count -gt 0) {
        $table = $dataset.Tables[0]
        Write-Host "  - Nr randuri in tabelul 1: $($table.Rows.Count)" -ForegroundColor White
        Write-Host "  - Nr coloane: $($table.Columns.Count)" -ForegroundColor White
        Write-Host ""
        
        if ($table.Rows.Count -gt 0) {
            Write-Host "Date returnate:" -ForegroundColor Green
            Write-Host "=============================================" -ForegroundColor Cyan
            
            # Afiseaza primele 5 coloane importante
            $table | Select-Object -First 5 `
                @{N='Cod_Pacient';E={$_.Cod_Pacient}},
                @{N='Nume';E={$_.Nume}},
                @{N='Prenume';E={$_.Prenume}},
                @{N='CNP';E={$_.CNP}},
                @{N='Telefon';E={$_.Telefon}},
                @{N='Email';E={$_.Email}},
                @{N='Activ';E={$_.Activ}} |
                Format-Table -AutoSize
        } else {
            Write-Host "Nu s-au gasit inregistrari." -ForegroundColor Yellow
        }
    }
    
    $connection.Close()
    Write-Host ""
    Write-Host "=============================================" -ForegroundColor Cyan
    Write-Host "TEST COMPLET - SUCCES!" -ForegroundColor Green
    
} catch {
    Write-Host ""
    Write-Host "EROARE:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "Stack Trace:" -ForegroundColor Yellow
    Write-Host $_.Exception.StackTrace -ForegroundColor Yellow
    exit 1
}
