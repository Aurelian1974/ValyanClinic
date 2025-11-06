# ========================================
# PowerShell Script: Deploy Utilizatori Table
# Database: ValyanMed
# Descriere: Script pentru deployment complet al tabelei Utilizatori
# Autor: System
# Data: 2025-01-24
# ========================================

<#
.SYNOPSIS
    Deploy tabela Utilizatori si stored procedures

.DESCRIPTION
    Acest script creeaza:
    - Tabela Utilizatori (asociata cu PersonalMedical)
    - 12 Stored Procedures pentru gestionarea utilizatorilor
    - Indecsi pentru performanta
    - Date de test (optional)

.EXAMPLE
    .\Deploy-Utilizatori.ps1
    
.EXAMPLE
    .\Deploy-Utilizatori.ps1 -SkipTestData
    
.EXAMPLE
  .\Deploy-Utilizatori.ps1 -ServerName "LOCALHOST\SQLEXPRESS"
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [string]$ServerName = "DESKTOP-3Q8HI82\ERP",  # Updated to match appsettings.json
    
    [Parameter(Mandatory=$false)]
    [string]$DatabaseName = "ValyanMed",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipTestData,

    [Parameter(Mandatory=$false)]
    [switch]$ForceRecreate,
    
    [Parameter(Mandatory=$false)]
    [switch]$CleanTable
)

# ========================================
# CONFIGURARE
# ========================================

$ErrorActionPreference = "Stop"
$ScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootPath = Split-Path -Parent (Split-Path -Parent $ScriptPath)

# Paths
$TableScriptPath = Join-Path $RootPath "Database\TableStructure\Utilizatori_Complete.sql"
$SPScriptPath = Join-Path $RootPath "Database\StoredProcedures\sp_Utilizatori.sql"

# Colors
$ColorSuccess = "Green"
$ColorWarning = "Yellow"
$ColorError = "Red"
$ColorInfo = "Cyan"
$ColorHeader = "Magenta"

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

function Write-Success {
    param([string]$Message)
    Write-Host "? $Message" -ForegroundColor $ColorSuccess
}

function Write-Warning {
    param([string]$Message)
    Write-Host "?? $Message" -ForegroundColor $ColorWarning
}

function Write-ErrorMessage {
    param([string]$Message)
    Write-Host "? $Message" -ForegroundColor $ColorError
}

function Write-InfoMessage {
    param([string]$Message)
 Write-Host "?? $Message" -ForegroundColor $ColorInfo
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

function Invoke-SqlScript {
    param(
        [string]$Server,
        [string]$Database,
        [string]$ScriptPath
    )
    
 if (-not (Test-Path $ScriptPath)) {
        throw "Script-ul nu a fost gasit: $ScriptPath"
    }
    
$connectionString = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;"
    $scriptContent = Get-Content $ScriptPath -Raw
  
    # Split script by GO statements (batch separator)
    # Match GO that is on its own line (with optional whitespace)
    $batches = [regex]::Split($scriptContent, '^\s*GO\s*$', [System.Text.RegularExpressions.RegexOptions]::Multiline -bor [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
  
  $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    try {
        $batchNumber = 0
        foreach ($batch in $batches) {
       $batch = $batch.Trim()
      $batchNumber++
 
            # Skip empty batches
    if ([string]::IsNullOrWhiteSpace($batch)) {
       continue
     }
            
# Skip comment-only batches
    if ($batch -match '^\s*--.*$' -and $batch -notmatch '\n[^\-]') {
    continue
      }
     
            try {
                $command = $connection.CreateCommand()
    $command.CommandText = $batch
                $command.CommandTimeout = 300 # 5 minutes
      $result = $command.ExecuteNonQuery()
                
     # Optional: Show progress for large scripts
if ($VerbosePreference -eq 'Continue') {
          Write-Verbose "Executed batch $batchNumber successfully ($result rows affected)"
   }
            }
            catch {
         Write-Host "Error executing batch $batchNumber : $($_.Exception.Message)" -ForegroundColor Red
Write-Host "Batch content (first 300 chars):" -ForegroundColor Yellow
      Write-Host $batch.Substring(0, [Math]::Min(300, $batch.Length)) -ForegroundColor Gray
           throw
            }
        }
    }
    finally {
  $connection.Close()
    }
}

function Get-TableInfo {
    param(
  [string]$Server,
        [string]$Database,
        [string]$TableName
    )
    
    $connectionString = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;"
    $query = @"
SELECT 
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '$TableName') AS ColumnCount,
    (SELECT COUNT(*) FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('$TableName')) AS FKCount,
    (SELECT COUNT(*) FROM sys.indexes WHERE object_id = OBJECT_ID('$TableName') AND name IS NOT NULL) AS IndexCount,
    (SELECT COUNT(*) FROM sys.check_constraints WHERE parent_object_id = OBJECT_ID('$TableName')) AS ConstraintCount
"@
    
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $command = $connection.CreateCommand()
    $command.CommandText = $query
    
    $connection.Open()
    try {
        $reader = $command.ExecuteReader()
   if ($reader.Read()) {
 return @{
       ColumnCount = $reader["ColumnCount"]
                FKCount = $reader["FKCount"]
       IndexCount = $reader["IndexCount"]
  ConstraintCount = $reader["ConstraintCount"]
            }
        }
 }
    finally {
        $connection.Close()
    }
    
    return $null
}

function Get-SPCount {
    param(
        [string]$Server,
        [string]$Database
    )
    
    $connectionString = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;"
  $query = "SELECT COUNT(*) AS SPCount FROM sys.objects WHERE type = 'P' AND name LIKE 'sp_Utilizatori_%'"
    
  $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $command = $connection.CreateCommand()
    $command.CommandText = $query
    
    $connection.Open()
    try {
        return [int]$command.ExecuteScalar()
    }
    finally {
   $connection.Close()
    }
}

function Add-TestData {
    param(
    [string]$Server,
        [string]$Database
    )
    
    $connectionString = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;"
    
    # Create test users for active PersonalMedical
    $query = @"
-- Get 3 active PersonalMedical without users
DECLARE @PersonalMedical TABLE (Id UNIQUEIDENTIFIER, Nume NVARCHAR(200), Email NVARCHAR(100), Specializare NVARCHAR(100));

INSERT INTO @PersonalMedical 
SELECT TOP 3 
    pm.PersonalID, 
    pm.Nume + ' ' + pm.Prenume AS NumeComplet,
    pm.Email,
    pm.Specializare
FROM PersonalMedical pm
LEFT JOIN Utilizatori u ON pm.PersonalID = u.PersonalMedicalID
WHERE pm.EsteActiv = 1 
  AND u.UtilizatorID IS NULL
  AND pm.Email IS NOT NULL
ORDER BY pm.Nume;

-- Create test users
DECLARE @PM1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM @PersonalMedical ORDER BY Nume);
DECLARE @PM2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM @PersonalMedical WHERE Id <> @PM1 ORDER BY Nume);
DECLARE @PM3 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM @PersonalMedical WHERE Id NOT IN (@PM1, @PM2) ORDER BY Nume);

-- User 1: Administrator (if we have PM1)
IF @PM1 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Utilizatori WHERE PersonalMedicalID = @PM1)
BEGIN
    DECLARE @Username1 NVARCHAR(100) = (SELECT LEFT(Nume, 50) FROM @PersonalMedical WHERE Id = @PM1);
    DECLARE @Email1 NVARCHAR(100) = (SELECT Email FROM @PersonalMedical WHERE Id = @PM1);
    
    EXEC sp_Utilizatori_Create 
        @PersonalMedicalID = @PM1,
        @Username = @Username1,
 @Email = @Email1,
        @PasswordHash = 'HASH_PLACEHOLDER_CHANGE_ME',
   @Salt = 'SALT_PLACEHOLDER',
     @Rol = 'Administrator',
   @EsteActiv = 1,
     @CreatDe = 'System';
END

-- User 2: Doctor (if we have PM2)
IF @PM2 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Utilizatori WHERE PersonalMedicalID = @PM2)
BEGIN
    DECLARE @Username2 NVARCHAR(100) = (SELECT LEFT(Nume, 50) FROM @PersonalMedical WHERE Id = @PM2);
    DECLARE @Email2 NVARCHAR(100) = (SELECT Email FROM @PersonalMedical WHERE Id = @PM2);
    
 EXEC sp_Utilizatori_Create 
        @PersonalMedicalID = @PM2,
      @Username = @Username2,
      @Email = @Email2,
        @PasswordHash = 'HASH_PLACEHOLDER_CHANGE_ME',
        @Salt = 'SALT_PLACEHOLDER',
     @Rol = 'Doctor',
 @EsteActiv = 1,
        @CreatDe = 'System';
END

-- User 3: Asistent (if we have PM3)
IF @PM3 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Utilizatori WHERE PersonalMedicalID = @PM3)
BEGIN
    DECLARE @Username3 NVARCHAR(100) = (SELECT LEFT(Nume, 50) FROM @PersonalMedical WHERE Id = @PM3);
  DECLARE @Email3 NVARCHAR(100) = (SELECT Email FROM @PersonalMedical WHERE Id = @PM3);
    
    EXEC sp_Utilizatori_Create 
        @PersonalMedicalID = @PM3,
        @Username = @Username3,
        @Email = @Email3,
        @PasswordHash = 'HASH_PLACEHOLDER_CHANGE_ME',
        @Salt = 'SALT_PLACEHOLDER',
  @Rol = 'Asistent',
        @EsteActiv = 1,
      @CreatDe = 'System';
END

SELECT 'Date de test adaugate cu succes!' AS Message;
"@
    
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $command = $connection.CreateCommand()
    $command.CommandText = $query
 
    $connection.Open()
    try {
      $command.ExecuteNonQuery() | Out-Null
    }
    finally {
        $connection.Close()
    }
}

# ========================================
# MAIN SCRIPT
# ========================================

Write-Header "DEPLOY UTILIZATORI TABLE"

Write-InfoMessage "Server: $ServerName"
Write-InfoMessage "Database: $DatabaseName"
Write-Host ""

# ========================================
# STEP 1: Verificare conexiune
# ========================================

Write-Host "Pasul 1: Verificare conexiune la database..." -ForegroundColor $ColorInfo

if (-not (Test-SqlConnection -Server $ServerName -Database $DatabaseName)) {
    Write-ErrorMessage "Nu s-a putut conecta la database!"
  Write-InfoMessage "Verifica ca SQL Server este pornit si connection string-ul este corect."
    exit 1
}

Write-Success "Conexiune stabila la database"
Write-Host ""

# ========================================
# STEP 2: Verificare scripturi
# ========================================

Write-Host "Pasul 2: Verificare existenta scripturi..." -ForegroundColor $ColorInfo

if (-not (Test-Path $TableScriptPath)) {
    Write-ErrorMessage "Script tabela nu a fost gasit: $TableScriptPath"
    exit 1
}
Write-Success "Script tabela gasit"

if (-not (Test-Path $SPScriptPath)) {
    Write-ErrorMessage "Script stored procedures nu a fost gasit: $SPScriptPath"
    exit 1
}
Write-Success "Script stored procedures gasit"
Write-Host ""

# ========================================
# STEP 3: Deploy tabela
# ========================================

Write-Host "Pasul 3: Creare tabela Utilizatori..." -ForegroundColor $ColorInfo

try {
    Invoke-SqlScript -Server $ServerName -Database $DatabaseName -ScriptPath $TableScriptPath
    Write-Success "Tabela creata cu succes"
}
catch {
    Write-ErrorMessage "Eroare la crearea tabelei: $_"
    exit 1
}
Write-Host ""

# ========================================
# STEP 4: Deploy stored procedures
# ========================================

Write-Host "Pasul 4: Creare stored procedures..." -ForegroundColor $ColorInfo

try {
    Invoke-SqlScript -Server $ServerName -Database $DatabaseName -ScriptPath $SPScriptPath
    Write-Success "Stored procedures create cu succes"
}
catch {
    Write-ErrorMessage "Eroare la crearea stored procedures: $_"
 exit 1
}
Write-Host ""

# ========================================
# STEP 5: Verificare deployment
# ========================================

Write-Host "Pasul 5: Verificare deployment..." -ForegroundColor $ColorInfo

$tableInfo = Get-TableInfo -Server $ServerName -Database $DatabaseName -TableName "Utilizatori"

if ($null -eq $tableInfo) {
    Write-ErrorMessage "Tabela nu a fost gasita dupa deployment!"
    exit 1
}

Write-Success "Tabela: Utilizatori exista"
Write-InfoMessage "  Coloane: $($tableInfo.ColumnCount) (asteptat: 18)"
Write-InfoMessage "  Foreign Keys: $($tableInfo.FKCount) (asteptat: 1 - PersonalMedical)"
Write-InfoMessage "  Indexes: $($tableInfo.IndexCount) (asteptat: 7)"
Write-InfoMessage "  Constraints: $($tableInfo.ConstraintCount) (asteptat: 4)"

$spCount = Get-SPCount -Server $ServerName -Database $DatabaseName
Write-Success "Stored Procedures: $spCount (asteptat: 12)"
Write-Host ""

# ========================================
# STEP 6: Date de test (optional)
# ========================================

if (-not $SkipTestData) {
    Write-Host "Pasul 6: Adaugare date de test..." -ForegroundColor $ColorInfo
    
    try {
   Add-TestData -Server $ServerName -Database $DatabaseName
     Write-Success "Date de test adaugate cu succes"
     Write-Warning "IMPORTANT: Parolele de test trebuie schimbate!"
   Write-InfoMessage "Hash-urile sunt placeholder-uri si trebuie inlocuite cu hash-uri reale"
    }
    catch {
    Write-Warning "Nu s-au putut adauga date de test: $_"
     Write-InfoMessage "Poti adauga manual utilizatori folosind sp_Utilizatori_Create"
    }
    Write-Host ""
}

# ========================================
# STEP 7: Golire tabela (optional)
# ========================================

if ($CleanTable) {
    Write-Host "Pasul 7: Golire tabela Utilizatori..." -ForegroundColor $ColorInfo
    
    try {
        # Check if table has data
        $connectionString = "Server=$ServerName;Database=$DatabaseName;Integrated Security=True;TrustServerCertificate=True;"
        $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
        $command = $connection.CreateCommand()
        $command.CommandText = "SELECT COUNT(*) FROM Utilizatori"
      
        $connection.Open()
        try {
            $count = [int]$command.ExecuteScalar()
  
        if ($count -gt 0) {
      Write-Warning "Tabela contine $count utilizatori"
      $confirmation = Read-Host "Confirmi golirea tabelei? (DA/NU)"
 
        if ($confirmation -eq "DA") {
          $command.CommandText = "DELETE FROM Utilizatori"
         $command.ExecuteNonQuery() | Out-Null
                 Write-Success "Tabela Utilizatori a fost golita cu succes"
     }
     else {
     Write-InfoMessage "Golirea tabelei a fost anulata"
     }
            }
 else {
   Write-InfoMessage "Tabela Utilizatori este deja goala"
     }
        }
        finally {
            $connection.Close()
  }
    }
    catch {
        Write-Warning "Nu s-a putut goli tabela: $_"
 }
  Write-Host ""
}

# ========================================
# FINALIZARE
# ========================================

Write-Header "DEPLOYMENT COMPLET!"

Write-Success "Tabela Utilizatori este gata de utilizare!"
Write-Host ""
Write-InfoMessage "Stored Procedures disponibile:"
Write-Host "  1. sp_Utilizatori_GetAll" -ForegroundColor Gray
Write-Host "  2. sp_Utilizatori_GetCount" -ForegroundColor Gray
Write-Host "  3. sp_Utilizatori_GetById" -ForegroundColor Gray
Write-Host "  4. sp_Utilizatori_GetByUsername" -ForegroundColor Gray
Write-Host "  5. sp_Utilizatori_GetByEmail" -ForegroundColor Gray
Write-Host "  6. sp_Utilizatori_Create" -ForegroundColor Gray
Write-Host "  7. sp_Utilizatori_Update" -ForegroundColor Gray
Write-Host "  8. sp_Utilizatori_ChangePassword" -ForegroundColor Gray
Write-Host "  9. sp_Utilizatori_UpdateUltimaAutentificare" -ForegroundColor Gray
Write-Host " 10. sp_Utilizatori_IncrementIncercariEsuate" -ForegroundColor Gray
Write-Host " 11. sp_Utilizatori_SetTokenResetareParola" -ForegroundColor Gray
Write-Host " 12. sp_Utilizatori_GetStatistics" -ForegroundColor Gray
Write-Host ""
Write-InfoMessage "Caracteristici importante:"
Write-Host "  ? Foreign Key: PersonalMedicalID -> PersonalMedical.PersonalID" -ForegroundColor Gray
Write-Host "  ? Un PersonalMedical = Un Utilizator (relatie 1:1)" -ForegroundColor Gray
Write-Host "  ? Roluri: Administrator, Doctor, Asistent, Receptioner, Manager, Utilizator" -ForegroundColor Gray
Write-Host "  ? Blocare automata dupa 5 incercari esuate" -ForegroundColor Gray
Write-Host "  ? Suport pentru resetare parola cu token" -ForegroundColor Gray
Write-Host "  ? Audit trail complet (CreatDe, ModificatDe, etc.)" -ForegroundColor Gray
Write-Host ""
Write-InfoMessage "Pasul urmator:"
Write-Host "  1. Testeaza stored procedures in SQL Server Management Studio" -ForegroundColor Gray
Write-Host "  2. Creeaza entitate Utilizator in ValyanClinic.Domain" -ForegroundColor Gray
Write-Host "  3. Creeaza Repository in ValyanClinic.Infrastructure" -ForegroundColor Gray
Write-Host "  4. Implementeaza autentificare si autorizare" -ForegroundColor Gray
Write-Host ""

Write-Success "Gata! ??"
Write-Host ""
