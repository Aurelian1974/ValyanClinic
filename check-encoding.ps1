# Script pentru verificarea ?i corectarea encoding-ului fi?ierelor
# PowerShell script pentru diacriticele române?ti

Write-Host "=== VERIFICARE ENCODING FI?IERE VALYANMED ===" -ForegroundColor Green

# Lista fi?ierelor de verificat
$files = @(
    "ValyanClinic\Components\Pages\Login.razor",
    "ValyanClinic\Components\App.razor", 
    "ValyanClinic\wwwroot\css\pages\login.css",
    "ValyanClinic\wwwroot\app.css"
)

foreach ($file in $files) {
    if (Test-Path $file) {
        Write-Host "Verificare: $file" -ForegroundColor Yellow
        
        # Cite?te fi?ierul ca bytes
        $bytes = [System.IO.File]::ReadAllBytes($file)
        
        # Verific? BOM UTF-8 (EF BB BF)
        if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
            Write-Host "  ? UTF-8 cu BOM" -ForegroundColor Green
        }
        # Verific? dac? pare s? fie UTF-8 f?r? BOM
        else {
            $content = [System.IO.File]::ReadAllText($file, [System.Text.Encoding]::UTF8)
            if ($content.Contains("?") -or $content.Contains("â") -or $content.Contains("î") -or $content.Contains("?") -or $content.Contains("?")) {
                Write-Host "  ? UTF-8 f?r? BOM (con?ine diacritice)" -ForegroundColor Orange
                Write-Host "    Reconvertesc la UTF-8 cu BOM..." -ForegroundColor Orange
                
                # Salveaz? cu UTF-8 BOM
                [System.IO.File]::WriteAllText($file, $content, [System.Text.Encoding]::UTF8)
                Write-Host "  ? Convertit la UTF-8 cu BOM" -ForegroundColor Green
            } else {
                Write-Host "  - ASCII/UTF-8 f?r? diacritice" -ForegroundColor Gray
            }
        }
        
        # Verific? con?inutul pentru caractere problematice
        $content = [System.IO.File]::ReadAllText($file, [System.Text.Encoding]::UTF8)
        if ($content.Contains("?") -and ($content.Contains("Bine a") -or $content.Contains("Introdu"))) {
            Write-Host "  ? PROBLEM?: Con?ine semne de întrebare în loc de diacritice!" -ForegroundColor Red
        }
        
        Write-Host ""
    } else {
        Write-Host "LIPS?: $file" -ForegroundColor Red
    }
}

Write-Host "=== INSTRUC?IUNI POST-VERIFICARE ===" -ForegroundColor Cyan
Write-Host "1. Opre?te aplica?ia (Ctrl+C în terminal)" -ForegroundColor White
Write-Host "2. Ruleaz?: dotnet clean && dotnet build" -ForegroundColor White  
Write-Host "3. Ruleaz?: dotnet run" -ForegroundColor White
Write-Host "4. Testeaz?: navigheaz? la /test-diacritice" -ForegroundColor White
Write-Host "5. Verific? în browser c? diacriticele se afi?eaz? corect" -ForegroundColor White
Write-Host ""
Write-Host "Dac? înc? nu func?ioneaz?, verific?:" -ForegroundColor Yellow
Write-Host "- Cache browser (Ctrl+F5)" -ForegroundColor White
Write-Host "- Developer Tools ? Network ? vezi Content-Type headers" -ForegroundColor White
Write-Host "- Developer Tools ? Console ? ruleaz?: document.characterSet" -ForegroundColor White