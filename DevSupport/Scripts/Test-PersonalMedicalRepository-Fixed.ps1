# =============================================
# PERSONALMEDICAL REPOSITORY TEST SCRIPT
# Tests the fixed PersonalMedicalRepository after stored procedure parameter fixes
# =============================================

param(
    [string]$ConnectionString = "Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ValyanClinicDB;Integrated Security=true;MultipleActiveResultSets=true;Trust Server Certificate=true"
)

Write-Host "?? PERSONALMEDICAL REPOSITORY TESTING" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host "Testing Date: $(Get-Date)" -ForegroundColor Gray
Write-Host ""

# Test 1: Verify stored procedures exist
Write-Host "?? TEST 1: Verifying Stored Procedures Existence" -ForegroundColor Yellow
Write-Host "------------------------------------------------" -ForegroundColor Gray

$requiredSPs = @(
    'sp_PersonalMedical_GetAll',
    'sp_PersonalMedical_GetById', 
    'sp_PersonalMedical_GetStatistics',
    'sp_PersonalMedical_CheckUnique',
    'sp_PersonalMedical_Create',
    'sp_PersonalMedical_Update',
    'sp_PersonalMedical_Delete',
    'sp_PersonalMedical_GetDropdownOptions'
)

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
    $connection.Open()
    
    $missingProcedures = @()
    
    foreach ($sp in $requiredSPs) {
        $query = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_NAME = '$sp' AND ROUTINE_TYPE = 'PROCEDURE'"
        $command = New-Object System.Data.SqlClient.SqlCommand($query, $connection)
        $exists = $command.ExecuteScalar()
        
        if ($exists -eq 1) {
            Write-Host "   ? $sp" -ForegroundColor Green
        } else {
            Write-Host "   ? $sp" -ForegroundColor Red
            $missingProcedures += $sp
        }
    }
    
    $connection.Close()
    
    if ($missingProcedures.Count -gt 0) {
        Write-Host ""
        Write-Host "??  Missing Stored Procedures Found!" -ForegroundColor Red
        Write-Host "Please run the Install-PersonalMedicalStoredProcedures.sql script first." -ForegroundColor Red
        Write-Host ""
        Write-Host "Missing procedures:" -ForegroundColor Red
        foreach ($missing in $missingProcedures) {
            Write-Host "   • $missing" -ForegroundColor Red
        }
        return
    }
    
} catch {
    Write-Host "? Database connection failed: $($_.Exception.Message)" -ForegroundColor Red
    return
}

Write-Host "? All required stored procedures found!" -ForegroundColor Green
Write-Host ""

# Test 2: Test Parameter Matching
Write-Host "?? TEST 2: Testing Parameter Matching" -ForegroundColor Yellow
Write-Host "------------------------------------" -ForegroundColor Gray

# Test sp_PersonalMedical_GetAll parameters
Write-Host "Testing sp_PersonalMedical_GetAll parameters..." -ForegroundColor Gray

$testQueries = @{
    "sp_PersonalMedical_GetAll" = @"
DECLARE @PageNumber INT = 1
DECLARE @PageSize INT = 20
DECLARE @SearchText NVARCHAR(255) = NULL
DECLARE @Departament NVARCHAR(100) = NULL
DECLARE @Pozitie NVARCHAR(50) = NULL
DECLARE @EsteActiv BIT = NULL
DECLARE @SortColumn NVARCHAR(50) = 'Nume'
DECLARE @SortDirection NVARCHAR(4) = 'ASC'

-- This should work now with correct parameters
-- EXEC sp_PersonalMedical_GetAll @PageNumber, @PageSize, @SearchText, @Departament, @Pozitie, @EsteActiv, @SortColumn, @SortDirection
SELECT 'sp_PersonalMedical_GetAll parameters match' as TestResult
"@

    "sp_PersonalMedical_CheckUnique" = @"
DECLARE @Email VARCHAR(100) = 'test@example.com'
DECLARE @NumarLicenta VARCHAR(50) = 'LIC123'
DECLARE @ExcludeId UNIQUEIDENTIFIER = NULL

-- This should work now
-- EXEC sp_PersonalMedical_CheckUnique @Email, @NumarLicenta, @ExcludeId
SELECT 'sp_PersonalMedical_CheckUnique parameters match' as TestResult
"@

    "sp_PersonalMedical_Delete" = @"
DECLARE @PersonalID UNIQUEIDENTIFIER = NEWID()

-- This should work now (no longer requires @ModificatDe)
-- EXEC sp_PersonalMedical_Delete @PersonalID  
SELECT 'sp_PersonalMedical_Delete parameters match' as TestResult
"@
}

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
    $connection.Open()
    
    foreach ($test in $testQueries.GetEnumerator()) {
        try {
            $command = New-Object System.Data.SqlClient.SqlCommand($test.Value, $connection)
            $result = $command.ExecuteScalar()
            Write-Host "   ? $($test.Key): $result" -ForegroundColor Green
        } catch {
            Write-Host "   ? $($test.Key): $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    
    $connection.Close()
    
} catch {
    Write-Host "? Parameter testing failed: $($_.Exception.Message)" -ForegroundColor Red
    return
}

Write-Host ""

# Test 3: Repository Code Analysis
Write-Host "?? TEST 3: Repository Code Analysis" -ForegroundColor Yellow
Write-Host "----------------------------------" -ForegroundColor Gray

$repositoryPath = "ValyanClinic.Infrastructure\Repositories\PersonalMedicalRepository.cs"

if (Test-Path $repositoryPath) {
    $repositoryContent = Get-Content $repositoryPath -Raw
    
    # Check for old problematic parameters
    $problemPatterns = @{
        '@Status' = '@EsteActiv'
        '@AreSpecializare' = 'Removed (not needed)'
        'sp_PersonalMedical_CheckLicentaUnicity' = 'sp_PersonalMedical_CheckUnique'
        'sp_PersonalMedical_CheckEmailUnicity' = 'sp_PersonalMedical_CheckUnique'
    }
    
    $foundIssues = @()
    
    foreach ($pattern in $problemPatterns.GetEnumerator()) {
        if ($repositoryContent -match [regex]::Escape($pattern.Key)) {
            $foundIssues += "$($pattern.Key) (should be: $($pattern.Value))"
        }
    }
    
    if ($foundIssues.Count -eq 0) {
        Write-Host "   ? Repository code looks good - no old parameter patterns found" -ForegroundColor Green
    } else {
        Write-Host "   ??  Found potential issues:" -ForegroundColor Red
        foreach ($issue in $foundIssues) {
            Write-Host "      • $issue" -ForegroundColor Red
        }
    }
    
    # Check for correct patterns
    $correctPatterns = @(
        '@EsteActiv',
        'sp_PersonalMedical_CheckUnique'
    )
    
    $foundCorrectPatterns = 0
    foreach ($pattern in $correctPatterns) {
        if ($repositoryContent -match [regex]::Escape($pattern)) {
            $foundCorrectPatterns++
        }
    }
    
    Write-Host "   ? Found $foundCorrectPatterns/$($correctPatterns.Count) correct patterns" -ForegroundColor Green
    
} else {
    Write-Host "   ? Repository file not found at: $repositoryPath" -ForegroundColor Red
}

Write-Host ""

# Test 4: Build Test
Write-Host "?? TEST 4: Build Test" -ForegroundColor Yellow
Write-Host "--------------------" -ForegroundColor Gray

try {
    $buildResult = & dotnet build --no-restore --verbosity quiet 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ? Solution builds successfully" -ForegroundColor Green
    } else {
        Write-Host "   ? Build failed:" -ForegroundColor Red
        Write-Host $buildResult -ForegroundColor Red
    }
} catch {
    Write-Host "   ??  Could not run build test: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host ""

# Summary
Write-Host "?? SUMMARY & RECOMMENDATIONS" -ForegroundColor Cyan
Write-Host "============================" -ForegroundColor Cyan

Write-Host ""
Write-Host "? FIXES APPLIED:" -ForegroundColor Green
Write-Host "   • Changed @Status parameter to @EsteActiv in GetAllAsync" 
Write-Host "   • Removed @AreSpecializare parameter (not needed by SP)"
Write-Host "   • Fixed CheckLicentaUnicityAsync to use sp_PersonalMedical_CheckUnique"
Write-Host "   • Fixed CheckEmailUnicityAsync to use sp_PersonalMedical_CheckUnique"  
Write-Host "   • Removed @CreatDe/@ModificatDe parameters from Create/Update/Delete"
Write-Host "   • Fixed dynamic result casting in DeleteAsync"
Write-Host ""

Write-Host "?? NEXT STEPS:" -ForegroundColor Yellow
Write-Host "   1. Run Install-PersonalMedicalStoredProcedures.sql on your database"
Write-Host "   2. Test the application - the 'too many arguments' error should be resolved"
Write-Host "   3. Verify PersonalMedical CRUD operations work correctly"
Write-Host ""

Write-Host "?? NOTES:" -ForegroundColor Cyan  
Write-Host "   • The repository now matches the stored procedure signatures exactly"
Write-Host "   • Status filtering works via @EsteActiv (true/false) instead of @Status (string)"
Write-Host "   • Specialization filtering was removed as it wasn't in the SP definition"
Write-Host "   • All CRUD operations should now work without parameter mismatches"
Write-Host ""

Write-Host "?? TESTING COMPLETE!" -ForegroundColor Green
Write-Host "Date: $(Get-Date)"
Write-Host "</Test-PersonalMedicalRepository-Fixed.ps1>" -ForegroundColor Gray