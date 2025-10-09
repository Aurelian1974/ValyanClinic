# Script pentru verificarea existentei tabelului Ocupatii_ISCO08
$configPath = "..\..\..\ValyanClinic\appsettings.json"

Write-Host "Verific daca tabelul Ocupatii_ISCO08 exista..." -ForegroundColor Yellow

try {
    $config = Get-Content $configPath -Raw | ConvertFrom-Json
    $connectionString = $config.ConnectionStrings.DefaultConnection
    
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()
    
    $command = $connection.CreateCommand()
    $command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Ocupatii_ISCO08'"
    $tableExists = $command.ExecuteScalar()
    
    if ($tableExists -gt 0) {
        Write-Host "? Tabelul Ocupatii_ISCO08 EXISTA deja in baza de date" -ForegroundColor Green
        
        # Verifica daca are date
        $command.CommandText = "SELECT COUNT(*) FROM Ocupatii_ISCO08"
        $recordCount = $command.ExecuteScalar()
        
        Write-Host "?? Numar inregistrari existente: $recordCount" -ForegroundColor Cyan
    } else {
        Write-Host "? Tabelul Ocupatii_ISCO08 NU EXISTA in baza de date" -ForegroundColor Red
        Write-Host "Trebuie sa rulezi mai intai scriptul de creare: Ocupatii_ISCO08_Structure.sql" -ForegroundColor Yellow
    }
    
    $connection.Close()
}
catch {
    Write-Host "? Eroare: $_" -ForegroundColor Red
}