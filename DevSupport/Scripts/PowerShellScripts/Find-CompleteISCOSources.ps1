# ========================================
# Script pentru G?sirea ?i Desc?rcarea Datelor ISCO-08 Complete
# Analiz? multiple surse pentru toate ocupa?iile
# ========================================

param(
    [Parameter(Mandatory=$false)]
    [switch]$SearchAlternatives = $true
)

Write-Host "========================================" -ForegroundColor Magenta
Write-Host "C?UTARE SURSE COMPLETE ISCO-08" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta

# URL-uri posibile pentru date ISCO-08
$sources = @(
    @{
        Name = "Data.gov.ro Original"
        Url = "https://data.gov.ro/dataset/695974d3-4be3-4bbe-a56a-bb639ad908e2/resource/cc7db3b5-da8a-4eaa-afcc-514dd373eac6/download/isco-08-lista-alfabetica-ocupatii-2024.xml"
        Type = "XML"
    },
    @{
        Name = "ILO Official ISCO-08"
        Url = "https://www.ilo.org/public/english/bureau/stat/isco/docs/publication08.pdf"
        Type = "PDF"
    },
    @{
        Name = "EUROSTAT ISCO-08"
        Url = "https://ec.europa.eu/eurostat/ramon/nomenclatures/index.cfm?TargetUrl=LST_NOM_DTL&StrNom=ISCO_08"
        Type = "HTML"
    },
    @{
        Name = "INS România - Clasific?ri"
        Url = "https://insse.ro/cms/ro/content/clasificarea-ocupatiilor-din-romania-cor"
        Type = "HTML"
    },
    @{
        Name = "GitHub ISCO datasets"
        Url = "https://api.github.com/search/repositories?q=isco-08+occupations+csv+json"
        Type = "API"
    }
)

Write-Host "`n[1] Verificare surse disponibile..." -ForegroundColor Yellow

foreach ($source in $sources) {
    Write-Host "`n?? Testez: $($source.Name)" -ForegroundColor Cyan
    try {
        $response = Invoke-WebRequest -Uri $source.Url -Method Head -TimeoutSec 10 -ErrorAction Stop
        $size = if ($response.Headers.'Content-Length') { 
            [math]::Round($response.Headers.'Content-Length' / 1KB, 2) 
        } else { "Necunoscut" }
        
        Write-Host "  ? Disponibil - Tip: $($source.Type), M?rime: ${size}KB" -ForegroundColor Green
        
        # Analiz? special? pentru fi?ierul XML principal
        if ($source.Name -eq "Data.gov.ro Original") {
            Write-Host "  ?? Descarc pentru analiz? detaliat?..." -ForegroundColor Cyan
            
            $tempFile = "isco-temp-analysis.xml"
            try {
                Invoke-WebRequest -Uri $source.Url -OutFile $tempFile -TimeoutSec 30
                
                # Analiz? structur? fi?ier
                $content = Get-Content $tempFile -Raw -Encoding UTF8
                Write-Host "  ?? M?rime fi?ier: $([math]::Round((Get-Item $tempFile).Length / 1KB, 2))KB" -ForegroundColor Gray
                
                # Detectare format real
                if ($content -match '<?xml.*?pkg:package') {
                    Write-Host "  ??  Format detectat: Microsoft Office XML Package" -ForegroundColor Yellow
                    Write-Host "     Acesta este un document Word/Excel, nu XML standard" -ForegroundColor Yellow
                    
                    # Încearc? s? extrag? text din document Office
                    Write-Host "  ?? Încerc extragerea textului din document..." -ForegroundColor Cyan
                    
                    # Caut? pattern-uri de text care ar putea con?ine ocupa?iile
                    $textMatches = [regex]::Matches($content, '<w:t[^>]*>([^<]+)</w:t>')
                    $extractedText = @()
                    
                    foreach ($match in $textMatches) {
                        $text = $match.Groups[1].Value.Trim()
                        if ($text.Length -gt 2 -and $text -notmatch '^[\s\d\.\-\(\)]+$') {
                            $extractedText += $text
                        }
                    }
                    
                    Write-Host "  ?? Text extras: $($extractedText.Count) fragmente" -ForegroundColor Gray
                    
                    # Caut? pattern-uri care par a fi ocupa?ii (cod + denumire)
                    $possibleOccupations = @()
                    for ($i = 0; $i -lt $extractedText.Count - 1; $i++) {
                        $current = $extractedText[$i]
                        $next = $extractedText[$i + 1]
                        
                        # Caut? pattern: cifre urmate de text descriptiv
                        if ($current -match '^\d{1,4}$' -and $next.Length -gt 10 -and $next -notmatch '^\d') {
                            $possibleOccupations += @{
                                Cod = $current
                                Denumire = $next
                            }
                        }
                    }
                    
                    Write-Host "  ?? Ocupa?ii posibile detectate: $($possibleOccupations.Count)" -ForegroundColor Green
                    
                    if ($possibleOccupations.Count -gt 0) {
                        Write-Host "  ?? Exemple g?site:" -ForegroundColor Gray
                        $possibleOccupations | Select-Object -First 5 | ForEach-Object {
                            Write-Host "    $($_.Cod): $($_.Denumire)" -ForegroundColor White
                        }
                        
                        # Salveaz? rezultatele pentru procesare ulterioar?
                        $possibleOccupations | ConvertTo-Json -Depth 2 | Out-File "extracted-occupations.json" -Encoding UTF8
                        Write-Host "  ?? Date salvate în: extracted-occupations.json" -ForegroundColor Green
                    }
                    
                } elseif ($content -match '<.*?>.*</.*?>') {
                    Write-Host "  ? Format XML standard detectat" -ForegroundColor Green
                } else {
                    Write-Host "  ? Format necunoscut" -ForegroundColor Yellow
                }
                
                Remove-Item $tempFile -Force
                
            } catch {
                Write-Host "  ? Eroare la desc?rcarea/analiza fi?ierului: $_" -ForegroundColor Red
            }
        }
        
    } catch {
        Write-Host "  ? Indisponibil: $_" -ForegroundColor Red
    }
}

Write-Host "`n[2] C?utare alternative online..." -ForegroundColor Yellow

# Caut? în GitHub pentru dataset-uri ISCO-08
Write-Host "`n?? C?utare în GitHub pentru dataset-uri ISCO..." -ForegroundColor Cyan
try {
    $githubSearch = "https://api.github.com/search/repositories?q=isco+occupations+csv+OR+isco-08+data&sort=stars&order=desc"
    $response = Invoke-RestMethod -Uri $githubSearch -TimeoutSec 15
    
    Write-Host "  ?? G?site $($response.total_count) repository-uri relevante" -ForegroundColor Gray
    
    $response.items | Select-Object -First 5 | ForEach-Object {
        Write-Host "  ?? $($_.full_name)" -ForegroundColor White
        Write-Host "     Descriere: $($_.description)" -ForegroundColor Gray
        Write-Host "     Stars: $($_.stargazers_count), URL: $($_.html_url)" -ForegroundColor Gray
        Write-Host "" -ForegroundColor Gray
    }
    
} catch {
    Write-Host "  ? Eroare la c?utarea GitHub: $_" -ForegroundColor Red
}

Write-Host "`n[3] Verificare surse oficiale INS România..." -ForegroundColor Yellow

try {
    $insUrl = "https://insse.ro/cms/ro/content/clasificarea-ocupatiilor-din-romania-cor"
    $response = Invoke-WebRequest -Uri $insUrl -TimeoutSec 15
    
    # Caut? link-uri c?tre fi?iere de date
    $dataLinks = $response.Links | Where-Object { 
        $_.href -match '\.(pdf|doc|docx|xls|xlsx|csv)$' -and 
        $_.href -match '(ocupat|isco|cor)' 
    }
    
    if ($dataLinks.Count -gt 0) {
        Write-Host "  ?? Link-uri c?tre date g?site:" -ForegroundColor Green
        $dataLinks | ForEach-Object {
            $fullUrl = if ($_.href -match '^http') { $_.href } else { "https://insse.ro$($_.href)" }
            Write-Host "    - $($_.innerText): $fullUrl" -ForegroundColor White
        }
    } else {
        Write-Host "  ??  Nu s-au g?sit link-uri directe c?tre fi?iere de date" -ForegroundColor Yellow
    }
    
} catch {
    Write-Host "  ? Eroare la accesarea site-ului INS: $_" -ForegroundColor Red
}

Write-Host "`n========================================" -ForegroundColor Magenta
Write-Host "ANALIZ? COMPLET? FINALIZAT?" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta

Write-Host "`n?? RECOMAND?RI:" -ForegroundColor Cyan

Write-Host "`n?? OP?IUNEA 1 - Extragere din document Office (RECOMANDAT)" -ForegroundColor Green
if (Test-Path "extracted-occupations.json") {
    $extractedData = Get-Content "extracted-occupations.json" -Raw | ConvertFrom-Json
    Write-Host "  ? Sunt disponibile $($extractedData.Count) ocupa?ii extrase" -ForegroundColor White
    Write-Host "  ?? Ruleaz?: .\Process-ExtractedOccupations.ps1" -ForegroundColor Cyan
} else {
    Write-Host "  ??  Nu s-au putut extrage date din documentul Office" -ForegroundColor Yellow
}

Write-Host "`n?? OP?IUNEA 2 - Dataset manual complet" -ForegroundColor Green
Write-Host "  ?? Creez un dataset complet cu toate ocupa?iile ISCO-08" -ForegroundColor White
Write-Host "  ?? Ruleaz?: .\Create-CompleteISCODataset.ps1" -ForegroundColor Cyan

Write-Host "`n?? OP?IUNEA 3 - Import din surse GitHub" -ForegroundColor Green
Write-Host "  ?? Verific? repository-urile GitHub g?site mai sus" -ForegroundColor White
Write-Host "  ?? Descarc? CSV/JSON din repo-uri cu multe stars" -ForegroundColor White

Write-Host "`n?? URM?TORUL PAS:" -ForegroundColor Yellow
Write-Host "Alege o op?iune ?i spune-mi s? continui cu implementarea!" -ForegroundColor White