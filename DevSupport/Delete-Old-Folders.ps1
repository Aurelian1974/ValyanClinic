# Safe Cleanup Script - Sterge Folderele Vechi
# Verifica si sterge doar daca totul e OK

Write-Host "?? DevSupport Cleanup - Safe Deletion" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Presupunem ca esti in folderul DevSupport
$root = Get-Location

Write-Host "?? Working in: $root" -ForegroundColor Yellow
Write-Host ""

# === Verificare: Exista folderele noi? ===
Write-Host "? Checking new folders exist..." -ForegroundColor Green
$newFolders = @("01_Database", "02_Scripts", "03_Documentation", "04_Tools", "05_Resources")
$allExist = $true

foreach ($folder in $newFolders) {
    if (Test-Path $folder) {
        Write-Host "  ? $folder exists" -ForegroundColor Gray
    } else {
        Write-Host "  ? $folder MISSING!" -ForegroundColor Red
        $allExist = $false
    }
}

if (-not $allExist) {
    Write-Host ""
    Write-Host "? ERROR: Not all new folders exist!" -ForegroundColor Red
    Write-Host "Cannot proceed with deletion." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "? All new folders exist!" -ForegroundColor Green
Write-Host ""

# === Verificare: Build functioneaza? ===
Write-Host "?? Testing build..." -ForegroundColor Green
$buildResult = dotnet build DevSupport.csproj --no-restore 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "  ? Build successful" -ForegroundColor Gray
} else {
    Write-Host "  ? Build failed!" -ForegroundColor Red
    Write-Host "Cannot proceed with deletion." -ForegroundColor Red
    exit 1
}

Write-Host ""

# === Lista folderelor vechi ===
$oldFolders = @("Database", "Scripts", "Documentation", "Refactoring")
$foldersToDelete = @()

foreach ($folder in $oldFolders) {
    if (Test-Path $folder) {
        $foldersToDelete += $folder
    }
}

if ($foldersToDelete.Count -eq 0) {
    Write-Host "? No old folders to delete - already clean!" -ForegroundColor Green
    exit 0
}

Write-Host "?? Old folders found:" -ForegroundColor Yellow
foreach ($folder in $foldersToDelete) {
    Write-Host "  • $folder" -ForegroundColor Gray
}
Write-Host ""

# === Confirmare ===
Write-Host "??  READY TO DELETE OLD FOLDERS" -ForegroundColor Yellow
Write-Host "==============================" -ForegroundColor Yellow
Write-Host ""
Write-Host "This will permanently delete:" -ForegroundColor White
foreach ($folder in $foldersToDelete) {
    Write-Host "  • $folder\" -ForegroundColor Red
}
Write-Host ""
Write-Host "New structure in 01_Database, 02_Scripts, etc. will be kept." -ForegroundColor Green
Write-Host ""

$confirmation = Read-Host "Type 'YES' to confirm deletion"

if ($confirmation -ne "YES") {
    Write-Host ""
    Write-Host "? Deletion cancelled." -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "???  Deleting old folders..." -ForegroundColor Red

# === Stergere ===
foreach ($folder in $foldersToDelete) {
    Write-Host "  Deleting $folder..." -ForegroundColor Gray
    try {
        Remove-Item -Path $folder -Recurse -Force -ErrorAction Stop
        Write-Host "  ? $folder deleted" -ForegroundColor Green
    } catch {
        Write-Host "  ? Failed to delete $folder" -ForegroundColor Red
        Write-Host "    Error: $_" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "? CLEANUP COMPLETE!" -ForegroundColor Green
Write-Host "===================" -ForegroundColor Green
Write-Host ""
Write-Host "DevSupport structure is now clean and organized!" -ForegroundColor Green
Write-Host ""
Write-Host "?? Active folders:" -ForegroundColor Cyan
Write-Host "  • 01_Database/" -ForegroundColor Gray
Write-Host "  • 02_Scripts/" -ForegroundColor Gray
Write-Host "  • 03_Documentation/" -ForegroundColor Gray
Write-Host "  • 04_Tools/" -ForegroundColor Gray
Write-Host "  • 05_Resources/" -ForegroundColor Gray
Write-Host ""
Write-Host "Next: Commit changes to Git! ??" -ForegroundColor Cyan
