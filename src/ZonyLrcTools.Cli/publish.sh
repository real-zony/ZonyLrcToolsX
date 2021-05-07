#!/bin/bash
read -r -p "请输入版本号:" Version
Platforms=('win-x64' 'linux-x64' 'osx-x64')

if ! [ -d './TempFiles' ];
then
    mkdir ./TempFiles
fi

rm -rf ./TempFiles/*

for platform in "${Platforms[@]}"
do
    dotnet publish -r "$platform" -c Release -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true    
    
    zip -r -j ./bin/Release/net5.0/"$platform"/publish/ZonyLrcTools_"$platform"_"$Version".zip ./bin/Release/net5.0/"$platform"/publish/

    mv ./bin/Release/net5.0/"$platform"/publish/ZonyLrcTools_"$platform"_"$Version".zip ./TempFiles
done