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
dotnet publish MSFramework.sln -c Release
nuget push src/MSFramework/bin/Release/*.nupkg -Source $NUGET_SERVER
nuget push src/MSFramework.AspNetCore/bin/Release/*.nupkg  -Source $NUGET_SERVER
nuget push src/MSFramework.Ef/bin/Release/*.nupkg  -Source $NUGET_SERVER
nuget push src/MSFramework.Ef.MySql/bin/Release/*.nupkg  -Source $NUGET_SERVER
nuget push src/MSFramework.Ef.SqlServer/bin/Release/*.nupkg  -Source $NUGET_SERVER
nuget push src/MSFramework.EventBus.RabbitMQ/bin/Release/*.nupkg  -Source $NUGET_SERVER
nuget push src/MSFramework.AutoMapper/bin/Release/*.nupkg  -Source $NUGET_SERVER
nuget push src/MSFramework.MySql/bin/Release/*.nupkg  -Source $NUGET_SERVER