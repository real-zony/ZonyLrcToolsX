#!/bin/bash
echo "${{ steps.date.outputs.date }}"
# Platforms=('win-x64' 'linux-x64' 'osx-x64')

# if ! [ -d './TempFiles' ];
# then
#     mkdir ./TempFiles
# fi

# rm -rf ./TempFiles/*

# for platform in "${Platforms[@]}"
# do
#     dotnet publish -r "$platform" -c Release -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true
    
#     cd ./bin/Release/net6.0/"$platform"/publish/ || exit
#     zip -r ./ZonyLrcTools_"$platform"_"${steps.date.outputs.date}".zip ./
#     cd ../../../../../

#     mv ./bin/Release/net6.0/"$platform"/publish/ZonyLrcTools_"$platform"_"$Version".zip ./TempFiles
# done