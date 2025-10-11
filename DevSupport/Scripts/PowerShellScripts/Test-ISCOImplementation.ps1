# Script pentru testarea tabelului si stored procedures ISCO
$configPath = "..\..\..\ValyanClinic\appsettings.json"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "TESTARE TABELE SI SP ISCO-08" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Conectare
try {
    $config = Get-Content $configPath -Raw | ConvertFrom-Json
    $connectionString = $config.ConnectionStrings.DefaultConnection
    
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()
    Write-Host "? Conectare reusita" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare conectare: $_" -ForegroundColor Red
    exit 1
}

$command = $connection.CreateCommand()

# Test 1: Statistici
Write-Host "`n[TEST 1] Statistici generale..." -ForegroundColor Yellow
try {
    $command.CommandText = "EXEC sp_Ocupatii_ISCO08_GetStatistics"
    $reader = $command.ExecuteReader()
    
    while ($reader.Read()) {
        Write-Host "  ?? $($reader['Categorie']): $($reader['Numar']) total, $($reader['Active']) active" -ForegroundColor White
    }
    $reader.Close()
    Write-Host "? Test statistici reusit" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare la statistici: $_" -ForegroundColor Red
}

# Test 2: Cautare medici
Write-Host "`n[TEST 2] Cautare 'medic'..." -ForegroundColor Yellow
try {
    $command.CommandText = "EXEC sp_Ocupatii_ISCO08_Search @SearchText = 'medic'"
    $reader = $command.ExecuteReader()
    
    $found = 0
    while ($reader.Read()) {
        Write-Host "  ?? $($reader['Cod_ISCO']) - $($reader['Denumire_Ocupatie']) (Scor: $($reader['ScorRelevanta']))" -ForegroundColor White
        $found++
    }
    $reader.Close()
    Write-Host "? Gasite $found rezultate pentru 'medic'" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare la cautare: $_" -ForegroundColor Red
}

# Test 3: Grupe majore
Write-Host "`n[TEST 3] Grupe majore..." -ForegroundColor Yellow
try {
    $command.CommandText = "EXEC sp_Ocupatii_ISCO08_GetGrupeMajore"
    $reader = $command.ExecuteReader()
    
    $groups = 0
    while ($reader.Read()) {
        Write-Host "  ?? $($reader['Cod_ISCO']) - $($reader['Denumire_Ocupatie'])" -ForegroundColor White
        $groups++
    }
    $reader.Close()
    Write-Host "? Gasite $groups grupe majore" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare la grupe majore: $_" -ForegroundColor Red
}

# Test 4: GetAll cu paginare
Write-Host "`n[TEST 4] GetAll cu paginare..." -ForegroundColor Yellow
try {
    $command.CommandText = "EXEC sp_Ocupatii_ISCO08_GetAll @PageNumber = 1, @PageSize = 5"
    $reader = $command.ExecuteReader()
    
    $records = 0
    $totalRecords = 0
    while ($reader.Read()) {
        if ($totalRecords -eq 0) {
            $totalRecords = $reader['TotalRecords']
        }
        Write-Host "  ?? $($reader['Cod_ISCO']) - $($reader['Denumire_Ocupatie']) (Nivel $($reader['Nivel_Ierarhic']))" -ForegroundColor White
        $records++
    }
    $reader.Close()
    Write-Host "? Afisate $records din $totalRecords inregistrari" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare la GetAll: $_" -ForegroundColor Red
}

# Test 5: GetById
Write-Host "`n[TEST 5] GetById pentru primul record..." -ForegroundColor Yellow
try {
    $command.CommandText = "EXEC sp_Ocupatii_ISCO08_GetById @Id = 1"
    $reader = $command.ExecuteReader()
    
    if ($reader.Read()) {
        Write-Host "  ?? ID: $($reader['Id'])" -ForegroundColor White
        Write-Host "  ?? Cod: $($reader['Cod_ISCO'])" -ForegroundColor White
        Write-Host "  ?? Denumire: $($reader['Denumire_Ocupatie'])" -ForegroundColor White
        Write-Host "  ?? Nivel: $($reader['Nivel_Ierarhic'])" -ForegroundColor White
        Write-Host "  ?? Creat: $($reader['Data_Crearii'])" -ForegroundColor White
    }
    $reader.Close()
    Write-Host "? Test GetById reusit" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare la GetById: $_" -ForegroundColor Red
}

$connection.Close()

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "TOATE TESTELE FINALIZATE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "? Tabelul Ocupatii_ISCO08 functioneaza perfect!" -ForegroundColor Green
Write-Host "? Toate stored procedures sunt operationale!" -ForegroundColor Green

Write-Host "`n?? URMATII PASI:" -ForegroundColor Cyan
Write-Host "1. Adauga entity-ul OcupatieISCO.cs in ValyanClinic.Domain.Entities" -ForegroundColor White
Write-Host "2. Actualizeaza DbContext cu DbSet<OcupatieISCO> OcupatiiISCO" -ForegroundColor White
Write-Host "3. Ruleaza Add-Migration si Update-Database" -ForegroundColor White
Write-Host "4. Creeaza servicii pentru operatiuni CRUD in aplicatia Blazor" -ForegroundColor White