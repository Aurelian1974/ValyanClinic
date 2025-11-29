param(
    [Parameter(Mandatory=$true)]
    [string]$Username,
    
    [Parameter(Mandatory=$true)]
    [string]$Password,
    
    [Parameter(Mandatory=$false)]
    [string]$Email = "",
    
  [Parameter(Mandatory=$false)]
    [string]$Rol = "User",
    
    [Parameter(Mandatory=$false)]
[switch]$UpdateExisting
)

# Connection string
$connectionString = "Server=DESKTOP-3Q8HI82\ERP;Database=ValyanMed;Integrated Security=True;TrustServerCertificate=True;"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "VALYANMED - CREATE/UPDATE USER" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Load BCrypt.Net assembly
$possiblePaths = @(
    "D:\Lucru\CMS\packages\bcrypt.net-next.4.0.3\lib\net6.0\BCrypt.Net-Next.dll",
    "D:\Lucru\CMS\packages\bcrypt.net-next.4.0.3\lib\net8.0\BCrypt.Net-Next.dll",
    "$PSScriptRoot\..\..\..\packages\bcrypt.net-next.4.0.3\lib\net6.0\BCrypt.Net-Next.dll",
    "$PSScriptRoot\..\..\..\ValyanClinic\bin\Debug\net9.0\BCrypt.Net-Next.dll"
)

$dllPath = $null
foreach ($path in $possiblePaths) {
    if (Test-Path $path) {
        $dllPath = $path
     break
    }
}

if ($dllPath) {
    Add-Type -Path $dllPath
    Write-Host "? BCrypt.Net loaded from: $dllPath" -ForegroundColor Green
} else {
    Write-Host "? ERROR: BCrypt.Net DLL not found!" -ForegroundColor Red
    Write-Host "  Searched in:" -ForegroundColor Yellow
    foreach ($path in $possiblePaths) {
        Write-Host "    - $path" -ForegroundColor Gray
    }
    Write-Host ""
    Write-Host "  Solution: Run 'dotnet build' first to restore packages!" -ForegroundColor Yellow
exit 1
}

# Generate BCrypt hash
Write-Host ""
Write-Host "Generating BCrypt hash for password..." -ForegroundColor Yellow
$passwordHash = [BCrypt.Net.BCrypt]::HashPassword($Password, 12)

Write-Host "Hash generated: $($passwordHash.Substring(0, 30))..." -ForegroundColor Green

# Verify hash
$isValid = [BCrypt.Net.BCrypt]::Verify($Password, $passwordHash)
if (-not $isValid) {
    Write-Host "? ERROR: Generated hash is INVALID!" -ForegroundColor Red
    exit 1
}
Write-Host "? Hash verification: VALID" -ForegroundColor Green

# Connect to database
Write-Host ""
Write-Host "Connecting to database..." -ForegroundColor Yellow

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    Write-Host "? Database connection successful" -ForegroundColor Green
    
    # Check if user exists
    $checkCmd = $connection.CreateCommand()
    $checkCmd.CommandText = "SELECT UtilizatorID, EsteActiv, NumarIncercariEsuate FROM Utilizatori WHERE Username = @Username"
    $checkCmd.Parameters.AddWithValue("@Username", $Username) | Out-Null
    
    $reader = $checkCmd.ExecuteReader()
    $userExists = $false
    $userId = $null
    $isActive = $false
    $failedAttempts = 0
    
    if ($reader.Read()) {
 $userExists = $true
   $userId = $reader["UtilizatorID"]
        $isActive = $reader["EsteActiv"]
 $failedAttempts = $reader["NumarIncercariEsuate"]
    }
    $reader.Close()
    
    if ($userExists) {
        Write-Host ""
        Write-Host "User exists:" -ForegroundColor Yellow
        Write-Host "  ID: $userId"
        Write-Host "  Active: $isActive"
   Write-Host "  Failed attempts: $failedAttempts"
        
   if (-not $UpdateExisting) {
            Write-Host ""
          Write-Host "? User already exists! Use -UpdateExisting to update password." -ForegroundColor Red
            $connection.Close()
 exit 1
     }
  
        # Update existing user
        Write-Host ""
        Write-Host "Updating user password and unlocking account..." -ForegroundColor Yellow
        
        $updateCmd = $connection.CreateCommand()
        $updateCmd.CommandText = @"
            UPDATE Utilizatori
            SET PasswordHash = @PasswordHash,
       Salt = '',
         NumarIncercariEsuate = 0,
 DataBlocare = NULL,
                EsteActiv = 1,
  DataUltimeiModificari = GETDATE(),
       ModificatDe = 'PowerShell_Script'
   WHERE Username = @Username
"@
    $updateCmd.Parameters.AddWithValue("@PasswordHash", $passwordHash) | Out-Null
        $updateCmd.Parameters.AddWithValue("@Username", $Username) | Out-Null
      
        $rowsAffected = $updateCmd.ExecuteNonQuery()
        
        if ($rowsAffected -gt 0) {
            Write-Host "? User updated successfully!" -ForegroundColor Green
       if ($failedAttempts -ge 5) {
                Write-Host "? Account UNLOCKED (failed attempts reset to 0)" -ForegroundColor Green
            }
        } else {
            Write-Host "? Update failed!" -ForegroundColor Red
       $connection.Close()
         exit 1
        }
        
    } else {
    # Create new user
Write-Host ""
        Write-Host "Creating new user..." -ForegroundColor Yellow
        
 # Get a PersonalMedical ID for testing (or use NULL if not required)
  $getPersonalCmd = $connection.CreateCommand()
        $getPersonalCmd.CommandText = @"
       SELECT TOP 1 pm.PersonalID
      FROM PersonalMedical pm
   LEFT JOIN Utilizatori u ON pm.PersonalID = u.PersonalMedicalID
 WHERE pm.EsteActiv = 1 
  AND u.UtilizatorID IS NULL
"@
        $personalMedicalID = $getPersonalCmd.ExecuteScalar()
        
  if ($personalMedicalID -eq $null) {
    Write-Host "? Warning: No available PersonalMedical ID found" -ForegroundColor Yellow
            Write-Host "  User will be created without PersonalMedical association" -ForegroundColor Yellow
        }
    
        $createCmd = $connection.CreateCommand()
        
        if ($personalMedicalID -ne $null) {
  $createCmd.CommandText = @"
           INSERT INTO Utilizatori (
           UtilizatorID, PersonalMedicalID, Username, Email, 
          PasswordHash, Salt, Rol, EsteActiv, 
   CreatDe, DataCrearii
     )
        VALUES (
     NEWID(), @PersonalMedicalID, @Username, @Email,
  @PasswordHash, '', @Rol, 1,
         'PowerShell_Script', GETDATE()
       )
"@
            $createCmd.Parameters.AddWithValue("@PersonalMedicalID", $personalMedicalID) | Out-Null
        } else {
            # Create without PersonalMedical (if FK allows NULL)
       $createCmd.CommandText = @"
    INSERT INTO Utilizatori (
         UtilizatorID, Username, Email, 
        PasswordHash, Salt, Rol, EsteActiv, 
    CreatDe, DataCrearii
      )
VALUES (
              NEWID(), @Username, @Email,
       @PasswordHash, '', @Rol, 1,
           'PowerShell_Script', GETDATE()
                )
"@
     }
        
        $createCmd.Parameters.AddWithValue("@Username", $Username) | Out-Null
        $createCmd.Parameters.AddWithValue("@Email", $Email) | Out-Null
     $createCmd.Parameters.AddWithValue("@PasswordHash", $passwordHash) | Out-Null
        $createCmd.Parameters.AddWithValue("@Rol", $Rol) | Out-Null
        
        try {
            $rowsAffected = $createCmd.ExecuteNonQuery()
        
            if ($rowsAffected -gt 0) {
    Write-Host "? User created successfully!" -ForegroundColor Green
    } else {
          Write-Host "? Create failed!" -ForegroundColor Red
     $connection.Close()
    exit 1
            }
        } catch {
            Write-Host "? ERROR: $($_.Exception.Message)" -ForegroundColor Red
     $connection.Close()
            exit 1
   }
    }
    
    # Display final user info
    Write-Host ""
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host "USER CREDENTIALS" -ForegroundColor Cyan
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host "  Username: $Username" -ForegroundColor Green
    Write-Host "  Password: $Password" -ForegroundColor Green
    Write-Host "  Email: $Email"
    Write-Host "  Role: $Rol"
    Write-Host ""
    Write-Host "  Hash (preview): $($passwordHash.Substring(0, 40))..." -ForegroundColor Gray
    Write-Host ""
  Write-Host "? You can now login with these credentials!" -ForegroundColor Green
    
    $connection.Close()
    
} catch {
    Write-Host "? ERROR: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "COMPLETED" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
