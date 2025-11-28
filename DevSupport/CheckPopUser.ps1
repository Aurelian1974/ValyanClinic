# Script pentru verificare utilizator "pop"
$connectionString = "Server=DESKTOP-3Q8HI82\ERP;Database=ValyanMed;Integrated Security=True;TrustServerCertificate=True;"

Write-Host "Verificare utilizator 'pop' in baza de date..." -ForegroundColor Cyan
Write-Host ""

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    # Cautare utilizator
    $query = @"
SELECT 
    UtilizatorID,
    Username,
    Email,
    Rol,
    EsteActiv,
    LEFT(PasswordHash, 50) AS PasswordHashPreview,
    DataUltimeiModificari,
    ModificatDe
FROM Utilizatori 
WHERE Username LIKE '%pop%' OR Email LIKE '%pop%'
"@
    
    $command = $connection.CreateCommand()
    $command.CommandText = $query
    
    $reader = $command.ExecuteReader()
    
    if ($reader.HasRows) {
        Write-Host "? Utilizatori gasiti:" -ForegroundColor Green
        Write-Host ""
        
        while ($reader.Read()) {
            Write-Host "  Username: $($reader['Username'])" -ForegroundColor Yellow
            Write-Host "  Email: $($reader['Email'])"
            Write-Host "  Rol: $($reader['Rol'])"
            Write-Host "  Activ: $($reader['EsteActiv'])"
            Write-Host "  Password Hash: $($reader['PasswordHashPreview'])..."
            Write-Host "  Ultima modificare: $($reader['DataUltimeiModificari'])"
            Write-Host "  Modificat de: $($reader['ModificatDe'])"
            Write-Host "  ---"
            Write-Host ""
        }
    } else {
        Write-Host "? Nu s-a gasit niciun utilizator cu 'pop' in username sau email" -ForegroundColor Red
    }
    
    $reader.Close()
    $connection.Close()
    
} catch {
    Write-Host "? EROARE: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Pentru a schimba parola pentru utilizatorul 'pop', foloseste:" -ForegroundColor Cyan
Write-Host ""
Write-Host "  cd DevSupport\Scripts\PowerShellScripts" -ForegroundColor Gray
Write-Host "  .\Create-User.ps1 -Username 'pop' -Password 'Pop123!@#' -UpdateExisting" -ForegroundColor Gray
Write-Host ""
