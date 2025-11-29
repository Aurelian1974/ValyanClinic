# ========================================
# Script Master pentru Executia Extractiei de Schema
# ValyanClinic - Master Database Extraction Runner
# ========================================

$ErrorActionPreference = "Continue"

Write-Host "========================================" -ForegroundColor Magenta
Write-Host "VALYAN CLINIC - DATABASE EXTRACTION" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta

Write-Host "`nAcest script va executa extractia completa a schemei de baza de date." -ForegroundColor White
Write-Host "Vor fi extrase: tabele, stored procedures, functions si views." -ForegroundColor White
Write-Host ""

# ========================================
# VERIFICARI PRELIMINARE
# ========================================
Write-Host "[VERIFICARI] Verific prerequisitele..." -ForegroundColor Yellow

# Verifica ca appsettings.json exista
$configPath = "..\..\..\ValyanClinic\appsettings.json"
if (-not (Test-Path $configPath)) {
    Write-Host "? EROARE: Nu gasesc fisierul appsettings.json la: $configPath" -ForegroundColor Red
    Write-Host "  Verifica ca esti in directorul corect (DevSupport\Scripts\PowerShellScripts)" -ForegroundColor Yellow
    exit 1
}

# Verifica ca avem SQL Server client
try {
    $null = [System.Data.SqlClient.SqlConnection]::new()
    Write-Host "? SQL Server Client: DISPONIBIL" -ForegroundColor Green
}
catch {
    Write-Host "? EROARE: SQL Server Client nu este disponibil" -ForegroundColor Red
    Write-Host "  Instaleaza SQL Server tools sau verifica .NET dependencies" -ForegroundColor Yellow
    exit 1
}

# Verifica directorul de output
$outputPath = "..\..\..\Database"
if (-not (Test-Path $outputPath)) {
    Write-Host "? Director Database: VA FI CREAT" -ForegroundColor Yellow
} else {
    Write-Host "? Director Database: EXISTA" -ForegroundColor Green
}

Write-Host ""

# ========================================
# MENIU DE OPTIUNI
# ========================================
Write-Host "Alege tipul de operatie:" -ForegroundColor Cyan
Write-Host "1. Extractie COMPLETA (toate tabelele si SP-urile)" -ForegroundColor White
Write-Host "2. Extractie STANDARD (cu functii avansate)" -ForegroundColor White
Write-Host "3. Extractie SPECIFICA (doar tabele relevante pentru app)" -ForegroundColor White
Write-Host "4. COMPARARE schema DB vs Entity Models" -ForegroundColor White
Write-Host "5. Doar VERIFICARE schema (fara extragere)" -ForegroundColor White
Write-Host "6. IESIRE" -ForegroundColor White
Write-Host ""

do {
    $choice = Read-Host "Selecteaza optiunea (1-6)"
    
    switch ($choice) {
        "1" {
            Write-Host "`n[OPTIUNE 1] Extractie completa selectata" -ForegroundColor Green
            Write-Host "Se va rula: Extract-AllTables.ps1" -ForegroundColor Gray
            Write-Host "Extrage TOATE tabelele (29) si toate SP-urile (36) cu constraint-uri complete" -ForegroundColor Gray
            Write-Host ""
            
            $confirm = Read-Host "Continui? (Y/N)"
            if ($confirm -eq "Y" -or $confirm -eq "y") {
                Write-Host "`nRulez extractia completa..." -ForegroundColor Cyan
                try {
                    & ".\Extract-AllTables.ps1" -ConfigPath $configPath -OutputPath $outputPath
                    Write-Host "`n? Extractie completa finalizata cu succes!" -ForegroundColor Green
                }
                catch {
                    Write-Host "`n? Eroare in timpul extractiei: $_" -ForegroundColor Red
                }
            }
            break
        }
        
        "2" {
            Write-Host "`n[OPTIUNE 2] Extractie standard selectata" -ForegroundColor Green
            Write-Host "Se va rula: Extract-DatabaseSchema.ps1 (versiunea corrigata)" -ForegroundColor Gray
            Write-Host ""
            
            $confirm = Read-Host "Continui? (Y/N)"
            if ($confirm -eq "Y" -or $confirm -eq "y") {
                Write-Host "`nRulez extractia standard..." -ForegroundColor Cyan
                try {
                    & ".\Extract-DatabaseSchema.ps1" -ConfigPath $configPath -OutputPath $outputPath
                    Write-Host "`n? Extractie standard finalizata cu succes!" -ForegroundColor Green
                }
                catch {
                    Write-Host "`n? Eroare in timpul extractiei: $_" -ForegroundColor Red
                }
            }
            break
        }
        
        "3" {
            Write-Host "`n[OPTIUNE 3] Extractie specifica selectata" -ForegroundColor Green
            Write-Host "Se va rula: Extract-Complete.ps1" -ForegroundColor Gray
            Write-Host "Tabele tinta: Personal, PersonalMedical, Judete, Localitati, Departamente" -ForegroundColor Gray
            Write-Host ""
            
            $confirm = Read-Host "Continui? (Y/N)"
            if ($confirm -eq "Y" -or $confirm -eq "y") {
                Write-Host "`nRulez extractia specifica..." -ForegroundColor Cyan
                try {
                    & ".\Extract-Complete.ps1" -ConfigPath $configPath -OutputPath $outputPath
                    Write-Host "`n? Extractie specifica finalizata cu succes!" -ForegroundColor Green
                }
                catch {
                    Write-Host "`n? Eroare in timpul extractiei: $_" -ForegroundColor Red
                }
            }
            break
        }
        
        "4" {
            Write-Host "`n[OPTIUNE 4] Comparare schema vs cod selectata" -ForegroundColor Green
            Write-Host "Se va rula: Compare-SchemaWithCode.ps1" -ForegroundColor Gray
            Write-Host "Compara structura din DB cu entity models din cod" -ForegroundColor Gray
            Write-Host ""
            
            $confirm = Read-Host "Continui? (Y/N)"
            if ($confirm -eq "Y" -or $confirm -eq "y") {
                Write-Host "`nRulez compararea..." -ForegroundColor Cyan
                try {
                    & ".\Compare-SchemaWithCode.ps1" -ConfigPath $configPath
                    Write-Host "`n? Comparare finalizata cu succes!" -ForegroundColor Green
                }
                catch {
                    Write-Host "`n? Eroare in timpul compararii: $_" -ForegroundColor Red
                }
            }
            break
        }
        
        "5" {
            Write-Host "`n[OPTIUNE 5] Verificare schema selectata" -ForegroundColor Green
            Write-Host "Se va rula: Validate-DatabaseSchema.ps1" -ForegroundColor Gray
            Write-Host ""
            
            $confirm = Read-Host "Continui? (Y/N)"
            if ($confirm -eq "Y" -or $confirm -eq "y") {
                Write-Host "`nRulez verificarea schemei..." -ForegroundColor Cyan
                try {
                    & ".\Validate-DatabaseSchema.ps1" -ConfigPath $configPath
                    Write-Host "`n? Verificare schema finalizata cu succes!" -ForegroundColor Green
                }
                catch {
                    Write-Host "`n? Eroare in timpul verificarii: $_" -ForegroundColor Red
                }
            }
            break
        }
        
        "6" {
            Write-Host "`nIesire..." -ForegroundColor Yellow
            exit 0
        }
        
        default {
            Write-Host "Optiune invalida. Alege 1, 2, 3, 4, 5 sau 6." -ForegroundColor Red
        }
    }
    
    if ($choice -in @("1", "2", "3", "4", "5")) {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Magenta
        Write-Host "OPERATIE COMPLETA" -ForegroundColor Magenta
        Write-Host "========================================" -ForegroundColor Magenta
        
        # Afiseaza ce s-a generat
        if (Test-Path $outputPath) {
            Write-Host "`nFisiere generate in $outputPath`:" -ForegroundColor Cyan
            
            if (Test-Path "$outputPath\TableStructure") {
                $tableFiles = Get-ChildItem "$outputPath\TableStructure" -Filter "*.sql" | Measure-Object
                Write-Host "  ? TableStructure: $($tableFiles.Count) fisiere" -ForegroundColor Green
            }
            
            if (Test-Path "$outputPath\StoredProcedures") {
                $spFiles = Get-ChildItem "$outputPath\StoredProcedures" -Filter "*.sql" | Measure-Object
                Write-Host "  ? StoredProcedures: $($spFiles.Count) fisiere" -ForegroundColor Green
            }
            
            if (Test-Path "$outputPath\Functions") {
                $funcFiles = Get-ChildItem "$outputPath\Functions" -Filter "*.sql" | Measure-Object
                Write-Host "  ? Functions: $($funcFiles.Count) fisiere" -ForegroundColor Green
            }
            
            if (Test-Path "$outputPath\Views") {
                $viewFiles = Get-ChildItem "$outputPath\Views" -Filter "*.sql" | Measure-Object
                Write-Host "  ? Views: $($viewFiles.Count) fisiere" -ForegroundColor Green
            }
            
            # Verifica daca exista rapoarte
            $reportFiles = Get-ChildItem $outputPath -Filter "*Report*.md" -ErrorAction SilentlyContinue
            if ($reportFiles) {
                Write-Host "  ? Rapoarte: $($reportFiles.Count) fisiere" -ForegroundColor Cyan
                foreach ($report in $reportFiles) {
                    Write-Host "    - $($report.Name)" -ForegroundColor Gray
                }
            }
        }
        
        Write-Host ""
        $runAgain = Read-Host "Vrei sa rulezi alt script? (Y/N)"
        if ($runAgain -ne "Y" -and $runAgain -ne "y") {
            break
        }
        Write-Host ""
        Write-Host "Alege din nou:" -ForegroundColor Cyan
        Write-Host "1. Extractie COMPLETA (RECOMANDAT - toate tabelele)" -ForegroundColor White
        Write-Host "2. Extractie STANDARD" -ForegroundColor White
        Write-Host "3. Extractie SPECIFICA" -ForegroundColor White
        Write-Host "4. COMPARARE schema vs cod" -ForegroundColor White
        Write-Host "5. Doar VERIFICARE schema" -ForegroundColor White
        Write-Host "6. IESIRE" -ForegroundColor White
        Write-Host ""
    }
    
} while ($choice -ne "6")

Write-Host "Script finalizat." -ForegroundColor Green