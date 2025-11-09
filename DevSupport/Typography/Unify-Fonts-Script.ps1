# Typography Unification Script - FONTS ONLY (Colors Preserved)
# Acest script actualizeaz? DOAR font-size ?i font-weight, l?sând culorile neschimbate

$ErrorActionPreference = "Stop"

Write-Host "?? Starting Font Unification - Colors Preserved Edition" -ForegroundColor Cyan
Write-Host "=" * 80

# Define base path
$basePath = "D:\Lucru\CMS\ValyanClinic\Components"

# Find all .razor.css files
$cssFiles = Get-ChildItem -Path $basePath -Recurse -Filter "*.razor.css" | Where-Object {
    $_.Name -notmatch "PersonalViewModal" -and 
    $_.Name -notmatch "PersonalMedicalViewModal" -and
    $_.Name -notmatch "PacientViewModal"
}

Write-Host "?? Found $($cssFiles.Count) CSS files to process" -ForegroundColor Yellow
Write-Host ""

$processedCount = 0
$errorCount = 0

foreach ($file in $cssFiles) {
    try {
        Write-Host "?? Processing: $($file.Name)" -ForegroundColor White
    
        $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8
 $originalContent = $content
        
        # Font-Size Replacements (ONLY font-size, preserve everything else)
        
 # Tab text & buttons (14px)
        $content = $content -replace 'font-size:\s*0\.875rem(\s*;)', 'font-size: var(--modal-tab-text)$1'
  $content = $content -replace 'font-size:\s*0\.95rem(\s*;)', 'font-size: var(--modal-tab-text)$1'
        $content = $content -replace 'font-size:\s*14px(\s*;)', 'font-size: var(--modal-tab-text)$1'
        $content = $content -replace 'font-size:\s*0\.9rem(\s*;)', 'font-size: var(--modal-tab-text)$1'
        
        # Labels (13px)
        $content = $content -replace 'font-size:\s*0\.8rem(\s*;)', 'font-size: var(--modal-label)$1'
        $content = $content -replace 'font-size:\s*0\.85rem(\s*;)', 'font-size: var(--modal-label)$1'
        $content = $content -replace 'font-size:\s*0\.8125rem(\s*;)', 'font-size: var(--modal-label)$1'
        $content = $content -replace 'font-size:\s*13px(\s*;)', 'font-size: var(--modal-label)$1'
        $content = $content -replace 'font-size:\s*0\.75rem(\s*;)', 'font-size: var(--modal-label)$1'
   
        # Values (15px)
        $content = $content -replace 'font-size:\s*0\.9375rem(\s*;)', 'font-size: var(--modal-value)$1'
        $content = $content -replace 'font-size:\s*1rem(\s*;)', 'font-size: var(--modal-value)$1'
  $content = $content -replace 'font-size:\s*15px(\s*;)', 'font-size: var(--modal-value)$1'
        $content = $content -replace 'font-size:\s*16px(\s*;)', 'font-size: var(--modal-value)$1'
        
     # Card titles (16.4px)
        $content = $content -replace 'font-size:\s*1\.025rem(\s*;)', 'font-size: var(--modal-card-title)$1'
    $content = $content -replace 'font-size:\s*1\.1rem(\s*;)', 'font-size: var(--modal-card-title)$1'
        $content = $content -replace 'font-size:\s*1\.0625rem(\s*;)', 'font-size: var(--font-size-lg)$1'
        
 # Card icons (18px)
      $content = $content -replace 'font-size:\s*1\.125rem(\s*;)', 'font-size: var(--modal-card-title-icon)$1'
        $content = $content -replace 'font-size:\s*1\.25rem(\s*;)', 'font-size: var(--modal-card-title-icon)$1'
        $content = $content -replace 'font-size:\s*18px(\s*;)', 'font-size: var(--font-size-xl)$1'
        
     # Modal headers (22px)
        $content = $content -replace 'font-size:\s*1\.5rem(\s*;)', 'font-size: var(--modal-header-title)$1'
      $content = $content -replace 'font-size:\s*1\.375rem(\s*;)', 'font-size: var(--modal-header-title)$1'
        $content = $content -replace 'font-size:\s*22px(\s*;)', 'font-size: var(--modal-header-title)$1'
        
        # Page headers (28px)
        $content = $content -replace 'font-size:\s*2rem(\s*;)', 'font-size: var(--page-header-title)$1'
        $content = $content -replace 'font-size:\s*1\.75rem(\s*;)', 'font-size: var(--page-header-title)$1'
      $content = $content -replace 'font-size:\s*28px(\s*;)', 'font-size: var(--page-header-title)$1'
        $content = $content -replace 'font-size:\s*32px(\s*;)', 'font-size: var(--page-header-title)$1'
 
        # Extra small (11px)
        $content = $content -replace 'font-size:\s*0\.6875rem(\s*;)', 'font-size: var(--font-size-xs)$1'
        $content = $content -replace 'font-size:\s*11px(\s*;)', 'font-size: var(--font-size-xs)$1'
      $content = $content -replace 'font-size:\s*12px(\s*;)', 'font-size: var(--font-size-xs)$1'
        
        # Font-Weight Replacements (ONLY font-weight)
      $content = $content -replace 'font-weight:\s*400(\s*;)', 'font-weight: var(--font-weight-normal)$1'
        $content = $content -replace 'font-weight:\s*500(\s*;)', 'font-weight: var(--font-weight-medium)$1'
        $content = $content -replace 'font-weight:\s*600(\s*;)', 'font-weight: var(--font-weight-semibold)$1'
        $content = $content -replace 'font-weight:\s*700(\s*;)', 'font-weight: var(--font-weight-bold)$1'
   
   # Padding replacements (optional, for consistency)
        $content = $content -replace 'padding:\s*0\.625rem\s+1rem(\s*;)', 'padding: var(--tab-padding)$1'
        $content = $content -replace 'padding:\s*0\.375rem\s+0\.875rem(\s*;)', 'padding: var(--badge-padding)$1'
    
        # Only save if content changed
 if ($content -ne $originalContent) {
   Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
            Write-Host "   ? Updated successfully" -ForegroundColor Green
       $processedCount++
        } else {
            Write-Host "   ??  No changes needed" -ForegroundColor Gray
        }
    }
    catch {
        Write-Host "   ? Error: $($_.Exception.Message)" -ForegroundColor Red
        $errorCount++
  }
}

Write-Host ""
Write-Host "=" * 80
Write-Host "? Font Unification Complete!" -ForegroundColor Cyan
Write-Host "   ?? Processed: $processedCount files" -ForegroundColor Green
Write-Host "   ??  Skipped: $($cssFiles.Count - $processedCount - $errorCount) files (no changes)" -ForegroundColor Gray
if ($errorCount -gt 0) {
    Write-Host "   ? Errors: $errorCount files" -ForegroundColor Red
}
Write-Host ""
Write-Host "?? All colors preserved - ONLY fonts unified!" -ForegroundColor Yellow
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Run: dotnet build" -ForegroundColor White
Write-Host "  2. Test 2-3 modals visually" -ForegroundColor White
Write-Host "  3. Check Implementation-Tracking.md for progress" -ForegroundColor White
Write-Host ""
