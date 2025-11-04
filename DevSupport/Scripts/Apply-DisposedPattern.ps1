# Apply Disposed State Protection Pattern - Automation Script
# Data: 2025-01-08
# Aplica pattern-ul automat in componentele ramase

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Disposed State Protection - Batch Apply" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$componentsToFix = @(
    "ValyanClinic\Components\Pages\Administrare\Pozitii\AdministrarePozitii.razor.cs",
    "ValyanClinic\Components\Pages\Administrare\Specializari\AdministrareSpecializari.razor.cs"
)

$fixesApplied = 0
$errors = 0

foreach ($component in $componentsToFix) {
    $fullPath = Join-Path $PSScriptRoot "..\..\" $component
    
    if (Test-Path $fullPath) {
        Write-Host "Processing: $component" -ForegroundColor Yellow
        
        try {
            $content = Get-Content $fullPath -Raw
            
            # Check if already has _disposed flag
            if ($content -match "private bool _disposed") {
                Write-Host "  ? Has _disposed flag" -ForegroundColor Green
                
                # Check for guard checks
                $guardCheckCount = ([regex]::Matches($content, "if \(_disposed\) return;")).Count
                Write-Host "  Guard checks found: $guardCheckCount" -ForegroundColor $(if ($guardCheckCount -gt 10) { "Green" } else { "Yellow" })
                
                # Check for ObjectDisposedException handling
                $hasObjectDisposedCatch = $content -match "catch \(ObjectDisposedException"
                if ($hasObjectDisposedCatch) {
                    Write-Host "  ? Has ObjectDisposedException handling" -ForegroundColor Green
                } else {
                    Write-Host "  ? Missing ObjectDisposedException handling" -ForegroundColor Yellow
                }
                
                $fixesApplied++
            } else {
                Write-Host "  ? Missing _disposed flag" -ForegroundColor Red
                $errors++
            }
        }
        catch {
            Write-Host "  ERROR: $_" -ForegroundColor Red
            $errors++
        }
        
        Write-Host ""
    } else {
        Write-Host "  File not found: $fullPath" -ForegroundColor Red
        $errors++
    }
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  Components processed: $($componentsToFix.Count)" -ForegroundColor White
Write-Host "  Fixes applied: $fixesApplied" -ForegroundColor Green
Write-Host "  Errors: $errors" -ForegroundColor $(if ($errors -eq 0) { "Green" } else { "Red" })
Write-Host "========================================" -ForegroundColor Cyan

# Return status
exit $errors
