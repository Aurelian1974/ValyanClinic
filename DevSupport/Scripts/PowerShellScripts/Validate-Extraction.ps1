# ========================================
# Script pentru Validarea Extractiei Complete
# ValyanClinic - Validation Script
# ========================================

$outputPath = "..\..\Database"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "VALIDARE EXTRACTIE COMPLETA" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "`n[1/4] Verificare directoare..." -ForegroundColor Yellow

$directories = @("TableStructure", "StoredProcedures", "Functions", "Views")
foreach ($dir in $directories) {
    $fullPath = "$outputPath\$dir"
    if (Test-Path $fullPath) {
        $fileCount = (Get-ChildItem $fullPath -Filter "*.sql" | Measure-Object).Count
        Write-Host "  ? $dir : $fileCount fisiere" -ForegroundColor Green
    } else {
        Write-Host "  ? $dir : LIPSESTE" -ForegroundColor Red
    }
}

Write-Host "`n[2/4] Verificare fisiere principale..." -ForegroundColor Yellow

$mainFiles = @("README.md")
foreach ($file in $mainFiles) {
    $fullPath = "$outputPath\$file"
    if (Test-Path $fullPath) {
        $size = (Get-Item $fullPath).Length
        Write-Host "  ? $file : EXISTS ($size bytes)" -ForegroundColor Green
    } else {
        Write-Host "  ? $file : LIPSESTE" -ForegroundColor Red
    }
}

Write-Host "`n[3/4] Lista tabele extrase..." -ForegroundColor Yellow

$expectedTables = @(
    'Audit_Persoana', 'Audit_Utilizator', 'Audit_UtilizatorDetaliat', 'ComenziTeste',
    'Consultatii', 'Departamente', 'DepartamenteIerarhie', 'Diagnostice',
    'DispozitiveMedicale', 'FormulareConsimtamant', 'IstoricMedical', 'Judet',
    'Localitate', 'MaterialeSanitare', 'Medicament', 'MedicamenteNoi',
    'Pacienti', 'Partener', 'Personal', 'PersonalMedical',
    'PersonalMedical_Backup_Migration', 'Prescriptii', 'Programari',
    'RezultateTeste', 'RoluriSistem', 'SemneVitale', 'TipLocalitate',
    'TipuriTeste', 'TriajPacienti'
)

$missingTables = @()
$presentTables = @()

foreach ($table in $expectedTables) {
    $filePath = "$outputPath\TableStructure\$table`_Complete.sql"
    if (Test-Path $filePath) {
        $presentTables += $table
        Write-Host "  ? $table" -ForegroundColor Green
    } else {
        $missingTables += $table
        Write-Host "  ? $table : LIPSESTE" -ForegroundColor Red
    }
}

Write-Host "`n[4/4] Raport final..." -ForegroundColor Yellow

Write-Host "`nSTATISTICI EXTRACTIE:" -ForegroundColor Cyan
Write-Host "  Total tabele asteptate: $($expectedTables.Count)" -ForegroundColor White
Write-Host "  Tabele extrase: $($presentTables.Count)" -ForegroundColor Green
Write-Host "  Tabele lipsa: $($missingTables.Count)" -ForegroundColor $(if($missingTables.Count -gt 0){'Red'}else{'Green'})

if ($missingTables.Count -gt 0) {
    Write-Host "`nTabele care lipsesc:" -ForegroundColor Red
    foreach ($table in $missingTables) {
        Write-Host "  - $table" -ForegroundColor Red
    }
}

$tableFiles = Get-ChildItem "$outputPath\TableStructure" -Filter "*.sql" -ErrorAction SilentlyContinue
$spFiles = Get-ChildItem "$outputPath\StoredProcedures" -Filter "*.sql" -ErrorAction SilentlyContinue
$funcFiles = Get-ChildItem "$outputPath\Functions" -Filter "*.sql" -ErrorAction SilentlyContinue
$viewFiles = Get-ChildItem "$outputPath\Views" -Filter "*.sql" -ErrorAction SilentlyContinue

Write-Host "`nFISIERE GENERATE:" -ForegroundColor Cyan
Write-Host "  TableStructure: $($tableFiles.Count) fisiere" -ForegroundColor White
Write-Host "  StoredProcedures: $($spFiles.Count) fisiere" -ForegroundColor White
Write-Host "  Functions: $($funcFiles.Count) fisiere" -ForegroundColor White
Write-Host "  Views: $($viewFiles.Count) fisiere" -ForegroundColor White

$successRate = [math]::Round(($presentTables.Count / $expectedTables.Count) * 100, 2)
Write-Host "`nRATA DE SUCCES: $successRate%" -ForegroundColor $(if($successRate -eq 100){'Green'}elseif($successRate -ge 90){'Yellow'}else{'Red'})

if ($successRate -eq 100) {
    Write-Host "`n? EXTRACTIE COMPLETA CU SUCCES!" -ForegroundColor Green
    Write-Host "Toate tabelele au fost extrase corect." -ForegroundColor Green
} elseif ($successRate -ge 90) {
    Write-Host "`n? Extractie aproape completa" -ForegroundColor Yellow
    Write-Host "Verifica tabelele care lipsesc." -ForegroundColor Yellow
} else {
    Write-Host "`n? Extractie incompleta" -ForegroundColor Red
    Write-Host "Multe tabele lipsesc. Ruleaza din nou scriptul de extractie." -ForegroundColor Red
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "VALIDARE COMPLETA" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan