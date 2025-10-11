# ========================================
# Script Complet pentru Creare Tabel ?i Import Date ISCO-08
# ValyanClinic - Complete ISCO Setup
# ========================================

param(
    [Parameter(Mandatory=$false)]
    [string]$ConfigPath = "..\..\..\ValyanClinic\appsettings.json",
    [Parameter(Mandatory=$false)]
    [switch]$CreateTable = $true,
    [Parameter(Mandatory=$false)]
    [switch]$InsertSampleData = $true,
    [Parameter(Mandatory=$false)]
    [switch]$DownloadRealData = $false
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Magenta
Write-Host "SETUP COMPLET OCUPATII ISCO-08" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta

# ========================================
# VERIFICARI PRELIMINARE
# ========================================
Write-Host "`n[1/6] Verificari preliminare..." -ForegroundColor Yellow

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
# CONECTAREA LA BAZA DE DATE
# ========================================
Write-Host "`n[2/6] Conectarea la baza de date..." -ForegroundColor Yellow

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()
    Write-Host "? Conectare reusita la baza de date" -ForegroundColor Green
    
    # Extrage numele bazei de date
    $command = $connection.CreateCommand()
    $command.CommandText = "SELECT DB_NAME()"
    $databaseName = $command.ExecuteScalar()
    Write-Host "?? Database: $databaseName" -ForegroundColor Gray
}
catch {
    Write-Host "? Eroare la conectare: $_" -ForegroundColor Red
    exit 1
}

# ========================================
# VERIFICARE TABEL EXISTENT
# ========================================
Write-Host "`n[3/6] Verificare tabel existent..." -ForegroundColor Yellow

$command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Ocupatii_ISCO08'"
$tableExists = $command.ExecuteScalar()

if ($tableExists -gt 0) {
    Write-Host "??  Tabelul Ocupatii_ISCO08 exista deja" -ForegroundColor Cyan
    
    $command.CommandText = "SELECT COUNT(*) FROM Ocupatii_ISCO08"
    $recordCount = $command.ExecuteScalar()
    Write-Host "?? Inregistrari existente: $recordCount" -ForegroundColor Gray
    
    if ($recordCount -gt 0) {
        $overwrite = Read-Host "??  Tabelul contine deja $recordCount inregistrari. Stergi si recreezi? (Y/N)"
        if ($overwrite -ne "Y" -and $overwrite -ne "y") {
            Write-Host "?? Operatie anulata de utilizator" -ForegroundColor Yellow
            $connection.Close()
            exit 0
        }
        
        # Sterge datele existente
        Write-Host "???  Sterg datele existente..." -ForegroundColor Yellow
        $command.CommandText = "DELETE FROM Ocupatii_ISCO08"
        $command.ExecuteNonQuery() | Out-Null
        Write-Host "? Date existente sterse" -ForegroundColor Green
    }
    
    $CreateTable = $false # Nu mai recreaza tabelul daca exista
} else {
    Write-Host "?? Tabelul nu exista - va fi creat" -ForegroundColor Yellow
}

# ========================================
# CREARE TABEL (daca este necesar)
# ========================================
if ($CreateTable -and $tableExists -eq 0) {
    Write-Host "`n[4/6] Creare tabel Ocupatii_ISCO08..." -ForegroundColor Yellow
    
    $createTableScript = Get-Content "..\..\Database\TableStructure\Ocupatii_ISCO08_Structure.sql" -Raw
    
    try {
        # Executia scriptului SQL poate sa aiba mai multe comenzi separate prin GO
        $sqlCommands = $createTableScript -split "\bGO\b" | Where-Object { $_.Trim() -ne "" }
        
        foreach ($sqlCommand in $sqlCommands) {
            $cleanCommand = $sqlCommand.Trim()
            if ($cleanCommand -and -not $cleanCommand.StartsWith("--") -and $cleanCommand -ne "GO") {
                try {
                    $command.CommandText = $cleanCommand
                    $command.ExecuteNonQuery() | Out-Null
                }
                catch {
                    # Ignora erorile pentru USE DATABASE si alte comenzi specifice
                    if (-not $_.Exception.Message.Contains("USE statement")) {
                        Write-Host "??  Warning la executia SQL: $($_.Exception.Message)" -ForegroundColor Yellow
                    }
                }
            }
        }
        
        Write-Host "? Tabel Ocupatii_ISCO08 creat cu succes" -ForegroundColor Green
    }
    catch {
        Write-Host "? Eroare la crearea tabelului: $_" -ForegroundColor Red
        $connection.Close()
        exit 1
    }
} else {
    Write-Host "`n[4/6] Saltare creare tabel (exista deja)..." -ForegroundColor Gray
}

# ========================================
# INSERARE DATE EXEMPLE
# ========================================
if ($InsertSampleData) {
    Write-Host "`n[5/6] Inserare date exemple..." -ForegroundColor Yellow
    
    $sampleDataScript = Get-Content "..\..\Database\TableStructure\Ocupatii_ISCO08_SampleData.sql" -Raw
    
    try {
        $sqlCommands = $sampleDataScript -split "\bGO\b" | Where-Object { $_.Trim() -ne "" }
        
        $insertedCount = 0
        foreach ($sqlCommand in $sqlCommands) {
            $cleanCommand = $sqlCommand.Trim()
            if ($cleanCommand -and -not $cleanCommand.StartsWith("--") -and $cleanCommand -ne "GO") {
                try {
                    $command.CommandText = $cleanCommand
                    if ($cleanCommand.ToUpper().Contains("INSERT")) {
                        $rowsAffected = $command.ExecuteNonQuery()
                        $insertedCount += $rowsAffected
                    } else {
                        $command.ExecuteNonQuery() | Out-Null
                    }
                }
                catch {
                    if (-not $_.Exception.Message.Contains("USE statement")) {
                        Write-Host "??  Warning la inserarea datelor: $($_.Exception.Message)" -ForegroundColor Yellow
                    }
                }
            }
        }
        
        Write-Host "? $insertedCount inregistrari exemple inserate" -ForegroundColor Green
    }
    catch {
        Write-Host "? Eroare la inserarea datelor exemple: $_" -ForegroundColor Red
    }
} else {
    Write-Host "`n[5/6] Saltare inserare date exemple..." -ForegroundColor Gray
}

# ========================================
# DOWNLOAD SI IMPORT DATE REALE (optional)
# ========================================
if ($DownloadRealData) {
    Write-Host "`n[6/6] Download si import date reale..." -ForegroundColor Yellow
    
    $xmlUrl = "https://data.gov.ro/dataset/695974d3-4be3-4bbe-a56a-bb639ad908e2/resource/cc7db3b5-da8a-4eaa-afcc-514dd373eac6/download/isco-08-lista-alfabetica-ocupatii-2024.xml"
    $xmlFile = "isco-08-ocupatii-2024.xml"
    
    try {
        Write-Host "?? Descarc fisierul XML..." -ForegroundColor Cyan
        Invoke-WebRequest -Uri $xmlUrl -OutFile $xmlFile -TimeoutSec 30
        
        if (Test-Path $xmlFile) {
            $fileSize = (Get-Item $xmlFile).Length
            Write-Host "? Fisier descarcat: $xmlFile ($fileSize bytes)" -ForegroundColor Green
            
            # Verifica daca fisierul este XML valid
            try {
                [xml]$testXml = Get-Content $xmlFile -Encoding UTF8
                Write-Host "? Fisier XML valid" -ForegroundColor Green
                
                # Aici ar trebui sa parsam XML-ul si sa insertam datele
                # Pentru moment, afisam doar informatii despre structura
                Write-Host "?? Root element: $($testXml.DocumentElement.Name)" -ForegroundColor Gray
                Write-Host "??  Pentru import complet, utilizeaza scriptul Import-OcupatiiISCO.ps1" -ForegroundColor Cyan
            }
            catch {
                Write-Host "??  Fisierul descarcat nu pare sa fie XML valid sau este in alt format" -ForegroundColor Yellow
                Write-Host "?? Poate fi un fisier Word sau alt format. Verifica manual fisierul: $xmlFile" -ForegroundColor Cyan
            }
        }
    }
    catch {
        Write-Host "? Eroare la descarcarea fisierului: $_" -ForegroundColor Red
        Write-Host "?? Poti descarca manual de la:" -ForegroundColor Cyan
        Write-Host "   $xmlUrl" -ForegroundColor Gray
    }
} else {
    Write-Host "`n[6/6] Saltare download date reale..." -ForegroundColor Gray
}

# ========================================
# VERIFICARE FINALA SI STATISTICI
# ========================================
Write-Host "`n========================================" -ForegroundColor Magenta
Write-Host "SETUP FINALIZAT" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta

try {
    $command.CommandText = "SELECT COUNT(*) FROM Ocupatii_ISCO08"
    $totalRecords = $command.ExecuteScalar()
    
    $command.CommandText = "SELECT COUNT(*) FROM Ocupatii_ISCO08 WHERE Nivel_Ierarhic = 1"
    $grupeMajore = $command.ExecuteScalar()
    
    $command.CommandText = "SELECT COUNT(*) FROM Ocupatii_ISCO08 WHERE Nivel_Ierarhic = 4"
    $ocupatiiDetaliate = $command.ExecuteScalar()
    
    Write-Host "`n?? Statistici finale:" -ForegroundColor Green
    Write-Host "  ?? Total inregistrari: $totalRecords" -ForegroundColor White
    Write-Host "  ?? Grupe majore: $grupeMajore" -ForegroundColor White
    Write-Host "  ?? Ocupatii detaliate: $ocupatiiDetaliate" -ForegroundColor White
    
    if ($totalRecords -gt 0) {
        Write-Host "`n? SUCCES! Tabelul Ocupatii_ISCO08 este gata de utilizare!" -ForegroundColor Green
        Write-Host "`n?? Poti acum sa:" -ForegroundColor Cyan
        Write-Host "  1. Adaugi entity-ul OcupatieISCO in ValyanClinic.Domain" -ForegroundColor White
        Write-Host "  2. Actualizezi DbContext-ul cu DbSet<OcupatieISCO>" -ForegroundColor White
        Write-Host "  3. Rulezi migrarea Entity Framework" -ForegroundColor White
        Write-Host "  4. Utilizezi stored procedures pentru operatiuni CRUD" -ForegroundColor White
        
        Write-Host "`n?? Pentru testare rapida:" -ForegroundColor Cyan
        Write-Host "  EXEC sp_Ocupatii_ISCO08_GetStatistics" -ForegroundColor Gray
        Write-Host "  EXEC sp_Ocupatii_ISCO08_GetGrupeMajore" -ForegroundColor Gray
    } else {
        Write-Host "`n??  Tabelul este gol. Ruleaza cu -InsertSampleData pentru date test" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "??  Nu pot obtine statistici: $_" -ForegroundColor Yellow
}

$connection.Close()

Write-Host "`n?? Pentru import date reale complete:" -ForegroundColor Cyan
Write-Host "   .\Setup-ISCOComplete.ps1 -DownloadRealData" -ForegroundColor Gray