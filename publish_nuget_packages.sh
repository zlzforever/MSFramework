#!/bin/bash
rm -rf output
dotnet build MSFramework.sln -c Release -o ../../output
files=$(ls output | grep nupkg)
for filename in $files
do
 nuget push output/$filename -Source https://www.nuget.org/api/v2/package
done
rm -rf output