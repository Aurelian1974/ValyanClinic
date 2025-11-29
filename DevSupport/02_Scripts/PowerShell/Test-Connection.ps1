# Script de test pentru verificarea conexiunii
$configPath = "..\..\..\ValyanClinic\appsettings.json"

try {
    $config = Get-Content $configPath -Raw | ConvertFrom-Json
    $connectionString = $config.ConnectionStrings.DefaultConnection
    Write-Host "Connection string: $connectionString" -ForegroundColor Green
    
    # Test conexiune
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()
    
    Write-Host "Conexiune OK!" -ForegroundColor Green
    
    # Test query simplu
    $command = $connection.CreateCommand()
    $command.CommandText = "SELECT name FROM sys.tables WHERE name IN ('Personal', 'PersonalMedical') ORDER BY name"
    
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter $command
    $dataset = New-Object System.Data.DataSet
    [void]$adapter.Fill($dataset)
    
    $tables = $dataset.Tables[0]
    Write-Host "Tabele gasite: $($tables.Rows.Count)" -ForegroundColor Green
    
    foreach ($row in $tables.Rows) {
        Write-Host "  - $($row['name'])" -ForegroundColor Cyan
    }
    
    $connection.Close()
}
catch {
    Write-Host "Eroare: $_" -ForegroundColor Red
}