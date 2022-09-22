#!/bin/bash
dotnet publish -r "linux-x64" -c Release -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true || exit 1
cat /home/runner/work/ZonyLrcToolsX/ZonyLrcToolsX/src/ZonyLrcTools.Cli/obj/Release/net6.0/linux-x64/ZonyLrcTools.Cli.AssemblyInfo.cs
# Platforms=('win-x64' 'linux-x64' 'osx-x64')

# if ! [ -d './TempFiles' ];
# then
#     mkdir ./TempFiles
# fi

# rm -rf ./TempFiles/*

# for platform in "${Platforms[@]}"
# do
#     dotnet publish -r "$platform" -c Release -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true

#     ls -a    
#     # cd ./bin/Release/net6.0/"$platform"/publish/ || exit
#     # zip -r ./ZonyLrcTools_"$platform"_"${VERSION}".zip ./
#     # cd ../../../../../

#     # mv ./bin/Release/net6.0/"$platform"/publish/ZonyLrcTools_"$platform"_"$Version".zip ./TempFiles
# done