# Script pentru monitorizare log-uri ValyanClinic în timp real
Write-Host "===========================================" -ForegroundColor Cyan
Write-Host "   Monitorizare Log-uri ValyanClinic      " -ForegroundColor Cyan
Write-Host "===========================================" -ForegroundColor Cyan
Write-Host ""

$LogDirectory = "D:\Lucru\CMS\ValyanClinic\Logs"
$Today = Get-Date -Format "yyyyMMdd"

# Caut? cel mai recent fi?ier de log
$ErrorLogFile = "$LogDirectory\errors-$Today.log"
$MainLogFile = "$LogDirectory\valyan-clinic-$Today.log"

Write-Host "Directorul de loguri: $LogDirectory" -ForegroundColor Yellow
Write-Host ""

# Verific? dac? exist? fi?ierele
if (Test-Path $ErrorLogFile) {
    Write-Host "? Fisier erori gasit: $ErrorLogFile" -ForegroundColor Green
} else {
    Write-Host "! Fisier erori NU exista inca: $ErrorLogFile" -ForegroundColor Yellow
}

if (Test-Path $MainLogFile) {
    Write-Host "? Fisier log principal gasit: $MainLogFile" -ForegroundColor Green
} else {
    Write-Host "! Fisier log principal NU exista inca: $MainLogFile" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "===========================================" -ForegroundColor Cyan
Write-Host "   PORNESTE APLICATIA ACUM!               " -ForegroundColor Green
Write-Host "===========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Apasa Ctrl+C pentru a opri monitorizarea..." -ForegroundColor Gray
Write-Host ""
Write-Host "===========================================" -ForegroundColor Cyan
Write-Host ""

# Monitorizare în timp real
while ($true) {
    Start-Sleep -Milliseconds 500
    
    # Cite?te ultimele 30 de linii din fi?ierul de erori (dac? exist?)
    if (Test-Path $ErrorLogFile) {
        $errorContent = Get-Content $ErrorLogFile -Tail 5 -ErrorAction SilentlyContinue
        
        if ($errorContent) {
            foreach ($line in $errorContent) {
                if ($line -match "ERROR|EROARE|Exception|EXCEPTION") {
                    Write-Host $line -ForegroundColor Red
                } elseif ($line -match "WARNING|WRN|AVERTIZARE") {
                    Write-Host $line -ForegroundColor Yellow
                } elseif ($line -match "PacientDataAdaptor|GetPacientListQueryHandler") {
                    Write-Host $line -ForegroundColor Cyan
                } else {
                    Write-Host $line -ForegroundColor White
                }
            }
        }
    }
    
    # Cite?te ultimele linii din log-ul principal
    if (Test-Path $MainLogFile) {
        $mainContent = Get-Content $MainLogFile -Tail 10 -ErrorAction SilentlyContinue
        
        if ($mainContent) {
            foreach ($line in $mainContent) {
                if ($line -match "PacientDataAdaptor") {
                    Write-Host $line -ForegroundColor Magenta
                } elseif ($line -match "GetPacientListQueryHandler") {
                    Write-Host $line -ForegroundColor Blue
                } elseif ($line -match "START|END") {
                    Write-Host $line -ForegroundColor Green
                } elseif ($line -match "SUCCESS") {
                    Write-Host $line -ForegroundColor Green
                } elseif ($line -match "FAILED|EXCEPTION") {
                    Write-Host $line -ForegroundColor Red
                }
            }
        }
    }
}
