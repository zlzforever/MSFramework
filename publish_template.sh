#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")" && pwd)"
: "${NUGET_KEY:?请通过安全环境变量设置 NUGET_KEY}"
OUTPUT="$(mktemp -d)"
# 发布前验证实际生成物；版本必须与 version.props 保持一致。
bash "$ROOT/tests/verify-template.sh"
dotnet pack "$ROOT/template/MSFramework.API.Template.csproj" -c Release -o "$OUTPUT"
dotnet nuget push "$OUTPUT"/MSFramework.API.Template.*.nupkg \
    -s "${NUGET_SERVER:-https://api.nuget.org/v3/index.json}" -k "$NUGET_KEY" --skip-duplicate
