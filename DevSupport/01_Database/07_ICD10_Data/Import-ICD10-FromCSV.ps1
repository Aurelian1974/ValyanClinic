# ========================================
# PowerShell Script: Import ICD-10 from CSV
# Database: ValyanMed
# Descriere: Importa coduri ICD-10 dintr-un fisier CSV
# ========================================

param(
    [string]$CsvFilePath = ".\icd10_codes.csv",
    [string]$ServerInstance = "DESKTOP-9H54BCS\SQLSERVER",
    [string]$Database = "ValyanMed",
    [int]$BatchSize = 1000,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   ICD-10 CSV IMPORT" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if (-not (Test-Path $CsvFilePath)) {
    Write-Host "? CSV file not found: $CsvFilePath" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please download ICD-10 CSV from one of these sources:" -ForegroundColor Yellow
    Write-Host "  1. CMS.gov: https://www.cms.gov/medicare/coding-billing/icd-10-codes" -ForegroundColor White
    Write-Host "  2. GitHub: https://github.com/kamillamagna/ICD-10-CSV" -ForegroundColor White
    Write-Host "  3. WHO: https://icd.who.int/browse10/2019/en" -ForegroundColor White
    Write-Host ""
    exit 1
}

Write-Host "?? CSV File: $CsvFilePath" -ForegroundColor Yellow
Write-Host "???  Server: $ServerInstance" -ForegroundColor Yellow
Write-Host "?? Database: $Database" -ForegroundColor Yellow
Write-Host ""

# Import CSV
Write-Host "?? Reading CSV file..." -ForegroundColor Cyan
$csvData = Import-Csv $CsvFilePath

Write-Host "? Found $($csvData.Count) codes in CSV" -ForegroundColor Green
Write-Host ""

# Detect CSV structure (different sources have different columns)
$sampleRow = $csvData[0]
$columns = $sampleRow.PSObject.Properties.Name

Write-Host "?? Detected CSV columns:" -ForegroundColor Cyan
$columns | ForEach-Object { Write-Host "   - $_" -ForegroundColor White }
Write-Host ""

# Map columns (adapt based on CSV structure)
$codeColumn = $columns | Where-Object { $_ -match 'code|icd' } | Select-Object -First 1
$descColumn = $columns | Where-Object { $_ -match 'desc|name|title' } | Select-Object -First 1

Write-Host "Mapped columns:" -ForegroundColor Yellow
Write-Host "   Code: $codeColumn" -ForegroundColor White
Write-Host "   Description: $descColumn" -ForegroundColor White
Write-Host ""

# Confirm import
$confirm = Read-Host "Continue with import? (Y/N)"
if ($confirm -ne 'Y') {
    Write-Host "Import cancelled." -ForegroundColor Yellow
    exit 0
}

# Connect to database
Write-Host ""
Write-Host "?? Connecting to database..." -ForegroundColor Cyan

$connectionString = "Server=$ServerInstance;Database=$Database;Integrated Security=True;TrustServerCertificate=True;"
$connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$connection.Open()

Write-Host "? Connected!" -ForegroundColor Green
Write-Host ""

# ? Prepare insert statement (f?r? ICD10_ID - se genereaz? automat cu NEWSEQUENTIALID)
$insertQuery = @"
INSERT INTO ICD10_Codes 
    (Code, FullCode, Category, ShortDescription, LongDescription, IsLeafNode, IsCommon, SearchTerms, DataCreare)
VALUES 
    (@Code, @FullCode, @Category, @ShortDescription, @LongDescription, @IsLeafNode, @IsCommon, @SearchTerms, GETDATE())
"@

$command = $connection.CreateCommand()
$command.CommandText = $insertQuery

# Add parameters
$command.Parameters.Add("@Code", [System.Data.SqlDbType]::NVarChar, 10) | Out-Null
$command.Parameters.Add("@FullCode", [System.Data.SqlDbType]::NVarChar, 20) | Out-Null
$command.Parameters.Add("@Category", [System.Data.SqlDbType]::NVarChar, 50) | Out-Null
$command.Parameters.Add("@ShortDescription", [System.Data.SqlDbType]::NVarChar, 200) | Out-Null
$command.Parameters.Add("@LongDescription", [System.Data.SqlDbType]::NVarChar, 1000) | Out-Null
$command.Parameters.Add("@IsLeafNode", [System.Data.SqlDbType]::Bit) | Out-Null
$command.Parameters.Add("@IsCommon", [System.Data.SqlDbType]::Bit) | Out-Null
$command.Parameters.Add("@SearchTerms", [System.Data.SqlDbType]::NVarChar, -1) | Out-Null

# Import data
Write-Host "?? Importing data..." -ForegroundColor Cyan
$successCount = 0
$errorCount = 0
$batchCount = 0

foreach ($row in $csvData) {
    try {
        $code = $row.$codeColumn
        $description = $row.$descColumn
        
        # Skip empty rows
        if ([string]::IsNullOrWhiteSpace($code)) {
            continue
        }
        
        # Determine category from code prefix
        $category = switch -Regex ($code) {
            '^A|^B' { 'Infectioase' }
            '^C|^D[0-4]' { 'Neoplasme' }
            '^D[5-8]' { 'Sange' }
            '^E' { 'Endocrin' }
            '^F' { 'Mental' }
            '^G' { 'Nervos' }
            '^H[0-5]' { 'Ochi' }
            '^H[6-9]' { 'Ureche' }
            '^I' { 'Cardiovascular' }
            '^J' { 'Respirator' }
            '^K' { 'Digestiv' }
            '^L' { 'Piele' }
            '^M' { 'Musculo-scheletic' }
            '^N' { 'Genito-urinar' }
            '^O' { 'Obstetric' }
            '^P' { 'Perinatal' }
            '^Q' { 'Congenital' }
            '^R' { 'Simptome' }
            '^S|^T' { 'Traumatisme' }
            '^V|^W|^X|^Y' { 'Cauze externe' }
            '^Z' { 'Alte cauze' }
            default { 'Altele' }
        }
        
        # Determine if leaf node (has decimal)
        $isLeaf = $code -match '\.'
        
        # Set parameters
        $command.Parameters["@Code"].Value = $code.Trim()
        $command.Parameters["@FullCode"].Value = $code.Trim()
        $command.Parameters["@Category"].Value = $category
        $command.Parameters["@ShortDescription"].Value = $description.Substring(0, [Math]::Min(200, $description.Length))
        $command.Parameters["@LongDescription"].Value = $description
        $command.Parameters["@IsLeafNode"].Value = $isLeaf
        $command.Parameters["@IsCommon"].Value = $false
        $command.Parameters["@SearchTerms"].Value = $description.ToLower()
        
        # Execute
        $command.ExecuteNonQuery() | Out-Null
        $successCount++
        
        if ($successCount % $BatchSize -eq 0) {
            Write-Host "   ? Imported $successCount codes..." -ForegroundColor Green
        }
    }
    catch {
        $errorCount++
        if ($Verbose) {
            Write-Host "   ??  Error importing $code : $($_.Exception.Message)" -ForegroundColor Yellow
        }
    }
}

# Close connection
$connection.Close()

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   IMPORT SUMMARY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "? Success: $successCount codes" -ForegroundColor Green
Write-Host "? Errors: $errorCount codes" -ForegroundColor Red
Write-Host ""

if ($successCount -gt 0) {
    Write-Host "?? Import completed successfully!" -ForegroundColor Green
    
    # Test query
    Write-Host ""
    Write-Host "?? Testing database..." -ForegroundColor Cyan
    
    $testQuery = "SELECT COUNT(*) as TotalCodes, COUNT(DISTINCT Category) as TotalCategories FROM ICD10_Codes"
    $results = Invoke-Sqlcmd -ServerInstance $ServerInstance -Database $Database -Query $testQuery
    
    Write-Host "   Total codes: $($results.TotalCodes)" -ForegroundColor White
    Write-Host "   Total categories: $($results.TotalCategories)" -ForegroundColor White
}

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
