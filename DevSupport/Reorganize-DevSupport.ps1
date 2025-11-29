# DevSupport Reorganization Script
# Reorganizes DevSupport folder into a clean, logical structure

Write-Host "?? DevSupport Reorganization Script" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"
$DevSupportRoot = "D:\Lucru\CMS\DevSupport"

# Change to DevSupport directory
Set-Location $DevSupportRoot
Write-Host "?? Working directory: $DevSupportRoot" -ForegroundColor Yellow
Write-Host ""

# ============================================
# STEP 1: Create New Folder Structure
# ============================================
Write-Host "?? STEP 1: Creating new folder structure..." -ForegroundColor Green

$NewFolders = @(
    "01_Database",
    "01_Database/01_Tables",
    "01_Database/02_StoredProcedures",
    "01_Database/02_StoredProcedures/Consultatie",
    "01_Database/02_StoredProcedures/ICD10",
    "01_Database/02_StoredProcedures/ISCO",
    "01_Database/02_StoredProcedures/Programari",
    "01_Database/03_Functions",
    "01_Database/04_Views",
    "01_Database/05_Triggers",
    "01_Database/06_Migrations",
    "01_Database/07_ICD10_Data",
    "01_Database/08_Verification",
    "01_Database/09_Debug",
    
    "02_Scripts",
    "02_Scripts/PowerShell",
    "02_Scripts/PowerShell/Database",
    "02_Scripts/PowerShell/Deployment",
    "02_Scripts/PowerShell/Utilities",
    
    "03_Documentation",
    "03_Documentation/01_Setup",
    "03_Documentation/02_Development",
    "03_Documentation/03_Database",
    "03_Documentation/04_Features",
    "03_Documentation/04_Features/Consultatie",
    "03_Documentation/04_Features/Programari",
    "03_Documentation/04_Features/Settings",
    "03_Documentation/05_Refactoring",
    "03_Documentation/05_Refactoring/ConsultatieModal",
    "03_Documentation/05_Refactoring/EventHandlers",
    "03_Documentation/05_Refactoring/MemoryLeaks",
    "03_Documentation/05_Refactoring/CodeCleanup",
    "03_Documentation/06_Fixes",
    "03_Documentation/07_Security",
    "03_Documentation/08_Deployment",
    "03_Documentation/09_Patterns",
    "03_Documentation/10_Changes",
    
    "04_Tools",
    "04_Tools/PasswordFix",
    "04_Tools/PopUser",
    
    "05_Resources",
    "05_Resources/PDFs",
    "05_Resources/Templates",
    "05_Resources/Images"
)

foreach ($folder in $NewFolders) {
    $fullPath = Join-Path $DevSupportRoot $folder
    if (-not (Test-Path $fullPath)) {
        New-Item -ItemType Directory -Path $fullPath -Force | Out-Null
        Write-Host "  ? Created: $folder" -ForegroundColor Gray
    }
}

Write-Host "  ? Folder structure created!" -ForegroundColor Green
Write-Host ""

# ============================================
# STEP 2: Move Root Files to 04_Tools
# ============================================
Write-Host "?? STEP 2: Moving root files to Tools..." -ForegroundColor Green

# Password Fix Tools
$PasswordFixFiles = @(
    "FixPasswordTool.html",
    "AdminPasswordHashFix.csproj",
    "TestValeriaHash.csx"
)

foreach ($file in $PasswordFixFiles) {
    $source = Join-Path $DevSupportRoot $file
    if (Test-Path $source) {
        $dest = Join-Path $DevSupportRoot "04_Tools\PasswordFix\$file"
        Move-Item -Path $source -Destination $dest -Force
        Write-Host "  ? Moved: $file ? 04_Tools/PasswordFix/" -ForegroundColor Gray
    }
}

# PopUser Tools
$PopUserFiles = @(
    "CheckPopUser.ps1",
    "InvestigatePopUser.sql",
    "QuickCheckPopUser.sql",
    "VerifyPopUser.sql"
)

foreach ($file in $PopUserFiles) {
    $source = Join-Path $DevSupportRoot $file
    if (Test-Path $source) {
        $dest = Join-Path $DevSupportRoot "04_Tools\PopUser\$file"
        Move-Item -Path $source -Destination $dest -Force
        Write-Host "  ? Moved: $file ? 04_Tools/PopUser/" -ForegroundColor Gray
    }
}

Write-Host "  ? Root files moved!" -ForegroundColor Green
Write-Host ""

# ============================================
# STEP 3: Move PDF to Resources
# ============================================
Write-Host "?? STEP 3: Moving PDFs to Resources..." -ForegroundColor Green

$source = Join-Path $DevSupportRoot "SCRISOARE-MEDICALA-2024.pdf"
if (Test-Path $source) {
    $dest = Join-Path $DevSupportRoot "05_Resources\PDFs\SCRISOARE-MEDICALA-2024.pdf"
    Move-Item -Path $source -Destination $dest -Force
    Write-Host "  ? Moved: SCRISOARE-MEDICALA-2024.pdf ? 05_Resources/PDFs/" -ForegroundColor Gray
}

Write-Host "  ? PDFs moved!" -ForegroundColor Green
Write-Host ""

# ============================================
# STEP 4: Move Database Folders
# ============================================
Write-Host "??? STEP 4: Reorganizing Database folders..." -ForegroundColor Green

# Move Database subfolders to new structure
$DatabaseMappings = @{
    "Database\TableStructure" = "01_Database\01_Tables"
    "Database\StoredProcedures\Consultatie" = "01_Database\02_StoredProcedures\Consultatie"
    "Database\StoredProcedures\ICD10" = "01_Database\02_StoredProcedures\ICD10"
    "Database\StoredProcedures\ISCO" = "01_Database\02_StoredProcedures\ISCO"
    "Database\StoredProcedures\Programari" = "01_Database\02_StoredProcedures\Programari"
    "Database\Functions" = "01_Database\03_Functions"
    "Database\Views" = "01_Database\04_Views"
    "Database\Migrations" = "01_Database\06_Migrations"
    "Database\ICD10" = "01_Database\07_ICD10_Data"
    "Database\Verification" = "01_Database\08_Verification"
    "Database\Debug" = "01_Database\09_Debug"
}

foreach ($mapping in $DatabaseMappings.GetEnumerator()) {
    $source = Join-Path $DevSupportRoot $mapping.Key
    $dest = Join-Path $DevSupportRoot $mapping.Value
    
    if (Test-Path $source) {
        # Copy contents
        Get-ChildItem -Path $source -Recurse | ForEach-Object {
            $targetPath = $_.FullName.Replace($source, $dest)
            $targetDir = Split-Path $targetPath -Parent
            
            if (-not (Test-Path $targetDir)) {
                New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
            }
            
            Copy-Item -Path $_.FullName -Destination $targetPath -Force
        }
        Write-Host "  ? Copied: $($mapping.Key) ? $($mapping.Value)" -ForegroundColor Gray
    }
}

Write-Host "  ? Database folders reorganized!" -ForegroundColor Green
Write-Host ""

# ============================================
# STEP 5: Move Scripts Folders
# ============================================
Write-Host "?? STEP 5: Reorganizing Scripts folders..." -ForegroundColor Green

$source = Join-Path $DevSupportRoot "Scripts\PowerShellScripts"
if (Test-Path $source) {
    Get-ChildItem -Path $source -Recurse | ForEach-Object {
        $dest = Join-Path $DevSupportRoot "02_Scripts\PowerShell\$($_.Name)"
        Copy-Item -Path $_.FullName -Destination $dest -Force
    }
    Write-Host "  ? Copied: Scripts/PowerShellScripts ? 02_Scripts/PowerShell/" -ForegroundColor Gray
}

$source = Join-Path $DevSupportRoot "Scripts\SQLScripts"
if (Test-Path $source) {
    Get-ChildItem -Path $source -Recurse | ForEach-Object {
        $dest = Join-Path $DevSupportRoot "02_Scripts\PowerShell\Database\$($_.Name)"
        Copy-Item -Path $_.FullName -Destination $dest -Force
    }
    Write-Host "  ? Copied: Scripts/SQLScripts ? 02_Scripts/PowerShell/Database/" -ForegroundColor Gray
}

Write-Host "  ? Scripts folders reorganized!" -ForegroundColor Green
Write-Host ""

# ============================================
# STEP 6: Move Documentation Folders
# ============================================
Write-Host "?? STEP 6: Reorganizing Documentation..." -ForegroundColor Green

$DocMappings = @{
    "Documentation\Setup" = "03_Documentation\01_Setup"
    "Documentation\Development" = "03_Documentation\02_Development"
    "Documentation\Database" = "03_Documentation\03_Database"
    "Documentation\Features" = "03_Documentation\04_Features"
    "Documentation\Programari" = "03_Documentation\04_Features\Programari"
    "Documentation\Settings" = "03_Documentation\04_Features\Settings"
    "Documentation\Refactoring" = "03_Documentation\05_Refactoring"
    "Documentation\Fixes" = "03_Documentation\06_Fixes"
    "Documentation\Fix-Reports" = "03_Documentation\06_Fixes"
    "Documentation\Security" = "03_Documentation\07_Security"
    "Documentation\Deployment" = "03_Documentation\08_Deployment"
    "Documentation\Patterns" = "03_Documentation\09_Patterns"
    "Documentation\Changes" = "03_Documentation\10_Changes"
}

foreach ($mapping in $DocMappings.GetEnumerator()) {
    $source = Join-Path $DevSupportRoot $mapping.Key
    $dest = Join-Path $DevSupportRoot $mapping.Value
    
    if (Test-Path $source) {
        Get-ChildItem -Path $source -Recurse | ForEach-Object {
            $targetPath = $_.FullName.Replace($source, $dest)
            $targetDir = Split-Path $targetPath -Parent
            
            if (-not (Test-Path $targetDir)) {
                New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
            }
            
            Copy-Item -Path $_.FullName -Destination $targetPath -Force
        }
        Write-Host "  ? Copied: $($mapping.Key) ? $($mapping.Value)" -ForegroundColor Gray
    }
}

Write-Host "  ? Documentation reorganized!" -ForegroundColor Green
Write-Host ""

# ============================================
# STEP 7: Move Refactoring Docs (from root)
# ============================================
Write-Host "?? STEP 7: Moving Refactoring docs..." -ForegroundColor Green

$source = Join-Path $DevSupportRoot "Refactoring"
if (Test-Path $source) {
    Get-ChildItem -Path $source -Filter "*.md" | ForEach-Object {
        $dest = Join-Path $DevSupportRoot "03_Documentation\05_Refactoring\ConsultatieModal\$($_.Name)"
        Copy-Item -Path $_.FullName -Destination $dest -Force
        Write-Host "  ? Copied: Refactoring/$($_.Name) ? 03_Documentation/05_Refactoring/ConsultatieModal/" -ForegroundColor Gray
    }
}

Write-Host "  ? Refactoring docs moved!" -ForegroundColor Green
Write-Host ""

# ============================================
# STEP 8: Copy specific refactoring docs
# ============================================
Write-Host "?? STEP 8: Copying specific refactoring reports..." -ForegroundColor Green

$RefactoringReports = @{
    "Documentation\Refactoring\EventHandlers-Refactoring-Report-2025-01-08.md" = "03_Documentation\05_Refactoring\EventHandlers\"
    "Documentation\Refactoring\Memory-Leaks-Fix-Report-2025-01-08.md" = "03_Documentation\05_Refactoring\MemoryLeaks\"
    "Documentation\Refactoring\CodeCleanup-Report-2025-01-08.md" = "03_Documentation\05_Refactoring\CodeCleanup\"
}

foreach ($mapping in $RefactoringReports.GetEnumerator()) {
    $source = Join-Path $DevSupportRoot $mapping.Key
    if (Test-Path $source) {
        $dest = Join-Path $DevSupportRoot $mapping.Value
        $destFile = Join-Path $dest (Split-Path $source -Leaf)
        Copy-Item -Path $source -Destination $destFile -Force
        Write-Host "  ? Copied: $(Split-Path $source -Leaf) ? $($mapping.Value)" -ForegroundColor Gray
    }
}

Write-Host "  ? Refactoring reports organized!" -ForegroundColor Green
Write-Host ""

# ============================================
# STEP 9: Create README files
# ============================================
Write-Host "?? STEP 9: Creating README files..." -ForegroundColor Green

$ReadmeContents = @{
    "01_Database\README.md" = @"
# ?? Database Scripts & Structures

This folder contains all database-related SQL scripts and structures.

## Structure

- **01_Tables/** - Table creation scripts
- **02_StoredProcedures/** - All stored procedures (organized by feature)
- **03_Functions/** - SQL functions
- **04_Views/** - Database views
- **05_Triggers/** - Database triggers
- **06_Migrations/** - Migration scripts
- **07_ICD10_Data/** - ICD-10 medical codes data
- **08_Verification/** - Verification scripts
- **09_Debug/** - Debug scripts

## Usage

Execute scripts in numerical order for initial setup.
"@

    "02_Scripts\README.md" = @"
# ?? Scripts & Automation

This folder contains automation scripts for various tasks.

## Structure

- **PowerShell/** - PowerShell automation scripts
  - **Database/** - Database deployment & migration
  - **Deployment/** - Application deployment
  - **Utilities/** - General utilities

## Usage

Run scripts from project root directory.
"@

    "03_Documentation\README.md" = @"
# ?? Documentation

This folder contains all project documentation.

## Structure

- **01_Setup/** - Setup guides
- **02_Development/** - Development documentation
- **03_Database/** - Database documentation
- **04_Features/** - Feature-specific docs
- **05_Refactoring/** - Refactoring reports
- **06_Fixes/** - Bug fixes documentation
- **07_Security/** - Security documentation
- **08_Deployment/** - Deployment guides
- **09_Patterns/** - Design patterns
- **10_Changes/** - Change logs

## Navigation

Folders are numbered for logical reading order.
"@

    "04_Tools\README.md" = @"
# ??? Tools & Utilities

This folder contains utility tools and helpers.

## Structure

- **PasswordFix/** - Password hashing tools
- **PopUser/** - PopUser investigation tools

## Usage

Each subfolder contains its own README with specific instructions.
"@

    "05_Resources\README.md" = @"
# ?? Resources

This folder contains project resources and assets.

## Structure

- **PDFs/** - PDF documents
- **Templates/** - Document templates
- **Images/** - Images and graphics

## Usage

Reference these resources in documentation or application.
"@
}

foreach ($mapping in $ReadmeContents.GetEnumerator()) {
    $path = Join-Path $DevSupportRoot $mapping.Key
    Set-Content -Path $path -Value $mapping.Value -Force
    Write-Host "  ? Created: $($mapping.Key)" -ForegroundColor Gray
}

Write-Host "  ? README files created!" -ForegroundColor Green
Write-Host ""

# ============================================
# STEP 10: Create .gitignore
# ============================================
Write-Host "?? STEP 10: Creating .gitignore..." -ForegroundColor Green

$gitignore = @"
# Build outputs
bin/
obj/

# Visual Studio
.vs/
*.user
*.suo

# Temporary files
*.tmp
*.bak
*~
"@

Set-Content -Path (Join-Path $DevSupportRoot ".gitignore") -Value $gitignore -Force
Write-Host "  ? .gitignore created!" -ForegroundColor Green
Write-Host ""

# ============================================
# FINAL MESSAGE
# ============================================
Write-Host ""
Write-Host "? REORGANIZATION COMPLETE!" -ForegroundColor Green
Write-Host "==============================" -ForegroundColor Green
Write-Host ""
Write-Host "?? New structure created in: $DevSupportRoot" -ForegroundColor Yellow
Write-Host ""
Write-Host "??  IMPORTANT NOTES:" -ForegroundColor Yellow
Write-Host "  1. Old folders (Database, Scripts, Documentation) are PRESERVED" -ForegroundColor Gray
Write-Host "  2. Files were COPIED (not moved) for safety" -ForegroundColor Gray
Write-Host "  3. Review the new structure before deleting old folders" -ForegroundColor Gray
Write-Host "  4. Test that everything works correctly" -ForegroundColor Gray
Write-Host ""
Write-Host "?? Next steps:" -ForegroundColor Cyan
Write-Host "  1. Review new folder structure" -ForegroundColor Gray
Write-Host "  2. Test project compilation" -ForegroundColor Gray
Write-Host "  3. Delete old folders if satisfied:" -ForegroundColor Gray
Write-Host "     - Database/" -ForegroundColor DarkGray
Write-Host "     - Scripts/" -ForegroundColor DarkGray
Write-Host "     - Documentation/" -ForegroundColor DarkGray
Write-Host "     - Refactoring/" -ForegroundColor DarkGray
Write-Host "  4. Commit changes to Git" -ForegroundColor Gray
Write-Host ""
Write-Host "? Happy coding! ?" -ForegroundColor Green
