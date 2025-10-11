# Debug script pentru verificarea datelor
$ocupatiiISCO = @(
    @{ Cod='0'; Denumire='Test 0' },
    @{ Cod='1'; Denumire='Test 1' }
)

Write-Host "Total elemente: $($ocupatiiISCO.Count)"
foreach($item in $ocupatiiISCO) {
    Write-Host "Cod: '$($item.Cod)' - Denumire: '$($item.Denumire)'"
}

# Test group by
$grupe = $ocupatiiISCO | Group-Object Cod
Write-Host "Grupe:"
$grupe | ForEach-Object { Write-Host "  $($_.Name): $($_.Count)" }