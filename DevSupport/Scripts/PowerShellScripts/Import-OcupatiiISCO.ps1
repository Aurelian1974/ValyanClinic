# ========================================
# Script pentru Import Ocupatii ISCO-08 din XML
# ValyanClinic - ISCO Data Import
# ========================================

param(
    [Parameter(Mandatory=$false)]
    [string]$XmlFilePath = "isco-08-ocupatii-2024.xml",
    [Parameter(Mandatory=$false)]
    [string]$ConfigPath = "..\..\..\ValyanClinic\appsettings.json"
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "IMPORT OCUPATII ISCO-08 DIN XML" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# ========================================
# VERIFICARI PRELIMINARE
# ========================================
Write-Host "`n[1/5] Verificari preliminare..." -ForegroundColor Yellow

if (-not (Test-Path $XmlFilePath)) {
    Write-Host "? Eroare: Fisierul XML nu exista: $XmlFilePath" -ForegroundColor Red
    Write-Host "Descarca fisierul din:" -ForegroundColor Yellow
    Write-Host "https://data.gov.ro/dataset/695974d3-4be3-4bbe-a56a-bb639ad908e2/resource/cc7db3b5-da8a-4eaa-afcc-514dd373eac6/download/isco-08-lista-alfabetica-ocupatii-2024.xml" -ForegroundColor Gray
    exit 1
}

if (-not (Test-Path $ConfigPath)) {
    Write-Host "? Eroare: Fisierul de configuratie nu exista: $ConfigPath" -ForegroundColor Red
    exit 1
}

# Citire configuratie
try {
    $config = Get-Content $ConfigPath -Raw | ConvertFrom-Json
    $connectionString = $config.ConnectionStrings.DefaultConnection
    Write-Host "? Connection string incarcat" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare la citirea configuratiei: $_" -ForegroundColor Red
    exit 1
}

# ========================================
# INCARCAREA FISIERULUI XML
# ========================================
Write-Host "`n[2/5] Incarcarea fisierului XML..." -ForegroundColor Yellow

try {
    [xml]$xmlData = Get-Content $XmlFilePath -Encoding UTF8
    Write-Host "? Fisier XML incarcat cu succes" -ForegroundColor Green
    
    # Detecteaza structura XML-ului
    $rootNode = $xmlData.DocumentElement
    Write-Host "  Root node: $($rootNode.Name)" -ForegroundColor Gray
    Write-Host "  Namespace: $($rootNode.NamespaceURI)" -ForegroundColor Gray
}
catch {
    Write-Host "? Eroare la incarcarea XML: $_" -ForegroundColor Red
    exit 1
}

# ========================================
# CONECTAREA LA BAZA DE DATE
# ========================================
Write-Host "`n[3/5] Conectarea la baza de date..." -ForegroundColor Yellow

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()
    Write-Host "? Conectare reusita la baza de date" -ForegroundColor Green
}
catch {
    Write-Host "? Eroare la conectare: $_" -ForegroundColor Red
    exit 1
}

# ========================================
# FUNCTII HELPER
# ========================================
function Insert-OcupationISCO {
    param(
        [string]$CodISCO,
        [string]$Denumire,
        [string]$DenumireEN,
        [int]$NivelIerarhic,
        [string]$CodParinte,
        [string]$GrupaMajora,
        [string]$GrupaMajoraDenumire,
        [string]$Subgrupa,
        [string]$SubgrupaDenumire,
        [string]$GrupaMinora,
        [string]$GrupaMinoraDenumire,
        [string]$Descriere,
        [System.Data.SqlClient.SqlConnection]$Connection
    )
    
    $query = @"
INSERT INTO dbo.Ocupatii_ISCO08 (
    [Cod_ISCO], [Denumire_Ocupatie], [Denumire_Ocupatie_EN], [Nivel_Ierarhic],
    [Cod_Parinte], [Grupa_Majora], [Grupa_Majora_Denumire],
    [Subgrupa], [Subgrupa_Denumire], [Grupa_Minora], [Grupa_Minora_Denumire],
    [Descriere]
) VALUES (
    @CodISCO, @Denumire, @DenumireEN, @NivelIerarhic,
    @CodParinte, @GrupaMajora, @GrupaMajoraDenumire,
    @Subgrupa, @SubgrupaDenumire, @GrupaMinora, @GrupaMinoraDenumire,
    @Descriere
)
"@
    
    $command = $Connection.CreateCommand()
    $command.CommandText = $query
    
    $command.Parameters.AddWithValue("@CodISCO", $CodISCO) | Out-Null
    $command.Parameters.AddWithValue("@Denumire", $Denumire) | Out-Null
    $command.Parameters.AddWithValue("@DenumireEN", [System.DBNull]::Value) | Out-Null
    $command.Parameters.AddWithValue("@NivelIerarhic", $NivelIerarhic) | Out-Null
    $command.Parameters.AddWithValue("@CodParinte", if($CodParinte) { $CodParinte } else { [System.DBNull]::Value }) | Out-Null
    $command.Parameters.AddWithValue("@GrupaMajora", if($GrupaMajora) { $GrupaMajora } else { [System.DBNull]::Value }) | Out-Null
    $command.Parameters.AddWithValue("@GrupaMajoraDenumire", if($GrupaMajoraDenumire) { $GrupaMajoraDenumire } else { [System.DBNull]::Value }) | Out-Null
    $command.Parameters.AddWithValue("@Subgrupa", if($Subgrupa) { $Subgrupa } else { [System.DBNull]::Value }) | Out-Null
    $command.Parameters.AddWithValue("@SubgrupaDenumire", if($SubgrupaDenumire) { $SubgrupaDenumire } else { [System.DBNull]::Value }) | Out-Null
    $command.Parameters.AddWithValue("@GrupaMinora", if($GrupaMinora) { $GrupaMinora } else { [System.DBNull]::Value }) | Out-Null
    $command.Parameters.AddWithValue("@GrupaMinoraDenumire", if($GrupaMinoraDenumire) { $GrupaMinoraDenumire } else { [System.DBNull]::Value }) | Out-Null
    $command.Parameters.AddWithValue("@Descriere", if($Descriere) { $Descriere } else { [System.DBNull]::Value }) | Out-Null
    
    try {
        $command.ExecuteNonQuery() | Out-Null
        return $true
    }
    catch {
        Write-Host "    ? Eroare la inserarea $CodISCO : $_" -ForegroundColor Red
        return $false
    }
}

function Get-NivelIerarhic {
    param([string]$CodISCO)
    
    $length = $CodISCO.Length
    switch ($length) {
        1 { return 1 } # Grupa majora
        2 { return 2 } # Subgrupa
        3 { return 3 } # Grupa minora
        4 { return 4 } # Ocupatie
        default { return 4 }
    }
}

function Get-CodParinte {
    param([string]$CodISCO)
    
    $length = $CodISCO.Length
    if ($length -le 1) {
        return $null
    }
    
    return $CodISCO.Substring(0, $length - 1)
}

# ========================================
# PROCESAREA DATELOR XML
# ========================================
Write-Host "`n[4/5] Procesarea datelor XML..." -ForegroundColor Yellow

# Sterge datele existente (optional)
$clearData = Read-Host "Stergi datele existente din tabela? (Y/N)"
if ($clearData -eq "Y" -or $clearData -eq "y") {
    try {
        $command = $connection.CreateCommand()
        $command.CommandText = "DELETE FROM dbo.Ocupatii_ISCO08"
        $command.ExecuteNonQuery() | Out-Null
        Write-Host "? Date existente sterse" -ForegroundColor Green
    }
    catch {
        Write-Host "? Eroare la stergerea datelor: $_" -ForegroundColor Red
    }
}

# Procesare bazata pe structura XML detectata
# NOTA: Aceasta sectiune trebuie adaptata in functie de structura reala a XML-ului
$insertedCount = 0
$errorCount = 0

try {
    # Exemplu de procesare - trebuie adaptat la structura reala
    # Detectez automat nodurile care contin informatii despre ocupatii
    $ocupatiiNodes = $xmlData.SelectNodes("//ocupatie") # Adapteaza selectorul
    
    if ($ocupatiiNodes.Count -eq 0) {
        # Incearca alte selectoare comune
        $ocupatiiNodes = $xmlData.SelectNodes("//item") 
        if ($ocupatiiNodes.Count -eq 0) {
            $ocupatiiNodes = $xmlData.SelectNodes("//entry")
        }
        if ($ocupatiiNodes.Count -eq 0) {
            $ocupatiiNodes = $xmlData.SelectNodes("//row")
        }
    }
    
    Write-Host "? Gasite $($ocupatiiNodes.Count) noduri de procesat" -ForegroundColor Green
    
    foreach ($node in $ocupatiiNodes) {
        try {
            # Extrage informatiile din nod (adapteaza la structura reala)
            $codISCO = $node.cod ?? $node.GetAttribute("cod") ?? $node.isco_code ?? ""
            $denumire = $node.denumire ?? $node.GetAttribute("denumire") ?? $node.title ?? $node.InnerText ?? ""
            $descriere = $node.descriere ?? $node.GetAttribute("descriere") ?? $node.description ?? ""
            
            if ($codISCO -and $denumire) {
                $nivelIerarhic = Get-NivelIerarhic -CodISCO $codISCO
                $codParinte = Get-CodParinte -CodISCO $codISCO
                
                # Determina grupele bazat pe cod
                $grupaMajora = if ($codISCO.Length -ge 1) { $codISCO.Substring(0, 1) } else { $null }
                $subgrupa = if ($codISCO.Length -ge 2) { $codISCO.Substring(0, 2) } else { $null }
                $grupaMinora = if ($codISCO.Length -ge 3) { $codISCO.Substring(0, 3) } else { $null }
                
                $success = Insert-OcupationISCO -CodISCO $codISCO -Denumire $denumire -DenumireEN $null -NivelIerarhic $nivelIerarhic -CodParinte $codParinte -GrupaMajora $grupaMajora -GrupaMajoraDenumire $null -Subgrupa $subgrupa -SubgrupaDenumire $null -GrupaMinora $grupaMinora -GrupaMinoraDenumire $null -Descriere $descriere -Connection $connection
                
                if ($success) {
                    $insertedCount++
                    if ($insertedCount % 100 -eq 0) {
                        Write-Host "  Procesate: $insertedCount ocupatii..." -ForegroundColor Gray
                    }
                } else {
                    $errorCount++
                }
            }
        }
        catch {
            Write-Host "  ? Eroare la procesarea nodului: $_" -ForegroundColor Red
            $errorCount++
        }
    }
}
catch {
    Write-Host "? Eroare la procesarea XML: $_" -ForegroundColor Red
}

# ========================================
# FINALIZARE SI RAPORTARE
# ========================================
Write-Host "`n[5/5] Finalizare import..." -ForegroundColor Yellow

$connection.Close()

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "IMPORT FINALIZAT" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "`nRezultate import:" -ForegroundColor Green
Write-Host "  ? Ocupatii inserate: $insertedCount" -ForegroundColor White
Write-Host "  ? Erori: $errorCount" -ForegroundColor $(if($errorCount -gt 0){'Red'}else{'Green'})

if ($insertedCount -gt 0) {
    Write-Host "`n? Import realizat cu succes!" -ForegroundColor Green
    Write-Host "Poti acum sa utilizezi stored procedures pentru a accesa datele:" -ForegroundColor Cyan
    Write-Host "  - sp_Ocupatii_ISCO08_GetAll" -ForegroundColor White
    Write-Host "  - sp_Ocupatii_ISCO08_Search" -ForegroundColor White
    Write-Host "  - sp_Ocupatii_ISCO08_GetStatistics" -ForegroundColor White
} else {
    Write-Host "`n?? Nu au fost inserate date!" -ForegroundColor Yellow
    Write-Host "Verifica structura fisierului XML si adapteaza script-ul." -ForegroundColor Yellow
}

Write-Host "`nPentru a vedea statisticile, ruleaza:" -ForegroundColor Cyan
Write-Host "EXEC sp_Ocupatii_ISCO08_GetStatistics" -ForegroundColor Gray