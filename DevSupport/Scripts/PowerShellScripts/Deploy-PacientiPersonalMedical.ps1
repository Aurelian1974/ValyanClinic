# ========================================
# PowerShell Script: Deploy Pacienti_PersonalMedical Junction Table
# Database: ValyanMed
# Descriere: Script pentru deployment complet al tabelei de legatura Many-to-Many
# Autor: System
# Data: 2025-01-23
# ========================================

<#
.SYNOPSIS
    Deploy tabela de legatura Pacienti_PersonalMedical si stored procedures

.DESCRIPTION
    Acest script creeaza:
    - Tabela Pacienti_PersonalMedical (junction table pentru Many-to-Many)
    - 8 Stored Procedures pentru gestionarea relatiilor
    - Indecsi pentru performanta
    - Trigger pentru audit
    - Date de test (optional)

.EXAMPLE
    .\Deploy-PacientiPersonalMedical.ps1
    
.EXAMPLE
    .\Deploy-PacientiPersonalMedical.ps1 -SkipTestData
    
.EXAMPLE
    .\Deploy-PacientiPersonalMedical.ps1 -ServerName "LOCALHOST\SQLEXPRESS"
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [string]$ServerName = "DESKTOP-3Q8HI82\ERP",
    
    [Parameter(Mandatory=$false)]
    [string]$DatabaseName = "ValyanMed",
    
    [Parameter(Mandatory=$false)]
  [switch]$SkipTestData,
    
    [Parameter(Mandatory=$false)]
    [switch]$ForceRecreate,
    
    [Parameter(Mandatory=$false)]
    [switch]$Verbose
)

# ========================================
# CONFIGURARE
# ========================================

$ErrorActionPreference = "Stop"
$ScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootPath = Split-Path -Parent (Split-Path -Parent $ScriptPath)

# Paths
$TableScriptPath = Join-Path $RootPath "Database\TableStructure\Pacienti_PersonalMedical_Complete.sql"
$SPScriptPath = Join-Path $RootPath "Database\StoredProcedures\sp_Pacienti_PersonalMedical.sql"

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
    Write-Host "? $Message" -ForegroundColor $ColorWarning
}

function Write-ErrorMessage {
    param([string]$Message)
    Write-Host "? $Message" -ForegroundColor $ColorError
}

function Write-InfoMessage {
    param([string]$Message)
    Write-Host "? $Message" -ForegroundColor $ColorInfo
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
 
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    try {
        $command = $connection.CreateCommand()
      $command.CommandText = $scriptContent
 $command.CommandTimeout = 300 # 5 minutes
        $command.ExecuteNonQuery() | Out-Null
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
    (SELECT COUNT(*) FROM sys.check_constraints WHERE parent_object_id = OBJECT_ID('$TableName')) AS ConstraintCount,
    (SELECT COUNT(*) FROM sys.triggers WHERE parent_id = OBJECT_ID('$TableName')) AS TriggerCount
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
           TriggerCount = $reader["TriggerCount"]
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
    $query = "SELECT COUNT(*) AS SPCount FROM sys.objects WHERE type = 'P' AND name LIKE 'sp_PacientiPersonalMedical_%'"
    
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
    
    # Get sample Pacienti and PersonalMedical IDs
    $query = @"
-- Get 3 pacienti
DECLARE @Pacienti TABLE (Id UNIQUEIDENTIFIER, Nume NVARCHAR(100));
INSERT INTO @Pacienti SELECT TOP 3 Id, Nume + ' ' + Prenume FROM Pacienti WHERE Activ = 1;

-- Get 3 doctori
DECLARE @Doctori TABLE (Id UNIQUEIDENTIFIER, Nume NVARCHAR(100));
INSERT INTO @Doctori SELECT TOP 3 PersonalID, Nume + ' ' + Prenume FROM PersonalMedical WHERE EsteActiv = 1;

-- Create relations
DECLARE @P1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM @Pacienti ORDER BY Nume);
DECLARE @P2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM @Pacienti WHERE Id <> @P1 ORDER BY Nume);
DECLARE @D1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM @Doctori ORDER BY Nume);
DECLARE @D2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM @Doctori WHERE Id <> @D1 ORDER BY Nume);

-- Pacient 1 -> Doctor 1 (Medic Primar)
IF NOT EXISTS (SELECT 1 FROM Pacienti_PersonalMedical WHERE PacientID = @P1 AND PersonalMedicalID = @D1)
    EXEC sp_PacientiPersonalMedical_AddRelatie @P1, @D1, 'MedicPrimar', 'Asociere automata test', 'Relatie de test', 'System';

-- Pacient 1 -> Doctor 2 (Specialist)
IF NOT EXISTS (SELECT 1 FROM Pacienti_PersonalMedical WHERE PacientID = @P1 AND PersonalMedicalID = @D2)
    EXEC sp_PacientiPersonalMedical_AddRelatie @P1, @D2, 'Specialist', 'Asociere automata test', 'Relatie de test', 'System';

-- Pacient 2 -> Doctor 1 (Medic Consultant)
IF NOT EXISTS (SELECT 1 FROM Pacienti_PersonalMedical WHERE PacientID = @P2 AND PersonalMedicalID = @D1)
    EXEC sp_PacientiPersonalMedical_AddRelatie @P2, @D1, 'MedicConsultant', 'Asociere automata test', 'Relatie de test', 'System';

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

Write-Header "DEPLOY PACIENTI_PERSONALMEDICAL JUNCTION TABLE"

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

Write-Host "Pasul 3: Creare tabela Pacienti_PersonalMedical..." -ForegroundColor $ColorInfo

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

$tableInfo = Get-TableInfo -Server $ServerName -Database $DatabaseName -TableName "Pacienti_PersonalMedical"

if ($null -eq $tableInfo) {
    Write-ErrorMessage "Tabela nu a fost gasita dupa deployment!"
    exit 1
}

Write-Success "Tabela: Pacienti_PersonalMedical exista"
Write-InfoMessage "  Coloane: $($tableInfo.ColumnCount) (asteptat: 13)"
Write-InfoMessage "  Foreign Keys: $($tableInfo.FKCount) (asteptat: 2)"
Write-InfoMessage "  Indexes: $($tableInfo.IndexCount) (asteptat: 6)"
Write-InfoMessage "  Constraints: $($tableInfo.ConstraintCount) (asteptat: 1)"
Write-InfoMessage "  Triggers: $($tableInfo.TriggerCount) (asteptat: 1)"

$spCount = Get-SPCount -Server $ServerName -Database $DatabaseName
Write-Success "Stored Procedures: $spCount (asteptat: 8)"
Write-Host ""

# ========================================
# STEP 6: Date de test (optional)
# ========================================

if (-not $SkipTestData) {
    Write-Host "Pasul 6: Adaugare date de test..." -ForegroundColor $ColorInfo
    
    try {
        Add-TestData -Server $ServerName -Database $DatabaseName
        Write-Success "Date de test adaugate cu succes"
    }
    catch {
        Write-Warning "Nu s-au putut adauga date de test: $_"
  Write-InfoMessage "Poti adauga manual date de test folosind stored procedures"
    }
    Write-Host ""
}

# ========================================
# FINALIZARE
# ========================================

Write-Header "DEPLOYMENT COMPLET!"

Write-Success "Tabela Pacienti_PersonalMedical este gata de utilizare!"
Write-Host ""
Write-InfoMessage "Stored Procedures disponibile:"
Write-Host "  1. sp_PacientiPersonalMedical_GetDoctoriByPacient" -ForegroundColor Gray
Write-Host "  2. sp_PacientiPersonalMedical_GetPacientiByDoctor" -ForegroundColor Gray
Write-Host "  3. sp_PacientiPersonalMedical_AddRelatie" -ForegroundColor Gray
Write-Host "  4. sp_PacientiPersonalMedical_RemoveRelatie" -ForegroundColor Gray
Write-Host "  5. sp_PacientiPersonalMedical_ReactiveazaRelatie" -ForegroundColor Gray
Write-Host "  6. sp_PacientiPersonalMedical_UpdateRelatie" -ForegroundColor Gray
Write-Host "  7. sp_PacientiPersonalMedical_GetStatistici" -ForegroundColor Gray
Write-Host "  8. sp_PacientiPersonalMedical_GetRelatieById" -ForegroundColor Gray
Write-Host ""
Write-InfoMessage "Pasul urmator:"
Write-Host "  1. Testeaza stored procedures in SQL Server Management Studio" -ForegroundColor Gray
Write-Host "  2. Actualizeaza entitati C# in ValyanClinic.Domain" -ForegroundColor Gray
Write-Host "  3. Creeaza DTOs si Queries in ValyanClinic.Application" -ForegroundColor Gray
Write-Host "  4. Actualizeaza UI in Blazor components" -ForegroundColor Gray
Write-Host ""

Write-Success "Gata! ??"
Write-Host ""
