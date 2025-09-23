# PowerShell script to normalize Romanian diacritics in text files and fix common encoding issues.
# Creates .bak backups for modified files and writes files as UTF8 without BOM.

$root = Get-Location
$excludeDirs = @('.git','bin','obj','packages','node_modules','.vs')
$extRegex = '\.(cs|razor|cshtml|resx|json|md|txt|html|htm|js|css|xml|sql|yml|yaml|config|csproj|sln)$'

# Map Romanian diacritics to ASCII approximations using Unicode code points
$mappings = @(
    @{ k = [char]0x0103; v = 'a' }, # U+0103 ?
    @{ k = [char]0x0102; v = 'A' }, # U+0102 ?
    @{ k = [char]0x00E2; v = 'a' }, # U+00E2 â
    @{ k = [char]0x00C2; v = 'A' }, # U+00C2 Â
    @{ k = [char]0x00EE; v = 'i' }, # U+00EE î
    @{ k = [char]0x00CE; v = 'I' }, # U+00CE Î
    @{ k = [char]0x0219; v = 's' }, # U+0219 ?
    @{ k = [char]0x0218; v = 'S' }, # U+0218 ?
    @{ k = [char]0x015F; v = 's' }, # U+015F ?
    @{ k = [char]0x015E; v = 'S' }, # U+015E ?
    @{ k = [char]0x021B; v = 't' }, # U+021B ?
    @{ k = [char]0x021A; v = 'T' }, # U+021A ?
    @{ k = [char]0x0163; v = 't' }, # U+0163 ?
    @{ k = [char]0x0162; v = 'T' }  # U+0162 ?
)

# Common markers that indicate mojibake (keep ASCII only)
$mojibakeMarkers = @('Ã','Å')

$files = Get-ChildItem -Path $root -Recurse -File -ErrorAction SilentlyContinue | Where-Object {
    $_.FullName -notmatch ([string]::Join('|', ($excludeDirs | ForEach-Object { [regex]::Escape("\\$_") } ))) -and
    $_.FullName -match $extRegex
}

if ($files.Count -eq 0) {
    Write-Output "No candidate files found."
    exit 0
}

$totalChanged = 0
foreach ($file in $files) {
    try {
        $bytes = [System.IO.File]::ReadAllBytes($file.FullName)

        # Decode as UTF8 first
        $content = [System.Text.Encoding]::UTF8.GetString($bytes)

        # If mojibake markers or replacement chars are present, re-decode as Windows-1252
        $needRecode = $false
        foreach ($m in $mojibakeMarkers) { if ($content -like "*$m*") { $needRecode = $true; break } }
        if ($content -match '\uFFFD') { $needRecode = $true }
        if ($needRecode) {
            try {
                $content = [System.Text.Encoding]::GetEncoding(1252).GetString($bytes)
            } catch {
                # ignore and keep UTF8-decoded content
            }
        }

        $original = $content

        # Remove various control characters that sometimes appear
        $content = $content -replace '\u001f', ''
        $content = $content -replace '\u000f', ''
        $content = $content -replace '\u001a', ''
        $content = $content -replace '\u001e', ''
        $content = $content -replace '\u001d', ''
        $content = $content -replace '\u001c', ''
        $content = $content -replace [char]0xFFFD, ''

        # Remove question marks that are likely placeholders inside words (letter?letter)
        $content = [regex]::Replace($content, '(?<=\p{L})\?(?=\p{L})', '')

        # Convert proper diacritics to ASCII approximations
        foreach ($p in $mappings) {
            $k = $p.k
            $v = $p.v
            if ($content -like "*$k*") {
                $content = $content -replace [regex]::Escape($k), $v
            }
        }

        if ($content -ne $original) {
            # backup
            $bak = $file.FullName + '.bak'
            if (-not (Test-Path $bak)) {
                Copy-Item -Path $file.FullName -Destination $bak -ErrorAction SilentlyContinue
            }
            # Write as UTF8 without BOM
            [System.IO.File]::WriteAllText($file.FullName, $content, (New-Object System.Text.UTF8Encoding($false)))
            Write-Output "Modified: $($file.FullName)"
            $totalChanged++
        }
    } catch {
        Write-Warning "Failed to process $($file.FullName): $_"
    }
}

Write-Output "Done. Files modified: $totalChanged"
