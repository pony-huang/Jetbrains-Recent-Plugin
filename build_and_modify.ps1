# Set the location to the script's directory
Push-Location -Path $PSScriptRoot

# Path to the build.ps1 and modify_plugin.ps1 scripts
$buildScript = "$PSScriptRoot/build.ps1"
$modifyScript = "$PSScriptRoot/modify_plugin.ps1"

# Ensure both build and modify scripts exist
if (-not (Test-Path $buildScript)) {
    Write-Host "Error: $buildScript not found!" -ForegroundColor Red
    exit 1
}
if (-not (Test-Path $modifyScript)) {
    Write-Host "Error: $modifyScript not found!" -ForegroundColor Red
    exit 1
}

# Execute build.ps1
Write-Host "Running build.ps1..."
& $buildScript
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed. Exiting..." -ForegroundColor Red
    exit $LASTEXITCODE
}

# Execute modify_plugin.ps1
Write-Host "Running modify_plugin.ps1..."
& $modifyScript
if ($LASTEXITCODE -ne 0) {
    Write-Host "Modify plugin failed. Exiting..." -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "Both scripts executed successfully." -ForegroundColor Green

# Return to the original directory
Pop-Location
