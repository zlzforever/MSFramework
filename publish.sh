#!/usr/bin/env bash
echo $NUGET_SERVER
rm -rf src/MSFramework/bin/Release
rm -rf src/MSFramework.AspNetCore/bin/Release
rm -rf src/MSFramework.AutoMapper/bin/Release
rm -rf src/MSFramework.Ef/bin/Release
rm -rf src/MSFramework.Ef.MySql/bin/Release
rm -rf src/MSFramework.Ef.PostgreSql/bin/Release
rm -rf src/MSFramework.Ef.SqlServer/bin/Release
rm -rf src/MSFramework.Migrator/bin/Release
rm -rf src/MSFramework.Migrator.MySql/bin/Release
rm -rf src/MSFramework.RabbitMQ/bin/Release
dotnet build -c Release
dotnet pack -c Release
nuget push src/MSFramework/bin/Release/*.nupkg -Source $NUGET_SERVER
nuget push src/MSFramework.AspNetCore/bin/Release/*.nupkg  -Source $NUGET_SERVER
nuget push src/MSFramework.AutoMapper/bin/Release/*.nupkg  -Source $NUGET_SERVER
nuget push src/MSFramework.Ef/bin/Release/*.nupkg  -Source $NUGET_SERVER
nuget push src/MSFramework.Ef.MySql/bin/Release/*.nupkg  -Source $NUGET_SERVER
nuget push src/MSFramework.Ef.PostgreSql/bin/Release/*.nupkg  -Source  $NUGET_SERVER
nuget push src/MSFramework.Ef.SqlServer/bin/Release/*.nupkg  -Source $NUGET_SERVER
nuget push src/MSFramework.Migrator/bin/Release/*.nupkg  -Source $NUGET_SERVER
nuget push src/MSFramework.Migrator.MySql/bin/Release/*.nupkg  -Source $NUGET_SERVER
nuget push src/MSFramework.RabbitMQ/bin/Release/*.nupkg  -Source $NUGET_SERVER