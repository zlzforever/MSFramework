#!/bin/bash

export NUGET_SERVER=https://zlzforever-nuget.pkg.coding.net/repository/main/v3/index.json
function push_to_nuget {
    folder="$1"
    for file in "$folder"*.nupkg; do
        if [ -f "$file" ]; then
            nuget push -ApiKey api -Source "main" "$file" -SkipDuplicate
        fi
    done
}
rm -rf src/MSFramework/bin/Release
rm -rf src/MSFramework.AspNetCore/bin/Release
rm -rf src/MSFramework.AutoMapper/bin/Release
rm -rf src/MSFramework.Ef/bin/Release
rm -rf src/MSFramework.Ef.MySql/bin/Release
rm -rf src/MSFramework.Ef.PostgreSql/bin/Release
rm -rf src/MSFramework.Ef.SqlServer/bin/Release
rm -rf src/MSFramework.Ef.Design/bin/Release
rm -rf src/MSFramework.AspNetCore.Swagger/bin/Release
rm -rf src/MSFramework.Serialization.Newtonsoft/bin/Release
rm -rf src/DotNetCore.CAP.Dapr/bin/Release
rm -rf src/MSFramework.Auditing.Loki/bin/Release
dotnet build -c Release
dotnet pack -c Release

push_to_nuget "src/MSFramework/bin/Release/"
push_to_nuget "src/MSFramework.AspNetCore/bin/Release/"
push_to_nuget "src/MSFramework.AutoMapper/bin/Release/"
push_to_nuget "src/MSFramework.Ef/bin/Release/"
push_to_nuget "src/MSFramework.Ef.MySql/bin/Release/"
push_to_nuget "src/MSFramework.Ef.PostgreSql/bin/Release/"
push_to_nuget "src/MSFramework.Ef.SqlServer/bin/Release/"
push_to_nuget "src/MSFramework.Ef.Design/bin/Release/"
push_to_nuget "src/MSFramework.AspNetCore.Swagger/bin/Release/"
push_to_nuget "src/MSFramework.Serialization.Newtonsoft/bin/Release/"
push_to_nuget "src/DotNetCore.CAP.Dapr/bin/Release/"
push_to_nuget "src/MSFramework.Auditing.Loki/bin/Release/"

