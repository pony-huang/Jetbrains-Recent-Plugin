# Set the location to the script's directory
Push-Location
Set-Location $PSScriptRoot

# Project name
$projectName = "JetBrains-Recent-Plugin"

# Build configurations
$configurations = @("Release")
$platforms = @("x64", "ARM64")

# Build the project
foreach ($configuration in $configurations) {
    foreach ($platform in $platforms) {
        Write-Host "Building $projectName for $platform in $configuration configuration..."
        dotnet build -c $configuration /p:Platform=$platform
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Build failed for $platform in $configuration configuration." -ForegroundColor Red
            exit $LASTEXITCODE
        }
    }
}

Write-Host "Build completed successfully." -ForegroundColor Green

# Return to the original directory
Pop-Location