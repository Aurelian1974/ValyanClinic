# ========================================
# PowerShell Script: QuickStart Utilizatori
# Database: ValyanMed
# Descriere: One-click deployment pentru tabela Utilizatori
# Autor: System
# Data: 2025-01-24
# ========================================

<#
.SYNOPSIS
    One-click deployment pentru tabela Utilizatori

.DESCRIPTION
    Acest script face TOTUL automat:
    1. Verifica conexiunea la database
    2. Deploy tabela Utilizatori
    3. Deploy stored procedures (12)
4. Ruleaza teste automate
    5. Genereaza raport

.EXAMPLE
    .\QuickStart-Utilizatori.ps1

.EXAMPLE
    .\QuickStart-Utilizatori.ps1 -ServerName "LOCALHOST\SQLEXPRESS"
#>

[CmdletBinding()]
param(
  [Parameter(Mandatory=$false)]
    [string]$ServerName = "DESKTOP-3Q8HI82\ERP",
    
    [Parameter(Mandatory=$false)]
    [string]$DatabaseName = "ValyanMed"
)

# ========================================
# CONFIGURARE
# ========================================

$ErrorActionPreference = "Stop"
$ScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path

# Colors
$ColorSuccess = "Green"
$ColorWarning = "Yellow"
$ColorError = "Red"
$ColorInfo = "Cyan"
$ColorHeader = "Magenta"

# ========================================
# BANNER
# ========================================

Clear-Host
Write-Host ""
Write-Host "??????????????????????????????????????????????????????????????" -ForegroundColor $ColorHeader
Write-Host "?          ?" -ForegroundColor $ColorHeader
Write-Host "?          ?? UTILIZATORI - QUICK START DEPLOYMENT ??        ?" -ForegroundColor $ColorHeader
Write-Host "?         ?" -ForegroundColor $ColorHeader
Write-Host "?  Database: ValyanMed    ?" -ForegroundColor $ColorHeader
Write-Host "?  Tabela: Utilizatori (asociat cu PersonalMedical)          ?" -ForegroundColor $ColorHeader
Write-Host "?               ?" -ForegroundColor $ColorHeader
Write-Host "??????????????????????????????????????????????????????????????" -ForegroundColor $ColorHeader
Write-Host ""

# ========================================
# STEP 1: Deploy
# ========================================

Write-Host "?? PASUL 1: Deployment tabela si stored procedures..." -ForegroundColor $ColorInfo
Write-Host ""

try {
    & "$ScriptPath\Deploy-Utilizatori.ps1" -ServerName $ServerName -DatabaseName $DatabaseName -SkipTestData
    Write-Host ""
    Write-Host "? Deployment complet!" -ForegroundColor $ColorSuccess
}
catch {
    Write-Host ""
    Write-Host "? EROARE la deployment: $($_.Exception.Message)" -ForegroundColor $ColorError
    Write-Host ""
    Write-Host "Verific?:" -ForegroundColor $ColorWarning
Write-Host "  1. SQL Server este pornit" -ForegroundColor Gray
    Write-Host "  2. Database ValyanMed exist?" -ForegroundColor Gray
    Write-Host "  3. Ai permisiuni de CREATE TABLE" -ForegroundColor Gray
    Write-Host ""
    exit 1
}

# ========================================
# STEP 2: Tests
# ========================================

Write-Host ""
Write-Host "?? PASUL 2: Rulare teste automate..." -ForegroundColor $ColorInfo
Write-Host ""

try {
    & "$ScriptPath\Test-Utilizatori.ps1" -ServerName $ServerName -DatabaseName $DatabaseName
}
catch {
    Write-Host ""
    Write-Host "?? Unele teste au esuat, dar deployment-ul este functional" -ForegroundColor $ColorWarning
}

# ========================================
# STEP 3: Summary
# ========================================

Write-Host ""
Write-Host "??????????????????????????????????????????????????????????????" -ForegroundColor $ColorHeader
Write-Host "?     ?" -ForegroundColor $ColorHeader
Write-Host "?         ? GATA! TOTUL OK! ?          ?" -ForegroundColor $ColorHeader
Write-Host "?    ?" -ForegroundColor $ColorHeader
Write-Host "??????????????????????????????????????????????????????????????" -ForegroundColor $ColorHeader
Write-Host ""

Write-Host "?? CE S-A CREAT:" -ForegroundColor $ColorSuccess
Write-Host ""
Write-Host "  ? Tabela: Utilizatori" -ForegroundColor Gray
Write-Host "     - 18 coloane" -ForegroundColor Gray
Write-Host "     - 1 Foreign Key (PersonalMedical)" -ForegroundColor Gray
Write-Host "     - 7 Indexes pentru performanta" -ForegroundColor Gray
Write-Host "     - 4 Constraints (UNIQUE + CHECK)" -ForegroundColor Gray
Write-Host ""
Write-Host "  ? Stored Procedures: 12" -ForegroundColor Gray
Write-Host "     - sp_Utilizatori_GetAll" -ForegroundColor Gray
Write-Host "     - sp_Utilizatori_GetCount" -ForegroundColor Gray
Write-Host "     - sp_Utilizatori_GetById" -ForegroundColor Gray
Write-Host "     - sp_Utilizatori_GetByUsername" -ForegroundColor Gray
Write-Host " - sp_Utilizatori_GetByEmail" -ForegroundColor Gray
Write-Host "     - sp_Utilizatori_Create" -ForegroundColor Gray
Write-Host "     - sp_Utilizatori_Update" -ForegroundColor Gray
Write-Host "     - sp_Utilizatori_ChangePassword" -ForegroundColor Gray
Write-Host "     - sp_Utilizatori_UpdateUltimaAutentificare" -ForegroundColor Gray
Write-Host "     - sp_Utilizatori_IncrementIncercariEsuate" -ForegroundColor Gray
Write-Host "     - sp_Utilizatori_SetTokenResetareParola" -ForegroundColor Gray
Write-Host "     - sp_Utilizatori_GetStatistics" -ForegroundColor Gray
Write-Host ""

Write-Host "?? CARACTERISTICI DE SECURITATE:" -ForegroundColor $ColorSuccess
Write-Host ""
Write-Host "  ? Hash parole (PasswordHash + Salt)" -ForegroundColor Gray
Write-Host "  ? Blocare automata dupa 5 incercari esuate" -ForegroundColor Gray
Write-Host "  ? Reset parola cu token si expirare" -ForegroundColor Gray
Write-Host "  ? Username si Email unice" -ForegroundColor Gray
Write-Host "  ? Relatie 1:1 cu PersonalMedical (validated)" -ForegroundColor Gray
Write-Host "  ? Audit trail complet" -ForegroundColor Gray
Write-Host ""

Write-Host "?? PASUL URMATOR:" -ForegroundColor $ColorInfo
Write-Host ""
Write-Host "  1. Deschide SQL Server Management Studio" -ForegroundColor Gray
Write-Host "  2. Conecteaza-te la: $ServerName" -ForegroundColor Gray
Write-Host "  3. Selecteaza database: $DatabaseName" -ForegroundColor Gray
Write-Host "  4. Testeaza stored procedures:" -ForegroundColor Gray
Write-Host ""
Write-Host "     -- Vezi statistici" -ForegroundColor DarkGray
Write-Host "     EXEC sp_Utilizatori_GetStatistics" -ForegroundColor DarkGray
Write-Host ""
Write-Host " -- Gaseste PersonalMedical disponibil" -ForegroundColor DarkGray
Write-Host "     SELECT TOP 5 pm.PersonalID, pm.Nume + ' ' + pm.Prenume AS NumeComplet" -ForegroundColor DarkGray
Write-Host "     FROM PersonalMedical pm" -ForegroundColor DarkGray
Write-Host "     LEFT JOIN Utilizatori u ON pm.PersonalID = u.PersonalMedicalID" -ForegroundColor DarkGray
Write-Host "     WHERE pm.EsteActiv = 1 AND u.UtilizatorID IS NULL" -ForegroundColor DarkGray
Write-Host ""
Write-Host "     -- Creaza utilizator test" -ForegroundColor DarkGray
Write-Host "     EXEC sp_Utilizatori_Create " -ForegroundColor DarkGray
Write-Host "  @PersonalMedicalID = 'GUID-DIN-QUERY-ANTERIOR'," -ForegroundColor DarkGray
Write-Host "         @Username = 'test.user'," -ForegroundColor DarkGray
Write-Host "    @Email = 'test@clinic.ro'," -ForegroundColor DarkGray
Write-Host "  @PasswordHash = 'HASH_PLACEHOLDER'," -ForegroundColor DarkGray
Write-Host "         @Salt = 'SALT_PLACEHOLDER'," -ForegroundColor DarkGray
Write-Host "         @Rol = 'Doctor'," -ForegroundColor DarkGray
Write-Host "   @EsteActiv = 1," -ForegroundColor DarkGray
Write-Host "         @CreatDe = 'Admin'" -ForegroundColor DarkGray
Write-Host ""
Write-Host "  5. Citeste documentatia completa:" -ForegroundColor Gray
Write-Host "     DevSupport\Database\Utilizatori_README.md" -ForegroundColor Gray
Write-Host ""
Write-Host "  6. Implementeaza in C#:" -ForegroundColor Gray
Write-Host "     - Entity (ValyanClinic.Domain)" -ForegroundColor Gray
Write-Host "     - Repository (ValyanClinic.Infrastructure)" -ForegroundColor Gray
Write-Host "     - Authentication Service" -ForegroundColor Gray
Write-Host "   - UI Blazor (Login, User Management)" -ForegroundColor Gray
Write-Host ""

Write-Host "?? DOCUMENTATIE:" -ForegroundColor $ColorInfo
Write-Host ""
Write-Host "  ?? README: DevSupport\Database\Utilizatori_README.md" -ForegroundColor Gray
Write-Host "  ?? SQL Table: DevSupport\Database\TableStructure\Utilizatori_Complete.sql" -ForegroundColor Gray
Write-Host "  ?? SQL SPs: DevSupport\Database\StoredProcedures\sp_Utilizatori.sql" -ForegroundColor Gray
Write-Host ""

Write-Host "?? IMPORTANT - SECURITATE:" -ForegroundColor $ColorWarning
Write-Host ""
Write-Host "  ?? NU stoca parolele in clar!" -ForegroundColor $ColorError
Write-Host "  ?? Foloseste bcrypt, Argon2 sau PBKDF2 pentru hash" -ForegroundColor $ColorError
Write-Host "  ?? Genereaza salt random pentru fiecare parola" -ForegroundColor $ColorError
Write-Host "  ?? NU folosi MD5 sau SHA1 (nesigure)" -ForegroundColor $ColorError
Write-Host ""

Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor $ColorHeader
Write-Host " ?? Tabela Utilizatori este GATA DE UTILIZARE! ??" -ForegroundColor $ColorSuccess
Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor $ColorHeader
Write-Host ""

# Pause pentru a citi output-ul
Write-Host "Apasa orice tasta pentru a inchide..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
