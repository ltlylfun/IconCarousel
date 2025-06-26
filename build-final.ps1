# Simple build script for IconCarousel
param(
    [string]$Version = "1.0.0"
)

Write-Host "Building IconCarousel v$Version..." -ForegroundColor Green

# Clean previous builds
if (Test-Path "bin\Release") {
    Remove-Item "bin\Release" -Recurse -Force
}

# Build and publish
Write-Host "Building project..." -ForegroundColor Yellow
dotnet publish IconCarousel.csproj -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Create release directory
$releaseDir = "release"
if (Test-Path $releaseDir) {
    Remove-Item $releaseDir -Recurse -Force
}
New-Item -ItemType Directory -Path $releaseDir

# Package the published files
Write-Host "Packaging files..." -ForegroundColor Yellow
$zipFile = "$releaseDir\IconCarousel-v$Version.zip"
$publishDir = "bin\Release\net6.0-windows\publish"

if (Test-Path $publishDir) {
    Compress-Archive -Path "$publishDir\*" -DestinationPath $zipFile
    
    Write-Host "Build completed!" -ForegroundColor Green
    Write-Host "Package location: $zipFile" -ForegroundColor Cyan
    
    # Display file size
    $fileSize = [math]::Round((Get-Item $zipFile).Length / 1MB, 2)
    Write-Host "File size: ${fileSize} MB" -ForegroundColor White
    
    Write-Host ""
    Write-Host "Ready to upload to GitHub Release!" -ForegroundColor Green
} else {
    Write-Host "Publish directory not found!" -ForegroundColor Red
    exit 1
}
