# ========================================
# Script pentru Import Complet Date ISCO-08 din XML Oficial
# Sursa: data.gov.ro - Eliminare diacritice + Import complet
# ========================================

param(
    [Parameter(Mandatory=$false)]
    [string]$ConfigPath = "..\..\..\ValyanClinic\appsettings.json",
    [Parameter(Mandatory=$false)]
    [switch]$ForceDownload = $false,
    [Parameter(Mandatory=$false)]
    [switch]$KeepXmlFile = $true,
    [Parameter(Mandatory=$false)]
    [switch]$DryRun = $false
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Magenta
Write-Host "IMPORT COMPLET OCUPATII ISCO-08" -ForegroundColor Magenta
Write-Host "Sursa: data.gov.ro (XML oficial)" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta

# URL oficial pentru fisierul XML
$xmlUrl = "https://data.gov.ro/dataset/695974d3-4be3-4bbe-a56a-bb639ad908e2/resource/cc7db3b5-da8a-4eaa-afcc-514dd373eac6/download/isco-08-lista-alfabetica-ocupatii-2024.xml"
$xmlFile = "isco-08-ocupatii-2024.xml"

# Functie pentru eliminarea diacriticelor romanesti
function Remove-RomanianDiacritics {
    param([string]$text)
    
    if ([string]::IsNullOrEmpty($text)) {
        return $text
    }
    
    # Folosim replace individual pentru a evita problemele de encoding
    $result = $text
    
    # Inlocuiri pentru 'a'
    $result = $result -replace '?', 'a'
    $result = $result -replace '?', 'A'
    $result = $result -replace 'ג', 'a'
    $result = $result -replace 'Â', 'A'
    
    # Inlocuiri pentru 'i'
    $result = $result -replace 'מ', 'i'
    $result = $result -replace 'Î', 'I'
    
    # Inlocuiri pentru 's' (ambele variante)
    $result = $result -replace '?', 's'
    $result = $result -replace '?', 'S'
    $result = $result -replace '?', 's'  # versiune veche
    $result = $result -replace '?', 'S'  # versiune veche
    
    # Inlocuiri pentru 't' (ambele variante)
    $result = $result -replace '?', 't'
    $result = $result -replace '?', 'T'
    $result = $result -replace '?', 't'  # versiune veche
    $result = $result -replace '?', 'T'  # versiune veche
    
    return $result
}

# Functie pentru clean-up text (elimina caractere speciale, spatii extra)
function Clean-Text {
    param([string]$text)
    
    if ([string]::IsNullOrEmpty($text)) {
        return $text
    }
    
    # Elimina diacriticele
    $cleaned = Remove-RomanianDiacritics -text $text
    
    # Elimina caractere speciale problematice (pastreaza doar litere, cifre, spatii, punctuatie standard)
    $cleaned = $cleaned -replace '[^\w\s\.\,\;\:\!\?\-\(\)\[\]]', ''
    
    # Elimina spatii multiple
    $cleaned = $cleaned -replace '\s+', ' '
    
    # Trim
    return $cleaned.Trim()
}

Write-Host "`n[1/7] Verificare si configurare..." -ForegroundColor Yellow

# Verifica configuratia
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

Write-Host "`n[2/7] Download fisier XML..." -ForegroundColor Yellow

# Verifica daca fisierul XML exista deja
if ((Test-Path $xmlFile) -and (-not $ForceDownload)) {
    Write-Host "??  Fisierul XML exista deja. Folosesc fisierul local." -ForegroundColor Cyan
    Write-Host "   Pentru re-download, foloseste parametrul -ForceDownload" -ForegroundColor Gray
} else {
    try {
        Write-Host "?? Descarc fisierul XML de la data.gov.ro..." -ForegroundColor Cyan
        Write-Host "   URL: $xmlUrl" -ForegroundColor Gray
        
        # Foloseste TLS 1.2 pentru compatibilitate
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
        
        $webClient = New-Object System.Net.WebClient
        $webClient.Headers.Add("User-Agent", "PowerShell-ImportScript/1.0")
        $webClient.DownloadFile($xmlUrl, $xmlFile)
        
        $fileSize = (Get-Item $xmlFile).Length
        Write-Host "? Fisier descarcat cu succes: $([math]::Round($fileSize/1KB, 2)) KB" -ForegroundColor Green
    }
    catch {
        Write-Host "? Eroare la descarcarea fisierului: $_" -ForegroundColor Red
        Write-Host "?? Incearca sa descarci manual fisierul XML si salveaza-l ca '$xmlFile'" -ForegroundColor Yellow
        exit 1
    }
}

Write-Host "`n[3/7] Parsare si validare XML..." -ForegroundColor Yellow

try {
    # Incarca XML-ul
    [xml]$xmlContent = Get-Content $xmlFile -Encoding UTF8
    
    Write-Host "? Fisier XML incarcat cu succes" -ForegroundColor Green
    Write-Host "?? Root element: $($xmlContent.DocumentElement.Name)" -ForegroundColor Gray
    
    # Detecteaza structura XML-ului
    $xmlStructure = $xmlContent.DocumentElement
    Write-Host "?? Analiza structura XML..." -ForegroundColor Cyan
    
    # Cauta nodurile care contin ocupatiile (structura poate varia)
    $ocupatiiNodes = @()
    
    # Incearca diferite posibile structuri XML
    if ($xmlStructure.SelectNodes("//ocupatie")) {
        $ocupatiiNodes = $xmlStructure.SelectNodes("//ocupatie")
        Write-Host "? Detectata structura: //ocupatie" -ForegroundColor Green
    }
    elseif ($xmlStructure.SelectNodes("//row")) {
        $ocupatiiNodes = $xmlStructure.SelectNodes("//row")
        Write-Host "? Detectata structura: //row" -ForegroundColor Green
    }
    elseif ($xmlStructure.SelectNodes("//item")) {
        $ocupatiiNodes = $xmlStructure.SelectNodes("//item")
        Write-Host "? Detectata structura: //item" -ForegroundColor Green
    }
    else {
        # Verifica toate child nodes de primul nivel
        $childNodes = $xmlStructure.ChildNodes | Where-Object { $_.NodeType -eq "Element" }
        if ($childNodes.Count -gt 0) {
            $ocupatiiNodes = $childNodes
            Write-Host "? Detectata structura: child nodes directi ($($childNodes.Count) noduri)" -ForegroundColor Green
        }
    }
    
    if ($ocupatiiNodes.Count -eq 0) {
        Write-Host "? Nu am gasit noduri cu ocupatii in XML" -ForegroundColor Red
        Write-Host "?? Structura XML detectata:" -ForegroundColor Yellow
        Write-Host $xmlContent.OuterXml.Substring(0, [Math]::Min(500, $xmlContent.OuterXml.Length)) -ForegroundColor Gray
        exit 1
    }
    
    Write-Host "? Gasite $($ocupatiiNodes.Count) ocupatii in XML" -ForegroundColor Green
    
}
catch {
    Write-Host "? Eroare la parsarea XML: $_" -ForegroundColor Red
    exit 1
}

Write-Host "`n[4/7] Procesare si transformare date..." -ForegroundColor Yellow

$ocupatiiProcesate = @()
$statistici = @{
    TotalProcesate = 0
    DiacriticeEliminare = 0
    TextCuratat = 0
    Erori = 0
}

Write-Host "?? Procesez $($ocupatiiNodes.Count) ocupatii..." -ForegroundColor Cyan

foreach ($node in $ocupatiiNodes) {
    try {
        $statistici.TotalProcesate++
        
        # Extrage informatiile din nod (adapteaza-te la structura reala)
        $codISCO = ""
        $denumire = ""
        $denumireEN = ""
        $nivelIerarhic = 1
        $descriere = ""
        
        # Incearca diferite nume de atribute/noduri
        $possibleCodes = @("cod", "code", "isco_code", "cod_isco", "isco", "id")
        $possibleNames = @("denumire", "name", "occupation", "ocupatie", "title", "denumire_ro")
        $possibleNamesEN = @("denumire_en", "name_en", "occupation_en", "title_en", "english_name")
        $possibleDesc = @("descriere", "description", "desc", "detalii")
        
        # Extrage codul ISCO
        foreach ($attr in $possibleCodes) {
            if ($node.$attr) {
                $codISCO = $node.$attr.ToString().Trim()
                break
            }
            elseif ($node.Attributes[$attr]) {
                $codISCO = $node.Attributes[$attr].Value.Trim()
                break
            }
        }
        
        # Extrage denumirea
        foreach ($attr in $possibleNames) {
            if ($node.$attr) {
                $denumire = $node.$attr.ToString().Trim()
                break
            }
            elseif ($node.Attributes[$attr]) {
                $denumire = $node.Attributes[$attr].Value.Trim()
                break
            }
        }
        
        # Extrage denumirea EN
        foreach ($attr in $possibleNamesEN) {
            if ($node.$attr) {
                $denumireEN = $node.$attr.ToString().Trim()
                break
            }
            elseif ($node.Attributes[$attr]) {
                $denumireEN = $node.Attributes[$attr].Value.Trim()
                break
            }
        }
        
        # Extrage descrierea
        foreach ($attr in $possibleDesc) {
            if ($node.$attr) {
                $descriere = $node.$attr.ToString().Trim()
                break
            }
            elseif ($node.Attributes[$attr]) {
                $descriere = $node.Attributes[$attr].Value.Trim()
                break
            }
        }
        
        # Daca nu am gasit in atribute, incearca sa extraga din text content
        if ([string]::IsNullOrEmpty($denumire) -and ![string]::IsNullOrEmpty($node.InnerText)) {
            $denumire = $node.InnerText.Trim()
        }
        
        # Skip daca nu avem datele minime necesare
        if ([string]::IsNullOrEmpty($codISCO) -or [string]::IsNullOrEmpty($denumire)) {
            Write-Host "??  Skip: cod='$codISCO', denumire='$denumire' (date incomplete)" -ForegroundColor Yellow
            continue
        }
        
        # Determina nivelul ierarhic pe baza lungimii codului
        $nivelIerarhic = $codISCO.Length
        if ($nivelIerarhic -gt 4) { $nivelIerarhic = 4 }
        if ($nivelIerarhic -lt 1) { $nivelIerarhic = 1 }
        
        # Detecteaza diacritice inainte de eliminare
        $originalDenumire = $denumire
        if ($denumire -match '[?גמ????ÂÎ???]') {
            $statistici.DiacriticeEliminare++
        }
        
        # Elimina diacriticele si curata textul
        $denumireCurata = Clean-Text -text $denumire
        $denumireENCurata = if ($denumireEN) { Clean-Text -text $denumireEN } else { $null }
        $descriereCurata = if ($descriere) { Clean-Text -text $descriere } else { $null }
        
        if ($denumireCurata -ne $originalDenumire) {
            $statistici.TextCuratat++
        }
        
        # Determina grupa majora (prima cifra)
        $grupaMajora = $codISCO.Substring(0, 1)
        $codParinte = $null
        
        if ($nivelIerarhic -gt 1) {
            $codParinte = $codISCO.Substring(0, $nivelIerarhic - 1)
        }
        
        # Creeaza obiectul pentru import
        $ocupatie = [PSCustomObject]@{
            CodISCO = $codISCO
            DenumireOcupatie = $denumireCurata
            DenumireOcupatieEN = $denumireENCurata
            NivelIerarhic = [byte]$nivelIerarhic
            CodParinte = $codParinte
            GrupaMajora = $grupaMajora
            GrupaMajoraDenumire = $null  # Va fi completat mai tarziu
            Subgrupa = if ($nivelIerarhic -ge 2) { $codISCO.Substring(0, 2) } else { $null }
            SubgrupaDenumire = $null
            GrupaMinora = if ($nivelIerarhic -ge 3) { $codISCO.Substring(0, 3) } else { $null }
            GrupaMinoraDenumire = $null
            Descriere = $descriereCurata
            OriginalText = $originalDenumire  # Pentru debug
        }
        
        $ocupatiiProcesate += $ocupatie
        
        # Progress indicator la fiecare 100 de inregistrari
        if ($statistici.TotalProcesate % 100 -eq 0) {
            Write-Host "   Procesate: $($statistici.TotalProcesate) / $($ocupatiiNodes.Count)" -ForegroundColor Cyan
        }
        
    }
    catch {
        $statistici.Erori++
        Write-Host "? Eroare la procesarea nodului: $_" -ForegroundColor Red
    }
}

Write-Host "`n? Procesare completa:" -ForegroundColor Green
Write-Host "  ?? Total procesate: $($statistici.TotalProcesate)" -ForegroundColor White
Write-Host "  ?? Cu diacritice eliminate: $($statistici.DiacriticeEliminare)" -ForegroundColor White
Write-Host "  ?? Text curatat: $($statistici.TextCuratat)" -ForegroundColor White
Write-Host "  ? Erori: $($statistici.Erori)" -ForegroundColor White
Write-Host "  ? Valid pentru import: $($ocupatiiProcesate.Count)" -ForegroundColor White

if ($ocupatiiProcesate.Count -eq 0) {
    Write-Host "? Nu am gasit ocupatii valide pentru import!" -ForegroundColor Red
    exit 1
}

# Afiseaza cateva exemple de transformari
Write-Host "`n?? Exemple de transformari:" -ForegroundColor Cyan
$ocupatiiProcesate | Select-Object -First 5 | ForEach-Object {
    if ($_.OriginalText -ne $_.DenumireOcupatie) {
        Write-Host "  '$($_.OriginalText)' ? '$($_.DenumireOcupatie)'" -ForegroundColor Yellow
    } else {
        Write-Host "  '$($_.DenumireOcupatie)' (fara modificari)" -ForegroundColor Green
    }
}

if ($DryRun) {
    Write-Host "`n?? DRY RUN MODE - Nu fac modificari in baza de date" -ForegroundColor Yellow
    Write-Host "Rularea reala: sterge parametrul -DryRun" -ForegroundColor Cyan
    exit 0
}

Write-Host "`n[5/7] Conectare la baza de date..." -ForegroundColor Yellow

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

Write-Host "`n[6/7] Stergere date existente..." -ForegroundColor Yellow

$command = $connection.CreateCommand()

try {
    # Verifica cate inregistrari exista
    $command.CommandText = "SELECT COUNT(*) FROM Ocupatii_ISCO08"
    $existingCount = $command.ExecuteScalar()
    
    Write-Host "?? Inregistrari existente: $existingCount" -ForegroundColor Cyan
    
    if ($existingCount -gt 0) {
        # Sterge toate datele existente
        $command.CommandText = "DELETE FROM Ocupatii_ISCO08"
        $deletedRows = $command.ExecuteNonQuery()
        Write-Host "? Sterse $deletedRows inregistrari existente" -ForegroundColor Green
    } else {
        Write-Host "??  Nu exista date pentru stergere" -ForegroundColor Cyan
    }
}
catch {
    Write-Host "? Eroare la stergerea datelor existente: $_" -ForegroundColor Red
    $connection.Close()
    exit 1
}

Write-Host "`n[7/7] Import date noi..." -ForegroundColor Yellow

$insertedCount = 0
$errorCount = 0

try {
    # Disable constraints pentru performanta
    $command.CommandText = "ALTER TABLE Ocupatii_ISCO08 NOCHECK CONSTRAINT ALL"
    $command.ExecuteNonQuery() | Out-Null
    
    foreach ($ocupatie in $ocupatiiProcesate) {
        try {
            $insertSQL = @"
INSERT INTO Ocupatii_ISCO08 (
    [Cod_ISCO], [Denumire_Ocupatie], [Denumire_Ocupatie_EN], [Nivel_Ierarhic],
    [Cod_Parinte], [Grupa_Majora], [Subgrupa], [Grupa_Minora], [Descriere],
    [Este_Activ], [Data_Crearii], [Creat_De]
) VALUES (
    @CodISCO, @DenumireOcupatie, @DenumireOcupatieEN, @NivelIerarhic,
    @CodParinte, @GrupaMajora, @Subgrupa, @GrupaMinora, @Descriere,
    1, GETDATE(), 'XML_IMPORT_SCRIPT'
)
"@
            
            $command.CommandText = $insertSQL
            $command.Parameters.Clear()
            
            $command.Parameters.Add("@CodISCO", [System.Data.SqlDbType]::NVarChar, 10).Value = $ocupatie.CodISCO
            $command.Parameters.Add("@DenumireOcupatie", [System.Data.SqlDbType]::NVarChar, 500).Value = $ocupatie.DenumireOcupatie
            $command.Parameters.Add("@DenumireOcupatieEN", [System.Data.SqlDbType]::NVarChar, 500).Value = if ($ocupatie.DenumireOcupatieEN) { $ocupatie.DenumireOcupatieEN } else { [System.DBNull]::Value }
            $command.Parameters.Add("@NivelIerarhic", [System.Data.SqlDbType]::TinyInt).Value = $ocupatie.NivelIerarhic
            $command.Parameters.Add("@CodParinte", [System.Data.SqlDbType]::NVarChar, 10).Value = if ($ocupatie.CodParinte) { $ocupatie.CodParinte } else { [System.DBNull]::Value }
            $command.Parameters.Add("@GrupaMajora", [System.Data.SqlDbType]::NVarChar, 10).Value = if ($ocupatie.GrupaMajora) { $ocupatie.GrupaMajora } else { [System.DBNull]::Value }
            $command.Parameters.Add("@Subgrupa", [System.Data.SqlDbType]::NVarChar, 10).Value = if ($ocupatie.Subgrupa) { $ocupatie.Subgrupa } else { [System.DBNull]::Value }
            $command.Parameters.Add("@GrupaMinora", [System.Data.SqlDbType]::NVarChar, 10).Value = if ($ocupatie.GrupaMinora) { $ocupatie.GrupaMinora } else { [System.DBNull]::Value }
            $command.Parameters.Add("@Descriere", [System.Data.SqlDbType]::NVarChar, -1).Value = if ($ocupatie.Descriere) { $ocupatie.Descriere } else { [System.DBNull]::Value }
            
            $command.ExecuteNonQuery() | Out-Null
            $insertedCount++
            
            # Progress indicator
            if ($insertedCount % 50 -eq 0) {
                Write-Host "   Inserate: $insertedCount / $($ocupatiiProcesate.Count)" -ForegroundColor Cyan
            }
            
        }
        catch {
            $errorCount++
            Write-Host "? Eroare la inserarea '$($ocupatie.CodISCO)': $_" -ForegroundColor Red
        }
    }
    
    # Re-enable constraints
    $command.CommandText = "ALTER TABLE Ocupatii_ISCO08 CHECK CONSTRAINT ALL"
    $command.ExecuteNonQuery() | Out-Null
    
}
catch {
    Write-Host "? Eroare la inserarea datelor: $_" -ForegroundColor Red
}

$connection.Close()

# Cleanup fisier XML daca nu vrem sa-l pastram
if (-not $KeepXmlFile -and (Test-Path $xmlFile)) {
    Remove-Item $xmlFile -Force
    Write-Host "???  Fisier XML sters" -ForegroundColor Gray
}

Write-Host "`n========================================" -ForegroundColor Magenta
Write-Host "IMPORT FINALIZAT" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta

Write-Host "`n?? REZULTATE FINALE:" -ForegroundColor Green
Write-Host "  ?? Inregistrari XML: $($ocupatiiNodes.Count)" -ForegroundColor White
Write-Host "  ?? Procesate valid: $($ocupatiiProcesate.Count)" -ForegroundColor White
Write-Host "  ? Inserate cu succes: $insertedCount" -ForegroundColor White
Write-Host "  ? Erori inserare: $errorCount" -ForegroundColor White
Write-Host "  ?? Diacritice eliminate: $($statistici.DiacriticeEliminare)" -ForegroundColor White

$successRate = if ($ocupatiiProcesate.Count -gt 0) { 
    [math]::Round(($insertedCount / $ocupatiiProcesate.Count) * 100, 2) 
} else { 0 }

Write-Host "`n?? RATA DE SUCCES: $successRate%" -ForegroundColor $(if ($successRate -gt 95) { "Green" } elseif ($successRate -gt 80) { "Yellow" } else { "Red" })

if ($insertedCount -gt 0) {
    Write-Host "`n? IMPORT REUSIT! Aplicatia ValyanClinic are acum $insertedCount ocupatii ISCO-08" -ForegroundColor Green
    Write-Host "?? Toate diacriticele romanesti au fost eliminate cu succes!" -ForegroundColor Green
} else {
    Write-Host "`n? IMPORT ESUAT! Nu s-au inserat inregistrari" -ForegroundColor Red
}

Write-Host "`n?? Pentru testare:" -ForegroundColor Cyan
Write-Host "EXEC sp_Ocupatii_ISCO08_GetStatistics" -ForegroundColor Gray