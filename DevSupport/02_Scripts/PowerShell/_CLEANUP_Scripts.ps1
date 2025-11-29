# Script pentru curatarea scripturilor PowerShell neesen?iale
# P?streaz? doar scripturile necesare pentru opera?iuni cu baza de date

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "CLEANUP SCRIPTURI POWERSHELL" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Lista scripturilor ESEN?IALE (NU SE ?TERG)
$essentialScripts = @(
    "Test-Connection.ps1",
    "Query-ValyanMedDatabase.ps1",
    "Run-DatabaseExtraction.ps1",
    "Extract-AllTables.ps1",
    "Extract-DatabaseSchema.ps1",
    "Extract-Complete.ps1",
    "Compare-SchemaWithCode.ps1",
    "Validate-DatabaseSchema.ps1",
    "README.md",
    "_CLEANUP_Scripts.ps1"
)

# Ob?ine toate fi?ierele din director
$allFiles = Get-ChildItem -Path "." -File

Write-Host "Fi?iere g?site: $($allFiles.Count)" -ForegroundColor Yellow
Write-Host "Fi?iere esen?iale (p?strate): $($essentialScripts.Count)" -ForegroundColor Green
Write-Host ""

# Fi?iere de ?ters
$filesToDelete = $allFiles | Where-Object { $essentialScripts -notcontains $_.Name }

Write-Host "Fi?iere de ?ters: $($filesToDelete.Count)" -ForegroundColor Red
Write-Host ""

if ($filesToDelete.Count -eq 0) {
    Write-Host "Nu exist? fi?iere de ?ters!" -ForegroundColor Green
    exit 0
}

# Afi?eaz? lista fi?ierelor de ?ters
Write-Host "Lista fi?iere care vor fi ?terse:" -ForegroundColor Yellow
foreach ($file in $filesToDelete) {
    Write-Host "  - $($file.Name)" -ForegroundColor Gray
}
Write-Host ""

# Confirm? ?tergerea
$confirm = Read-Host "Continui cu ?tergerea? (Y/N)"

if ($confirm -ne "Y" -and $confirm -ne "y") {
    Write-Host "Opera?ie anulat?." -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "?tergere în curs..." -ForegroundColor Cyan

# ?terge fi?ierele
$deletedCount = 0
$errorCount = 0

foreach ($file in $filesToDelete) {
    try {
        Remove-Item $file.FullName -Force
        Write-Host "? ?ters: $($file.Name)" -ForegroundColor Green
        $deletedCount++
    }
    catch {
        Write-Host "? Eroare la ?tergere: $($file.Name) - $_" -ForegroundColor Red
        $errorCount++
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "REZULTATE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Fi?iere ?terse: $deletedCount" -ForegroundColor Green
Write-Host "Erori: $errorCount" -ForegroundColor $(if ($errorCount -eq 0) { "Green" } else { "Red" })
Write-Host ""

# Afi?eaz? fi?ierele r?mase
Write-Host "Fi?iere r?mase în director:" -ForegroundColor Cyan
$remainingFiles = Get-ChildItem -Path "." -File
foreach ($file in $remainingFiles) {
    Write-Host "  ? $($file.Name)" -ForegroundColor Green
}

Write-Host ""
Write-Host "Cur??are complet?!" -ForegroundColor Green
