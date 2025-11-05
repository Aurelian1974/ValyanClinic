# ========================================
# Script Automat: Migrare Alert/Confirm ? NotificationService
# ========================================
# Aplic? pattern-ul de migrare în toate componentele r?mase
# Data: 2025-01-20

Write-Host "================================" -ForegroundColor Cyan
Write-Host "Migrare Alert/Confirm ? Toast" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Array cu fi?ierele de migrat
$filesToMigrate = @(
    "ValyanClinic\Components\Pages\Administrare\Personal\AdministrarePersonal.razor.cs",
    "ValyanClinic\Components\Pages\Administrare\Pozitii\AdministrarePozitii.razor.cs",
    "ValyanClinic\Components\Pages\Administrare\Departamente\AdministrareDepartamente.razor.cs",
    "ValyanClinic\Components\Pages\Administrare\Specializari\AdministrareSpecializari.razor.cs"
)

$totalFiles = $filesToMigrate.Count
$currentFile = 0

foreach ($file in $filesToMigrate) {
  $currentFile++
    $fileName = Split-Path $file -Leaf
    
 Write-Host "[$currentFile/$totalFiles] Processing: $fileName" -ForegroundColor Yellow
    
    if (Test-Path $file) {
        $content = Get-Content $file -Raw
      
    # Check if already migrated
      if ($content -match "INotificationService") {
            Write-Host "  ? Already migrated - SKIP" -ForegroundColor Green
          continue
 }
        
        # Count alert() and confirm()
        $alertCount = ([regex]::Matches($content, 'JSRuntime\.InvokeVoidAsync\("alert"')).Count
        $confirmCount = ([regex]::Matches($content, 'JSRuntime\.InvokeAsync<bool>\("confirm"')).Count
        
        Write-Host "  Found: $alertCount alerts, $confirmCount confirms" -ForegroundColor Cyan
        
        if ($alertCount -eq 0 -and $confirmCount -eq 0) {
            Write-Host "  ? No alerts/confirms found - SKIP" -ForegroundColor Green
       continue
        }
   
        # 1. Add using statement
     if ($content -notmatch "using ValyanClinic\.Services;") {
     $content = $content -replace "(using Microsoft\.Extensions\.Logging;)", "`$1`nusing ValyanClinic.Services;"
         Write-Host "  ? Added using ValyanClinic.Services" -ForegroundColor Green
        }
        
   # 2. Add [Inject] property
 if ($content -notmatch "INotificationService") {
       $content = $content -replace "(\[Inject\] private ILogger<.+?> Logger)", "`$1`n[Inject] private INotificationService NotificationService { get; set; } = default!;"
            Write-Host "  ? Added INotificationService injection" -ForegroundColor Green
      }
   
        # 3. Replace ShowToast calls with NotificationService
 # Pattern: await ShowToast("Title", "Message", "e-toast-success")
        $content = $content -replace 'await ShowToast\("Succes", "(.+?)", "e-toast-success"\)', 'await NotificationService.ShowSuccessAsync("$1")'
        $content = $content -replace 'await ShowToast\("Eroare", "(.+?)", "e-toast-danger"\)', 'await NotificationService.ShowErrorAsync("$1")'
   $content = $content -replace 'await ShowToast\("Atentie", "(.+?)", "e-toast-warning"\)', 'await NotificationService.ShowWarningAsync("$1")'
        $content = $content -replace 'await ShowToast\("Informatie", "(.+?)", "e-toast-info"\)', 'await NotificationService.ShowInfoAsync("$1")'
        
Write-Host "  ? Replaced ShowToast calls" -ForegroundColor Green
        
        # Save modified content
        Set-Content $file -Value $content -NoNewline
        Write-Host "  ? File saved successfully" -ForegroundColor Green
        Write-Host ""
      
    } else {
        Write-Host "  ? File not found: $file" -ForegroundColor Red
    }
}

Write-Host "================================" -ForegroundColor Cyan
Write-Host "Migration Summary" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Files processed: $totalFiles" -ForegroundColor Green
Write-Host ""
Write-Host "NEXT STEPS:" -ForegroundColor Yellow
Write-Host "1. Run: dotnet build" -ForegroundColor White
Write-Host "2. Fix any compilation errors" -ForegroundColor White
Write-Host "3. Test in browser" -ForegroundColor White
Write-Host ""
Write-Host "? Script completed successfully!" -ForegroundColor Green
