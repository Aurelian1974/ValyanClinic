# ========================================
# PowerShell Script: Clean Utilizatori Table
# Database: ValyanMed
# Descriere: Goleste tabela Utilizatori (sterge toate inregistrarile)
# Autor: System
# Data: 2025-01-24
# ========================================

<#
.SYNOPSIS
    Goleste tabela Utilizatori

.DESCRIPTION
    Acest script sterge toate inregistrarile din tabela Utilizatori.
    ATENTIE: Operatiunea este ireversibila!

.EXAMPLE
    .\Clean-Utilizatori.ps1
    
.EXAMPLE
    .\Clean-Utilizatori.ps1 -ServerName "LOCALHOST\SQLEXPRESS" -Confirm:$false
#>

[CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact='High')]
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

function Get-UtilizatoriCount {
    param(
        [string]$Server,
   [string]$Database
    )
    
    $connectionString = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;"
    $query = "SELECT COUNT(*) AS UserCount FROM Utilizatori"
 
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

function Clear-UtilizatoriTable {
 param(
        [string]$Server,
        [string]$Database
    )
    
    $connectionString = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;"
    $query = "DELETE FROM Utilizatori"
  
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
  $command = $connection.CreateCommand()
    $command.CommandText = $query
  
    $connection.Open()
    try {
        $rowsAffected = $command.ExecuteNonQuery()
        return $rowsAffected
    }
    finally {
        $connection.Close()
    }
}

# ========================================
# MAIN SCRIPT
# ========================================

Write-Header "CLEAN UTILIZATORI TABLE"

Write-Host "Server: $ServerName" -ForegroundColor $ColorInfo
Write-Host "Database: $DatabaseName" -ForegroundColor $ColorInfo
Write-Host ""

# ========================================
# STEP 1: Verificare conexiune
# ========================================

Write-Host "Verificare conexiune la database..." -ForegroundColor $ColorInfo

if (-not (Test-SqlConnection -Server $ServerName -Database $DatabaseName)) {
    Write-Host "? Nu s-a putut conecta la database!" -ForegroundColor $ColorError
    Write-Host "Verifica ca SQL Server este pornit si connection string-ul este corect." -ForegroundColor $ColorWarning
    exit 1
}

Write-Host "? Conexiune stabila" -ForegroundColor $ColorSuccess
Write-Host ""

# ========================================
# STEP 2: Verificare date existente
# ========================================

Write-Host "Verificare date existente..." -ForegroundColor $ColorInfo

try {
    $userCount = Get-UtilizatoriCount -Server $ServerName -Database $DatabaseName
    
    if ($userCount -eq 0) {
    Write-Host "?? Tabela Utilizatori este deja goala" -ForegroundColor $ColorInfo
        exit 0
    }
    
    Write-Host "?? Tabela contine $userCount utilizatori" -ForegroundColor $ColorWarning
}
catch {
    Write-Host "? Eroare la verificarea datelor: $($_.Exception.Message)" -ForegroundColor $ColorError
    exit 1
}

Write-Host ""

# ========================================
# STEP 3: Confirmare
# ========================================

if (-not $Force) {
    Write-Host "????????????????????????????????????????" -ForegroundColor $ColorError
    Write-Host "  ??  ATENTIE! OPERATIUNE PERICULOASA! ??" -ForegroundColor $ColorError
    Write-Host "????????????????????????????????????????" -ForegroundColor $ColorError
    Write-Host ""
Write-Host "Urmeaza sa STERGI TOTI UTILIZATORII din tabela Utilizatori!" -ForegroundColor $ColorError
    Write-Host "Operatiunea este IREVERSIBILA!" -ForegroundColor $ColorError
  Write-Host ""
    Write-Host "Numar utilizatori care vor fi stersi: $userCount" -ForegroundColor $ColorWarning
    Write-Host ""
    
    $confirmation = Read-Host "Scrie 'DELETE ALL' pentru a confirma stergerea"
    
    if ($confirmation -ne "DELETE ALL") {
        Write-Host ""
        Write-Host "?? Operatiune anulata" -ForegroundColor $ColorInfo
        exit 0
    }
}

# ========================================
# STEP 4: Stergere date
# ========================================

Write-Host ""
Write-Host "Stergere utilizatori..." -ForegroundColor $ColorInfo

try {
    $rowsDeleted = Clear-UtilizatoriTable -Server $ServerName -Database $DatabaseName
    
    Write-Host ""
    Write-Host "? Succes!" -ForegroundColor $ColorSuccess
    Write-Host " $rowsDeleted utilizatori stersi" -ForegroundColor $ColorSuccess
    Write-Host ""
    
    # Verify
    $remainingCount = Get-UtilizatoriCount -Server $ServerName -Database $DatabaseName
    
    if ($remainingCount -eq 0) {
      Write-Host "? Verificare: Tabela este goala" -ForegroundColor $ColorSuccess
    }
    else {
  Write-Host "?? Verificare: Mai sunt $remainingCount inregistrari" -ForegroundColor $ColorWarning
    }
}
catch {
  Write-Host ""
    Write-Host "? EROARE la stergerea datelor!" -ForegroundColor $ColorError
    Write-Host "   $($_.Exception.Message)" -ForegroundColor $ColorError
    exit 1
}

Write-Host ""
Write-Header "CLEANUP COMPLET!"
Write-Host ""
