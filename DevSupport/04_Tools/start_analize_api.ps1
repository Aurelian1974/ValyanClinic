# Start Analize PDF Parser API
# Pornește API-ul Python FastAPI pentru parsarea buletinelor de analize medicale

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$DevSupportDir = Split-Path -Parent $ScriptDir
$ParserDir = Join-Path $DevSupportDir "05_Resources\PDFs"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Analize PDF Parser API" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verifică dacă există folderul
if (-not (Test-Path $ParserDir)) {
    Write-Host "EROARE: Folderul parser nu există: $ParserDir" -ForegroundColor Red
    exit 1
}

# Verifică dacă există fișierele necesare
$ApiFile = Join-Path $ParserDir "api_analize.py"
$ParserFile = Join-Path $ParserDir "parsere_laboratoare.py"

if (-not (Test-Path $ApiFile)) {
    Write-Host "EROARE: Fișierul API nu există: $ApiFile" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $ParserFile)) {
    Write-Host "EROARE: Fișierul parser nu există: $ParserFile" -ForegroundColor Red
    exit 1
}

# Verifică dacă există requirements
$RequirementsFile = Join-Path $ParserDir "requirements.txt"
if (-not (Test-Path $RequirementsFile)) {
    Write-Host "Creez requirements.txt..." -ForegroundColor Yellow
    @"
fastapi>=0.109.0
uvicorn>=0.27.0
python-multipart>=0.0.6
PyMuPDF>=1.23.0
"@ | Out-File -FilePath $RequirementsFile -Encoding utf8
}

Write-Host "Pornesc API-ul pe http://localhost:5050" -ForegroundColor Green
Write-Host "Press Ctrl+C pentru a opri" -ForegroundColor Yellow
Write-Host ""

# Schimbă în directorul parser și pornește API
Push-Location $ParserDir
try {
    # Verifică dacă Python este instalat
    $PythonPath = Get-Command python -ErrorAction SilentlyContinue
    if (-not $PythonPath) {
        Write-Host "EROARE: Python nu este instalat sau nu este în PATH" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Folosesc Python: $($PythonPath.Source)" -ForegroundColor Gray
    Write-Host ""
    
    # Pornește serverul
    python -m uvicorn api_analize:app --host 0.0.0.0 --port 5050 --reload
}
finally {
    Pop-Location
}
