$Platforms = @('win-x64', 'linux-x64', 'osx-x64', 'win-arm64', 'linux-arm64', 'osx-arm64')

if (-not (Test-Path ./TempFiles)) {
    New-Item -ItemType Directory -Path ./TempFiles | Out-Null
}

Remove-Item ./TempFiles/* -Recurse -Force

foreach ($platform in $Platforms) {
    dotnet publish -r $platform -c Release -p:PublishSingleFile=true --self-contained true | Out-Null
    if ($LASTEXITCODE -ne 0) {
        exit 1
    }
    
    Set-Location ./bin/Release/net7.0/$platform/publish/
    Compress-Archive -Path ./* -DestinationPath ./ZonyLrcTools_${platform}_${Env:PUBLISH_VERSION}.zip | Out-Null
    Set-Location ../../../../../

    Move-Item ./bin/Release/net7.0/$platform/publish/ZonyLrcTools_${platform}_${Env:PUBLISH_VERSION}.zip ./TempFiles
}