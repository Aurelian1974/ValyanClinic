# Script pentru actualizarea stored procedures cu ora local?
# Ruleaz? din directoriul principal al proiectului: D:\Projects\CMS

param(
    [string]$ServerName = "TS1828\ERP",
    [string]$DatabaseName = "ValyanMed"
)

# Configurare pentru erori
$ErrorActionPreference = "Stop"

Write-Host "?? ACTUALIZARE STORED PROCEDURES PENTRU ORA LOCALA" -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host ""

# Verificare conexiune SQL Server
try {
    Write-Host "?? Testare conexiune la server: $ServerName\$DatabaseName" -ForegroundColor Yellow
    
    # Test basic de conexiune
    $connectionString = "Server=$ServerName;Database=$DatabaseName;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True"
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    $connection.Close()
    
    Write-Host "? Conexiunea la baza de date este func?ional?" -ForegroundColor Green
} catch {
    Write-Host "? EROARE: Nu se poate conecta la baza de date: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Lista de stored procedures de actualizat
$storedProcedures = @(
    @{
        Name = "sp_Personal_Create"
        File = "DevSupport\Scripts\SP_Personal_Create.sql"
        Description = "Creare personal cu GETDATE() în loc de GETUTCDATE()"
    },
    @{
        Name = "sp_Personal_Update" 
        File = "DevSupport\Scripts\SP_Personal_Update.sql"
        Description = "Actualizare personal cu GETDATE() în loc de GETUTCDATE()"
    }
)

# Func?ie pentru executarea SQL
function Execute-SqlCommand {
    param(
        [string]$SqlCommand,
        [string]$ConnectionString,
        [string]$Description
    )
    
    try {
        $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
        $connection.Open()
        
        $command = New-Object System.Data.SqlClient.SqlCommand($SqlCommand, $connection)
        $command.CommandTimeout = 120  # 2 minute timeout
        $result = $command.ExecuteNonQuery()
        
        $connection.Close()
        Write-Host "    ? $Description - executat cu succes" -ForegroundColor Green
        return $true
    } catch {
        Write-Host "    ? EROARE la $Description : $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

$totalUpdated = 0
$totalErrors = 0

foreach ($sp in $storedProcedures) {
    Write-Host "?? Actualizare stored procedure: $($sp.Name)" -ForegroundColor Magenta
    
    # Verificare existen?a fi?ierului
    if (-not (Test-Path $sp.File)) {
        Write-Host "    ? Fi?ierul nu exist?: $($sp.File)" -ForegroundColor Red
        $totalErrors++
        continue
    }
    
    # Citire con?inut SQL
    try {
        $sqlContent = Get-Content -Path $sp.File -Raw -Encoding UTF8
        
        if ([string]::IsNullOrWhiteSpace($sqlContent)) {
            Write-Host "    ? Fi?ierul este gol: $($sp.File)" -ForegroundColor Red
            $totalErrors++
            continue
        }
        
        Write-Host "    ?? Fi?ier înc?rcat: $($sp.File)" -ForegroundColor Gray
        
        # Verificare dac? stored procedure-ul exist? ?i îl drop
        $dropSql = "IF OBJECT_ID('$($sp.Name)', 'P') IS NOT NULL DROP PROCEDURE $($sp.Name)"
        
        if (Execute-SqlCommand -SqlCommand $dropSql -ConnectionString $connectionString -Description "Drop existent $($sp.Name)") {
            Write-Host "    ???  Stored procedure existent eliminat" -ForegroundColor Yellow
        }
        
        # Crearea noului stored procedure
        if (Execute-SqlCommand -SqlCommand $sqlContent -ConnectionString $connectionString -Description $sp.Description) {
            Write-Host "    ? $($sp.Name) actualizat cu succes!" -ForegroundColor Green
            $totalUpdated++
        } else {
            $totalErrors++
        }
        
    } catch {
        Write-Host "    ? Eroare la procesarea fi?ierului: $($_.Exception.Message)" -ForegroundColor Red
        $totalErrors++
    }
    
    Write-Host ""
}

# Verificare final? - testare stored procedures
Write-Host "?? TESTARE STORED PROCEDURES ACTUALIZATE" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

foreach ($sp in $storedProcedures) {
    $testSql = "SELECT OBJECT_ID('$($sp.Name)', 'P') as ObjectId"
    
    try {
        $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
        $connection.Open()
        
        $command = New-Object System.Data.SqlClient.SqlCommand($testSql, $connection)
        $result = $command.ExecuteScalar()
        
        $connection.Close()
        
        if ($result -and $result -ne [System.DBNull]::Value) {
            Write-Host "? $($sp.Name) exist? ?i este disponibil" -ForegroundColor Green
        } else {
            Write-Host "? $($sp.Name) NU exist? în baza de date" -ForegroundColor Red
            $totalErrors++
        }
    } catch {
        Write-Host "? Eroare la testarea $($sp.Name): $($_.Exception.Message)" -ForegroundColor Red
        $totalErrors++
    }
}

# Sumar final
Write-Host ""
Write-Host "?? SUMAR ACTUALIZARE" -ForegroundColor Cyan
Write-Host "=====================" -ForegroundColor Cyan
Write-Host "Stored procedures actualizate: $totalUpdated" -ForegroundColor Green
Write-Host "Erori întâlnite: $totalErrors" -ForegroundColor $(if($totalErrors -eq 0){"Green"}else{"Red"})

if ($totalErrors -eq 0) {
    Write-Host ""
    Write-Host "?? ACTUALIZARE COMPLET? CU SUCCES!" -ForegroundColor Green
    Write-Host "Toate stored procedures folosesc acum GETDATE() pentru consisten?? cu aplica?ia." -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "??  ACTUALIZARE COMPLETAT? CU ERORI" -ForegroundColor Yellow
    Write-Host "Verifica?i erorile de mai sus ?i relua?i procesul dac? este necesar." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "? Pentru a testa func?ionalitatea:" -ForegroundColor Cyan
Write-Host "1. Rula?i aplica?ia ValyanClinic" -ForegroundColor White
Write-Host "2. Naviga?i la Administrare > Personal" -ForegroundColor White  
Write-Host "3. Testa?i ad?ugarea ?i editarea unei persoane" -ForegroundColor White
Write-Host "4. Verifica?i c? nu mai apar erori de validare pentru Data_Ultimei_Modificari" -ForegroundColor White