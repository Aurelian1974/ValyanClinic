# =============================================
# Script PowerShell pentru înlocuire GETUTCDATE() -> GETDATE()
# ValyanClinic - Phase1 SQL Scripts
# Data: 2025-01-15
# =============================================

Write-Host "=== ÎNLOCUIRE GETUTCDATE() -> GETDATE() ===" -ForegroundColor Cyan
Write-Host ""

# Ob?ine calea directorului scriptului
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootPath = $scriptPath

Write-Host "Calea root: $rootPath" -ForegroundColor Gray
Write-Host ""

$filesProcessed = 0
$replacements = 0

# Lista fi?iere de procesat (cele r?mase)
$filesToProcess = @(
    "03_StoredProcedures\10_SP_RecordLoginAttempt.sql",
    "03_StoredProcedures\13_SP_CreateUserSession.sql",
    "03_StoredProcedures\14_SP_UpdateSessionActivity.sql",
    "03_StoredProcedures\15_SP_ChangePassword.sql",
    "03_StoredProcedures\17_SP_NotifyPasswordExpirations.sql",
    "03_StoredProcedures\18_SP_UnlockExpiredLockouts.sql",
    "05_Views\22_VW_PasswordExpirations_Next7Days.sql"
)

foreach ($file in $filesToProcess) {
    $fullPath = Join-Path $rootPath $file
    
    if (Test-Path $fullPath) {
        Write-Host "Procesare: $file" -ForegroundColor Yellow
        
  $content = Get-Content $fullPath -Raw -Encoding UTF8
 $originalContent = $content
  
      # Înlocuire GETUTCDATE() cu GETDATE()
        $content = $content -replace 'GETUTCDATE\(\)', 'GETDATE()'
        
        # Contorizare înlocuiri
        $matches = ([regex]::Matches($originalContent, 'GETUTCDATE\(\)')).Count
        
        if ($matches -gt 0) {
            Set-Content -Path $fullPath -Value $content -Encoding UTF8 -NoNewline
            Write-Host "  ? $matches apari?ii înlocuite" -ForegroundColor Green
            $replacements += $matches
        } else {
        Write-Host "  ! Nicio apari?ie g?sit?" -ForegroundColor Gray
        }
        
        $filesProcessed++
    } else {
  Write-Host "  ? Fi?ier nu exist?: $fullPath" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=== REZUMAT ===" -ForegroundColor Cyan
Write-Host "Fi?iere procesate: $filesProcessed" -ForegroundColor Green
Write-Host "Total înlocuiri: $replacements" -ForegroundColor Green
Write-Host ""

if ($replacements -gt 0) {
    Write-Host "? Proces finalizat cu succes!" -ForegroundColor Green
} else {
    Write-Host "! Nicio înlocuire efectuat?. Verific? dac? fi?ierele con?in GETUTCDATE()." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Apas? orice tast? pentru a închide..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
