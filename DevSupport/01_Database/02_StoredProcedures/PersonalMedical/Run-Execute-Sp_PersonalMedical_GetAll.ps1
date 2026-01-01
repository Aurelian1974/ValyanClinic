<#
.SYNOPSIS
Executes the stored-procedure migration SQL file using the connection string stored in the ValyanClinic appsettings JSON.

.DESCRIPTION
- Reads `ConnectionStrings.DefaultConnection` from an appsettings file (Development/Production or explicit path).
- Tries to use `Invoke-Sqlcmd` (SqlServer module). If not available, falls back to `sqlcmd.exe`.

.PARAMETER Environment
Which environment appsettings to use: Development (default), Production, or "" to use appsettings.json.

.PARAMETER AppSettingsPath
Optional explicit path to the appsettings json file.

.PARAMETER SqlFile
Path to the SQL file to execute. Defaults to the migration created alongside this script.

.EXAMPLE
# Use the Development appsettings (default)
.
\Run-Execute-Sp_PersonalMedical_GetAll.ps1

.EXAMPLE
# Run against production appsettings explicitly
.
\Run-Execute-Sp_PersonalMedical_GetAll.ps1 -Environment Production

# Note: Requires either SqlServer PowerShell module (Invoke-Sqlcmd) or sqlcmd.exe available on PATH.
##>

param(
    [ValidateSet('Development','Production','')]
    [string]$Environment = 'Development',

    [string]$AppSettingsPath = '',

    [string]$SqlFile = (Join-Path -Path (Split-Path -Parent $MyInvocation.MyCommand.Path) -ChildPath 'sp_PersonalMedical_GetAll__add_column_filters.sql')
)

function Fail([string]$msg) {
    Write-Error $msg
    exit 1
}

# Locate appsettings if path not provided
if ([string]::IsNullOrWhiteSpace($AppSettingsPath)) {
    # Search upward for ValyanClinic folder
    $cursor = Split-Path -Parent $MyInvocation.MyCommand.Path
    $found = $null
    while ($cursor -and -not $found -and (Get-Item -Path $cursor -ErrorAction SilentlyContinue)) {
        $cand = Join-Path -Path $cursor -ChildPath 'ValyanClinic'
        if (Test-Path $cand) { $found = $cand; break }
        $cursor = Split-Path -Parent $cursor
    }

    if ($found) {
        switch ($Environment) {
            'Development' { $AppSettingsPath = Join-Path $found 'appsettings.Development.json' }
            'Production'  { $AppSettingsPath = Join-Path $found 'appsettings.Production.json' }
            default       { $AppSettingsPath = Join-Path $found 'appsettings.json' }
        }
    } else {
        Fail "Could not locate ValyanClinic folder automatically. Please pass -AppSettingsPath explicitly pointing to appsettings.json."
    }
}

if (-not (Test-Path $AppSettingsPath)) {
    Fail ("AppSettings file not found at '${AppSettingsPath}'")
}

Write-Host "Using appsettings file: $AppSettingsPath"

# Read appsettings and extract DefaultConnection
try {
    $json = Get-Content -Raw -Path $AppSettingsPath | ConvertFrom-Json
} catch {
    Fail ("Failed to read or parse JSON from ${AppSettingsPath}: $($_.Exception.Message)")
}

if (-not $json.ConnectionStrings -or -not $json.ConnectionStrings.DefaultConnection) {
    Fail ("ConnectionStrings.DefaultConnection not found in ${AppSettingsPath}")
}

$connectionString = $json.ConnectionStrings.DefaultConnection
Write-Host "Connection string (masked):" ($connectionString -replace '[^;]+Password=[^;]+','Password=********')

# Parse connection string (robust: try System.Data.SqlClient, then Microsoft.Data, fallback to DbConnectionStringBuilder)
try {
    try {
        $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder $connectionString
    } catch {
        try {
            $builder = New-Object Microsoft.Data.SqlClient.SqlConnectionStringBuilder $connectionString
        } catch {
            $tmp = New-Object System.Data.Common.DbConnectionStringBuilder
            $tmp.ConnectionString = $connectionString
            $builder = $tmp
        }
    }
} catch {
    Fail ("Failed to parse connection string: $($_.Exception.Message)")
}

# Extract common keys (works for typed builders and DbConnectionStringBuilder fallback)
function FirstExists([string[]]$keys, $dict) {
    foreach ($k in $keys) {
        if ($dict.ContainsKey($k) -and ($dict[$k] -ne $null) -and ($dict[$k].ToString().Trim() -ne '')) { return $dict[$k] }
    }
    return $null
}

if ($builder -is [System.Data.Common.DbConnectionStringBuilder]) {
    $dict = @{}
    foreach ($k in $builder.Keys) { $dict[$k.ToString()] = $builder[$k] }
    $server = FirstExists @('Data Source','Server','Address','DataSource') $dict
    $database = FirstExists @('Initial Catalog','Database') $dict
    $user = FirstExists @('User ID','UID') $dict
    $pwd = FirstExists @('Password','Pwd') $dict
} else {
    $server = $builder.DataSource
    $database = $builder.InitialCatalog
    $user = $builder.UserID
    $pwd = $builder.Password
}
$integrated = $false
if (($builder.PersistSecurityInfo -eq $null) -or ($builder.UserID -eq '' -or $null -eq $builder.UserID)) {
    # If no user specified or Trusted_Connection / Integrated Security present
    if ($connectionString -match '(?i:Trusted_Connection|Integrated Security)') { $integrated = $true }
}

Write-Host "Server: $server" "Database: $database"

if (-not (Test-Path $SqlFile)) {
    Fail ("SQL file not found: ${SqlFile}")
}

# Try Invoke-Sqlcmd if available
if (Get-Command Invoke-Sqlcmd -ErrorAction SilentlyContinue) {
    Write-Host "Using Invoke-Sqlcmd to run: $SqlFile"
    try {
        Invoke-Sqlcmd -ConnectionString $connectionString -InputFile $SqlFile -Verbose
        Write-Host "SQL executed successfully with Invoke-Sqlcmd."
        exit 0
    } catch {
        Write-Warning "Invoke-Sqlcmd failed: $_. Falling back to sqlcmd.exe if available."
    }
}

# Fallback to sqlcmd.exe
if (Get-Command sqlcmd -ErrorAction SilentlyContinue) {
    Write-Host "Using sqlcmd.exe to run: $SqlFile"

    $args = @()
    $args += '-S'; $args += $server
    $args += '-d'; $args += $database
    $args += '-i'; $args += (Resolve-Path $SqlFile)

    if ($integrated) {
        $args += '-E'
    } else {
        if ($user) { $args += '-U'; $args += $user }
        if ($pwd)  { $args += '-P'; $args += $pwd }
    }

    $proc = Start-Process -FilePath 'sqlcmd' -ArgumentList $args -NoNewWindow -Wait -PassThru -RedirectStandardError stderr.txt -RedirectStandardOutput stdout.txt
    if ($proc.ExitCode -eq 0) {
        Write-Host "sqlcmd executed successfully. Output:"
        Get-Content stdout.txt -Raw | Write-Host
        exit 0
    } else {
        Write-Error "sqlcmd failed with exit code $($proc.ExitCode). Check stdout.txt/stderr.txt"
        Write-Error (Get-Content stderr.txt -Raw)
        exit $proc.ExitCode
    }
} else {
    Fail "Neither Invoke-Sqlcmd nor sqlcmd.exe are available. Install SqlServer PowerShell module or make sqlcmd available on PATH."
}
