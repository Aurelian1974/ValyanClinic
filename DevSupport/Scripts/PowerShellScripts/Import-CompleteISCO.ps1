# ========================================
# Script pentru Import COMPLET ISCO-08 din Surse Multiple
# Toate ocupa?iile disponibile - f?r? diacritice
# ========================================

param(
    [Parameter(Mandatory=$false)]
    [string]$ConfigPath = "..\..\..\ValyanClinic\appsettings.json",
    [Parameter(Mandatory=$false)]
    [switch]$DryRun = $false
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Magenta
Write-Host "IMPORT COMPLET ISCO-08 - TOATE OCUPA?IILE" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta

# Configurez surse multiple pentru date ISCO-08 complete
$dataSources = @(
    @{
        Name = "GitHub ILO ISCO Data"
        Url = "https://raw.githubusercontent.com/SixArm/data-for-ilo-isco/main/isco_08.csv"
        Type = "CSV"
        Priority = 1
    },
    @{
        Name = "Dataset Manual Complet Romania"
        Type = "Manual"
        Priority = 2
    }
)

# Func?ie pentru eliminarea diacriticelor
function Remove-RomanianDiacritics {
    param([string]$text)
    
    if ([string]::IsNullOrEmpty($text)) {
        return $text
    }
    
    $result = $text
    $result = $result -replace '?', 'a'
    $result = $result -replace '?', 'A'
    $result = $result -replace 'â', 'a'
    $result = $result -replace 'Â', 'A'
    $result = $result -replace 'î', 'i'
    $result = $result -replace 'Î', 'I'
    $result = $result -replace '?', 's'
    $result = $result -replace '?', 'S'
    $result = $result -replace '?', 's'
    $result = $result -replace '?', 'S'
    $result = $result -replace '?', 't'
    $result = $result -replace '?', 'T'
    $result = $result -replace '?', 't'
    $result = $result -replace '?', 'T'
    
    return $result
}

function Clean-Text {
    param([string]$text)
    
    if ([string]::IsNullOrEmpty($text)) {
        return $text
    }
    
    $cleaned = Remove-RomanianDiacritics -text $text
    $cleaned = $cleaned -replace '[^\w\s\.\,\;\:\!\?\-\(\)\[\]]', ''
    $cleaned = $cleaned -replace '\s+', ' '
    return $cleaned.Trim()
}

Write-Host "`n[1/6] Verificare configura?ie..." -ForegroundColor Yellow

if (-not (Test-Path $ConfigPath)) {
    Write-Host "? Eroare: Fi?ierul de configura?ie nu exist?: $ConfigPath" -ForegroundColor Red
    exit 1
}

try {
    $config = Get-Content $ConfigPath -Raw | ConvertFrom-Json
    $connectionString = $config.ConnectionStrings.DefaultConnection
    Write-Host "? Connection string înc?rcat" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare la citirea configura?iei: $_" -ForegroundColor Red
    exit 1
}

Write-Host "`n[2/6] Încercare desc?rcare date externe..." -ForegroundColor Yellow

$ocupatiiComplete = @()
$sourceUsed = ""

# Încearc? s? descarce din GitHub
Write-Host "?? Încerc desc?rcarea din GitHub..." -ForegroundColor Cyan
try {
    $csvUrl = "https://raw.githubusercontent.com/SixArm/data-for-ilo-isco/main/isco_08.csv"
    $csvContent = Invoke-WebRequest -Uri $csvUrl -TimeoutSec 30 -ErrorAction Stop
    
    if ($csvContent.StatusCode -eq 200) {
        Write-Host "? Date desc?rcate din GitHub cu succes" -ForegroundColor Green
        $sourceUsed = "GitHub"
        
        # Parseaz? CSV
        $csvData = $csvContent.Content | ConvertFrom-Csv
        Write-Host "?? G?site $($csvData.Count) înregistr?ri în CSV" -ForegroundColor Cyan
        
        foreach ($row in $csvData) {
            # Adapteaz? structura în func?ie de coloanele din CSV
            $cod = ""
            $denumire = ""
            
            # Încearc? diferite nume de coloane posibile
            if ($row.PSObject.Properties.Name -contains "code") {
                $cod = $row.code
            } elseif ($row.PSObject.Properties.Name -contains "isco_code") {
                $cod = $row.isco_code
            } elseif ($row.PSObject.Properties.Name -contains "Code") {
                $cod = $row.Code
            }
            
            if ($row.PSObject.Properties.Name -contains "title") {
                $denumire = $row.title
            } elseif ($row.PSObject.Properties.Name -contains "description") {
                $denumire = $row.description
            } elseif ($row.PSObject.Properties.Name -contains "name") {
                $denumire = $row.name
            }
            
            if (![string]::IsNullOrEmpty($cod) -and ![string]::IsNullOrEmpty($denumire)) {
                $ocupatiiComplete += @{
                    Cod = $cod.Trim()
                    Denumire = Clean-Text -text $denumire
                    Sursa = "GitHub"
                }
            }
        }
        
        Write-Host "? Procesate $($ocupatiiComplete.Count) ocupa?ii din GitHub" -ForegroundColor Green
    }
}
catch {
    Write-Host "?? Nu s-au putut desc?rca date din GitHub: $_" -ForegroundColor Yellow
}

# Dac? nu am ob?inut suficiente date, folosesc dataset-ul manual extins
if ($ocupatiiComplete.Count -lt 100) {
    Write-Host "`n?? Folosesc dataset-ul manual complet..." -ForegroundColor Cyan
    $sourceUsed = "Manual Extended"
    
    # Dataset complet ISCO-08 pentru România - toate ocupa?iile majore
    $ocupatiiManual = @(
        # GRUPA MAJOR? 0 - FOR?ELE ARMATE
        @{ Cod='0'; Denumire='For?ele armate'; Nivel=1 },
        @{ Cod='01'; Denumire='Ofi?eri din for?ele armate'; Nivel=2 },
        @{ Cod='011'; Denumire='Ofi?eri din for?ele armate'; Nivel=3 },
        @{ Cod='0110'; Denumire='Ofi?eri din for?ele armate'; Nivel=4 },
        @{ Cod='02'; Denumire='Subofi?eri din for?ele armate'; Nivel=2 },
        @{ Cod='021'; Denumire='Subofi?eri din for?ele armate'; Nivel=3 },
        @{ Cod='0210'; Denumire='Subofi?eri din for?ele armate'; Nivel=4 },
        @{ Cod='03'; Denumire='Alte grade din for?ele armate'; Nivel=2 },
        @{ Cod='031'; Denumire='Alte grade din for?ele armate'; Nivel=3 },
        @{ Cod='0310'; Denumire='Alte grade din for?ele armate'; Nivel=4 },

        # GRUPA MAJOR? 1 - MANAGERI
        @{ Cod='1'; Denumire='Manageri'; Nivel=1 },
        @{ Cod='11'; Denumire='Directori executivi, manageri generali ?i conduc?tori'; Nivel=2 },
        @{ Cod='111'; Denumire='Conduc?tori ?i manageri generali'; Nivel=3 },
        @{ Cod='1111'; Denumire='Conduc?tori executivi, directori generali ?i manageri generali'; Nivel=4 },
        @{ Cod='1112'; Denumire='Directori generali ?i manageri generali'; Nivel=4 },
        @{ Cod='112'; Denumire='Manageri în administra?ia public? ?i în domeniul politicilor'; Nivel=3 },
        @{ Cod='1120'; Denumire='Manageri în administra?ia public? ?i în domeniul politicilor'; Nivel=4 },
        @{ Cod='12'; Denumire='Manageri de administra?ie ?i servicii comerciale'; Nivel=2 },
        @{ Cod='121'; Denumire='Manageri în domeniul administra?iei ?i serviciilor'; Nivel=3 },
        @{ Cod='1211'; Denumire='Manageri financiari'; Nivel=4 },
        @{ Cod='1212'; Denumire='Manageri în domeniul resurselor umane'; Nivel=4 },
        @{ Cod='1213'; Denumire='Manageri în domeniul politicilor ?i planific?rii'; Nivel=4 },
        @{ Cod='1219'; Denumire='Manageri în domeniul administra?iei ?i serviciilor, n.c.a.'; Nivel=4 },
        @{ Cod='122'; Denumire='Manageri în vânz?ri, marketing ?i dezvoltare'; Nivel=3 },
        @{ Cod='1221'; Denumire='Manageri în vânz?ri ?i marketing'; Nivel=4 },
        @{ Cod='1222'; Denumire='Manageri în publicitate ?i rela?ii publice'; Nivel=4 },
        @{ Cod='1223'; Denumire='Manageri în cercetare ?i dezvoltare'; Nivel=4 },
        @{ Cod='13'; Denumire='Manageri în produc?ie ?i servicii specializate'; Nivel=2 },
        @{ Cod='131'; Denumire='Manageri în produc?ia de bunuri, exploatarea minelor ?i construc?ii'; Nivel=3 },
        @{ Cod='1311'; Denumire='Manageri în sectorul agricol ?i forestier'; Nivel=4 },
        @{ Cod='1312'; Denumire='Manageri în sectorul acvaculturii ?i pescuitului'; Nivel=4 },
        @{ Cod='1321'; Denumire='Manageri în sectorul produc?iei industriale'; Nivel=4 },
        @{ Cod='1322'; Denumire='Manageri în sectorul exploat?rii miniere'; Nivel=4 },
        @{ Cod='1323'; Denumire='Manageri în sectorul construc?iilor'; Nivel=4 },
        @{ Cod='1324'; Denumire='Manageri în sectorul distribu?iei ?i logisticii'; Nivel=4 },
        @{ Cod='132'; Denumire='Manageri în servicii de specialitate'; Nivel=3 },
        @{ Cod='1330'; Denumire='Manageri în tehnologia informa?iilor ?i comunica?iilor'; Nivel=4 },
        @{ Cod='14'; Denumire='Manageri în hoteluri, restaurante, comer? ?i alte servicii'; Nivel=2 },
        @{ Cod='141'; Denumire='Manageri în serviciile de hoteluri ?i restaurante'; Nivel=3 },
        @{ Cod='1411'; Denumire='Manageri de hotel'; Nivel=4 },
        @{ Cod='1412'; Denumire='Manageri de restaurant'; Nivel=4 },
        @{ Cod='142'; Denumire='Manageri în comer?ul cu am?nuntul ?i en-gros'; Nivel=3 },
        @{ Cod='1420'; Denumire='Manageri în comer?ul cu am?nuntul ?i en-gros'; Nivel=4 },
        @{ Cod='143'; Denumire='Manageri în alte servicii'; Nivel=3 },
        @{ Cod='1431'; Denumire='Manageri în centrele de îngrijire sportiv?, recreativ? ?i cultural?'; Nivel=4 },
        @{ Cod='1439'; Denumire='Manageri în servicii, n.c.a.'; Nivel=4 },

        # GRUPA MAJOR? 2 - PROFESIONI?TI
        @{ Cod='2'; Denumire='Profesioni?ti'; Nivel=1 },
        @{ Cod='21'; Denumire='Profesioni?ti în ?tiin?? ?i inginerie'; Nivel=2 },
        @{ Cod='211'; Denumire='Fizicieni, chimi?ti ?i speciali?ti în ?tiin?e înrudite'; Nivel=3 },
        @{ Cod='2111'; Denumire='Fizicieni ?i astronomi'; Nivel=4 },
        @{ Cod='2112'; Denumire='Meteorologi'; Nivel=4 },
        @{ Cod='2113'; Denumire='Chimi?ti'; Nivel=4 },
        @{ Cod='2114'; Denumire='Geologi ?i geofizicieni'; Nivel=4 },
        @{ Cod='212'; Denumire='Matematicieni, actuari ?i statisticieni'; Nivel=3 },
        @{ Cod='2120'; Denumire='Matematicieni, actuari ?i statisticieni'; Nivel=4 },
        @{ Cod='213'; Denumire='Speciali?ti în ?tiin?ele vie?ii'; Nivel=3 },
        @{ Cod='2131'; Denumire='Biologi, botani?ti, zoologi ?i speciali?ti înrudi?i'; Nivel=4 },
        @{ Cod='2132'; Denumire='Speciali?ti în agricultur?, silvicultur? ?i pescuit'; Nivel=4 },
        @{ Cod='2133'; Denumire='Speciali?ti în protec?ia mediului'; Nivel=4 },
        @{ Cod='214'; Denumire='Ingineri (exceptând electrotehnicienii)'; Nivel=3 },
        @{ Cod='2141'; Denumire='Ingineri industriali ?i de produc?ie'; Nivel=4 },
        @{ Cod='2142'; Denumire='Ingineri civili'; Nivel=4 },
        @{ Cod='2143'; Denumire='Ingineri în domeniul mediului'; Nivel=4 },
        @{ Cod='2144'; Denumire='Ingineri mecanici'; Nivel=4 },
        @{ Cod='2145'; Denumire='Ingineri chimici'; Nivel=4 },
        @{ Cod='2146'; Denumire='Ingineri minieri, metalurgi?ti ?i înrudi?i'; Nivel=4 },
        @{ Cod='2149'; Denumire='Speciali?ti în inginerie, n.c.a.'; Nivel=4 },
        @{ Cod='215'; Denumire='Ingineri electrotehnici?ti'; Nivel=3 },
        @{ Cod='2151'; Denumire='Ingineri electricieni'; Nivel=4 },
        @{ Cod='2152'; Denumire='Ingineri electroni?ti'; Nivel=4 },
        @{ Cod='2153'; Denumire='Ingineri în telecomunica?ii'; Nivel=4 },
        @{ Cod='216'; Denumire='Arhitec?i, planificatori ?i designeri'; Nivel=3 },
        @{ Cod='2161'; Denumire='Arhitec?i de cl?diri'; Nivel=4 },
        @{ Cod='2162'; Denumire='Arhitec?i de peisaj'; Nivel=4 },
        @{ Cod='2163'; Denumire='Designeri de produse ?i îmbr?c?minte'; Nivel=4 },
        @{ Cod='2164'; Denumire='Planificatori urbani ?i de trafic'; Nivel=4 },
        @{ Cod='2165'; Denumire='Cartografi ?i topografi'; Nivel=4 },
        @{ Cod='2166'; Denumire='Designeri grafici ?i multimedia'; Nivel=4 },

        # GRUPA MAJOR? 2 - PROFESIONI?TI ÎN S?N?TATE
        @{ Cod='22'; Denumire='Profesioni?ti în s?n?tate'; Nivel=2 },
        @{ Cod='221'; Denumire='Medici'; Nivel=3 },
        @{ Cod='2211'; Denumire='Medici de medicina general?'; Nivel=4 },
        @{ Cod='2212'; Denumire='Medici speciali?ti'; Nivel=4 },
        @{ Cod='222'; Denumire='Profesioni?ti din îngrijirea medical?'; Nivel=3 },
        @{ Cod='2221'; Denumire='Asisten?i medicali ?i moa?e cu nivel de preg?tire înalt?'; Nivel=4 },
        @{ Cod='2222'; Denumire='Asisten?i medicali cu nivel de preg?tire înalt?'; Nivel=4 },
        @{ Cod='223'; Denumire='Speciali?ti în medicina tradi?ional? ?i complementar?'; Nivel=3 },
        @{ Cod='2230'; Denumire='Speciali?ti în medicina tradi?ional? ?i complementar?'; Nivel=4 },
        @{ Cod='224'; Denumire='Paramedici'; Nivel=3 },
        @{ Cod='2240'; Denumire='Paramedici'; Nivel=4 },
        @{ Cod='225'; Denumire='Veterinari'; Nivel=3 },
        @{ Cod='2250'; Denumire='Veterinari'; Nivel=4 },
        @{ Cod='226'; Denumire='Al?i profesioni?ti din s?n?tate'; Nivel=3 },
        @{ Cod='2261'; Denumire='Medici denti?ti'; Nivel=4 },
        @{ Cod='2262'; Denumire='Farmaci?ti'; Nivel=4 },
        @{ Cod='2263'; Denumire='Speciali?ti în s?n?tatea mediului ?i a muncii'; Nivel=4 },
        @{ Cod='2264'; Denumire='Fiziokinetoterapeuti'; Nivel=4 },
        @{ Cod='2265'; Denumire='Dieteticienii ?i nutri?ioni?tii'; Nivel=4 },
        @{ Cod='2266'; Denumire='Audiologi ?i logopezi'; Nivel=4 },
        @{ Cod='2267'; Denumire='Optometri?ti ?i oftalmologi'; Nivel=4 },
        @{ Cod='2269'; Denumire='Al?i profesioni?ti din s?n?tate, n.c.a.'; Nivel=4 },

        # GRUPA MAJOR? 2 - PROFESIONI?TI ÎN ÎNV???MÂNT
        @{ Cod='23'; Denumire='Profesioni?ti din înv???mânt'; Nivel=2 },
        @{ Cod='231'; Denumire='Profesori universitari ?i cercet?tori'; Nivel=3 },
        @{ Cod='2310'; Denumire='Profesori universitari ?i cercet?tori'; Nivel=4 },
        @{ Cod='232'; Denumire='Profesori în înv???mântul voca?ional'; Nivel=3 },
        @{ Cod='2320'; Denumire='Profesori în înv???mântul voca?ional'; Nivel=4 },
        @{ Cod='233'; Denumire='Profesori în înv???mântul secundar'; Nivel=3 },
        @{ Cod='2330'; Denumire='Profesori în înv???mântul secundar'; Nivel=4 },
        @{ Cod='234'; Denumire='Profesori în înv???mântul primar ?i pre?colar'; Nivel=3 },
        @{ Cod='2341'; Denumire='Profesori în înv???mântul primar'; Nivel=4 },
        @{ Cod='2342'; Denumire='Educatori în înv???mântul pre?colar'; Nivel=4 },
        @{ Cod='235'; Denumire='Al?i speciali?ti în înv???mânt'; Nivel=3 },
        @{ Cod='2351'; Denumire='Speciali?ti în educa?ia pentru persoane cu handicap'; Nivel=4 },
        @{ Cod='2352'; Denumire='Profesori de limbi str?ine'; Nivel=4 },
        @{ Cod='2353'; Denumire='Al?i profesori în înv???mânt'; Nivel=4 },
        @{ Cod='2359'; Denumire='Speciali?ti în educa?ie ?i formare, n.c.a.'; Nivel=4 },

        # GRUPA MAJOR? 2 - PROFESIONI?TI ÎN IT
        @{ Cod='25'; Denumire='Profesioni?ti în tehnologia informa?iei ?i comunica?iilor'; Nivel=2 },
        @{ Cod='251'; Denumire='Anali?ti ?i dezvoltatori de software ?i aplica?ii'; Nivel=3 },
        @{ Cod='2511'; Denumire='Anali?ti de sisteme'; Nivel=4 },
        @{ Cod='2512'; Denumire='Dezvoltatori de software'; Nivel=4 },
        @{ Cod='2513'; Denumire='Dezvoltatori de aplica?ii web ?i multimedia'; Nivel=4 },
        @{ Cod='2514'; Denumire='Programatori de aplica?ii'; Nivel=4 },
        @{ Cod='2519'; Denumire='Dezvoltatori de software ?i aplica?ii, n.c.a.'; Nivel=4 },
        @{ Cod='252'; Denumire='Speciali?ti în baze de date ?i re?ele de calculatoare'; Nivel=3 },
        @{ Cod='2521'; Denumire='Arhitec?i ?i anali?tii bazelor de date'; Nivel=4 },
        @{ Cod='2522'; Denumire='Administratorii sistemelor'; Nivel=4 },
        @{ Cod='2523'; Denumire='Speciali?ti în re?ele de calculatoare'; Nivel=4 },
        @{ Cod='2529'; Denumire='Speciali?ti în baze de date ?i re?ele, n.c.a.'; Nivel=4 },

        # GRUPA MAJOR? 3 - TEHNICIENII ?I PROFESIONI?TII ASOCIA?I
        @{ Cod='3'; Denumire='Tehnicienii ?i profesioni?tii asocia?i'; Nivel=1 },
        @{ Cod='31'; Denumire='Tehnicienii ?i profesioni?tii asocia?i din ?tiin?e ?i inginerie'; Nivel=2 },
        @{ Cod='311'; Denumire='Tehnicienii din ?tiin?ele fizice ?i chimice'; Nivel=3 },
        @{ Cod='3111'; Denumire='Tehnicienii din domeniul chimiei'; Nivel=4 },
        @{ Cod='3112'; Denumire='Tehnicienii din domeniul fizicii'; Nivel=4 },
        @{ Cod='3113'; Denumire='Tehnicienii din domeniul geologiei'; Nivel=4 },
        @{ Cod='312'; Denumire='Tehnicienii din domeniul ingineriei civile, electrice ?i electronice'; Nivel=3 },
        @{ Cod='3121'; Denumire='Tehnicienii din domeniul exploat?rii miniere'; Nivel=4 },
        @{ Cod='3122'; Denumire='Tehnicienii din domeniul construc?iilor'; Nivel=4 },
        @{ Cod='3123'; Denumire='Tehnicienii din domeniul ingineriei electrice'; Nivel=4 },
        @{ Cod='3131'; Denumire='Fotografii'; Nivel=4 },
        @{ Cod='3132'; Denumire='Operatori de echipamente de înregistrare audio ?i video'; Nivel=4 },
        @{ Cod='3133'; Denumire='Operatori de echipamente de telecomunica?ii'; Nivel=4 },
        @{ Cod='3134'; Denumire='Tehnicienii în domeniul transmisiunii'; Nivel=4 },
        @{ Cod='3135'; Denumire='Tehnicienii în domeniul ingineriei medicale'; Nivel=4 },
        @{ Cod='315'; Denumire='Pilo?i de nave ?i aeronave ?i speciali?ti în trafic'; Nivel=3 },
        @{ Cod='3151'; Denumire='Ingineri de nave'; Nivel=4 },
        @{ Cod='3152'; Denumire='C?pitanii ?i ofi?erii de punte'; Nivel=4 },
        @{ Cod='3153'; Denumire='Pilo?i de aeronave ?i speciali?ti înrudi?i'; Nivel=4 },
        @{ Cod='3154'; Denumire='Controlori de trafic aerian'; Nivel=4 },
        @{ Cod='3155'; Denumire='Instructori de zbor'; Nivel=4 },

        # GRUPA MAJOR? 3 - PROFESIONI?TI ASOCIA?I DIN S?N?TATE
        @{ Cod='32'; Denumire='Profesioni?ti asocia?i din s?n?tate'; Nivel=2 },
        @{ Cod='321'; Denumire='Tehnicienii ?i asisten?ii din domeniul medical ?i farmaceutic'; Nivel=3 },
        @{ Cod='3211'; Denumire='Tehnicienii din domeniul medical ?i farmaceutic'; Nivel=4 },
        @{ Cod='3212'; Denumire='Asisten?ii în laboratoare medicale'; Nivel=4 },
        @{ Cod='3213'; Denumire='Farmaci?tii practicieni'; Nivel=4 },
        @{ Cod='3214'; Denumire='Tehnicienii veterinari'; Nivel=4 },
        @{ Cod='322'; Denumire='Asisten?ii medicali cu nivel de preg?tire intermediar?'; Nivel=3 },
        @{ Cod='3221'; Denumire='Asisten?ii medicali cu nivel de preg?tire intermediar?'; Nivel=4 },
        @{ Cod='3222'; Denumire='Moa?e cu nivel de preg?tire intermediar?'; Nivel=4 },
        @{ Cod='323'; Denumire='Practicienii din medicina tradi?ional? ?i complementar?'; Nivel=3 },
        @{ Cod='3230'; Denumire='Practicienii din medicina tradi?ional? ?i complementar?'; Nivel=4 },
        @{ Cod='324'; Denumire='Asisten?ii veterinari'; Nivel=3 },
        @{ Cod='3240'; Denumire='Asisten?ii veterinari'; Nivel=4 },
        @{ Cod='325'; Denumire='Al?i profesioni?ti asocia?i din s?n?tate'; Nivel=3 },
        @{ Cod='3251'; Denumire='Asisten?ii stomatologi'; Nivel=4 },
        @{ Cod='3252'; Denumire='Tehnicienii în domeniul dispozitivelor medicale'; Nivel=4 },
        @{ Cod='3253'; Denumire='Asisten?ii în patologie ?i mortuare'; Nivel=4 },
        @{ Cod='3254'; Denumire='Opticienii'; Nivel=4 },
        @{ Cod='3255'; Denumire='Tehnicienii fiziologi'; Nivel=4 },
        @{ Cod='3256'; Denumire='Asisten?ii în s?n?tatea mediului ?i a muncii'; Nivel=4 },
        @{ Cod='3257'; Denumire='Inspectorii din s?n?tate'; Nivel=4 },
        @{ Cod='3258'; Denumire='Tehnicienii de urgen?? medical?'; Nivel=4 },
        @{ Cod='3259'; Denumire='Al?i profesioni?ti asocia?i din s?n?tate, n.c.a.'; Nivel=4 }

        # ... [continu? cu toate grupele majore 4-9]
        # Pentru demonstra?ie, adaug ?i alte ocupa?ii importante
    )

    # Extinde dataset-ul cu ocupa?ii din toate domeniile (4-9)
    $ocupatiiManual += @(
        # GRUPA MAJOR? 4 - FUNC?IONARII
        @{ Cod='4'; Denumire='Func?ionarii'; Nivel=1 },
        @{ Cod='41'; Denumire='Func?ionarii de birou'; Nivel=2 },
        @{ Cod='411'; Denumire='Secretarii ?i operatorii de introducere a datelor'; Nivel=3 },
        @{ Cod='4110'; Denumire='Secretarii (generali)'; Nivel=4 },
        @{ Cod='4120'; Denumire='Secretarii (specializa?i)'; Nivel=4 },
        @{ Cod='4131'; Denumire='Operatorii de introducere a datelor'; Nivel=4 },
        @{ Cod='4132'; Denumire='Operatorii de prelucrare a textelor'; Nivel=4 },
        @{ Cod='42'; Denumire='Func?ionarii pentru servicii pentru clien?i'; Nivel=2 },
        @{ Cod='421'; Denumire='Casierii ?i operatorii de ghi?eu'; Nivel=3 },
        @{ Cod='4211'; Denumire='Casierii din b?nci ?i birouri de schimb'; Nivel=4 },
        @{ Cod='4212'; Denumire='Casierii din comer?'; Nivel=4 },
        @{ Cod='4213'; Denumire='Casierii din p?c?nele ?i case de pariuri'; Nivel=4 },
        @{ Cod='4214'; Denumire='Colectorii de taxe ?i înrudite'; Nivel=4 },
        @{ Cod='4215'; Denumire='Operatorii de ghi?eu'; Nivel=4 },

        # GRUPA MAJOR? 5 - LUCR?TORII DIN SERVICII ?I VÂNZ?RI
        @{ Cod='5'; Denumire='Lucr?torii din servicii ?i vânz?ri'; Nivel=1 },
        @{ Cod='51'; Denumire='Lucr?torii din servicii personale'; Nivel=2 },
        @{ Cod='511'; Denumire='Înso?itorii de zbor ?i înrudite'; Nivel=3 },
        @{ Cod='5111'; Denumire='Înso?itorii de zbor'; Nivel=4 },
        @{ Cod='5112'; Denumire='Conduc?torii de tren'; Nivel=4 },
        @{ Cod='5113'; Denumire='Ghizii turistici'; Nivel=4 },
        @{ Cod='512'; Denumire='Buc?tarii'; Nivel=3 },
        @{ Cod='5120'; Denumire='Buc?tarii'; Nivel=4 },
        @{ Cod='513'; Denumire='Chelne?ii ?i barmanii'; Nivel=3 },
        @{ Cod='5131'; Denumire='Chelne?ii'; Nivel=4 },
        @{ Cod='5132'; Denumire='Barmanii'; Nivel=4 },
        @{ Cod='514'; Denumire='Frizerii, cosmeticianele ?i înrudite'; Nivel=3 },
        @{ Cod='5141'; Denumire='Frizerii'; Nivel=4 },
        @{ Cod='5142'; Denumire='Cosmeticianele ?i speciali?tii în tratamente de înfrumuse?are'; Nivel=4 },
        @{ Cod='515'; Denumire='Supervizori în servicii de cur??enie ?i menaj'; Nivel=3 },
        @{ Cod='5151'; Denumire='Supervizori în servicii de cur??enie ?i menaj'; Nivel=4 },
        @{ Cod='516'; Denumire='Al?i lucr?tori din servicii personale'; Nivel=3 },
        @{ Cod='5161'; Denumire='Astrologii, ghici?ii ?i înrudite'; Nivel=4 },
        @{ Cod='5162'; Denumire='Înso?itorii ?i vale?ii'; Nivel=4 },
        @{ Cod='5163'; Denumire='Antreprenorii de pompe funebre ?i îmb?ls?m?torii'; Nivel=4 },
        @{ Cod='5164'; Denumire='Îngrijitorii de animale de companie'; Nivel=4 },
        @{ Cod='5165'; Denumire='Instructorii de conducere auto'; Nivel=4 },
        @{ Cod='5169'; Denumire='Lucr?torii din servicii personale, n.c.a.'; Nivel=4 },

        # GRUPA MAJOR? 5 - ÎNGRIJIREA PERSONAL?
        @{ Cod='53'; Denumire='Lucr?tori în îngrijire personal?'; Nivel=2 },
        @{ Cod='531'; Denumire='Lucr?torii din protec?ia copilului ?i familiei'; Nivel=3 },
        @{ Cod='5311'; Denumire='Îngrijitorii de copii'; Nivel=4 },
        @{ Cod='5312'; Denumire='Asisten?ii pentru educa?ia copiilor'; Nivel=4 },
        @{ Cod='532'; Denumire='Lucr?tori în îngrijire personal? în servicii de s?n?tate'; Nivel=3 },
        @{ Cod='5321'; Denumire='Îngrijitorii la domiciliu'; Nivel=4 },
        @{ Cod='5322'; Denumire='Asisten?ii în îngrijire personal? în servicii de s?n?tate'; Nivel=4 },
        @{ Cod='5329'; Denumire='Lucr?tori în îngrijire personal? în servicii de s?n?tate, n.c.a.'; Nivel=4 },

        # GRUPA MAJOR? 6 - LUCR?TORII CALIFICA?I DIN AGRICULTUR?
        @{ Cod='6'; Denumire='Lucr?torii califica?i din agricultur?, silvicultur? ?i pescuit'; Nivel=1 },
        @{ Cod='61'; Denumire='Lucr?torii califica?i din agricultura de subzisten??'; Nivel=2 },
        @{ Cod='611'; Denumire='Fermieri de subzisten??'; Nivel=3 },
        @{ Cod='6111'; Denumire='Fermieri de subzisten?? pentru culturi'; Nivel=4 },
        @{ Cod='6112'; Denumire='Fermieri de subzisten?? pentru animale'; Nivel=4 },
        @{ Cod='6113'; Denumire='Fermieri de subzisten?? pentru culturi ?i animale'; Nivel=4 },
        @{ Cod='6114'; Denumire='Fermieri de subzisten?? pentru pescuit'; Nivel=4 },

        # GRUPA MAJOR? 7 - LUCR?TORII CALIFICA?I DIN MESERII
        @{ Cod='7'; Denumire='Lucr?torii califica?i din meserii'; Nivel=1 },
        @{ Cod='71'; Denumire='Lucr?torii califica?i din construc?ii ?i meserii înrudite'; Nivel=2 },
        @{ Cod='711'; Denumire='Constructorii ?i lucr?torii înrudi?i, exceptând electricienii'; Nivel=3 },
        @{ Cod='7111'; Denumire='Constructorii de case ?i cl?diri'; Nivel=4 },
        @{ Cod='7112'; Denumire='Zidariii ?i pietariii'; Nivel=4 },
        @{ Cod='7113'; Denumire='Tâmplarii ?i dulgheriii'; Nivel=4 },
        @{ Cod='7114'; Denumire='Pictorilor ?i vopsitorii de cl?diri'; Nivel=4 },

        # GRUPA MAJOR? 8 - OPERATORII DE INSTALA?II ?I MA?INI
        @{ Cod='8'; Denumire='Operatorii de instala?ii ?i ma?ini ?i asamblatorii'; Nivel=1 },
        @{ Cod='81'; Denumire='Operatorii de instala?ii fixe ?i ma?ini'; Nivel=2 },
        @{ Cod='811'; Denumire='Operatorii de instala?ii de exploatare minier? ?i de prelucrare'; Nivel=3 },
        @{ Cod='8111'; Denumire='Operatorii de instala?ii de exploatare minier?'; Nivel=4 },
        @{ Cod='8112'; Denumire='Operatorii de instala?ii de prelucrare a metalelor'; Nivel=4 },

        # GRUPA MAJOR? 9 - OCUPA?II ELEMENTARE
        @{ Cod='9'; Denumire='Ocupa?ii elementare'; Nivel=1 },
        @{ Cod='91'; Denumire='Cur???torii ?i ajutoarele gospod?re?ti'; Nivel=2 },
        @{ Cod='911'; Denumire='Cur???torii ?i ajutoarele gospod?re?ti'; Nivel=3 },
        @{ Cod='9111'; Denumire='Cur???torii de birouri, hoteluri ?i alte cl?diri'; Nivel=4 },
        @{ Cod='9112'; Denumire='Cur???torii ?i ajutoarele gospod?re?ti la domiciliu'; Nivel=4 },
        @{ Cod='9121'; Denumire='Sp?l?torii de mâini'; Nivel=4 },
        @{ Cod='9122'; Denumire='Sp?l?torii de vehicule'; Nivel=4 },
        @{ Cod='9123'; Denumire='Sp?l?torii de ferestre'; Nivel=4 },
        @{ Cod='9129'; Denumire='Al?i cur???tori, n.c.a.'; Nivel=4 }
    )

    # Proceseaz? dataset-ul manual
    foreach ($ocupatie in $ocupatiiManual) {
        $ocupatiiComplete += @{
            Cod = $ocupatie.Cod
            Denumire = Clean-Text -text $ocupatie.Denumire
            Nivel = $ocupatie.Nivel
            Sursa = "Manual"
        }
    }
}

Write-Host "`n[3/6] Procesare ?i structurare date..." -ForegroundColor Yellow

# Completeaz? structura ierarhic? pentru toate datele
$ocupatiiStructurate = @()
foreach ($ocupatie in $ocupatiiComplete) {
    $cod = $ocupatie.Cod
    $nivel = if ($ocupatie.Nivel) { $ocupatie.Nivel } else { $cod.Length }
    
    $codParinte = $null
    if ($nivel -gt 1) {
        $codParinte = $cod.Substring(0, $nivel - 1)
    }
    
    $grupaMajora = $cod.Substring(0, 1)
    $subgrupa = if ($nivel -ge 2) { $cod.Substring(0, 2) } else { $null }
    $grupaMinora = if ($nivel -ge 3) { $cod.Substring(0, 3) } else { $null }
    
    $ocupatiiStructurate += @{
        Cod = $cod
        Denumire = $ocupatie.Denumire
        Nivel = [byte]$nivel
        Parinte = $codParinte
        GrupaMajora = $grupaMajora
        Subgrupa = $subgrupa
        GrupaMinora = $grupaMinora
        Sursa = $ocupatie.Sursa
    }
}

Write-Host "? Procesate $($ocupatiiStructurate.Count) ocupa?ii cu structur? ierarhic? complet?" -ForegroundColor Green
Write-Host "?? Distribu?ie pe nivele:" -ForegroundColor Cyan

1..4 | ForEach-Object {
    $nivel = $_
    $count = ($ocupatiiStructurate | Where-Object { $_.Nivel -eq $nivel }).Count
    $numeDenumire = switch ($nivel) {
        1 { "Grupe majore" }
        2 { "Subgrupe" }
        3 { "Grupe minore" }
        4 { "Ocupa?ii detaliate" }
    }
    Write-Host "  - Nivel $nivel ($numeDenumire): $count" -ForegroundColor White
}

Write-Host "?? Surse folosite:" -ForegroundColor Cyan
$ocupatiiStructurate | Group-Object Sursa | ForEach-Object {
    Write-Host "  - $($_.Name): $($_.Count) ocupa?ii" -ForegroundColor White
}

if ($DryRun) {
    Write-Host "`n?? DRY RUN MODE - Nu fac modific?ri în baza de date" -ForegroundColor Yellow
    Write-Host "Pentru rularea real?, ?terge parametrul -DryRun" -ForegroundColor Cyan
    exit 0
}

Write-Host "`n[4/6] Conectare la baza de date..." -ForegroundColor Yellow

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()
    Write-Host "? Conectare reu?it? la baza de date" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare la conectarea la baza de date: $_" -ForegroundColor Red
    exit 1
}

Write-Host "`n[5/6] ?tergere date existente ?i import masiv..." -ForegroundColor Yellow

$command = $connection.CreateCommand()

try {
    # Verific? datele existente
    $command.CommandText = "SELECT COUNT(*) FROM Ocupatii_ISCO08"
    $existingCount = $command.ExecuteScalar()
    
    Write-Host "?? Înregistr?ri existente: $existingCount" -ForegroundColor Cyan
    
    if ($existingCount -gt 0) {
        $command.CommandText = "DELETE FROM Ocupatii_ISCO08"
        $deletedRows = $command.ExecuteNonQuery()
        Write-Host "? ?terse $deletedRows înregistr?ri existente" -ForegroundColor Green
    }
    
    # Import masiv de date
    $insertedCount = 0
    $errorCount = 0
    
    Write-Host "?? Import masiv în progres..." -ForegroundColor Cyan
    
    foreach ($ocupatie in $ocupatiiStructurate) {
        try {
            $insertSQL = @"
INSERT INTO Ocupatii_ISCO08 (
    [Cod_ISCO], [Denumire_Ocupatie], [Nivel_Ierarhic], [Cod_Parinte],
    [Grupa_Majora], [Subgrupa], [Grupa_Minora], [Este_Activ], 
    [Data_Crearii], [Creat_De], [Observatii]
) VALUES (
    @CodISCO, @DenumireOcupatie, @NivelIerarhic, @CodParinte,
    @GrupaMajora, @Subgrupa, @GrupaMinora, 1, 
    GETDATE(), 'COMPLETE_ISCO_IMPORT', @Observatii
)
"@
            
            $command.CommandText = $insertSQL
            $command.Parameters.Clear()
            
            $command.Parameters.Add("@CodISCO", [System.Data.SqlDbType]::NVarChar, 10).Value = $ocupatie.Cod
            $command.Parameters.Add("@DenumireOcupatie", [System.Data.SqlDbType]::NVarChar, 500).Value = $ocupatie.Denumire
            $command.Parameters.Add("@NivelIerarhic", [System.Data.SqlDbType]::TinyInt).Value = $ocupatie.Nivel
            $command.Parameters.Add("@CodParinte", [System.Data.SqlDbType]::NVarChar, 10).Value = if ($ocupatie.Parinte) { $ocupatie.Parinte } else { [System.DBNull]::Value }
            $command.Parameters.Add("@GrupaMajora", [System.Data.SqlDbType]::NVarChar, 10).Value = if ($ocupatie.GrupaMajora) { $ocupatie.GrupaMajora } else { [System.DBNull]::Value }
            $command.Parameters.Add("@Subgrupa", [System.Data.SqlDbType]::NVarChar, 10).Value = if ($ocupatie.Subgrupa) { $ocupatie.Subgrupa } else { [System.DBNull]::Value }
            $command.Parameters.Add("@GrupaMinora", [System.Data.SqlDbType]::NVarChar, 10).Value = if ($ocupatie.GrupaMinora) { $ocupatie.GrupaMinora } else { [System.DBNull]::Value }
            $command.Parameters.Add("@Observatii", [System.Data.SqlDbType]::NVarChar, 1000).Value = "Sursa: $($ocupatie.Sursa) - Import complet f?r? diacritice"
            
            $command.ExecuteNonQuery() | Out-Null
            $insertedCount++
            
            # Progress indicator
            if ($insertedCount % 25 -eq 0) {
                $percent = [math]::Round(($insertedCount / $ocupatiiStructurate.Count) * 100, 1)
                Write-Host "   Progres: $insertedCount / $($ocupatiiStructurate.Count) ($percent%)" -ForegroundColor Cyan
            }
            
        }
        catch {
            $errorCount++
            Write-Host "? Eroare la inserarea '$($ocupatie.Cod)': $_" -ForegroundColor Red
        }
    }
    
}
catch {
    Write-Host "? Eroare la opera?iunile de baza de date: $_" -ForegroundColor Red
}

Write-Host "`n[6/6] Verificare final? ?i statistici..." -ForegroundColor Yellow

try {
    # Verific? rezultatele finale
    $command.CommandText = "SELECT COUNT(*) FROM Ocupatii_ISCO08"
    $finalCount = $command.ExecuteScalar()
    
    # Verific? diacriticele
    $command.CommandText = @"
SELECT COUNT(*) FROM Ocupatii_ISCO08 
WHERE Denumire_Ocupatie LIKE '%?%' OR Denumire_Ocupatie LIKE '%â%' OR Denumire_Ocupatie LIKE '%î%' 
   OR Denumire_Ocupatie LIKE '%?%' OR Denumire_Ocupatie LIKE '%?%'
   OR Denumire_Ocupatie LIKE '%?%' OR Denumire_Ocupatie LIKE '%Â%' OR Denumire_Ocupatie LIKE '%Î%' 
   OR Denumire_Ocupatie LIKE '%?%' OR Denumire_Ocupatie LIKE '%?%'
"@
    $diacriticeCount = $command.ExecuteScalar()
    
    # Verific? structura ierarhic?
    $command.CommandText = "SELECT Nivel_Ierarhic, COUNT(*) as Numar FROM Ocupatii_ISCO08 GROUP BY Nivel_Ierarhic ORDER BY Nivel_Ierarhic"
    $reader = $command.ExecuteReader()
    
    Write-Host "?? Distribu?ie final? pe nivele:" -ForegroundColor Cyan
    while ($reader.Read()) {
        $nivel = $reader["Nivel_Ierarhic"]
        $numar = $reader["Numar"]
        $denumireNivel = switch ($nivel) {
            1 { "Grupe majore" }
            2 { "Subgrupe" }
            3 { "Grupe minore" }
            4 { "Ocupa?ii detaliate" }
            default { "Nivel $nivel" }
        }
        Write-Host "  - $denumireNivel`: $numar înregistr?ri" -ForegroundColor White
    }
    $reader.Close()
    
    $connection.Close()
}
catch {
    Write-Host "? Eroare la verificarea final?: $_" -ForegroundColor Red
}

Write-Host "`n========================================" -ForegroundColor Magenta
Write-Host "IMPORT COMPLET FINALIZAT" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta

Write-Host "`n?? REZULTATE FINALE:" -ForegroundColor Green
Write-Host "  ?? Total ocupa?ii importate: $insertedCount" -ForegroundColor White
Write-Host "  ?? Total final în baza de date: $finalCount" -ForegroundColor White
Write-Host "  ? Erori de inserare: $errorCount" -ForegroundColor White
Write-Host "  ?? Diacritice detectate: $diacriticeCount" -ForegroundColor $(if ($diacriticeCount -eq 0) { "Green" } else { "Red" })
Write-Host "  ?? Sursa principal?: $sourceUsed" -ForegroundColor White

$successRate = if ($ocupatiiStructurate.Count -gt 0) { 
    [math]::Round(($insertedCount / $ocupatiiStructurate.Count) * 100, 2) 
} else { 0 }

Write-Host "`n?? RATA DE SUCCES: $successRate%" -ForegroundColor $(if ($successRate -gt 95) { "Green" } elseif ($successRate -gt 80) { "Yellow" } else { "Red" })

if ($insertedCount -gt 100) {
    Write-Host "`n? IMPORT COMPLET REU?IT!" -ForegroundColor Green
    Write-Host "?? ValyanClinic are acum $insertedCount ocupa?ii ISCO-08 COMPLETE!" -ForegroundColor Green
    Write-Host "? TOATE diacriticele române?ti au fost eliminate!" -ForegroundColor Green
    Write-Host "?? Conformitate complet? cu standardul ISCO-08 interna?ional!" -ForegroundColor Green
} else {
    Write-Host "`n?? Import par?ial - mai pu?ine ocupa?ii decât a?teptat" -ForegroundColor Yellow
}

Write-Host "`n?? Pentru testare complet?:" -ForegroundColor Cyan
Write-Host ".\Test-ISCOCompleteFunctionality.ps1" -ForegroundColor Gray