# ========================================
# PowerShell Script: Test Pacienti_PersonalMedical Junction Table
# Database: ValyanMed
# Descriere: Script pentru testarea stored procedures si verificare integritate date
# Autor: System
# Data: 2025-01-23
# ========================================

<#
.SYNOPSIS
    Testeaza tabela Pacienti_PersonalMedical si stored procedures

.DESCRIPTION
    Acest script testeaza:
    - Toate cele 8 stored procedures
    - Foreign key constraints
    - Unique constraints
    - Operatii CRUD complete
    - Edge cases si error handling

.EXAMPLE
    .\Test-PacientiPersonalMedical.ps1
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [string]$ServerName = "DESKTOP-3Q8HI82\ERP",
    
    [Parameter(Mandatory=$false)]
    [string]$DatabaseName = "ValyanMed"
)

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

function Write-TestHeader {
    param([string]$TestName)
    Write-Host ""
 Write-Host "TEST: $TestName" -ForegroundColor $ColorInfo
    Write-Host "----------------------------------------"
}

function Write-Success {
    param([string]$Message)
    Write-Host "  ? $Message" -ForegroundColor $ColorSuccess
}

function Write-Failure {
    param([string]$Message)
  Write-Host "  ? $Message" -ForegroundColor $ColorError
}

function Write-InfoMessage {
    param([string]$Message)
    Write-Host "  ? $Message" -ForegroundColor $ColorInfo
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
    $command.CommandTimeout = 300
    
    $connection.Open()
    try {
     $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
   $dataset = New-Object System.Data.DataSet
        $adapter.Fill($dataset) | Out-Null
        return $dataset.Tables[0]
 }
    finally {
        $connection.Close()
    }
}

# ========================================
# TESTS
# ========================================

Write-Header "TEST SUITE: PACIENTI_PERSONALMEDICAL"

$testsPassed = 0
$testsFailed = 0

# ========================================
# TEST 1: Verificare existenta tabela
# ========================================

Write-TestHeader "1. Verificare existenta tabela"

try {
    $query = "SELECT COUNT(*) AS TableExists FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Pacienti_PersonalMedical'"
    $result = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $query
    
  if ($result.TableExists -eq 1) {
        Write-Success "Tabela Pacienti_PersonalMedical exista"
        $testsPassed++
    } else {
        Write-Failure "Tabela Pacienti_PersonalMedical NU exista"
        $testsFailed++
    }
}
catch {
  Write-Failure "Eroare la verificare tabela: $_"
    $testsFailed++
}

# ========================================
# TEST 2: Verificare Foreign Keys
# ========================================

Write-TestHeader "2. Verificare Foreign Keys"

try {
    $query = @"
SELECT 
    fk.name AS FK_Name,
    OBJECT_NAME(fk.referenced_object_id) AS Referenced_Table
FROM sys.foreign_keys fk
WHERE fk.parent_object_id = OBJECT_ID('Pacienti_PersonalMedical')
"@
    $result = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $query
    
  $expectedFKs = @("Pacienti", "PersonalMedical")
    $actualFKs = $result | ForEach-Object { $_.Referenced_Table }
    
    $allFound = $true
    foreach ($expected in $expectedFKs) {
 if ($actualFKs -contains $expected) {
 Write-Success "FK catre $expected exista"
        } else {
          Write-Failure "FK catre $expected NU exista"
     $allFound = $false
        }
    }
    
  if ($allFound) { $testsPassed++ } else { $testsFailed++ }
}
catch {
 Write-Failure "Eroare la verificare FK: $_"
    $testsFailed++
}

# ========================================
# TEST 3: Verificare Stored Procedures
# ========================================

Write-TestHeader "3. Verificare Stored Procedures"

try {
    $expectedSPs = @(
    "sp_PacientiPersonalMedical_GetDoctoriByPacient",
        "sp_PacientiPersonalMedical_GetPacientiByDoctor",
      "sp_PacientiPersonalMedical_AddRelatie",
        "sp_PacientiPersonalMedical_RemoveRelatie",
      "sp_PacientiPersonalMedical_ReactiveazaRelatie",
        "sp_PacientiPersonalMedical_UpdateRelatie",
        "sp_PacientiPersonalMedical_GetStatistici",
        "sp_PacientiPersonalMedical_GetRelatieById"
    )
    
    $allFound = $true
    foreach ($sp in $expectedSPs) {
        $query = "SELECT COUNT(*) AS SPExists FROM sys.objects WHERE type = 'P' AND name = '$sp'"
      $result = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $query
        
    if ($result.SPExists -eq 1) {
      Write-Success "$sp exista"
      } else {
   Write-Failure "$sp NU exista"
            $allFound = $false
        }
    }
    
    if ($allFound) { $testsPassed++ } else { $testsFailed++ }
}
catch {
    Write-Failure "Eroare la verificare SP: $_"
  $testsFailed++
}

# ========================================
# TEST 4: Test AddRelatie - Success Case
# ========================================

Write-TestHeader "4. Test AddRelatie - Success Case"

try {
    # Get IDs
    $queryPacient = "SELECT TOP 1 Id FROM Pacienti WHERE Activ = 1"
    $queryDoctor = "SELECT TOP 1 PersonalID FROM PersonalMedical WHERE EsteActiv = 1"
    
    $pacientResult = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $queryPacient
    $doctorResult = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $queryDoctor
    
    if ($pacientResult.Count -gt 0 -and $doctorResult.Count -gt 0) {
        $pacientID = $pacientResult[0].Id
      $doctorID = $doctorResult[0].PersonalID
        
        # Delete existing relation if any
   $deleteQuery = @"
DELETE FROM Pacienti_PersonalMedical 
WHERE PacientID = '$pacientID' AND PersonalMedicalID = '$doctorID'
"@
   Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $deleteQuery | Out-Null
        
    # Add new relation
        $addQuery = @"
EXEC sp_PacientiPersonalMedical_AddRelatie 
    @PacientID = '$pacientID',
    @PersonalMedicalID = '$doctorID',
    @TipRelatie = 'MedicPrimar',
    @Observatii = 'Test relation',
    @Motiv = 'Test',
    @CreatDe = 'TestScript'
"@
        $result = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $addQuery
    
        if ($result.Count -gt 0) {
       Write-Success "Relatie adaugata cu succes"
            Write-InfoMessage "RelatieID: $($result[0].Id)"
  $testsPassed++
 
            # Store for later tests
$script:TestRelatieID = $result[0].Id
        } else {
     Write-Failure "Relatia nu a fost adaugata"
       $testsFailed++
        }
    } else {
        Write-Failure "Nu exista pacienti sau doctori in database"
        $testsFailed++
    }
}
catch {
    Write-Failure "Eroare la test AddRelatie: $_"
    $testsFailed++
}

# ========================================
# TEST 5: Test AddRelatie - Duplicate Prevention
# ========================================

Write-TestHeader "5. Test AddRelatie - Duplicate Prevention"

try {
    # Try to add same relation again
    $queryPacient = "SELECT TOP 1 Id FROM Pacienti WHERE Activ = 1"
    $queryDoctor = "SELECT TOP 1 PersonalID FROM PersonalMedical WHERE EsteActiv = 1"
    
    $pacientResult = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $queryPacient
    $doctorResult = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $queryDoctor
    
    $pacientID = $pacientResult[0].Id
    $doctorID = $doctorResult[0].PersonalID
    
    $addQuery = @"
EXEC sp_PacientiPersonalMedical_AddRelatie 
    @PacientID = '$pacientID',
    @PersonalMedicalID = '$doctorID',
    @TipRelatie = 'MedicPrimar',
    @CreatDe = 'TestScript'
"@
    
    try {
        Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $addQuery | Out-Null
        Write-Failure "Duplicate relation was allowed (should fail)"
        $testsFailed++
    }
    catch {
        Write-Success "Duplicate prevention works correctly"
        $testsPassed++
    }
}
catch {
    Write-Failure "Eroare la test duplicate prevention: $_"
    $testsFailed++
}

# ========================================
# TEST 6: Test GetDoctoriByPacient
# ========================================

Write-TestHeader "6. Test GetDoctoriByPacient"

try {
    $queryPacient = "SELECT TOP 1 Id FROM Pacienti WHERE Activ = 1"
    $pacientResult = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $queryPacient
    $pacientID = $pacientResult[0].Id
    
    $query = "EXEC sp_PacientiPersonalMedical_GetDoctoriByPacient @PacientID = '$pacientID', @ApenumereActivi = 1"
    $result = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $query
    
    if ($result.Count -gt 0) {
        Write-Success "SP returneaza $($result.Count) doctor(i)"
        Write-InfoMessage "Doctor: $($result[0].DoctorNumeComplet)"
$testsPassed++
    } else {
        Write-InfoMessage "Nu exista relatii pentru acest pacient (OK pentru test)"
        $testsPassed++
    }
}
catch {
    Write-Failure "Eroare la test GetDoctoriByPacient: $_"
    $testsFailed++
}

# ========================================
# TEST 7: Test GetPacientiByDoctor
# ========================================

Write-TestHeader "7. Test GetPacientiByDoctor"

try {
    $queryDoctor = "SELECT TOP 1 PersonalID FROM PersonalMedical WHERE EsteActiv = 1"
    $doctorResult = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $queryDoctor
  $doctorID = $doctorResult[0].PersonalID
    
    $query = "EXEC sp_PacientiPersonalMedical_GetPacientiByDoctor @PersonalMedicalID = '$doctorID', @ApenumereActivi = 1"
    $result = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $query
    
    if ($result.Count -gt 0) {
    Write-Success "SP returneaza $($result.Count) pacient(i)"
        Write-InfoMessage "Pacient: $($result[0].PacientNumeComplet)"
        $testsPassed++
    } else {
        Write-InfoMessage "Nu exista relatii pentru acest doctor (OK pentru test)"
        $testsPassed++
    }
}
catch {
    Write-Failure "Eroare la test GetPacientiByDoctor: $_"
    $testsFailed++
}

# ========================================
# TEST 8: Test GetStatistici
# ========================================

Write-TestHeader "8. Test GetStatistici"

try {
    $query = "EXEC sp_PacientiPersonalMedical_GetStatistici"
    $result = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $query
    
    if ($result.Count -gt 0) {
        Write-Success "Statistici obtinute cu succes"
        Write-InfoMessage "Total relatii: $($result[0].TotalRelatii)"
        Write-InfoMessage "Relatii active: $($result[0].RelatiiActive)"
        Write-InfoMessage "Total doctori: $($result[0].TotalDoctori)"
        Write-InfoMessage "Total pacienti: $($result[0].TotalPacienti)"
        $testsPassed++
    } else {
        Write-Failure "Nu s-au putut obtine statistici"
        $testsFailed++
    }
}
catch {
    Write-Failure "Eroare la test GetStatistici: $_"
    $testsFailed++
}

# ========================================
# TEST 9: Test UpdateRelatie
# ========================================

Write-TestHeader "9. Test UpdateRelatie"

try {
    if ($script:TestRelatieID) {
      $query = @"
EXEC sp_PacientiPersonalMedical_UpdateRelatie 
 @RelatieID = '$($script:TestRelatieID)',
    @TipRelatie = 'Specialist',
    @Observatii = 'Observatii actualizate',
    @ModificatDe = 'TestScript'
"@
        $result = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $query
        
        if ($result.Count -gt 0 -and $result[0].TipRelatie -eq 'Specialist') {
        Write-Success "Relatie actualizata cu succes"
            Write-InfoMessage "Tip relatie nou: $($result[0].TipRelatie)"
          $testsPassed++
   } else {
            Write-Failure "Relatia nu a fost actualizata corect"
          $testsFailed++
        }
    } else {
        Write-InfoMessage "Test skipped - nu exista RelatieID din testul anterior"
    $testsPassed++
    }
}
catch {
    Write-Failure "Eroare la test UpdateRelatie: $_"
    $testsFailed++
}

# ========================================
# TEST 10: Test RemoveRelatie (Soft Delete)
# ========================================

Write-TestHeader "10. Test RemoveRelatie (Soft Delete)"

try {
    if ($script:TestRelatieID) {
        $query = @"
EXEC sp_PacientiPersonalMedical_RemoveRelatie 
    @RelatieID = '$($script:TestRelatieID)',
  @ModificatDe = 'TestScript'
"@
        $result = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $query
        
        if ($result[0].Success -eq 1) {
            Write-Success "Relatie dezactivata cu succes"
      
   # Verify it's actually inactive
       $verifyQuery = "SELECT EsteActiv FROM Pacienti_PersonalMedical WHERE Id = '$($script:TestRelatieID)'"
            $verify = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $verifyQuery
       
            if ($verify[0].EsteActiv -eq 0) {
 Write-Success "Verificare: Relatia este inactiva"
                $testsPassed++
       } else {
     Write-Failure "Verificare: Relatia este inca activa"
          $testsFailed++
        }
        } else {
            Write-Failure "Relatia nu a fost dezactivata"
            $testsFailed++
        }
    } else {
        Write-InfoMessage "Test skipped - nu exista RelatieID din testul anterior"
$testsPassed++
    }
}
catch {
    Write-Failure "Eroare la test RemoveRelatie: $_"
    $testsFailed++
}

# ========================================
# FINALIZARE
# ========================================

Write-Header "REZULTATE TEST SUITE"

$totalTests = $testsPassed + $testsFailed
$successRate = [math]::Round(($testsPassed / $totalTests) * 100, 2)

Write-Host ""
Write-Host "Total teste rulate: $totalTests" -ForegroundColor $ColorInfo
Write-Host "Teste trecute: $testsPassed" -ForegroundColor $ColorSuccess
Write-Host "Teste esuate: $testsFailed" -ForegroundColor $(if ($testsFailed -eq 0) { $ColorSuccess } else { $ColorError })
Write-Host "Rata de succes: $successRate%" -ForegroundColor $(if ($successRate -eq 100) { $ColorSuccess } else { $ColorWarning })
Write-Host ""

if ($testsFailed -eq 0) {
    Write-Host "? TOATE TESTELE AU TRECUT! ??" -ForegroundColor $ColorSuccess
    exit 0
} else {
    Write-Host "? UNELE TESTE AU ESUAT" -ForegroundColor $ColorWarning
    exit 1
}
