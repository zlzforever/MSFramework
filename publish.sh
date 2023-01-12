#!/usr/bin/env bash
echo $NUGET_SERVER
rm -rf src/MSFramework/bin/Release
rm -rf src/MSFramework.AspNetCore/bin/Release
#rm -rf src/MSFramework.AspNetCore.AccessControl/bin/Release
rm -rf src/MSFramework.AutoMapper/bin/Release
rm -rf src/MSFramework.Ef/bin/Release
rm -rf src/MSFramework.Ef.MySql/bin/Release
rm -rf src/MSFramework.Ef.PostgreSql/bin/Release
rm -rf src/MSFramework.Ef.SqlServer/bin/Release
rm -rf src/MSFramework.EventBus.RabbitMQ/bin/Release
rm -rf src/MSFramework.Ef.Design/bin/Release
rm -rf src/MSFramework.AspNetCore.Swagger/bin/Release
rm -rf src/MSFramework.Serialization.Newtonsoft/bin/Release
rm -rf src/DotNetCore.CAP.Dapr/bin/Release
echo $NUGET_KEY
dotnet build -c Release
dotnet pack -c Release
dotnet nuget push src/MSFramework/bin/Release/*.nupkg -s $NUGET_SERVER -k $NUGET_KEY --skip-duplicate
dotnet nuget push src/MSFramework.AspNetCore/bin/Release/*.nupkg  -s $NUGET_SERVER -k $NUGET_KEY --skip-duplicate
#dotnet nuget push src/MSFramework.AspNetCore.AccessControl/bin/Release/*.nupkg  -s $NUGET_SERVER -k $NUGET_KEY --skip-duplicate
dotnet nuget push src/MSFramework.AutoMapper/bin/Release/*.nupkg  -s $NUGET_SERVER -k $NUGET_KEY --skip-duplicate
dotnet nuget push src/MSFramework.Ef/bin/Release/*.nupkg  -s $NUGET_SERVER -k $NUGET_KEY --skip-duplicate
dotnet nuget push src/MSFramework.Ef.MySql/bin/Release/*.nupkg  -s $NUGET_SERVER -k $NUGET_KEY --skip-duplicate
dotnet nuget push src/MSFramework.Ef.PostgreSql/bin/Release/*.nupkg  -s  $NUGET_SERVER -k $NUGET_KEY --skip-duplicate
dotnet nuget push src/MSFramework.Ef.SqlServer/bin/Release/*.nupkg  -s $NUGET_SERVER -k $NUGET_KEY --skip-duplicate
dotnet nuget push src/MSFramework.EventBus.RabbitMQ/bin/Release/*.nupkg  -s $NUGET_SERVER -k $NUGET_KEY --skip-duplicate
dotnet nuget push src/MSFramework.Ef.Design/bin/Release/*.nupkg  -s $NUGET_SERVER -k $NUGET_KEY --skip-duplicate
dotnet nuget push src/MSFramework.AspNetCore.Swagger/bin/Release/*.nupkg  -s $NUGET_SERVER -k $NUGET_KEY --skip-duplicate
dotnet nuget push src/MSFramework.Serialization.Newtonsoft/bin/Release/*.nupkg  -s $NUGET_SERVER -k $NUGET_KEY --skip-duplicate
dotnet nuget push src/DotNetCore.CAP.Dapr/bin/Release/*.nupkg  -s $NUGET_SERVER -k $NUGET_KEY --skip-duplicate
