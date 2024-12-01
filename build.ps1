# Set the location to the script's directory
Push-Location -Path $PSScriptRoot

# Project name and safe name
$workspace = "Jetbrains-Recent-Plugin"
$projectName = "Jetbrains-Recent-Plugin"
$safeProjectName = "JetBrains_Recent_Plugin"

# Temporary directory for builds
$tempDir = "./$workspace/out/temp"
$releaseBasePath = "./$workspace/bin"

# Build configurations and platforms
$configurations = @("Release")
$platforms = @("x64", "ARM64")

# Assembly name
$assembly = "Community.PowerToys.Run.Plugin.$safeProjectName"

# Ensure the temporary directory exists
if (-not (Test-Path $tempDir)) {
    New-Item -ItemType Directory -Path $tempDir | Out-Null
}

foreach ($configuration in $configurations) {
    foreach ($platform in $platforms) {
        Write-Host "Building $projectName for $platform in $configuration configuration"

        # Clean build output before starting a new build
        $buildOutputPath = "./$workspace/bin/$platform/$configuration"
        if (Test-Path $buildOutputPath) {
            Write-Host "Cleaning previous build output..."
            Remove-Item "$buildOutputPath/*" -Recurse -Force -ErrorAction Ignore
        }

        # Build the project
        dotnet build -c $configuration /p:Platform=$platform
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Build failed for $platform in $configuration configuration." -ForegroundColor Red
            exit $LASTEXITCODE
        }

        # Define release path
        $releasePath = "$releaseBasePath/$platform/$configuration/net8.0-windows"

        # Clear temporary directory
        Remove-Item "$tempDir/*" -Recurse -Force -ErrorAction Ignore

        # Define items to copy
        $items = @(
            "$releasePath/$assembly.deps.json",
            "$releasePath/$assembly.dll",
            "$releasePath/plugin.json",
            "$releasePath/Images"
        )

        # Copy items to temporary directory
        Write-Host "Copying $items"
        Copy-Item $items "$tempDir" -Recurse -Force -ErrorAction Stop
        
        # Define output zip file name
        $outputZip = "./$($projectName)/out/$($projectName)-$($configuration)-$platform.zip"
        # Compress the directory
        Compress-Archive -Path "$tempDir/*" -DestinationPath $outputZip -Force
    }
}

Write-Host "Build completed successfully." -ForegroundColor Green

# Return to the original directory
Pop-Location
