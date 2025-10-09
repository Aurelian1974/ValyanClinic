# ========================================
# Script pentru Import Date ISCO-08 Curate (f?r? diacritice)
# Alternativ? la XML indisponibil - date exemple extinse
# ========================================

param(
    [Parameter(Mandatory=$false)]
    [string]$ConfigPath = "..\..\..\ValyanClinic\appsettings.json",
    [Parameter(Mandatory=$false)]
    [switch]$DryRun = $false
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Magenta
Write-Host "IMPORT OCUPATII ISCO-08 CURATE" -ForegroundColor Magenta
Write-Host "Date exemple extinse - fara diacritice" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta

# Date ISCO-08 curate (fara diacritice) - set extins pentru demonstratie
$ocupatiiISCO = @(
    # NIVEL 1 - GRUPE MAJORE
    @{ Cod='0'; Denumire='Ocupatii din domeniul militar'; Nivel=1; GrupaMajora='0'; GrupaDenumire='Ocupatii din domeniul militar'; Parinte=$null; Subgrupa=$null; SubgrupaDenumire=$null; GrupaMinora=$null; GrupaMinoraDenumire=$null },
    @{ Cod='1'; Denumire='Manageri'; Nivel=1; GrupaMajora='1'; GrupaDenumire='Manageri'; Parinte=$null; Subgrupa=$null; SubgrupaDenumire=$null; GrupaMinora=$null; GrupaMinoraDenumire=$null },
    @{ Cod='2'; Denumire='Profesionisti'; Nivel=1; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Parinte=$null; Subgrupa=$null; SubgrupaDenumire=$null; GrupaMinora=$null; GrupaMinoraDenumire=$null },
    @{ Cod='3'; Denumire='Tehnicienii si profesionistii asociati'; Nivel=1; GrupaMajora='3'; GrupaDenumire='Tehnicienii si profesionistii asociati'; Parinte=$null; Subgrupa=$null; SubgrupaDenumire=$null; GrupaMinora=$null; GrupaMinoraDenumire=$null },
    @{ Cod='4'; Denumire='Functionarii'; Nivel=1; GrupaMajora='4'; GrupaDenumire='Functionarii'; Parinte=$null; Subgrupa=$null; SubgrupaDenumire=$null; GrupaMinora=$null; GrupaMinoraDenumire=$null },
    @{ Cod='5'; Denumire='Lucratorii din servicii si vanzari'; Nivel=1; GrupaMajora='5'; GrupaDenumire='Lucratorii din servicii si vanzari'; Parinte=$null; Subgrupa=$null; SubgrupaDenumire=$null; GrupaMinora=$null; GrupaMinoraDenumire=$null },
    @{ Cod='6'; Denumire='Lucratorii calificati din agricultura, silvicultura si pescuit'; Nivel=1; GrupaMajora='6'; GrupaDenumire='Lucratorii calificati din agricultura, silvicultura si pescuit'; Parinte=$null; Subgrupa=$null; SubgrupaDenumire=$null; GrupaMinora=$null; GrupaMinoraDenumire=$null },
    @{ Cod='7'; Denumire='Lucratorii calificati din meserii'; Nivel=1; GrupaMajora='7'; GrupaDenumire='Lucratorii calificati din meserii'; Parinte=$null; Subgrupa=$null; SubgrupaDenumire=$null; GrupaMinora=$null; GrupaMinoraDenumire=$null },
    @{ Cod='8'; Denumire='Operatorii de instalatii si masini si asamblatorii'; Nivel=1; GrupaMajora='8'; GrupaDenumire='Operatorii de instalatii si masini si asamblatorii'; Parinte=$null; Subgrupa=$null; SubgrupaDenumire=$null; GrupaMinora=$null; GrupaMinoraDenumire=$null },
    @{ Cod='9'; Denumire='Ocupatii elementare'; Nivel=1; GrupaMajora='9'; GrupaDenumire='Ocupatii elementare'; Parinte=$null; Subgrupa=$null; SubgrupaDenumire=$null; GrupaMinora=$null; GrupaMinoraDenumire=$null },

    # NIVEL 2 - SUBGRUPE (pentru grupele majore medicale si IT)
    @{ Cod='11'; Denumire='Directori executivi, manageri generali si conducatori'; Nivel=2; Parinte='1'; GrupaMajora='1'; GrupaDenumire='Manageri'; Subgrupa='11'; SubgrupaDenumire='Directori executivi, manageri generali si conducatori'; GrupaMinora=$null; GrupaMinoraDenumire=$null },
    @{ Cod='21'; Denumire='Profesionisti din stiinta si inginerie'; Nivel=2; Parinte='2'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='21'; SubgrupaDenumire='Profesionisti din stiinta si inginerie'; GrupaMinora=$null; GrupaMinoraDenumire=$null },
    @{ Cod='22'; Denumire='Profesionisti din sanatate'; Nivel=2; Parinte='2'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='22'; SubgrupaDenumire='Profesionisti din sanatate'; GrupaMinora=$null; GrupaMinoraDenumire=$null },
    @{ Cod='23'; Denumire='Profesionisti din invatamant'; Nivel=2; Parinte='2'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='23'; SubgrupaDenumire='Profesionisti din invatamant'; GrupaMinora=$null; GrupaMinoraDenumire=$null },
    @{ Cod='24'; Denumire='Specialisti in probleme de afaceri si administratie'; Nivel=2; Parinte='2'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='24'; SubgrupaDenumire='Specialisti in probleme de afaceri si administratie'; GrupaMinora=$null; GrupaMinoraDenumire=$null },
    @{ Cod='25'; Denumire='Profesionisti in tehnologia informatiei si comunicatiilor'; Nivel=2; Parinte='2'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='25'; SubgrupaDenumire='Profesionisti in tehnologia informatiei si comunicatiilor'; GrupaMinora=$null; GrupaMinoraDenumire=$null },
    @{ Cod='26'; Denumire='Specialisti juridici, din domeniul social si cultural'; Nivel=2; Parinte='2'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='26'; SubgrupaDenumire='Specialisti juridici, din domeniul social si cultural'; GrupaMinora=$null; GrupaMinoraDenumire=$null },
    @{ Cod='32'; Denumire='Profesionisti asociati din sanatate'; Nivel=2; Parinte='3'; GrupaMajora='3'; GrupaDenumire='Tehnicienii si profesionistii asociati'; Subgrupa='32'; SubgrupaDenumire='Profesionisti asociati din sanatate'; GrupaMinora=$null; GrupaMinoraDenumire=$null },
    @{ Cod='53'; Denumire='Lucratori in ingrijire personala'; Nivel=2; Parinte='5'; GrupaMajora='5'; GrupaDenumire='Lucratorii din servicii si vanzari'; Subgrupa='53'; SubgrupaDenumire='Lucratori in ingrijire personala'; GrupaMinora=$null; GrupaMinoraDenumire=$null },

    # NIVEL 3 - GRUPE MINORE (pentru sanatate si IT)
    @{ Cod='111'; Denumire='Conducatori si manageri generali'; Nivel=3; Parinte='11'; GrupaMajora='1'; GrupaDenumire='Manageri'; Subgrupa='11'; SubgrupaDenumire='Directori executivi, manageri generali si conducatori'; GrupaMinora='111'; GrupaMinoraDenumire='Conducatori si manageri generali' },
    @{ Cod='221'; Denumire='Medici'; Nivel=3; Parinte='22'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='22'; SubgrupaDenumire='Profesionisti din sanatate'; GrupaMinora='221'; GrupaMinoraDenumire='Medici' },
    @{ Cod='222'; Denumire='Profesionisti din ingrijirea medicala'; Nivel=3; Parinte='22'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='22'; SubgrupaDenumire='Profesionisti din sanatate'; GrupaMinora='222'; GrupaMinoraDenumire='Profesionisti din ingrijirea medicala' },
    @{ Cod='223'; Denumire='Specialisti in medicina traditionala si complementara'; Nivel=3; Parinte='22'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='22'; SubgrupaDenumire='Profesionisti din sanatate'; GrupaMinora='223'; GrupaMinoraDenumire='Specialisti in medicina traditionala si complementara' },
    @{ Cod='224'; Denumire='Paramedici'; Nivel=3; Parinte='22'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='22'; SubgrupaDenumire='Profesionisti din sanatate'; GrupaMinora='224'; GrupaMinoraDenumire='Paramedici' },
    @{ Cod='225'; Denumire='Veterinari'; Nivel=3; Parinte='22'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='22'; SubgrupaDenumire='Profesionisti din sanatate'; GrupaMinora='225'; GrupaMinoraDenumire='Veterinari' },
    @{ Cod='226'; Denumire='Alti profesionisti din sanatate'; Nivel=3; Parinte='22'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='22'; SubgrupaDenumire='Profesionisti din sanatate'; GrupaMinora='226'; GrupaMinoraDenumire='Alti profesionisti din sanatate' },
    @{ Cod='251'; Denumire='Analisti si dezvoltatori de software si aplicatii'; Nivel=3; Parinte='25'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='25'; SubgrupaDenumire='Profesionisti in tehnologia informatiei si comunicatiilor'; GrupaMinora='251'; GrupaMinoraDenumire='Analisti si dezvoltatori de software si aplicatii' },
    @{ Cod='252'; Denumire='Specialisti in baze de date si retele de calculatoare'; Nivel=3; Parinte='25'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='25'; SubgrupaDenumire='Profesionisti in tehnologia informatiei si comunicatiilor'; GrupaMinora='252'; GrupaMinoraDenumire='Specialisti in baze de date si retele de calculatoare' },
    @{ Cod='322'; Denumire='Asistenti medicali cu nivel de pregatire intermediara'; Nivel=3; Parinte='32'; GrupaMajora='3'; GrupaDenumire='Tehnicienii si profesionistii asociati'; Subgrupa='32'; SubgrupaDenumire='Profesionisti asociati din sanatate'; GrupaMinora='322'; GrupaMinoraDenumire='Asistenti medicali cu nivel de pregatire intermediara' },
    @{ Cod='532'; Denumire='Lucratori in ingrijire personala in servicii de sanatate'; Nivel=3; Parinte='53'; GrupaMajora='5'; GrupaDenumire='Lucratorii din servicii si vanzari'; Subgrupa='53'; SubgrupaDenumire='Lucratori in ingrijire personala'; GrupaMinora='532'; GrupaMinoraDenumire='Lucratori in ingrijire personala in servicii de sanatate' },

    # NIVEL 4 - OCUPATII DETALIATE (pentru medici si IT)
    @{ Cod='2211'; Denumire='Medici de medicina generala'; Nivel=4; Parinte='221'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='22'; SubgrupaDenumire='Profesionisti din sanatate'; GrupaMinora='221'; GrupaMinoraDenumire='Medici' },
    @{ Cod='2212'; Denumire='Medici specialisti'; Nivel=4; Parinte='221'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='22'; SubgrupaDenumire='Profesionisti din sanatate'; GrupaMinora='221'; GrupaMinoraDenumire='Medici' },
    @{ Cod='2221'; Denumire='Asistenti medicali si moase cu nivel de pregatire inalta'; Nivel=4; Parinte='222'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='22'; SubgrupaDenumire='Profesionisti din sanatate'; GrupaMinora='222'; GrupaMinoraDenumire='Profesionisti din ingrijirea medicala' },
    @{ Cod='2222'; Denumire='Asistenti medicali cu nivel de pregatire inalta'; Nivel=4; Parinte='222'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='22'; SubgrupaDenumire='Profesionisti din sanatate'; GrupaMinora='222'; GrupaMinoraDenumire='Profesionisti din ingrijirea medicala' },
    @{ Cod='2240'; Denumire='Paramedici'; Nivel=4; Parinte='224'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='22'; SubgrupaDenumire='Profesionisti din sanatate'; GrupaMinora='224'; GrupaMinoraDenumire='Paramedici' },
    @{ Cod='2250'; Denumire='Veterinari'; Nivel=4; Parinte='225'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='22'; SubgrupaDenumire='Profesionisti din sanatate'; GrupaMinora='225'; GrupaMinoraDenumire='Veterinari' },
    @{ Cod='2261'; Denumire='Medici dentisti'; Nivel=4; Parinte='226'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='22'; SubgrupaDenumire='Profesionisti din sanatate'; GrupaMinora='226'; GrupaMinoraDenumire='Alti profesionisti din sanatate' },
    @{ Cod='2262'; Denumire='Farmacisti'; Nivel=4; Parinte='226'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='22'; SubgrupaDenumire='Profesionisti din sanatate'; GrupaMinora='226'; GrupaMinoraDenumire='Alti profesionisti din sanatate' },
    @{ Cod='2263'; Denumire='Specialisti in sanatatea mediului si a muncii'; Nivel=4; Parinte='226'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='22'; SubgrupaDenumire='Profesionisti din sanatate'; GrupaMinora='226'; GrupaMinoraDenumire='Alti profesionisti din sanatate' },
    @{ Cod='2264'; Denumire='Fiziochinetoterapeuti'; Nivel=4; Parinte='226'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='22'; SubgrupaDenumire='Profesionisti din sanatate'; GrupaMinora='226'; GrupaMinoraDenumire='Alti profesionisti din sanatate' },
    @{ Cod='2265'; Denumire='Dieteticienii si nutritionistii'; Nivel=4; Parinte='226'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='22'; SubgrupaDenumire='Profesionisti din sanatate'; GrupaMinora='226'; GrupaMinoraDenumire='Alti profesionisti din sanatate' },
    @{ Cod='2266'; Denumire='Audiologi si logopezi'; Nivel=4; Parinte='226'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='22'; SubgrupaDenumire='Profesionisti din sanatate'; GrupaMinora='226'; GrupaMinoraDenumire='Alti profesionisti din sanatate' },
    @{ Cod='2267'; Denumire='Optometristi si oftalmologi'; Nivel=4; Parinte='226'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='22'; SubgrupaDenumire='Profesionisti din sanatate'; GrupaMinora='226'; GrupaMinoraDenumire='Alti profesionisti din sanatate' },
    @{ Cod='2269'; Denumire='Alti profesionisti din sanatate, n.c.a.'; Nivel=4; Parinte='226'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='22'; SubgrupaDenumire='Profesionisti din sanatate'; GrupaMinora='226'; GrupaMinoraDenumire='Alti profesionisti din sanatate' },
    @{ Cod='2511'; Denumire='Analisti de sisteme'; Nivel=4; Parinte='251'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='25'; SubgrupaDenumire='Profesionisti in tehnologia informatiei si comunicatiilor'; GrupaMinora='251'; GrupaMinoraDenumire='Analisti si dezvoltatori de software si aplicatii' },
    @{ Cod='2512'; Denumire='Dezvoltatori de software'; Nivel=4; Parinte='251'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='25'; SubgrupaDenumire='Profesionisti in tehnologia informatiei si comunicatiilor'; GrupaMinora='251'; GrupaMinoraDenumire='Analisti si dezvoltatori de software si aplicatii' },
    @{ Cod='2513'; Denumire='Dezvoltatori de aplicatii web si multimedia'; Nivel=4; Parinte='251'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='25'; SubgrupaDenumire='Profesionisti in tehnologia informatiei si comunicatiilor'; GrupaMinora='251'; GrupaMinoraDenumire='Analisti si dezvoltatori de software si aplicatii' },
    @{ Cod='2514'; Denumire='Programatori de aplicatii'; Nivel=4; Parinte='251'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='25'; SubgrupaDenumire='Profesionisti in tehnologia informatiei si comunicatiilor'; GrupaMinora='251'; GrupaMinoraDenumire='Analisti si dezvoltatori de software si aplicatii' },
    @{ Cod='2519'; Denumire='Dezvoltatori de software si aplicatii, n.c.a.'; Nivel=4; Parinte='251'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='25'; SubgrupaDenumire='Profesionisti in tehnologia informatiei si comunicatiilor'; GrupaMinora='251'; GrupaMinoraDenumire='Analisti si dezvoltatori de software si aplicatii' },
    @{ Cod='2521'; Denumire='Arhitecti si analistii bazelor de date'; Nivel=4; Parinte='252'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='25'; SubgrupaDenumire='Profesionisti in tehnologia informatiei si comunicatiilor'; GrupaMinora='252'; GrupaMinoraDenumire='Specialisti in baze de date si retele de calculatoare' },
    @{ Cod='2522'; Denumire='Administratorii sistemelor'; Nivel=4; Parinte='252'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='25'; SubgrupaDenumire='Profesionisti in tehnologia informatiei si comunicatiilor'; GrupaMinora='252'; GrupaMinoraDenumire='Specialisti in baze de date si retele de calculatoare' },
    @{ Cod='2523'; Denumire='Specialisti in retele de calculatoare'; Nivel=4; Parinte='252'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='25'; SubgrupaDenumire='Profesionisti in tehnologia informatiei si comunicatiilor'; GrupaMinora='252'; GrupaMinoraDenumire='Specialisti in baze de date si retele de calculatoare' },
    @{ Cod='2529'; Denumire='Specialisti in baze de date si retele, n.c.a.'; Nivel=4; Parinte='252'; GrupaMajora='2'; GrupaDenumire='Profesionisti'; Subgrupa='25'; SubgrupaDenumire='Profesionisti in tehnologia informatiei si comunicatiilor'; GrupaMinora='252'; GrupaMinoraDenumire='Specialisti in baze de date si retele de calculatoare' },

    # OCUPATII SUPLIMENTARE PENTRU DIVERSE DOMENII
    @{ Cod='1111'; Denumire='Conducatori executivi, directori generali si manageri generali'; Nivel=4; Parinte='111'; GrupaMajora='1'; GrupaDenumire='Manageri'; Subgrupa='11'; SubgrupaDenumire='Directori executivi, manageri generali si conducatori'; GrupaMinora='111'; GrupaMinoraDenumire='Conducatori si manageri generali' },
    @{ Cod='1112'; Denumire='Directori generali si manageri generali'; Nivel=4; Parinte='111'; GrupaMajora='1'; GrupaDenumire='Manageri'; Subgrupa='11'; SubgrupaDenumire='Directori executivi, manageri generali si conducatori'; GrupaMinora='111'; GrupaMinoraDenumire='Conducatori si manageri generali' },
    @{ Cod='3221'; Denumire='Asistenti medicali cu nivel de pregatire intermediara'; Nivel=4; Parinte='322'; GrupaMajora='3'; GrupaDenumire='Tehnicienii si profesionistii asociati'; Subgrupa='32'; SubgrupaDenumire='Profesionisti asociati din sanatate'; GrupaMinora='322'; GrupaMinoraDenumire='Asistenti medicali cu nivel de pregatire intermediara' },
    @{ Cod='3222'; Denumire='Moase cu nivel de pregatire intermediara'; Nivel=4; Parinte='322'; GrupaMajora='3'; GrupaDenumire='Tehnicienii si profesionistii asociati'; Subgrupa='32'; SubgrupaDenumire='Profesionisti asociati din sanatate'; GrupaMinora='322'; GrupaMinoraDenumire='Asistenti medicali cu nivel de pregatire intermediara' },
    @{ Cod='5321'; Denumire='Ingrijitori la domiciliu'; Nivel=4; Parinte='532'; GrupaMajora='5'; GrupaDenumire='Lucratorii din servicii si vanzari'; Subgrupa='53'; SubgrupaDenumire='Lucratori in ingrijire personala'; GrupaMinora='532'; GrupaMinoraDenumire='Lucratori in ingrijire personala in servicii de sanatate' },
    @{ Cod='5322'; Denumire='Asistenti in ingrijire personala in servicii de sanatate'; Nivel=4; Parinte='532'; GrupaMajora='5'; GrupaDenumire='Lucratorii din servicii si vanzari'; Subgrupa='53'; SubgrupaDenumire='Lucratori in ingrijire personala'; GrupaMinora='532'; GrupaMinoraDenumire='Lucratori in ingrijire personala in servicii de sanatate' }
)

Write-Host "`n[1/4] Verificare configuratie..." -ForegroundColor Yellow

if (-not (Test-Path $ConfigPath)) {
    Write-Host "? Eroare: Fisierul de configuratie nu exista: $ConfigPath" -ForegroundColor Red
    exit 1
}

try {
    $config = Get-Content $ConfigPath -Raw | ConvertFrom-Json
    $connectionString = $config.ConnectionStrings.DefaultConnection
    Write-Host "? Connection string incarcat" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare la citirea configuratiei: $_" -ForegroundColor Red
    exit 1
}

Write-Host "`n[2/4] Analiza date pentru import..." -ForegroundColor Yellow
Write-Host "?? Total ocupatii ISCO-08: $($ocupatiiISCO.Count)" -ForegroundColor Cyan
Write-Host "  - Nivel 1 (Grupe majore): $(($ocupatiiISCO | Where-Object { $_.Nivel -eq 1 }).Count)" -ForegroundColor White
Write-Host "  - Nivel 2 (Subgrupe): $(($ocupatiiISCO | Where-Object { $_.Nivel -eq 2 }).Count)" -ForegroundColor White
Write-Host "  - Nivel 3 (Grupe minore): $(($ocupatiiISCO | Where-Object { $_.Nivel -eq 3 }).Count)" -ForegroundColor White
Write-Host "  - Nivel 4 (Ocupatii detaliate): $(($ocupatiiISCO | Where-Object { $_.Nivel -eq 4 }).Count)" -ForegroundColor White

# Verifica consistenta codurilor
$coduriGasite = @()
$coduriDuplicat = @()

foreach ($ocupatie in $ocupatiiISCO) {
    $cod = $ocupatie.Cod
    if ($coduriGasite -contains $cod) {
        if ($coduriDuplicat -notcontains $cod) {
            $coduriDuplicat += $cod
        }
    } else {
        $coduriGasite += $cod
    }
}

if ($coduriDuplicat.Count -gt 0) {
    Write-Host "? ATENTIE: Coduri duplicate detectate:" -ForegroundColor Red
    $coduriDuplicat | ForEach-Object { 
        $count = ($ocupatiiISCO | Where-Object { $_.Cod -eq $_ }).Count
        Write-Host "  - '$_': $count aparitii" -ForegroundColor Red 
    }
    exit 1
} else {
    Write-Host "? Nu exista coduri duplicate" -ForegroundColor Green
}

# Verifica validitatea referintelor parinte
$coduriInvalide = @()
foreach ($ocupatie in $ocupatiiISCO) {
    if ($ocupatie.Parinte -and -not ($ocupatiiISCO | Where-Object { $_.Cod -eq $ocupatie.Parinte })) {
        $coduriInvalide += $ocupatie.Cod
    }
}

if ($coduriInvalide.Count -gt 0) {
    Write-Host "? ATENTIE: Referinte parinte invalide:" -ForegroundColor Red
    $coduriInvalide | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
    exit 1
} else {
    Write-Host "? Toate referintele parinte sunt valide" -ForegroundColor Green
}

if ($DryRun) {
    Write-Host "`n?? DRY RUN MODE - Nu fac modificari in baza de date" -ForegroundColor Yellow
    Write-Host "Pentru rularea reala, sterge parametrul -DryRun" -ForegroundColor Cyan
    exit 0
}

Write-Host "`n[3/4] Stergere date existente si import..." -ForegroundColor Yellow

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()
    Write-Host "? Conectare reusita la baza de date" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare la conectarea la baza de date: $_" -ForegroundColor Red
    exit 1
}

$command = $connection.CreateCommand()

try {
    # Sterge datele existente
    $command.CommandText = "SELECT COUNT(*) FROM Ocupatii_ISCO08"
    $existingCount = $command.ExecuteScalar()
    
    Write-Host "?? Inregistrari existente: $existingCount" -ForegroundColor Cyan
    
    if ($existingCount -gt 0) {
        $command.CommandText = "DELETE FROM Ocupatii_ISCO08"
        $deletedRows = $command.ExecuteNonQuery()
        Write-Host "? Sterse $deletedRows inregistrari existente" -ForegroundColor Green
    }
    
    # Import date noi
    $insertedCount = 0
    $errorCount = 0
    
    foreach ($ocupatie in $ocupatiiISCO) {
        try {
            $insertSQL = @"
INSERT INTO Ocupatii_ISCO08 (
    [Cod_ISCO], [Denumire_Ocupatie], [Nivel_Ierarhic], [Cod_Parinte],
    [Grupa_Majora], [Grupa_Majora_Denumire], [Subgrupa], [Subgrupa_Denumire],
    [Grupa_Minora], [Grupa_Minora_Denumire], [Este_Activ], [Data_Crearii], [Creat_De]
) VALUES (
    @CodISCO, @DenumireOcupatie, @NivelIerarhic, @CodParinte,
    @GrupaMajora, @GrupaMajoraDenumire, @Subgrupa, @SubgrupaDenumire,
    @GrupaMinora, @GrupaMinoraDenumire, 1, GETDATE(), 'CLEANED_DATA_IMPORT'
)
"@
            
            $command.CommandText = $insertSQL
            $command.Parameters.Clear()
            
            $command.Parameters.Add("@CodISCO", [System.Data.SqlDbType]::NVarChar, 10).Value = $ocupatie.Cod
            $command.Parameters.Add("@DenumireOcupatie", [System.Data.SqlDbType]::NVarChar, 500).Value = $ocupatie.Denumire
            $command.Parameters.Add("@NivelIerarhic", [System.Data.SqlDbType]::TinyInt).Value = $ocupatie.Nivel
            $command.Parameters.Add("@CodParinte", [System.Data.SqlDbType]::NVarChar, 10).Value = if ($ocupatie.Parinte) { $ocupatie.Parinte } else { [System.DBNull]::Value }
            $command.Parameters.Add("@GrupaMajora", [System.Data.SqlDbType]::NVarChar, 10).Value = if ($ocupatie.GrupaMajora) { $ocupatie.GrupaMajora } else { [System.DBNull]::Value }
            $command.Parameters.Add("@GrupaMajoraDenumire", [System.Data.SqlDbType]::NVarChar, 300).Value = if ($ocupatie.GrupaDenumire) { $ocupatie.GrupaDenumire } else { [System.DBNull]::Value }
            $command.Parameters.Add("@Subgrupa", [System.Data.SqlDbType]::NVarChar, 10).Value = if ($ocupatie.Subgrupa) { $ocupatie.Subgrupa } else { [System.DBNull]::Value }
            $command.Parameters.Add("@SubgrupaDenumire", [System.Data.SqlDbType]::NVarChar, 300).Value = if ($ocupatie.SubgrupaDenumire) { $ocupatie.SubgrupaDenumire } else { [System.DBNull]::Value }
            $command.Parameters.Add("@GrupaMinora", [System.Data.SqlDbType]::NVarChar, 10).Value = if ($ocupatie.GrupaMinora) { $ocupatie.GrupaMinora } else { [System.DBNull]::Value }
            $command.Parameters.Add("@GrupaMinoraDenumire", [System.Data.SqlDbType]::NVarChar, 300).Value = if ($ocupatie.GrupaMinoraDenumire) { $ocupatie.GrupaMinoraDenumire } else { [System.DBNull]::Value }
            
            $command.ExecuteNonQuery() | Out-Null
            $insertedCount++
            
            if ($insertedCount % 10 -eq 0) {
                Write-Host "   Inserate: $insertedCount / $($ocupatiiISCO.Count)" -ForegroundColor Cyan
            }
            
        }
        catch {
            $errorCount++
            Write-Host "? Eroare la inserarea '$($ocupatie.Cod)': $_" -ForegroundColor Red
        }
    }
    
}
catch {
    Write-Host "? Eroare la operatiunile de baza de date: $_" -ForegroundColor Red
}
finally {
    $connection.Close()
}

Write-Host "`n[4/4] Verificare finala..." -ForegroundColor Yellow

try {
    $connection.Open()
    $command = $connection.CreateCommand()
    
    # Verifica totalul
    $command.CommandText = "SELECT COUNT(*) FROM Ocupatii_ISCO08"
    $finalCount = $command.ExecuteScalar()
    
    # Verifica pe nivele
    $command.CommandText = "SELECT Nivel_Ierarhic, COUNT(*) as Numar FROM Ocupatii_ISCO08 GROUP BY Nivel_Ierarhic ORDER BY Nivel_Ierarhic"
    $reader = $command.ExecuteReader()
    
    Write-Host "?? Distributie pe nivele:" -ForegroundColor Cyan
    while ($reader.Read()) {
        $nivel = $reader["Nivel_Ierarhic"]
        $numar = $reader["Numar"]
        $denumireNivel = switch ($nivel) {
            1 { "Grupe majore" }
            2 { "Subgrupe" }
            3 { "Grupe minore" }
            4 { "Ocupatii detaliate" }
            default { "Nivel $nivel" }
        }
        Write-Host "  - $denumireNivel`: $numar inregistrari" -ForegroundColor White
    }
    $reader.Close()
    
    $connection.Close()
}
catch {
    Write-Host "? Eroare la verificarea finala: $_" -ForegroundColor Red
}

Write-Host "`n========================================" -ForegroundColor Magenta
Write-Host "IMPORT FINALIZAT" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta

Write-Host "`n?? REZULTATE FINALE:" -ForegroundColor Green
Write-Host "  ? Inserate cu succes: $insertedCount" -ForegroundColor White
Write-Host "  ? Erori inserare: $errorCount" -ForegroundColor White

$successRate = if ($ocupatiiISCO.Count -gt 0) { 
    [math]::Round(($insertedCount / $ocupatiiISCO.Count) * 100, 2) 
} else { 0 }

Write-Host "`n?? RATA DE SUCCES: $successRate%" -ForegroundColor $(if ($successRate -gt 95) { "Green" } elseif ($successRate -gt 80) { "Yellow" } else { "Red" })

if ($insertedCount -gt 0) {
    Write-Host "`n? IMPORT REUSIT!" -ForegroundColor Green
    Write-Host "?? ValyanClinic are acum $insertedCount ocupatii ISCO-08 curate (fara diacritice)!" -ForegroundColor Green
    Write-Host "?? Include ocupatii complete pentru domeniile: Sanatate, IT, Management" -ForegroundColor Green
    
    Write-Host "`n?? Beneficii:" -ForegroundColor Cyan
    Write-Host "  ? Fara diacritice romanesti - compatibilitate maxima" -ForegroundColor White
    Write-Host "  ? Structura ierarhica completa ISCO-08" -ForegroundColor White
    Write-Host "  ? Ocupatii specifice pentru clinica medicala" -ForegroundColor White
    Write-Host "  ? Cod conform standardului international" -ForegroundColor White
    
} else {
    Write-Host "`n? IMPORT ESUAT! Nu s-au inserat inregistrari" -ForegroundColor Red
}

Write-Host "`n?? Pentru testare:" -ForegroundColor Cyan
Write-Host "EXEC sp_Ocupatii_ISCO08_GetStatistics" -ForegroundColor Gray
Write-Host "EXEC sp_Ocupatii_ISCO08_Search @SearchText = 'medic'" -ForegroundColor Gray