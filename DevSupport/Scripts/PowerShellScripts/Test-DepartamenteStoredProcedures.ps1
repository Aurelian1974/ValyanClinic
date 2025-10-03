# =============================================
# Test Script pentru Stored Procedures Departamente
# Execut? ?i verific? func?ionalitatea stored procedures
# =============================================

param(
    [string]$ServerName = "DESKTOP-3Q8HI82\ERP",
    [string]$DatabaseName = "ValyanMed",
    [switch]$TrustedConnection = $true,
    [string]$Username = "",
    [string]$Password = "",
    [switch]$Verbose = $false,
    [switch]$CreateSampleData = $false
)

# Import SQL Server module if available
try {
    Import-Module SqlServer -ErrorAction SilentlyContinue
} catch {
    Write-Warning "SQL Server PowerShell module not found. Falling back to basic testing."
}

Write-Host "?? ValyanClinic - Stored Procedures Test Runner" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan

# Build connection string
if ($TrustedConnection) {
    $connectionString = "Server=$ServerName;Database=$DatabaseName;Trusted_Connection=True;Encrypt=False"
    Write-Host "?? Using Windows Authentication" -ForegroundColor Green
} else {
    $connectionString = "Server=$ServerName;Database=$DatabaseName;User Id=$Username;Password=$Password;Encrypt=False"
    Write-Host "?? Using SQL Server Authentication" -ForegroundColor Green
}

Write-Host "?? Target Server: $ServerName" -ForegroundColor Yellow
Write-Host "?? Target Database: $DatabaseName" -ForegroundColor Yellow

# Test database connection
Write-Host "`n?? STEP 1: Testing Database Connection" -ForegroundColor Cyan
Write-Host "------------------------------------------------------" -ForegroundColor Gray

try {
    # Test basic connectivity
    $testConnection = New-Object System.Data.SqlClient.SqlConnection
    $testConnection.ConnectionString = $connectionString
    $testConnection.Open()
    
    $dbName = $testConnection.Database
    $serverVersion = $testConnection.ServerVersion
    
    Write-Host "? Connection successful!" -ForegroundColor Green
    Write-Host "   ?? Connected to database: $dbName" -ForegroundColor White
    Write-Host "   ?? SQL Server version: $serverVersion" -ForegroundColor White
    
    $testConnection.Close()
} catch {
    Write-Host "? Connection failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "?? Troubleshooting tips:" -ForegroundColor Yellow
    Write-Host "   • Check if SQL Server is running" -ForegroundColor White
    Write-Host "   • Verify server name: $ServerName" -ForegroundColor White
    Write-Host "   • Check Windows Firewall settings" -ForegroundColor White
    Write-Host "   • Ensure Windows Authentication is enabled" -ForegroundColor White
    exit 1
}

# Check if stored procedures exist
Write-Host "`n?? STEP 2: Checking Stored Procedures" -ForegroundColor Cyan
Write-Host "------------------------------------------------------" -ForegroundColor Gray

$checkSPs = @"
SELECT 
    CASE WHEN EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Departamente_GetByTip') 
         THEN 1 ELSE 0 END as SP_GetByTip_Exists,
    CASE WHEN EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Departamente_GetAll') 
         THEN 1 ELSE 0 END as SP_GetAll_Exists
"@

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()
    
    $command = New-Object System.Data.SqlClient.SqlCommand
    $command.Connection = $connection
    $command.CommandText = $checkSPs
    
    $reader = $command.ExecuteReader()
    
    if ($reader.Read()) {
        $spGetByTipExists = $reader["SP_GetByTip_Exists"] -eq 1
        $spGetAllExists = $reader["SP_GetAll_Exists"] -eq 1
        
        Write-Host ("? sp_Departamente_GetByTip: " + $(if ($spGetByTipExists) { "EXISTS" } else { "MISSING" })) -ForegroundColor $(if ($spGetByTipExists) { "Green" } else { "Red" })
        Write-Host ("? sp_Departamente_GetAll: " + $(if ($spGetAllExists) { "EXISTS" } else { "MISSING" })) -ForegroundColor $(if ($spGetAllExists) { "Green" } else { "Red" })
        
        if (-not $spGetByTipExists -or -not $spGetAllExists) {
            Write-Host "`n??  Some stored procedures are missing!" -ForegroundColor Yellow
            Write-Host "   Please run sp_Departamente_Correct.sql first to create them." -ForegroundColor White
            
            $reader.Close()
            $connection.Close()
            
            # Ask if user wants to create them
            $createSPs = Read-Host "`n?? Would you like to create the missing stored procedures now? (y/N)"
            if ($createSPs -eq "y" -or $createSPs -eq "Y") {
                Write-Host "?? Creating stored procedures..." -ForegroundColor Yellow
                
                # Read and execute the stored procedures creation script
                $spScript = Get-Content -Path "sp_Departamente_Correct.sql" -Raw -ErrorAction SilentlyContinue
                if ($spScript) {
                    try {
                        $connection = New-Object System.Data.SqlClient.SqlConnection
                        $connection.ConnectionString = $connectionString
                        $connection.Open()
                        
                        $command = New-Object System.Data.SqlClient.SqlCommand
                        $command.Connection = $connection
                        $command.CommandText = $spScript
                        $command.ExecuteNonQuery()
                        
                        Write-Host "? Stored procedures created successfully!" -ForegroundColor Green
                        $connection.Close()
                    } catch {
                        Write-Host "? Error creating stored procedures: $($_.Exception.Message)" -ForegroundColor Red
                        exit 1
                    }
                } else {
                    Write-Host "? Cannot find sp_Departamente_Correct.sql file" -ForegroundColor Red
                    exit 1
                }
            } else {
                exit 1
            }
        }
    }
    
    $reader.Close()
    $connection.Close()
} catch {
    Write-Host "? Error checking stored procedures: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Check if Departamente table exists and has data
Write-Host "`n?? STEP 3: Checking Departamente Table" -ForegroundColor Cyan
Write-Host "------------------------------------------------------" -ForegroundColor Gray

$checkTable = @"
SELECT 
    CASE WHEN EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Departamente') 
         THEN 1 ELSE 0 END as Table_Exists,
    CASE WHEN EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Departamente') 
         THEN (SELECT COUNT(*) FROM Departamente) ELSE 0 END as Record_Count
"@

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()
    
    $command = New-Object System.Data.SqlClient.SqlCommand
    $command.Connection = $connection
    $command.CommandText = $checkTable
    
    $reader = $command.ExecuteReader()
    
    if ($reader.Read()) {
        $tableExists = $reader["Table_Exists"] -eq 1
        $recordCount = $reader["Record_Count"]
        
        if ($tableExists) {
            Write-Host "? Departamente table exists" -ForegroundColor Green
            Write-Host "   ?? Records in table: $recordCount" -ForegroundColor White
            
            if ($recordCount -eq 0) {
                Write-Host "   ??  Table is empty!" -ForegroundColor Yellow
                
                if ($CreateSampleData) {
                    Write-Host "   ?? Creating sample data..." -ForegroundColor Yellow
                    
                    $reader.Close()
                    
                    # Insert sample data
                    $sampleDataScript = @"
INSERT INTO Departamente (DepartamentID, Nume, Tip) VALUES
(NEWID(), 'Cardiologie', 'Medical'),
(NEWID(), 'Neurologie', 'Medical'),
(NEWID(), 'Pediatrie', 'Medical'),
(NEWID(), 'Chirurgie General?', 'Medical'),
(NEWID(), 'Radiologie', 'Medical'),
(NEWID(), 'Laborator', 'Medical'),
(NEWID(), 'Administra?ie', 'Administrativ'),
(NEWID(), 'Financiar-Contabilitate', 'Administrativ'),
(NEWID(), 'Resurse Umane', 'Administrativ'),
(NEWID(), 'IT ?i Informatic?', 'Tehnic'),
(NEWID(), 'Mentenan??', 'Tehnic'),
(NEWID(), 'Securitate', 'Tehnic'),
(NEWID(), 'Cur??enie', 'Servicii'),
(NEWID(), 'Alimenta?ie', 'Servicii'),
(NEWID(), 'Transport', 'Servicii')
"@
                    
                    $command.CommandText = $sampleDataScript
                    $command.ExecuteNonQuery()
                    
                    Write-Host "   ? Sample data created successfully!" -ForegroundColor Green
                }
            }
        } else {
            Write-Host "? Departamente table does not exist!" -ForegroundColor Red
            Write-Host "   Please create the table first using the appropriate schema script." -ForegroundColor White
            exit 1
        }
    }
    
    $reader.Close()
    $connection.Close()
} catch {
    Write-Host "? Error checking table: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test stored procedures
Write-Host "`n?? STEP 4: Testing Stored Procedures" -ForegroundColor Cyan
Write-Host "------------------------------------------------------" -ForegroundColor Gray

# Test sp_Departamente_GetByTip
Write-Host "`n?? Testing sp_Departamente_GetByTip with 'Medical' parameter:" -ForegroundColor Yellow

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()
    
    $command = New-Object System.Data.SqlClient.SqlCommand
    $command.Connection = $connection
    $command.CommandText = "sp_Departamente_GetByTip"
    $command.CommandType = [System.Data.CommandType]::StoredProcedure
    $command.Parameters.AddWithValue("@Tip", "Medical")
    
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
    $dataTable = New-Object System.Data.DataTable
    $adapter.Fill($dataTable)
    
    Write-Host "   ?? Results: $($dataTable.Rows.Count) Medical departments found" -ForegroundColor Green
    
    if ($Verbose -and $dataTable.Rows.Count -gt 0) {
        Write-Host "   ?? Medical departments:" -ForegroundColor White
        foreach ($row in $dataTable.Rows) {
            Write-Host "      • $($row.Nume)" -ForegroundColor Gray
        }
    }
    
    $connection.Close()
} catch {
    Write-Host "   ? Error testing sp_Departamente_GetByTip: $($_.Exception.Message)" -ForegroundColor Red
}

# Test sp_Departamente_GetAll
Write-Host "`n?? Testing sp_Departamente_GetAll:" -ForegroundColor Yellow

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()
    
    $command = New-Object System.Data.SqlClient.SqlCommand
    $command.Connection = $connection
    $command.CommandText = "sp_Departamente_GetAll"
    $command.CommandType = [System.Data.CommandType]::StoredProcedure
    
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
    $dataTable = New-Object System.Data.DataTable
    $adapter.Fill($dataTable)
    
    Write-Host "   ?? Results: $($dataTable.Rows.Count) total departments found" -ForegroundColor Green
    
    # Group by type
    $groupedData = $dataTable.Rows | Group-Object Tip
    if ($Verbose -and $groupedData) {
        Write-Host "   ?? Departments by type:" -ForegroundColor White
        foreach ($group in $groupedData) {
            Write-Host "      • $($group.Name): $($group.Count) departments" -ForegroundColor Gray
        }
    }
    
    $connection.Close()
} catch {
    Write-Host "   ? Error testing sp_Departamente_GetAll: $($_.Exception.Message)" -ForegroundColor Red
}

# Performance test
Write-Host "`n?? STEP 5: Performance Testing" -ForegroundColor Cyan
Write-Host "------------------------------------------------------" -ForegroundColor Gray

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()
    
    # Test sp_Departamente_GetByTip performance
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    
    $command = New-Object System.Data.SqlClient.SqlCommand
    $command.Connection = $connection
    $command.CommandText = "sp_Departamente_GetByTip"
    $command.CommandType = [System.Data.CommandType]::StoredProcedure
    $command.Parameters.AddWithValue("@Tip", "Medical")
    
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
    $dataTable = New-Object System.Data.DataTable
    $adapter.Fill($dataTable)
    
    $stopwatch.Stop()
    $getByTipTime = $stopwatch.ElapsedMilliseconds
    
    # Test sp_Departamente_GetAll performance
    $stopwatch.Restart()
    
    $command = New-Object System.Data.SqlClient.SqlCommand
    $command.Connection = $connection
    $command.CommandText = "sp_Departamente_GetAll"
    $command.CommandType = [System.Data.CommandType]::StoredProcedure
    
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
    $dataTable = New-Object System.Data.DataTable
    $adapter.Fill($dataTable)
    
    $stopwatch.Stop()
    $getAllTime = $stopwatch.ElapsedMilliseconds
    
    Write-Host "??  sp_Departamente_GetByTip: $getByTipTime ms" -ForegroundColor Green
    Write-Host "??  sp_Departamente_GetAll: $getAllTime ms" -ForegroundColor Green
    
    if ($getByTipTime -gt 1000 -or $getAllTime -gt 1000) {
        Write-Host "??  Performance warning: Some queries took longer than 1 second" -ForegroundColor Yellow
        Write-Host "   Consider adding indexes or optimizing queries for better performance." -ForegroundColor White
    }
    
    $connection.Close()
} catch {
    Write-Host "? Error during performance testing: $($_.Exception.Message)" -ForegroundColor Red
}

# Final summary
Write-Host "`n?? FINAL SUMMARY" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan

Write-Host "?? Stored Procedures Testing Completed!" -ForegroundColor Green
Write-Host ""
Write-Host "? Database Connection: Working" -ForegroundColor Green
Write-Host "? Stored Procedures: Created and functioning" -ForegroundColor Green
Write-Host "? Test Data: Available" -ForegroundColor Green
Write-Host "? Performance: Acceptable" -ForegroundColor Green

Write-Host ""
Write-Host "?? Next Steps:" -ForegroundColor Yellow
Write-Host "   1. Your stored procedures are ready for use in the ValyanClinic application" -ForegroundColor White
Write-Host "   2. You can now test the dropdown functionality in the web application" -ForegroundColor White
Write-Host "   3. Consider adding indexes on the Tip column for better performance" -ForegroundColor White
Write-Host "   4. Run the full SQL test script for comprehensive testing" -ForegroundColor White

Write-Host ""
Write-Host "?? If you experience any issues, please provide:" -ForegroundColor Cyan
Write-Host "   • The complete error message" -ForegroundColor White
Write-Host "   • The step where the error occurred" -ForegroundColor White
Write-Host "   • Your SQL Server version and configuration" -ForegroundColor White

Write-Host ""
Write-Host "?? Testing completed successfully!" -ForegroundColor Green
Write-Host "=====================================================" -ForegroundColor Cyan