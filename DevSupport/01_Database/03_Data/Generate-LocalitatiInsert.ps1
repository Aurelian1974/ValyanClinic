# =============================================
# Script: Generate-LocalitatiInsert.ps1
# Purpose: Genereaza script SQL pentru inserarea localitatilor din CSV
# Source: https://github.com/romania/localitati
# =============================================

$csvPath = "..\orase_romania.csv"
$outputPath = ".\Insert_Localitati_All.sql"

# Citeste CSV-ul
$localities = Import-Csv -Path $csvPath -Encoding UTF8

# Mapare JUDET AUTO -> IdJudet (din baza de date ValyanMed - verificat cu SELECT)
$judetMap = @{
    "AB" = 1   # Alba
    "AR" = 2   # Arad
    "AG" = 3   # Arges
    "BC" = 4   # Bacau
    "BH" = 5   # Bihor
    "BN" = 6   # Bistrita-Nasaud
    "BT" = 7   # Botosani
    "BV" = 8   # Brasov
    "BR" = 9   # Braila
    "BZ" = 10  # Buzau
    "CS" = 11  # Caras-Severin
    "CL" = 12  # Calarasi
    "CJ" = 13  # Cluj
    "CT" = 14  # Constanta
    "CV" = 15  # Covasna
    "DB" = 16  # Dambovita
    "DJ" = 17  # Dolj
    "GL" = 18  # Galati
    "GR" = 19  # Giurgiu
    "GJ" = 20  # Gorj
    "HR" = 21  # Harghita
    "HD" = 22  # Hunedoara
    "IL" = 23  # Ialomita
    "IS" = 24  # Iasi
    "IF" = 25  # Ilfov
    "MM" = 26  # Maramures
    "MH" = 27  # Mehedinti
    "MS" = 28  # Mures
    "NT" = 29  # Neamt
    "OT" = 30  # Olt
    "PH" = 31  # Prahova
    "SM" = 32  # Satu Mare
    "SJ" = 33  # Salaj
    "SB" = 34  # Sibiu
    "SV" = 35  # Suceava
    "TR" = 36  # Teleorman
    "TM" = 37  # Timis
    "TL" = 38  # Tulcea
    "VS" = 39  # Vaslui
    "VL" = 40  # Valcea
    "VN" = 41  # Vrancea
    "B"  = 42  # Bucuresti
}

Write-Host "Generare script SQL pentru $($localities.Count) localitati..." -ForegroundColor Cyan

# Genereaza SQL
$sql = @"
-- =============================================
-- Script: Insert_Localitati_All.sql
-- Purpose: Insereaza toate localitatile din Romania
-- Source: https://github.com/romania/localitati
-- Total: $($localities.Count) localitati
-- Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
-- =============================================

SET NOCOUNT ON;
GO

-- ATENTIE: Sterge TOATE localitatile existente
-- Decomentati urmatoarele linii daca doriti reset complet
PRINT 'Stergem localitatile existente...';
DELETE FROM Localitate;
DBCC CHECKIDENT ('Localitate', RESEED, 0);
GO

PRINT 'Incepem inserarea localitatilor...';
GO

"@

$idCounter = 1
$batchSize = 500
$currentBatch = 0

foreach ($loc in $localities) {
    $judetAuto = $loc.'JUDET AUTO'.Trim()
    $nume = $loc.NUME.Trim().Replace("'", "''")  # Escape quotes
    $populatie = [int]$loc.'POPULATIE (in 2002)'
    
    # Determina ID judet
    $idJudet = $judetMap[$judetAuto]
    if (-not $idJudet) {
        Write-Warning "Judet necunoscut: $judetAuto pentru $nume"
        continue
    }
    
    # Determina tip localitate bazat pe populatie (aproximativ)
    $tipLocalitate = 3  # Sat default
    if ($populatie -gt 50000) {
        $tipLocalitate = 1  # Municipiu
    } elseif ($populatie -gt 5000) {
        $tipLocalitate = 2  # Oras/Comuna
    }
    
    # Genereaza GUID
    $guid = [System.Guid]::NewGuid().ToString().ToUpper()
    
    $sql += @"
INSERT INTO Localitate (LocalitateGuid, IdJudet, Nume, Siruta, IdTipLocalitate, CodLocalitate)
VALUES ('$guid', $idJudet, N'$nume', 0, $tipLocalitate, $idCounter);
"@
    
    $idCounter++
    $currentBatch++
    
    # Batch commit pentru performanta
    if ($currentBatch -ge $batchSize) {
        $sql += @"

GO
PRINT 'Inserati $idCounter localitati...';
"@
        $currentBatch = 0
    }
}

$sql += @"

GO

-- Verifica rezultatele
SELECT 
    j.Nume AS Judet, 
    COUNT(l.IdOras) AS NrLocalitati
FROM Judet j
LEFT JOIN Localitate l ON j.IdJudet = l.IdJudet
GROUP BY j.Nume
ORDER BY NrLocalitati DESC;

SELECT COUNT(*) AS TotalLocalitati FROM Localitate;

PRINT '';
PRINT '========================================';
PRINT 'FINALIZAT! Total localitati inserate: $($idCounter - 1)';
PRINT '========================================';
GO
"@

# Salveaza fisierul
$sql | Out-File -FilePath $outputPath -Encoding UTF8

Write-Host ""
Write-Host "Script SQL generat cu succes!" -ForegroundColor Green
Write-Host "Fisier: $outputPath" -ForegroundColor Yellow
Write-Host "Total localitati: $($idCounter - 1)" -ForegroundColor Yellow
Write-Host ""
Write-Host "Pentru a rula scriptul:" -ForegroundColor Cyan
Write-Host "sqlcmd -S `"DESKTOP-3Q8HI82\ERP`" -d ValyanMed -E -i `"$outputPath`"" -ForegroundColor White
