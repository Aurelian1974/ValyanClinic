# DevSupport Simple Reorganization
# Runs in smaller batches to avoid blocking

Write-Host "?? Starting Simple Reorganization..." -ForegroundColor Cyan

$root = "D:\Lucru\CMS\DevSupport"
Set-Location $root

# === STEP 1: Verify new folders exist ===
Write-Host "`n? Step 1: Verifying folder structure..." -ForegroundColor Green
$folders = @("01_Database", "02_Scripts", "03_Documentation", "04_Tools", "05_Resources")
foreach ($folder in $folders) {
    if (Test-Path $folder) {
        Write-Host "  ? $folder exists" -ForegroundColor Gray
    } else {
        Write-Host "  ? $folder missing - run main script first!" -ForegroundColor Red
    }
}

# === STEP 2: Check what was already moved ===
Write-Host "`n? Step 2: Checking moved files..." -ForegroundColor Green

$movedTools = Test-Path "04_Tools\PasswordFix\FixPasswordTool.html"
$movedPDF = Test-Path "05_Resources\PDFs\SCRISOARE-MEDICALA-2024.pdf"
$movedRefactoring = Test-Path "03_Documentation\05_Refactoring\ConsultatieModal\SESSION_COMPLETE_FINAL.md"

if ($movedTools) { Write-Host "  ? Tools moved" -ForegroundColor Gray }
if ($movedPDF) { Write-Host "  ? PDF moved" -ForegroundColor Gray }
if ($movedRefactoring) { Write-Host "  ? Refactoring docs moved" -ForegroundColor Gray }

# === STEP 3: Status of old folders ===
Write-Host "`n? Step 3: Old folders status..." -ForegroundColor Green

if (Test-Path "Database") {
    $dbFiles = (Get-ChildItem "Database" -Recurse -File).Count
    Write-Host "  ?? Database/ - $dbFiles files (NEEDS MANUAL COPY)" -ForegroundColor Yellow
}

if (Test-Path "Scripts") {
    $scriptFiles = (Get-ChildItem "Scripts" -Recurse -File).Count
    Write-Host "  ?? Scripts/ - $scriptFiles files (NEEDS MANUAL COPY)" -ForegroundColor Yellow
}

if (Test-Path "Documentation") {
    $docFiles = (Get-ChildItem "Documentation" -Recurse -File).Count
    Write-Host "  ?? Documentation/ - $docFiles files (NEEDS MANUAL COPY)" -ForegroundColor Yellow
}

if (Test-Path "Refactoring") {
    $refFiles = (Get-ChildItem "Refactoring" -File).Count
    Write-Host "  ?? Refactoring/ - $refFiles files (already copied)" -ForegroundColor Gray
}

# === RECOMMENDATION ===
Write-Host "`n?? RECOMMENDATION:" -ForegroundColor Cyan
Write-Host "==================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Due to large number of files, MANUAL copy is safer:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Open File Explorer" -ForegroundColor White
Write-Host "2. Navigate to: $root" -ForegroundColor White
Write-Host "3. COPY (Ctrl+C) folders:" -ForegroundColor White
Write-Host "   - Database/* ? 01_Database/" -ForegroundColor Gray
Write-Host "   - Scripts/* ? 02_Scripts/" -ForegroundColor Gray
Write-Host "   - Documentation/* ? 03_Documentation/" -ForegroundColor Gray
Write-Host ""
Write-Host "4. After verification, DELETE old folders" -ForegroundColor White
Write-Host ""
Write-Host "? Current structure is already GOOD for documentation!" -ForegroundColor Green
Write-Host "   03_Documentation/05_Refactoring/ConsultatieModal/ ?" -ForegroundColor Green
Write-Host ""
