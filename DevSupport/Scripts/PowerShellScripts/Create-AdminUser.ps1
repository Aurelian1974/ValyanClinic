# ========================================
# PowerShell Script: Create Admin Superuser
# Database: ValyanMed
# Descriere: Creeaz? utilizatorul Admin cu parola admin123!@#
# Autor: System
# Data: 2025-01-24
# ========================================

<#
.SYNOPSIS
    Creeaz? utilizatorul Admin (superuser)

.DESCRIPTION
    Acest script creeaz? utilizatorul Admin în tabela Utilizatori.
    Username: Admin
    Parola: admin123!@#
    Rol: Administrator
    
  Hash-ul parolei este generat cu BCrypt (Work Factor 12).

.EXAMPLE
    .\Create-AdminUser.ps1
    
.EXAMPLE
  .\Create-AdminUser.ps1 -ServerName "LOCALHOST\SQLEXPRESS"
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [string]$ServerName = "DESKTOP-3Q8HI82\ERP",
    
    [Parameter(Mandatory=$false)]
    [string]$DatabaseName = "ValyanMed",
    
    [Parameter(Mandatory=$false)]
    [switch]$Force
)

# ========================================
# CONFIGURARE
# ========================================

$ErrorActionPreference = "Stop"

# Colors
$ColorSuccess = "Green"
$ColorWarning = "Yellow"
$ColorError = "Red"
$ColorInfo = "Cyan"
$ColorHeader = "Magenta"

# Admin credentials
$AdminUsername = "Admin"
$AdminPassword = "admin123!@#"
$AdminEmail = "admin@valyan.clinic"
$AdminRole = "Administrator"

# BCrypt hash for "admin123!@#" with Work Factor 12
# Generated using BCrypt.Net-Next
# IMPORTANT: Acest hash este pre-generat pentru simplitate
# În produc?ie, ar trebui generat dinamic
$AdminPasswordHash = '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMesbjx.U4T6wgSJc4xE7iW.Im'

# ========================================
# FUNCTII HELPER
# ========================================

function Write-Header {
    param([string]$Message)
    Write-Host ""
    Write-Host "========================================" -ForegroundColor $ColorHeader
    Write-Host " $Message" -ForegroundColor $ColorHeader
    Write-Host "========================================" -ForegroundColor $ColorHeader
    Write-Host ""
}

function Test-SqlConnection {
    param(
      [string]$Server,
      [string]$Database
)
    
    try {
        $connectionString = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;"
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
        $connection.Open()
        $connection.Close()
        return $true
    }
    catch {
        return $false
    }
}

function Invoke-SqlQuery {
    param(
        [string]$Server,
        [string]$Database,
        [string]$Query
    )
    
    $connectionString = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;"
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $command = $connection.CreateCommand()
    $command.CommandText = $Query
    
    $connection.Open()
    try {
        $adapter = New-Object System.Data.SqlClient.SqlDataAdapter $command
 $dataset = New-Object System.Data.DataSet
        $adapter.Fill($dataset) | Out-Null
    return $dataset.Tables[0]
    }
    finally {
        $connection.Close()
    }
}

function Invoke-SqlNonQuery {
    param(
        [string]$Server,
        [string]$Database,
        [string]$Query
    )
    
    $connectionString = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;"
 $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $command = $connection.CreateCommand()
    $command.CommandText = $Query
    
    $connection.Open()
    try {
        return $command.ExecuteNonQuery()
    }
    finally {
     $connection.Close()
    }
}

# ========================================
# MAIN SCRIPT
# ========================================

Write-Header "CREATE ADMIN SUPERUSER"

Write-Host "Server: $ServerName" -ForegroundColor $ColorInfo
Write-Host "Database: $DatabaseName" -ForegroundColor $ColorInfo
Write-Host ""
Write-Host "Username: $AdminUsername" -ForegroundColor $ColorInfo
Write-Host "Password: $AdminPassword" -ForegroundColor $ColorInfo
Write-Host "Email: $AdminEmail" -ForegroundColor $ColorInfo
Write-Host "Rol: $AdminRole" -ForegroundColor $ColorInfo
Write-Host ""

# ========================================
# STEP 1: Verificare conexiune
# ========================================

Write-Host "Pasul 1: Verificare conexiune la database..." -ForegroundColor $ColorInfo

if (-not (Test-SqlConnection -Server $ServerName -Database $DatabaseName)) {
    Write-Host "? Nu s-a putut conecta la database!" -ForegroundColor $ColorError
    exit 1
}

Write-Host "? Conexiune stabil?" -ForegroundColor $ColorSuccess
Write-Host ""

# ========================================
# STEP 2: Verificare utilizator existent
# ========================================

Write-Host "Pasul 2: Verificare utilizator Admin existent..." -ForegroundColor $ColorInfo

$checkUserQuery = "SELECT UtilizatorID, Username, Email FROM Utilizatori WHERE Username = '$AdminUsername'"
$existingUser = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $checkUserQuery

if ($existingUser.Rows.Count -gt 0) {
    Write-Host "?? Utilizatorul Admin exist? deja!" -ForegroundColor $ColorWarning
    Write-Host "   Username: $($existingUser.Rows[0].Username)" -ForegroundColor Gray
    Write-Host "   Email: $($existingUser.Rows[0].Email)" -ForegroundColor Gray
 Write-Host ""
    
    if (-not $Force) {
        $confirmation = Read-Host "Dore?ti s? ?tergi utilizatorul existent ?i s? creezi unul nou? (DA/NU)"
        
if ($confirmation -ne "DA") {
    Write-Host "?? Opera?iune anulat?" -ForegroundColor $ColorInfo
         exit 0
        }
        
        # Delete existing user
 $deleteQuery = "DELETE FROM Utilizatori WHERE Username = '$AdminUsername'"
 Invoke-SqlNonQuery -Server $ServerName -Database $DatabaseName -Query $deleteQuery | Out-Null
  Write-Host "? Utilizatorul existent a fost ?ters" -ForegroundColor $ColorSuccess
    }
    else {
        # Force delete
    $deleteQuery = "DELETE FROM Utilizatori WHERE Username = '$AdminUsername'"
        Invoke-SqlNonQuery -Server $ServerName -Database $DatabaseName -Query $deleteQuery | Out-Null
        Write-Host "? Utilizatorul existent a fost ?ters (Force)" -ForegroundColor $ColorSuccess
    }
}
else {
    Write-Host "? Utilizatorul Admin nu exist? - poate fi creat" -ForegroundColor $ColorSuccess
}

Write-Host ""

# ========================================
# STEP 3: G?sire/Creare PersonalMedical pentru Admin
# ========================================

Write-Host "Pasul 3: G?sire/Creare PersonalMedical pentru Admin..." -ForegroundColor $ColorInfo

# Check if PersonalMedical for Admin exists
$checkPMQuery = @"
SELECT PersonalID, Nume, Prenume, Email 
FROM PersonalMedical 
WHERE Nume = 'System' AND Prenume = 'Administrator'
"@

$existingPM = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $checkPMQuery

if ($existingPM.Rows.Count -eq 0) {
    Write-Host "?? PersonalMedical pentru Admin nu exist? - se creeaz?..." -ForegroundColor $ColorWarning
    
    # Create PersonalMedical for Admin
    $createPMQuery = @"
DECLARE @PersonalID UNIQUEIDENTIFIER = NEWID();

INSERT INTO PersonalMedical (
PersonalID,
    Nume,
    Prenume,
    Email,
    Telefon,
    Specializare,
    Departament,
    Pozitie,
    EsteActiv,
    DataCreare
)
VALUES (
    @PersonalID,
 'System',
    'Administrator',
    '$AdminEmail',
    NULL,
    'Administrare Sistem',
  'IT',
    'Super Administrator',
    1,
    GETDATE()
);

SELECT @PersonalID AS PersonalID;
"@
    
    $pmResult = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $createPMQuery
    $personalMedicalID = $pmResult.Rows[0].PersonalID
    
  Write-Host "? PersonalMedical creat: $personalMedicalID" -ForegroundColor $ColorSuccess
}
else {
    $personalMedicalID = $existingPM.Rows[0].PersonalID
    Write-Host "? PersonalMedical g?sit: $personalMedicalID" -ForegroundColor $ColorSuccess
}

Write-Host ""

# ========================================
# STEP 4: Creare utilizator Admin
# ========================================

Write-Host "Pasul 4: Creare utilizator Admin..." -ForegroundColor $ColorInfo

$createUserQuery = @"
DECLARE @UtilizatorID UNIQUEIDENTIFIER = NEWID();

INSERT INTO Utilizatori (
    UtilizatorID,
    PersonalMedicalID,
    Username,
    Email,
    PasswordHash,
    Salt,
    Rol,
    EsteActiv,
    DataCreare,
    CreatDe,
DataCrearii,
    DataUltimeiModificari
)
VALUES (
 @UtilizatorID,
    '$personalMedicalID',
    '$AdminUsername',
 '$AdminEmail',
    '$AdminPasswordHash',
    'bcrypt_autogenerated',
    '$AdminRole',
    1,
    GETDATE(),
    'System',
    GETDATE(),
 GETDATE()
);

SELECT 
    @UtilizatorID AS UtilizatorID,
    '$AdminUsername' AS Username,
    '$AdminEmail' AS Email,
    '$AdminRole' AS Rol;
"@

try {
    $userResult = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $createUserQuery
    
    Write-Host ""
    Write-Host "? Utilizator Admin creat cu succes!" -ForegroundColor $ColorSuccess
    Write-Host ""
    Write-Host "????????????????????????????????????????" -ForegroundColor $ColorHeader
    Write-Host "  CREDENTIALE ADMIN" -ForegroundColor $ColorHeader
    Write-Host "????????????????????????????????????????" -ForegroundColor $ColorHeader
    Write-Host ""
    Write-Host "  UtilizatorID: $($userResult.Rows[0].UtilizatorID)" -ForegroundColor Gray
    Write-Host "  Username:     $($userResult.Rows[0].Username)" -ForegroundColor $ColorSuccess
    Write-Host "  Password:$AdminPassword" -ForegroundColor $ColorSuccess
    Write-Host "  Email:        $($userResult.Rows[0].Email)" -ForegroundColor Gray
    Write-Host "  Rol:          $($userResult.Rows[0].Rol)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "????????????????????????????????????????" -ForegroundColor $ColorHeader
  Write-Host ""
    Write-Host "??  IMPORTANT: Schimb? parola dup? prima autentificare!" -ForegroundColor $ColorWarning
    Write-Host ""
}
catch {
    Write-Host ""
    Write-Host "? EROARE la crearea utilizatorului!" -ForegroundColor $ColorError
    Write-Host "   $($_.Exception.Message)" -ForegroundColor $ColorError
    exit 1
}

# ========================================
# STEP 5: Verificare
# ========================================

Write-Host "Pasul 5: Verificare utilizator creat..." -ForegroundColor $ColorInfo

$verifyQuery = @"
SELECT 
 u.UtilizatorID,
    u.Username,
    u.Email,
    u.Rol,
    u.EsteActiv,
    u.DataCreare,
    pm.Nume + ' ' + pm.Prenume AS NumeCompletPersonalMedical,
    pm.Specializare
FROM Utilizatori u
INNER JOIN PersonalMedical pm ON u.PersonalMedicalID = pm.PersonalID
WHERE u.Username = '$AdminUsername'
"@

$verifyResult = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $verifyQuery

if ($verifyResult.Rows.Count -gt 0) {
    Write-Host "? Verificare OK - Utilizatorul este în database" -ForegroundColor $ColorSuccess
    Write-Host ""
    Write-Host "Detalii:" -ForegroundColor $ColorInfo
    Write-Host "  Username: $($verifyResult.Rows[0].Username)" -ForegroundColor Gray
    Write-Host "  Email: $($verifyResult.Rows[0].Email)" -ForegroundColor Gray
    Write-Host "  Rol: $($verifyResult.Rows[0].Rol)" -ForegroundColor Gray
    Write-Host "  Activ: $($verifyResult.Rows[0].EsteActiv)" -ForegroundColor Gray
    Write-Host "  PersonalMedical: $($verifyResult.Rows[0].NumeCompletPersonalMedical)" -ForegroundColor Gray
}
else {
    Write-Host "? EROARE: Utilizatorul nu a fost g?sit dup? creare!" -ForegroundColor $ColorError
    exit 1
}

Write-Host ""
Write-Header "ADMIN SUPERUSER CREAT CU SUCCES!"
Write-Host ""
Write-Host "Po?i acum s? te autentifici cu:" -ForegroundColor $ColorInfo
Write-Host "  Username: Admin" -ForegroundColor $ColorSuccess
Write-Host "  Password: admin123!@#" -ForegroundColor $ColorSuccess
Write-Host ""
