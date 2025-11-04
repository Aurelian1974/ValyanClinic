# ========================================
# Script PowerShell pentru Rulare Scripturi SQL - Pacienti
# Database: ValyanMed
# ========================================

Write-Host "===========================================" -ForegroundColor Cyan
Write-Host "   Rulare Scripturi SQL pentru Pacienti   " -ForegroundColor Cyan
Write-Host "===========================================" -ForegroundColor Cyan
Write-Host ""

# Configurare conexiune SQL Server
$ServerInstance = "localhost"
$Database = "ValyanMed"

# Calea catre scripturile SQL
$BasePathTableStructure = "D:\Lucru\CMS\DevSupport\Database\TableStructure"
$BasePathStoredProcedures = "D:\Lucru\CMS\DevSupport\Database\StoredProcedures"

$TableScript = "$BasePathTableStructure\Pacienti_Complete.sql"
$StoredProcScript = "$BasePathStoredProcedures\sp_Pacienti.sql"

# Verificare existenta scripturilor
if (-not (Test-Path $TableScript)) {
    Write-Host "EROARE: Nu s-a gasit scriptul pentru tabela: $TableScript" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $StoredProcScript)) {
    Write-Host "EROARE: Nu s-a gasit scriptul pentru stored procedures: $StoredProcScript" -ForegroundColor Red
    exit 1
}

try {
    Write-Host "1. Creare tabela Pacienti..." -ForegroundColor Yellow
    Invoke-Sqlcmd -ServerInstance $ServerInstance -Database $Database -InputFile $TableScript -ErrorAction Stop
    Write-Host "   ? Tabela Pacienti creata cu succes!" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "2. Creare Stored Procedures pentru Pacienti..." -ForegroundColor Yellow
    Invoke-Sqlcmd -ServerInstance $ServerInstance -Database $Database -InputFile $StoredProcScript -ErrorAction Stop
    Write-Host "   ? Stored Procedures create cu succes!" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "===========================================" -ForegroundColor Cyan
    Write-Host "   SETUP COMPLET - SUCCES!                " -ForegroundColor Green
    Write-Host "===========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Baza de date este configurata corect pentru modulul Pacienti." -ForegroundColor Green
    Write-Host "Poti porni aplicatia acum!" -ForegroundColor Green
    Write-Host ""
    
    # Verificare tabela si stored procedures
    Write-Host "Verificare tabela si stored procedures..." -ForegroundColor Yellow
    $verifyQuery = @"
-- Verificare tabela
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Pacienti')
    PRINT '? Tabela Pacienti: GASITA'
ELSE
    PRINT '? Tabela Pacienti: NU EXISTA'

-- Verificare stored procedures
SELECT COUNT(*) AS [Nr_Stored_Procedures]
FROM sys.procedures 
WHERE name LIKE 'sp_Pacienti_%'
"@
    
    Invoke-Sqlcmd -ServerInstance $ServerInstance -Database $Database -Query $verifyQuery -ErrorAction Stop
    
} catch {
    Write-Host ""
    Write-Host "===========================================" -ForegroundColor Red
    Write-Host "   EROARE LA RULARE SCRIPTURI!            " -ForegroundColor Red
    Write-Host "===========================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Detalii eroare:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "Sugestii:" -ForegroundColor Yellow
    Write-Host "1. Verifica ca SQL Server ruleaza" -ForegroundColor Yellow
    Write-Host "2. Verifica ca baza de date 'ValyanMed' exista" -ForegroundColor Yellow
    Write-Host "3. Verifica ca ai permisiuni pentru a crea tabele si stored procedures" -ForegroundColor Yellow
    Write-Host ""
    exit 1
}

Write-Host "Apasa orice tasta pentru a inchide..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
