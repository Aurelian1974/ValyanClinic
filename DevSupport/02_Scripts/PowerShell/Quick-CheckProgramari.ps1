# ========================================
# Quick Check pentru Tabelul Programari
# ValyanClinic - Database Quick Verification
# ========================================

$Server = "DESKTOP-3Q8HI82\ERP"
$Database = "ValyanMed"

Write-Host "`n=== QUICK CHECK PROGRAMARI ===" -ForegroundColor Cyan
Write-Host "Server: $Server" -ForegroundColor Gray
Write-Host "Database: $Database`n" -ForegroundColor Gray

$connectionString = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;"

function Invoke-QuickQuery {
    param([string]$Query, [string]$Description)
    
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = $Query
        $command.CommandTimeout = 10
    
        $adapter = New-Object System.Data.SqlClient.SqlDataAdapter $command
        $dataset = New-Object System.Data.DataSet
        [void]$adapter.Fill($dataset)
        
        return $dataset.Tables[0]
    }
    catch {
        Write-Host "? Eroare la $Description`: $_" -ForegroundColor Red
     return $null
    }
    finally {
        if ($connection.State -eq 'Open') {
        $connection.Close()
        }
    }
}

# 1. Test conexiune
Write-Host "[1] Test conexiune..." -ForegroundColor Yellow
$testResult = Invoke-QuickQuery -Query "SELECT 1 AS Test" -Description "test conexiune"
if ($testResult) {
    Write-Host "    ? Conectat cu succes!`n" -ForegroundColor Green
} else {
    Write-Host "    ? Conexiune esuata!`n" -ForegroundColor Red
    exit 1
}

# 2. Verifica tabel Programari
Write-Host "[2] Verifica tabel Programari..." -ForegroundColor Yellow
$tableCheck = Invoke-QuickQuery -Query "SELECT CASE WHEN EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Programari') THEN 1 ELSE 0 END AS TableExists" -Description "verificare tabel"
if ($tableCheck -and $tableCheck.Rows[0]['TableExists'] -eq 1) {
    Write-Host "    ? Tabelul Programari EXISTA" -ForegroundColor Green
  
    # Numarare inregistrari
    $count = Invoke-QuickQuery -Query "SELECT COUNT(*) AS Total FROM Programari" -Description "numarare inregistrari"
    if ($count) {
 Write-Host "    Total inregistrari: $($count.Rows[0]['Total'])`n" -ForegroundColor Gray
    }
} else {
  Write-Host "    ? Tabelul Programari NU EXISTA!`n" -ForegroundColor Red
}

# 3. Verifica Stored Procedures
Write-Host "[3] Verifica Stored Procedures..." -ForegroundColor Yellow
$spCheck = Invoke-QuickQuery -Query "SELECT COUNT(*) AS Total FROM sys.procedures WHERE name LIKE 'sp_Programari_%'" -Description "verificare SP"
if ($spCheck) {
    $spCount = $spCheck.Rows[0]['Total']
    if ($spCount -gt 0) {
        Write-Host "    ? Gasite $spCount stored procedures" -ForegroundColor Green
        
        # Lista SP-uri
   $spList = Invoke-QuickQuery -Query "SELECT name FROM sys.procedures WHERE name LIKE 'sp_Programari_%' ORDER BY name" -Description "lista SP"
        if ($spList) {
            foreach ($sp in $spList.Rows) {
 Write-Host "      - $($sp['name'])" -ForegroundColor Gray
            }
        }
    } else {
        Write-Host "    ? NICIO Stored Procedure gasita!" -ForegroundColor Red
      Write-Host "      Trebuie create SP-uri pentru:" -ForegroundColor Yellow
        Write-Host "      - sp_Programari_GetAll" -ForegroundColor Gray
    Write-Host "   - sp_Programari_GetById" -ForegroundColor Gray
   Write-Host "      - sp_Programari_Create" -ForegroundColor Gray
        Write-Host "      - sp_Programari_Update" -ForegroundColor Gray
        Write-Host "      - sp_Programari_Delete" -ForegroundColor Gray
  }
    Write-Host ""
}

# 4. Verifica Foreign Keys
Write-Host "[4] Verifica Foreign Keys..." -ForegroundColor Yellow
$fkCheck = Invoke-QuickQuery -Query "SELECT COUNT(*) AS Total FROM sys.foreign_keys WHERE OBJECT_NAME(parent_object_id) = 'Programari'" -Description "verificare FK"
if ($fkCheck) {
    $fkCount = $fkCheck.Rows[0]['Total']
    if ($fkCount -gt 0) {
  Write-Host "    ? Gasite $fkCount foreign keys`n" -ForegroundColor Green
    } else {
        Write-Host "    ! Nicio foreign key gasita`n" -ForegroundColor Yellow
    }
}

# 5. Verifica tabele relationate
Write-Host "[5] Verifica tabele relationate..." -ForegroundColor Yellow
$tables = @('Pacienti', 'PersonalMedical', 'Consultatii')
foreach ($table in $tables) {
    $checkTable = Invoke-QuickQuery -Query "SELECT CASE WHEN EXISTS (SELECT 1 FROM sys.tables WHERE name = '$table') THEN 1 ELSE 0 END AS TableExists" -Description "verificare $table"
  if ($checkTable -and $checkTable.Rows[0]['TableExists'] -eq 1) {
        Write-Host "    ? $table" -ForegroundColor Green
 } else {
        Write-Host "    ? $table NU EXISTA" -ForegroundColor Red
    }
}
Write-Host ""

# Sumar final
Write-Host "=== SUMAR ===" -ForegroundColor Cyan
if ($tableCheck -and $tableCheck.Rows[0]['TableExists'] -eq 1) {
    Write-Host "? Tabelul Programari este PREZENT in baza de date" -ForegroundColor Green
    
    if ($spCheck -and $spCheck.Rows[0]['Total'] -gt 0) {
        Write-Host "? Stored Procedures sunt DISPONIBILE" -ForegroundColor Green
        Write-Host "`nREADY pentru implementare Application Layer!" -ForegroundColor Green
    } else {
        Write-Host "! Stored Procedures LIPSESC - trebuie create" -ForegroundColor Yellow
        Write-Host "`nNEXT STEP: Creaza stored procedures pentru Programari" -ForegroundColor Yellow
    }
} else {
    Write-Host "? Tabelul Programari LIPSESTE din baza de date" -ForegroundColor Red
    Write-Host "`nNEXT STEP: Ruleaza DevSupport/Database/TableStructure/Programari_Complete.sql" -ForegroundColor Yellow
}

Write-Host "`n========================================`n" -ForegroundColor Cyan
