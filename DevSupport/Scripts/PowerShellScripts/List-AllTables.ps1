# Script pentru listarea tuturor tabelelor din DB
$configPath = "..\..\..\ValyanClinic\appsettings.json"

try {
    $config = Get-Content $configPath -Raw | ConvertFrom-Json
    $connectionString = $config.ConnectionStrings.DefaultConnection
    
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()
    
    $command = $connection.CreateCommand()
    $command.CommandText = "SELECT name FROM sys.tables ORDER BY name"
    
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter $command
    $dataset = New-Object System.Data.DataSet
    [void]$adapter.Fill($dataset)
    
    $tables = $dataset.Tables[0]
    
    Write-Host "TOATE TABELELE DIN BAZA DE DATE:" -ForegroundColor Cyan
    Write-Host "=================================" -ForegroundColor Cyan
    
    for ($i = 0; $i -lt $tables.Rows.Count; $i++) {
        $tableName = $tables.Rows[$i]['name']
        Write-Host "$($i+1). $tableName" -ForegroundColor White
    }
    
    Write-Host "`nTotal tabele: $($tables.Rows.Count)" -ForegroundColor Green
    
    $connection.Close()
}
catch {
    Write-Host "Eroare: $_" -ForegroundColor Red
}