#!/usr/bin/env bash
echo $NUGET_SERVER
rm -rf src/MSFramework/bin/Release
rm -rf src/MSFramework.AspNetCore/bin/Release
rm -rf src/MSFramework.Ef/bin/Release
rm -rf src/MSFramework.Ef.MySql/bin/Release
rm -rf src/MSFramework.Ef.SqlServer/bin/Release
rm -rf src/MSFramework.EventBus.RabbitMQ/bin/Release
rm -rf src/MSFramework.AutoMapper/bin/Release
rm -rf src/MSFramework.MySql/bin/Release
rm -rf src/MSFramework.AspNetCore.Permission/bin/Release
rm -rf src/MSFramework.Mapster/bin/Release
rm -rf src/MSFramework.Jwt/bin/Release
dotnet build -c Release
dotnet pack -c Release
nuget push src/MSFramework/bin/Release/*.nupkg -SkipDuplicate -Source $NUGET_SERVER
nuget push src/MSFramework.AspNetCore/bin/Release/*.nupkg -SkipDuplicate  -Source $NUGET_SERVER
nuget push src/MSFramework.AspNetCore.Permission/bin/Release/*.nupkg -SkipDuplicate  -Source $NUGET_SERVER
nuget push src/MSFramework.Ef/bin/Release/*.nupkg -SkipDuplicate  -Source $NUGET_SERVER
nuget push src/MSFramework.Ef.MySql/bin/Release/*.nupkg -SkipDuplicate  -Source $NUGET_SERVER
nuget push src/MSFramework.Ef.SqlServer/bin/Release/*.nupkg -SkipDuplicate  -Source $NUGET_SERVER
nuget push src/MSFramework.EventBus.RabbitMQ/bin/Release/*.nupkg -SkipDuplicate  -Source $NUGET_SERVER
nuget push src/MSFramework.AutoMapper/bin/Release/*.nupkg -SkipDuplicate  -Source $NUGET_SERVER
nuget push src/MSFramework.MySql/bin/Release/*.nupkg -SkipDuplicate  -Source $NUGET_SERVER
nuget push src/MSFramework.Mapster/bin/Release/*.nupkg -SkipDuplicate  -Source $NUGET_SERVER
nuget push src/MSFramework.Jwt/bin/Release/*.nupkg -SkipDuplicate  -Source $NUGET_SERVER

sudo cp src/MSFramework/bin/Release/*.nupkg  /usr/local/share/dotnet/sdk/NuGetFallbackFolder
sudo cp src/MSFramework.AspNetCore/bin/Release/*.nupkg  /usr/local/share/dotnet/sdk/NuGetFallbackFolder
sudo cp src/MSFramework.AspNetCore.Permission/bin/Release/*.nupkg  /usr/local/share/dotnet/sdk/NuGetFallbackFolder
sudo cp src/MSFramework.Ef/bin/Release/*.nupkg  /usr/local/share/dotnet/sdk/NuGetFallbackFolder
sudo cp src/MSFramework.Ef.MySql/bin/Release/*.nupkg  /usr/local/share/dotnet/sdk/NuGetFallbackFolder
sudo cp src/MSFramework.Ef.SqlServer/bin/Release/*.nupkg  /usr/local/share/dotnet/sdk/NuGetFallbackFolder
sudo cp src/MSFramework.EventBus.RabbitMQ/bin/Release/*.nupkg  /usr/local/share/dotnet/sdk/NuGetFallbackFolder
sudo cp src/MSFramework.AutoMapper/bin/Release/*.nupkg  /usr/local/share/dotnet/sdk/NuGetFallbackFolder
sudo cp src/MSFramework.MySql/bin/Release/*.nupkg  /usr/local/share/dotnet/sdk/NuGetFallbackFolder
sudo cp src/MSFramework.Mapster/bin/Release/*.nupkg  /usr/local/share/dotnet/sdk/NuGetFallbackFolder
sudo cp src/MSFramework.Jwt/bin/Release/*.nupkg  /usr/local/share/dotnet/sdk/NuGetFallbackFolder