# ========================================
# PowerShell Script: Test Utilizatori Stored Procedures
# Database: ValyanMed
# Descriere: Test suite complet pentru Utilizatori
# Autor: System
# Data: 2025-01-24
# ========================================

<#
.SYNOPSIS
    Testeaza toate stored procedures pentru Utilizatori

.DESCRIPTION
    Ruleaza teste automate pentru:
    - Verificare existenta tabela
  - Verificare Foreign Keys
    - Verificare Constraints
    - Test CRUD operations
    - Test autentificare
    - Test securitate (blocare cont)
    - Test resetare parola

.EXAMPLE
    .\Test-Utilizatori.ps1
    
.EXAMPLE
    .\Test-Utilizatori.ps1 -ServerName "LOCALHOST\SQLEXPRESS" -Verbose
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
$TestResults = @()
$TestsPassed = 0
$TestsFailed = 0

# Colors
$ColorSuccess = "Green"
$ColorError = "Red"
$ColorInfo = "Cyan"
$ColorHeader = "Magenta"
$ColorWarning = "Yellow"

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

function Write-TestResult {
  param(
        [string]$TestName,
        [bool]$Passed,
        [string]$Message = ""
    )
    
  $script:TestResults += [PSCustomObject]@{
        TestName = $TestName
        Passed = $Passed
        Message = $Message
    }
    
    if ($Passed) {
        $script:TestsPassed++
      Write-Host "? $TestName" -ForegroundColor $ColorSuccess
     if ($Message) { Write-Host "   $Message" -ForegroundColor Gray }
    }
    else {
        $script:TestsFailed++
        Write-Host "? $TestName" -ForegroundColor $ColorError
      if ($Message) { Write-Host "   $Message" -ForegroundColor $ColorError }
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

# ========================================
# TESTS
# ========================================

Write-Header "TEST SUITE - UTILIZATORI"

Write-Host "Server: $ServerName" -ForegroundColor $ColorInfo
Write-Host "Database: $DatabaseName" -ForegroundColor $ColorInfo
Write-Host ""

# ========================================
# TEST 1: Verificare existenta tabela
# ========================================

Write-Host "Test 1: Verificare existenta tabela Utilizatori..." -ForegroundColor $ColorInfo

try {
 $query = "SELECT COUNT(*) AS Exists FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Utilizatori'"
    $result = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $query
    
    if ($result.Exists -eq 1) {
      Write-TestResult "Tabela Utilizatori exista" $true
    }
    else {
        Write-TestResult "Tabela Utilizatori exista" $false "Tabela nu a fost gasita"
    }
}
catch {
    Write-TestResult "Tabela Utilizatori exista" $false $_.Exception.Message
}

# ========================================
# TEST 2: Verificare Foreign Keys
# ========================================

Write-Host "`nTest 2: Verificare Foreign Keys..." -ForegroundColor $ColorInfo

try {
    $query = @"
SELECT 
    fk.name AS FK_Name,
  OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS ReferencedColumn
FROM sys.foreign_keys AS fk
INNER JOIN sys.foreign_key_columns AS fkc 
    ON fk.object_id = fkc.constraint_object_id
WHERE fk.parent_object_id = OBJECT_ID('Utilizatori')
"@
    
    $result = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $query
    
    if ($result.Rows.Count -gt 0) {
        $fkToPersonalMedical = $result | Where-Object { $_.ReferencedTable -eq 'PersonalMedical' }
    
        if ($fkToPersonalMedical) {
            Write-TestResult "FK: Utilizatori -> PersonalMedical" $true "Column: $($fkToPersonalMedical.ColumnName)"
        }
        else {
          Write-TestResult "FK: Utilizatori -> PersonalMedical" $false "FK nu a fost gasit"
        }
    }
 else {
        Write-TestResult "Foreign Keys" $false "Niciun Foreign Key gasit"
    }
}
catch {
    Write-TestResult "Foreign Keys" $false $_.Exception.Message
}

# ========================================
# TEST 3: Verificare Unique Constraints
# ========================================

Write-Host "`nTest 3: Verificare Unique Constraints..." -ForegroundColor $ColorInfo

try {
    $query = @"
SELECT 
    con.name AS ConstraintName,
    col.name AS ColumnName
FROM sys.key_constraints con
INNER JOIN sys.index_columns ic ON con.parent_object_id = ic.object_id AND con.unique_index_id = ic.index_id
INNER JOIN sys.columns col ON ic.object_id = col.object_id AND ic.column_id = col.column_id
WHERE con.parent_object_id = OBJECT_ID('Utilizatori')
  AND con.type = 'UQ'
ORDER BY con.name, ic.key_ordinal
"@
    
    $result = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $query
    
    $hasUsername = $result | Where-Object { $_.ColumnName -eq 'Username' }
    $hasEmail = $result | Where-Object { $_.ColumnName -eq 'Email' }
    $hasPersonalMedicalID = $result | Where-Object { $_.ColumnName -eq 'PersonalMedicalID' }
    
    Write-TestResult "UQ: Username" ($null -ne $hasUsername)
    Write-TestResult "UQ: Email" ($null -ne $hasEmail)
    Write-TestResult "UQ: PersonalMedicalID" ($null -ne $hasPersonalMedicalID)
}
catch {
    Write-TestResult "Unique Constraints" $false $_.Exception.Message
}

# ========================================
# TEST 4: Verificare Stored Procedures
# ========================================

Write-Host "`nTest 4: Verificare Stored Procedures..." -ForegroundColor $ColorInfo

$expectedSPs = @(
'sp_Utilizatori_GetAll',
 'sp_Utilizatori_GetCount',
    'sp_Utilizatori_GetById',
    'sp_Utilizatori_GetByUsername',
    'sp_Utilizatori_GetByEmail',
    'sp_Utilizatori_Create',
    'sp_Utilizatori_Update',
  'sp_Utilizatori_ChangePassword',
    'sp_Utilizatori_UpdateUltimaAutentificare',
    'sp_Utilizatori_IncrementIncercariEsuate',
    'sp_Utilizatori_SetTokenResetareParola',
 'sp_Utilizatori_GetStatistics'
)

try {
    $query = "SELECT name FROM sys.objects WHERE type = 'P' AND name LIKE 'sp_Utilizatori_%'"
    $result = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $query
    
  foreach ($spName in $expectedSPs) {
      $exists = $result | Where-Object { $_.name -eq $spName }
        Write-TestResult "SP: $spName" ($null -ne $exists)
    }
}
catch {
    Write-TestResult "Stored Procedures" $false $_.Exception.Message
}

# ========================================
# TEST 5: Test Create Utilizator
# ========================================

Write-Host "`nTest 5: Test Create Utilizator..." -ForegroundColor $ColorInfo

try {
    # Get an active PersonalMedical without user
    $query = @"
SELECT TOP 1 pm.PersonalID, pm.Nume, pm.Prenume, pm.Email
FROM PersonalMedical pm
LEFT JOIN Utilizatori u ON pm.PersonalID = u.PersonalMedicalID
WHERE pm.EsteActiv = 1 
  AND u.UtilizatorID IS NULL
  AND pm.Email IS NOT NULL
"@
    
    $pm = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $query
    
 if ($pm.Rows.Count -gt 0) {
        $personalID = $pm.Rows[0].PersonalID
$testUsername = "test_user_" + (Get-Random -Minimum 1000 -Maximum 9999)
   $testEmail = "test_" + (Get-Random -Minimum 1000 -Maximum 9999) + "@test.com"
        
        $createQuery = @"
EXEC sp_Utilizatori_Create 
  @PersonalMedicalID = '$personalID',
    @Username = '$testUsername',
    @Email = '$testEmail',
    @PasswordHash = 'TEST_HASH',
    @Salt = 'TEST_SALT',
    @Rol = 'Utilizator',
    @EsteActiv = 1,
    @CreatDe = 'TestScript'
"@
        
 $result = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $createQuery
     
        if ($result.Rows.Count -gt 0 -and $result.Rows[0].UtilizatorID) {
        Write-TestResult "Create Utilizator" $true "Username: $testUsername"

          # Save for cleanup
  $script:TestUtilizatorID = $result.Rows[0].UtilizatorID
         $script:TestUsername = $testUsername
        }
        else {
            Write-TestResult "Create Utilizator" $false "Nu s-a returnat UtilizatorID"
        }
    }
    else {
        Write-TestResult "Create Utilizator" $false "Nu exista PersonalMedical disponibil pentru test"
    }
}
catch {
    Write-TestResult "Create Utilizator" $false $_.Exception.Message
}

# ========================================
# TEST 6: Test GetByUsername
# ========================================

Write-Host "`nTest 6: Test GetByUsername..." -ForegroundColor $ColorInfo

if ($script:TestUsername) {
    try {
   $query = "EXEC sp_Utilizatori_GetByUsername @Username = '$($script:TestUsername)'"
        $result = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $query
        
 if ($result.Rows.Count -gt 0 -and $result.Rows[0].Username -eq $script:TestUsername) {
 Write-TestResult "GetByUsername" $true "User gasit: $($result.Rows[0].NumeComplet)"
        }
 else {
  Write-TestResult "GetByUsername" $false "User nu a fost gasit"
  }
    }
    catch {
        Write-TestResult "GetByUsername" $false $_.Exception.Message
    }
}
else {
  Write-TestResult "GetByUsername" $false "Test user nu a fost creat"
}

# ========================================
# TEST 7: Test UpdateUltimaAutentificare
# ========================================

Write-Host "`nTest 7: Test UpdateUltimaAutentificare..." -ForegroundColor $ColorInfo

if ($script:TestUtilizatorID) {
    try {
        $query = "EXEC sp_Utilizatori_UpdateUltimaAutentificare @UtilizatorID = '$($script:TestUtilizatorID)'"
        $result = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $query
     
     if ($result.Rows[0].Success -eq 1) {
            # Verify DataUltimaAutentificare was updated
          $verifyQuery = "SELECT DataUltimaAutentificare FROM Utilizatori WHERE UtilizatorID = '$($script:TestUtilizatorID)'"
            $verify = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $verifyQuery
            
          if ($null -ne $verify.Rows[0].DataUltimaAutentificare) {
                Write-TestResult "UpdateUltimaAutentificare" $true "Data updated"
    }
            else {
   Write-TestResult "UpdateUltimaAutentificare" $false "Data nu a fost actualizata"
            }
        }
        else {
   Write-TestResult "UpdateUltimaAutentificare" $false
        }
 }
    catch {
      Write-TestResult "UpdateUltimaAutentificare" $false $_.Exception.Message
    }
}
else {
    Write-TestResult "UpdateUltimaAutentificare" $false "Test user nu a fost creat"
}

# ========================================
# TEST 8: Test IncrementIncercariEsuate (blocare dupa 5)
# ========================================

Write-Host "`nTest 8: Test IncrementIncercariEsuate (blocare)..." -ForegroundColor $ColorInfo

if ($script:TestUtilizatorID) {
    try {
        # Increment 5 times to trigger block
        for ($i = 1; $i -le 5; $i++) {
         $query = "EXEC sp_Utilizatori_IncrementIncercariEsuate @UtilizatorID = '$($script:TestUtilizatorID)'"
   $result = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $query
  }
        
        # Verify user is blocked
        $verifyQuery = "SELECT DataBlocare, NumarIncercariEsuate FROM Utilizatori WHERE UtilizatorID = '$($script:TestUtilizatorID)'"
        $verify = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $verifyQuery
  
        if ($null -ne $verify.Rows[0].DataBlocare -and $verify.Rows[0].NumarIncercariEsuate -eq 5) {
       Write-TestResult "IncrementIncercariEsuate + Blocare" $true "User blocat dupa 5 incercari"
        }
        else {
          Write-TestResult "IncrementIncercariEsuate + Blocare" $false "User nu a fost blocat"
        }
    }
    catch {
        Write-TestResult "IncrementIncercariEsuate + Blocare" $false $_.Exception.Message
    }
}
else {
    Write-TestResult "IncrementIncercariEsuate + Blocare" $false "Test user nu a fost creat"
}

# ========================================
# TEST 9: Test ChangePassword (deblocheaza)
# ========================================

Write-Host "`nTest 9: Test ChangePassword (deblocheaza)..." -ForegroundColor $ColorInfo

if ($script:TestUtilizatorID) {
    try {
      $query = @"
EXEC sp_Utilizatori_ChangePassword 
  @UtilizatorID = '$($script:TestUtilizatorID)',
    @NewPasswordHash = 'NEW_HASH',
    @NewSalt = 'NEW_SALT',
    @ModificatDe = 'TestScript'
"@
        
        $result = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $query
  
        if ($result.Rows[0].Success -eq 1) {
         # Verify user is unblocked
   $verifyQuery = "SELECT DataBlocare, NumarIncercariEsuate FROM Utilizatori WHERE UtilizatorID = '$($script:TestUtilizatorID)'"
   $verify = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $verifyQuery
      
  if ($null -eq $verify.Rows[0].DataBlocare -and $verify.Rows[0].NumarIncercariEsuate -eq 0) {
       Write-TestResult "ChangePassword (deblocheaza)" $true "User deblocat si incercari resetate"
            }
        else {
 Write-TestResult "ChangePassword (deblocheaza)" $false "User nu a fost deblocat complet"
         }
        }
        else {
        Write-TestResult "ChangePassword (deblocheaza)" $false
      }
 }
    catch {
      Write-TestResult "ChangePassword (deblocheaza)" $false $_.Exception.Message
 }
}
else {
    Write-TestResult "ChangePassword (deblocheaza)" $false "Test user nu a fost creat"
}

# ========================================
# TEST 10: Test GetStatistics
# ========================================

Write-Host "`nTest 10: Test GetStatistics..." -ForegroundColor $ColorInfo

try {
  $query = "EXEC sp_Utilizatori_GetStatistics"
    $result = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $query
    
    if ($result.Rows.Count -gt 0) {
        $totalRow = $result | Where-Object { $_.Categorie -eq 'Total' }
 
        if ($totalRow) {
            Write-TestResult "GetStatistics" $true "Total utilizatori: $($totalRow.Numar), Activi: $($totalRow.Activi)"
        }
     else {
   Write-TestResult "GetStatistics" $false "Categorie 'Total' nu a fost gasita"
        }
    }
    else {
        Write-TestResult "GetStatistics" $false "Nu s-au returnat statistici"
    }
}
catch {
    Write-TestResult "GetStatistics" $false $_.Exception.Message
}

# ========================================
# CLEANUP: Delete test user
# ========================================

Write-Host "`nCleanup: Stergere utilizator de test..." -ForegroundColor $ColorInfo

if ($script:TestUtilizatorID) {
try {
        $query = "DELETE FROM Utilizatori WHERE UtilizatorID = '$($script:TestUtilizatorID)'"
     Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $query | Out-Null
        Write-Host "? Test user sters" -ForegroundColor $ColorSuccess
    }
    catch {
        Write-Host "?? Nu s-a putut sterge test user: $($_.Exception.Message)" -ForegroundColor $ColorWarning
    }
}

# ========================================
# CLEANUP FINAL: Goleste tabela Utilizatori dupa teste
# ========================================

Write-Host "`nCleanup Final: Golire tabela Utilizatori..." -ForegroundColor $ColorInfo

try {
    # Check if there are any users
  $countQuery = "SELECT COUNT(*) AS UserCount FROM Utilizatori"
    $countResult = Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $countQuery
    
    # Check if result has rows
    if ($countResult -and $countResult.Rows.Count -gt 0) {
  $userCount = $countResult.Rows[0].UserCount
    
        if ($userCount -gt 0) {
 Write-Host "Tabela contine $userCount utilizatori" -ForegroundColor Yellow
        
   # Ask for confirmation
  $confirmation = Read-Host "Doresti sa golesti tabela Utilizatori? (DA/NU)"
        
    if ($confirmation -eq "DA") {
             # Delete all users (cascade will handle FK constraints)
          $deleteQuery = "DELETE FROM Utilizatori"
   Invoke-SqlQuery -Server $ServerName -Database $DatabaseName -Query $deleteQuery | Out-Null
   
         Write-Host "? Tabela Utilizatori a fost golita cu succes" -ForegroundColor $ColorSuccess
            }
     else {
   Write-Host "?? Tabela Utilizatori nu a fost golita" -ForegroundColor $ColorInfo
      }
 }
        else {
Write-Host "? Tabela Utilizatori este deja goala" -ForegroundColor $ColorSuccess
 }
    }
    else {
    Write-Host "? Tabela Utilizatori este deja goala (no data)" -ForegroundColor $ColorSuccess
    }
}
catch {
    Write-Host "?? Eroare la golirea tabelei: $($_.Exception.Message)" -ForegroundColor $ColorWarning
}

Write-Host ""

# ========================================
# SUMMARY
# ========================================

Write-Header "TEST SUMMARY"

Write-Host "Total Tests: $($TestsPassed + $TestsFailed)" -ForegroundColor $ColorInfo
Write-Host "Passed: $TestsPassed" -ForegroundColor $ColorSuccess
Write-Host "Failed: $TestsFailed" -ForegroundColor $(if ($TestsFailed -eq 0) { $ColorSuccess } else { $ColorError })
Write-Host ""

if ($TestsFailed -eq 0) {
    Write-Host "?? Toate testele au trecut cu succes!" -ForegroundColor $ColorSuccess
}
else {
    Write-Host "?? Unele teste au esuat. Verifica erorile de mai sus." -ForegroundColor $ColorWarning
}

Write-Host ""

# Export results
$TestResults | Format-Table -AutoSize
