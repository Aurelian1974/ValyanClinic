# =============================================
# PowerShell Script: Direct Database Investigation
# Connects to ValyanMed database and investigates sp_Pacienti_GetAll
# =============================================

# Connection string from appsettings.json
$connectionString = "Server=DESKTOP-3Q8HI82\ERP;Database=ValyanMed;Trusted_Connection=True;Encrypt=False;MultipleActiveResultSets=True"

Write-Host "??????????????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?   DIRECT DATABASE INVESTIGATION: sp_Pacienti_GetAll                ?" -ForegroundColor Cyan
Write-Host "??????????????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

try {
    # Load SQL Client
    Add-Type -AssemblyName "System.Data"
    
    Write-Host "1. Connecting to database..." -ForegroundColor Yellow
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()
    
    Write-Host "   ? Connected successfully to: $($connection.Database)" -ForegroundColor Green
    Write-Host ""
    
    # =========================================
    # 2. Check if SP exists
    # =========================================
    Write-Host "2. Checking if sp_Pacienti_GetAll exists..." -ForegroundColor Yellow
    
    $checkSPQuery = @"
SELECT 
    name,
    create_date,
    modify_date,
    OBJECT_DEFINITION(OBJECT_ID('dbo.sp_Pacienti_GetAll')) AS Definition
FROM sys.objects 
WHERE type = 'P' AND name = 'sp_Pacienti_GetAll'
"@
    
    $command = $connection.CreateCommand()
    $command.CommandText = $checkSPQuery
    $reader = $command.ExecuteReader()
    
    if ($reader.Read()) {
        Write-Host "   ? SP exists" -ForegroundColor Green
        Write-Host "     Created: $($reader['create_date'])" -ForegroundColor Gray
        Write-Host "     Modified: $($reader['modify_date'])" -ForegroundColor Gray
        
        # Save definition for analysis
        $spDefinition = $reader['Definition']
    }
    else {
        Write-Host "   ? SP does NOT exist!" -ForegroundColor Red
        $reader.Close()
        $connection.Close()
        exit 1
    }
    
    $reader.Close()
    Write-Host ""
    
    # =========================================
    # 3. Count total patients in DB
    # =========================================
    Write-Host "3. Counting total patients in database..." -ForegroundColor Yellow
    
    $countQuery = "SELECT COUNT(*) FROM Pacienti"
    $command = $connection.CreateCommand()
    $command.CommandText = $countQuery
    $totalPatients = $command.ExecuteScalar()
    
    Write-Host "   ? Total patients in DB: $totalPatients" -ForegroundColor Green
    Write-Host ""
    
    # =========================================
    # 4. Test SP execution
    # =========================================
    Write-Host "4. Testing sp_Pacienti_GetAll execution..." -ForegroundColor Yellow
    Write-Host "   Parameters: PageNumber=1, PageSize=10, All filters=NULL" -ForegroundColor Gray
    Write-Host ""
    
    $command = $connection.CreateCommand()
    $command.CommandText = "sp_Pacienti_GetAll"
    $command.CommandType = [System.Data.CommandType]::StoredProcedure
    
    # Add parameters
    $command.Parameters.AddWithValue("@PageNumber", 1) | Out-Null
    $command.Parameters.AddWithValue("@PageSize", 10) | Out-Null
    $command.Parameters.AddWithValue("@SearchText", [DBNull]::Value) | Out-Null
    $command.Parameters.AddWithValue("@Judet", [DBNull]::Value) | Out-Null
    $command.Parameters.AddWithValue("@Asigurat", [DBNull]::Value) | Out-Null
    $command.Parameters.AddWithValue("@Activ", [DBNull]::Value) | Out-Null
    $command.Parameters.AddWithValue("@SortColumn", "Nume") | Out-Null
    $command.Parameters.AddWithValue("@SortDirection", "ASC") | Out-Null
    
    try {
        $reader = $command.ExecuteReader()
        
        # Get column names from the result set
        Write-Host "   ? SP executed successfully" -ForegroundColor Green
        Write-Host ""
        Write-Host "   Columns returned by SP:" -ForegroundColor Cyan
        Write-Host "   ????????????????????????????????????????????????????????????????????" -ForegroundColor Gray
        
        $columns = @()
        for ($i = 0; $i -lt $reader.FieldCount; $i++) {
            $columnName = $reader.GetName($i)
            $columnType = $reader.GetFieldType($i).Name
            $columns += [PSCustomObject]@{
                Index = $i + 1
                Name = $columnName
                Type = $columnType
            }
            Write-Host "   $($i + 1). $columnName ($columnType)" -ForegroundColor White
        }
        
        Write-Host "   ????????????????????????????????????????????????????????????????????" -ForegroundColor Gray
        Write-Host "   Total columns: $($reader.FieldCount)" -ForegroundColor Yellow
        Write-Host ""
        
        # Count records
        $recordCount = 0
        while ($reader.Read()) {
            $recordCount++
        }
        
        Write-Host "   ? Records returned: $recordCount" -ForegroundColor $(if ($recordCount -eq 0) { "Red" } else { "Green" })
        Write-Host ""
        
        if ($recordCount -eq 0 -and $totalPatients -gt 0) {
            Write-Host "   ??  WARNING: SP returned 0 records but DB has $totalPatients patients!" -ForegroundColor Red
            Write-Host "   ? Problem CONFIRMED: NULL handling bug" -ForegroundColor Red
            Write-Host ""
        }
        
        $reader.Close()
        
        # =========================================
        # 5. Analyze SP definition for NULL handling
        # =========================================
        Write-Host "5. Analyzing SP definition for NULL handling..." -ForegroundColor Yellow
        
        if ($spDefinition -match "WHERE\s+Activ\s*=\s*@Activ") {
            Write-Host "   ? FOUND BUG: 'WHERE Activ = @Activ' (incorrect)" -ForegroundColor Red
            Write-Host "   ? This returns 0 when @Activ is NULL" -ForegroundColor Red
        }
        elseif ($spDefinition -match "WHERE\s+\(@Activ\s+IS\s+NULL\s+OR\s+Activ\s*=\s*@Activ\)") {
            Write-Host "   ? NULL handling is CORRECT: 'WHERE (@Activ IS NULL OR Activ = @Activ)'" -ForegroundColor Green
        }
        else {
            Write-Host "   ??  Could not find WHERE clause for @Activ parameter" -ForegroundColor Yellow
        }
        
        Write-Host ""
        
        # =========================================
        # 6. Test with explicit filter
        # =========================================
        Write-Host "6. Testing SP with @Activ=1 (explicit filter)..." -ForegroundColor Yellow
        
        $command2 = $connection.CreateCommand()
        $command2.CommandText = "sp_Pacienti_GetAll"
        $command2.CommandType = [System.Data.CommandType]::StoredProcedure
        
        $command2.Parameters.AddWithValue("@PageNumber", 1) | Out-Null
        $command2.Parameters.AddWithValue("@PageSize", 10) | Out-Null
        $command2.Parameters.AddWithValue("@SearchText", [DBNull]::Value) | Out-Null
        $command2.Parameters.AddWithValue("@Judet", [DBNull]::Value) | Out-Null
        $command2.Parameters.AddWithValue("@Asigurat", [DBNull]::Value) | Out-Null
        $command2.Parameters.AddWithValue("@Activ", 1) | Out-Null  # Explicit value
        $command2.Parameters.AddWithValue("@SortColumn", "Nume") | Out-Null
        $command2.Parameters.AddWithValue("@SortDirection", "ASC") | Out-Null
        
        $reader2 = $command2.ExecuteReader()
        
        $recordCount2 = 0
        while ($reader2.Read()) {
            $recordCount2++
        }
        
        Write-Host "   ? Records returned with @Activ=1: $recordCount2" -ForegroundColor $(if ($recordCount2 -eq 0) { "Red" } else { "Green" })
        $reader2.Close()
        Write-Host ""
        
    }
    catch {
        Write-Host "   ? ERROR executing SP:" -ForegroundColor Red
        Write-Host "     $($_.Exception.Message)" -ForegroundColor Red
        Write-Host ""
        
        if ($_.Exception.Message -match "Column name or number") {
            Write-Host "   ? This is Msg 213 error: Column mismatch" -ForegroundColor Yellow
            Write-Host "   ? SP returns different columns than expected" -ForegroundColor Yellow
            Write-Host ""
            Write-Host "   SOLUTION: Check if SP SELECT statement matches Pacient entity in C#" -ForegroundColor Cyan
        }
    }
    
    # =========================================
    # 7. Recommendations
    # =========================================
    Write-Host "????????????????????????????????????????????????????????????????????" -ForegroundColor Cyan
    Write-Host "RECOMMENDATIONS" -ForegroundColor Cyan
    Write-Host "????????????????????????????????????????????????????????????????????" -ForegroundColor Cyan
    Write-Host ""
    
    if ($recordCount -eq 0 -and $totalPatients -gt 0) {
        Write-Host "? ACTION REQUIRED: Apply MASTER_FIX SQL script" -ForegroundColor Green
        Write-Host ""
        Write-Host "   1. Open SSMS" -ForegroundColor White
        Write-Host "   2. Connect to: DESKTOP-3Q8HI82\ERP ? ValyanMed" -ForegroundColor White
        Write-Host "   3. Open: DevSupport/Fixes/MASTER_FIX_AdministrarePacienti_NULL_Handling.sql" -ForegroundColor White
        Write-Host "   4. Execute (F5)" -ForegroundColor White
        Write-Host "   5. Verify Test 1 returns $totalPatients records" -ForegroundColor White
        Write-Host ""
    }
    elseif ($recordCount -gt 0) {
        Write-Host "? SP is working correctly - returned $recordCount records" -ForegroundColor Green
        Write-Host ""
        Write-Host "  If application still shows 0 records:" -ForegroundColor Yellow
        Write-Host "    1. Restart Blazor app (CTRL+C, dotnet run)" -ForegroundColor White
        Write-Host "    2. Clear browser cache (CTRL+F5)" -ForegroundColor White
        Write-Host "    3. Check C# logs for errors" -ForegroundColor White
        Write-Host ""
    }
    
    # Close connection
    $connection.Close()
    Write-Host "? Database connection closed" -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "????????????????????????????????????????????????????????????????????" -ForegroundColor Cyan
    Write-Host "INVESTIGATION COMPLETE" -ForegroundColor Cyan
    Write-Host "????????????????????????????????????????????????????????????????????" -ForegroundColor Cyan
    
}
catch {
    Write-Host ""
    Write-Host "? FATAL ERROR:" -ForegroundColor Red
    Write-Host "  $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Stack Trace:" -ForegroundColor Yellow
    Write-Host $_.Exception.StackTrace -ForegroundColor Gray
    
    if ($connection.State -eq 'Open') {
        $connection.Close()
    }
    
    exit 1
}
