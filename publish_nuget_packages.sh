#!/bin/bash
rm -rf src/MSFramework/bin/Release
rm -rf src/MSFramework.AspNetCore/bin/Release
rm -rf src/MSFramework.EntityFrameworkCore/bin/Release
rm -rf src/MSFramework.EntityFrameworkCore.MySql/bin/Release
rm -rf src/MSFramework.EntityFrameworkCore.SqlServer/bin/Release
rm -rf src/MSFramework.EventBus.RabbitMQ/bin/Release
dotnet publish MSFramework.sln -c Release
nuget push src/MSFramework/bin/Release/*.nupkg -Source https://api.nuget.org/v3/index.json
nuget push src/MSFramework.AspNetCore/bin/Release/*.nupkg  -Source https://api.nuget.org/v3/index.json
nuget push src/MSFramework.EntityFrameworkCore/bin/Release/*.nupkg  -Source https://api.nuget.org/v3/index.json
nuget push src/MSFramework.EntityFrameworkCore.MySql/bin/Release/*.nupkg  -Source https://api.nuget.org/v3/index.json
nuget push src/MSFramework.EntityFrameworkCore.SqlServer/bin/Release/*.nupkg  -Source https://api.nuget.org/v3/index.json
nuget push src/MSFramework.EventBus.RabbitMQ/bin/Release/*.nupkg  -Source https://api.nuget.org/v3/index.json