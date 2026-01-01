<#
Simple tester: runs COUNT, TOP5 and executes sp_PersonalMedical_GetAll
Usage: .\Run-Test-Sp_PersonalMedical_GetAll.ps1 -Environment Development
#>
param(
    [ValidateSet('Development','Production','')]
    [string]$Environment = 'Development',
    [string]$AppSettingsPath = ''
)

if ([string]::IsNullOrWhiteSpace($AppSettingsPath)) {
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
    } else { Write-Error 'Could not locate ValyanClinic folder; pass -AppSettingsPath' ; exit 1 }
}

$json = Get-Content -Raw $AppSettingsPath | ConvertFrom-Json
$cs = $json.ConnectionStrings.DefaultConnection
Write-Host "Using connection (masked):" ($cs -replace '[^;]+Password=[^;]+','Password=********')

$q1 = 'SELECT COUNT(1) AS Total FROM dbo.PersonalMedical;'
$q2 = 'SELECT TOP 5 PersonalID, Nume, Prenume, NumarLicenta FROM dbo.PersonalMedical;'
$q3 = "EXEC dbo.sp_PersonalMedical_GetAll @PageNumber=1, @PageSize=10;"

Write-Host "Running COUNT query..."
Invoke-Sqlcmd -ConnectionString $cs -Query $q1 | Format-Table -AutoSize

Write-Host "\nRunning TOP 5 query..."
Invoke-Sqlcmd -ConnectionString $cs -Query $q2 | Format-Table -AutoSize

Write-Host "\nRunning stored procedure sp_PersonalMedical_GetAll..."
Invoke-Sqlcmd -ConnectionString $cs -Query $q3 | Format-Table -AutoSize

# Tests with ColumnFiltersJson
$qf1 = 'EXEC dbo.sp_PersonalMedical_GetAll @PageNumber=1, @PageSize=10, @ColumnFiltersJson = N''[{""Column"":""Nume"",""Operator"":""Contains"",""Value"":""Iancu""}]'';'
Write-Host "\nRunning SP with ColumnFiltersJson: Nume Contains 'Iancu'"
Invoke-Sqlcmd -ConnectionString $cs -Query $qf1 | Format-Table -AutoSize

$qf2 = 'EXEC dbo.sp_PersonalMedical_GetAll @PageNumber=1, @PageSize=10, @ColumnFiltersJson = N''[{""Column"":""Specializare"",""Operator"":""StartsWith"",""Value"":""Pneumologie""}]'';'
Write-Host "\nRunning SP with ColumnFiltersJson: Specializare StartsWith 'Pneumologie'"
Invoke-Sqlcmd -ConnectionString $cs -Query $qf2 | Format-Table -AutoSize

$qf3 = 'EXEC dbo.sp_PersonalMedical_GetAll @PageNumber=1, @PageSize=10, @ColumnFiltersJson = N''[{""Column"":""NumarLicenta"",""Operator"":""Equals"",""Value"":""MED122456""}]'';'
Write-Host "\nRunning SP with ColumnFiltersJson: NumarLicenta Equals 'MED122456'"
Invoke-Sqlcmd -ConnectionString $cs -Query $qf3 | Format-Table -AutoSize

# Sanity test: filter that should return NO rows
$qf4 = 'EXEC dbo.sp_PersonalMedical_GetAll @PageNumber=1, @PageSize=10, @ColumnFiltersJson = N''[{""Column"":""Nume"",""Operator"":""Equals"",""Value"":""__NO_MATCH__""}]'';'
Write-Host "\nRunning SP with ColumnFiltersJson: Nume Equals '__NO_MATCH__' (should return no rows)"
Invoke-Sqlcmd -ConnectionString $cs -Query $qf4 | Format-Table -AutoSize

Write-Host "Done."