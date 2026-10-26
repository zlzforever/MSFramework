#!/usr/bin/env bash
# 任一步骤失败立即退出，禁止 build 失败后继续 pack/push 掩盖错误
set -euo pipefail
rm -rf src/MSFramework/bin/Release
rm -rf src/MSFramework.AspNetCore/bin/Release
#rm -rf src/MSFramework.AspNetCore.AccessControl/bin/Release
rm -rf src/MSFramework.AutoMapper/bin/Release
rm -rf src/MSFramework.Ef/bin/Release
rm -rf src/MSFramework.Ef.MySql/bin/Release
rm -rf src/MSFramework.Ef.PostgreSql/bin/Release
rm -rf src/MSFramework.Ef.SqlServer/bin/Release
rm -rf src/MSFramework.Ef.Design/bin/Release
#rm -rf src/MSFramework.AspNetCore.Swagger/bin/Release
rm -rf src/MSFramework.Serialization.Newtonsoft/bin/Release
rm -rf src/DotNetCore.CAP.Dapr/bin/Release
rm -rf src/MSFramework.Dapr/bin/Release
rm -rf src/MSFramework.Analyzers/bin/Release
rm -rf src/MSFramework.Ef.Analyzers/bin/Release
dotnet build -c Release
dotnet pack -c Release
# 凭据脱敏输出：NUGET_KEY 仅显示前 8 位，其余掩码，避免明文进入终端/CI 日志
echo "NUGET_SERVER: ${NUGET_SERVER:-(未设置)}"
echo "NUGET_KEY: ${NUGET_KEY:0:8}****"
dotnet nuget push src/MSFramework/bin/Release/*.nupkg -s "$NUGET_SERVER" -k "$NUGET_KEY" --skip-duplicate
dotnet nuget push src/MSFramework.AspNetCore/bin/Release/*.nupkg  -s "$NUGET_SERVER" -k "$NUGET_KEY" --skip-duplicate
dotnet nuget push src/MSFramework.AutoMapper/bin/Release/*.nupkg  -s "$NUGET_SERVER" -k "$NUGET_KEY" --skip-duplicate
dotnet nuget push src/MSFramework.Ef/bin/Release/*.nupkg  -s "$NUGET_SERVER" -k "$NUGET_KEY" --skip-duplicate
dotnet nuget push src/MSFramework.Ef.MySql/bin/Release/*.nupkg  -s "$NUGET_SERVER" -k "$NUGET_KEY" --skip-duplicate
dotnet nuget push src/MSFramework.Ef.PostgreSql/bin/Release/*.nupkg  -s "$NUGET_SERVER" -k "$NUGET_KEY" --skip-duplicate
dotnet nuget push src/MSFramework.Ef.SqlServer/bin/Release/*.nupkg  -s "$NUGET_SERVER" -k "$NUGET_KEY" --skip-duplicate
dotnet nuget push src/MSFramework.Ef.Design/bin/Release/*.nupkg  -s "$NUGET_SERVER" -k "$NUGET_KEY" --skip-duplicate
#dotnet nuget push src/MSFramework.AspNetCore.Swagger/bin/Release/*.nupkg  -s "$NUGET_SERVER" -k "$NUGET_KEY" --skip-duplicate
dotnet nuget push src/MSFramework.Serialization.Newtonsoft/bin/Release/*.nupkg  -s "$NUGET_SERVER" -k "$NUGET_KEY" --skip-duplicate
dotnet nuget push src/DotNetCore.CAP.Dapr/bin/Release/*.nupkg  -s "$NUGET_SERVER" -k "$NUGET_KEY" --skip-duplicate
dotnet nuget push src/MSFramework.Dapr/bin/Release/*.nupkg  -s "$NUGET_SERVER" -k "$NUGET_KEY" --skip-duplicate
dotnet nuget push src/MSFramework.Analyzers/bin/Release/*.nupkg  -s "$NUGET_SERVER" -k "$NUGET_KEY" --skip-duplicate
dotnet nuget push src/MSFramework.Ef.Analyzers/bin/Release/*.nupkg  -s "$NUGET_SERVER" -k "$NUGET_KEY" --skip-duplicate
