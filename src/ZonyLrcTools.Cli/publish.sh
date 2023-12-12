#!/bin/bash
Platforms=('win-x64' 'linux-x64' 'osx-x64' 'win-arm64' 'linux-arm64' 'osx-arm64')

if ! [ -d './TempFiles' ];
then
    mkdir ./TempFiles
fi

rm -rf ./TempFiles/*

for platform in "${Platforms[@]}"
do
    dotnet publish -r "$platform" -c Release -p:PublishSingleFile=true -p:DebugType=none --self-contained true || exit 1

    cd ./bin/Release/net8.0/"$platform"/publish/ || exit 1
    zip -r ./ZonyLrcTools_"$platform"_"${PUBLISH_VERSION}".zip ./ || exit 1
    cd ../../../../../ || exit 1

    mv ./bin/Release/net8.0/"$platform"/publish/ZonyLrcTools_"$platform"_"$PUBLISH_VERSION".zip ./TempFiles
done