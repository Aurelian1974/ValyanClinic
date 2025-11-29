# ========================================
# PowerShell Script: Generate BCrypt Hash
# Descriere: Genereaz? hash BCrypt pentru o parol?
# Autor: System
# Data: 2025-01-24
# ========================================

<#
.SYNOPSIS
    Genereaz? hash BCrypt pentru o parol?

.DESCRIPTION
    Folose?te BCrypt.Net-Next pentru a genera hash-uri sigure
    
.PARAMETER Password
    Parola pentru care se genereaz? hash-ul

.EXAMPLE
    .\Generate-BCryptHash.ps1 -Password "admin123!@#"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$Password
)

# Load BCrypt assembly
Add-Type -Path "C:\Users\$env:USERNAME\.nuget\packages\bcrypt.net-next\4.0.3\lib\net8.0\BCrypt.Net-Next.dll"

# Generate hash with Work Factor 12
$hash = [BCrypt.Net.BCrypt]::HashPassword($Password, 12)

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " BCRYPT HASH GENERATOR" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Password: " -NoNewline
Write-Host $Password -ForegroundColor Yellow
Write-Host ""
Write-Host "BCrypt Hash (Work Factor 12):" -ForegroundColor Green
Write-Host $hash -ForegroundColor White
Write-Host ""
Write-Host "Hash Length: $($hash.Length) characters" -ForegroundColor Gray
Write-Host ""
Write-Host "? Hash generat cu succes!" -ForegroundColor Green
Write-Host ""
