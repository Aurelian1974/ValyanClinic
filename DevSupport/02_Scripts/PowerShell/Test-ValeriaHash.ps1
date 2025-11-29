param()

Write-Host "==========================================="  -ForegroundColor Cyan
Write-Host "TEST VALERIA PASSWORD HASH" -ForegroundColor Cyan
Write-Host "==========================================="  -ForegroundColor Cyan
Write-Host ""

# Load BCrypt
$dllPath = "D:\Lucru\CMS\ValyanClinic\bin\Debug\net9.0\BCrypt.Net-Next.dll"
Add-Type -Path $dllPath

$password = "Valeria1973!"
$hashFromDB = '$2a$12$1u7PllF3FjyAg6jqFjUU0uJi6uluP7H99a1tBnG0leXlfr4Kbd91y'

Write-Host "Password: $password"
Write-Host "Hash from DB: $hashFromDB"
Write-Host ""

try {
    $result = [BCrypt.Net.BCrypt]::Verify($password, $hashFromDB)
    
    if ($result) {
        Write-Host "Verification: SUCCESS" -ForegroundColor Green
        Write-Host ""
     Write-Host "Hash is CORRECT! Problem is elsewhere." -ForegroundColor Yellow
    } else {
   Write-Host "Verification: FAILED" -ForegroundColor Red
        Write-Host ""
 Write-Host "PASSWORD DOES NOT MATCH!" -ForegroundColor Red
     Write-Host ""
        Write-Host "Generating NEW hash..." -ForegroundColor Yellow
        $newHash = [BCrypt.Net.BCrypt]::HashPassword($password, 12)
        Write-Host "New hash: $newHash" -ForegroundColor Green
        Write-Host ""
        Write-Host "Run this SQL to fix:" -ForegroundColor Yellow
        Write-Host "--------------------" -ForegroundColor Gray
    Write-Host "UPDATE Utilizatori SET PasswordHash = '$newHash', Salt = '' WHERE Username = 'valeria'" -ForegroundColor White
    }
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "==========================================="  -ForegroundColor Cyan
