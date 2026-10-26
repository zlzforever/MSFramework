#!/usr/bin/env bash
# 验证真正打包后的模板及 NuGet 消费链路，不修改开发者全局模板安装或包缓存。
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd -P)"
WORK="${TEMPLATE_VERIFY_DIR:-$(mktemp -d)}"
mkdir -p "$WORK/feed"
# macOS 的 /var 与 /private/var 指向同一路径，统一物理路径避免 NuGet 并行重复还原同一项目。
WORK="$(cd "$WORK" && pwd -P)"

python3 - "$ROOT" <<'PY'
import pathlib
import sys
import xml.etree.ElementTree as ET
root = pathlib.Path(sys.argv[1])
version = ET.parse(root / 'version.props').findtext('./{*}PropertyGroup/{*}PackageVersion')
if not version:
    raise SystemExit('version.props is missing PackageVersion')
template_version = ET.parse(root / 'template/Content/Directory.Packages.props').findtext('./PropertyGroup/MSFrameworkVersion')
if version != template_version:
    raise SystemExit(f'Template MSFrameworkVersion {template_version} does not match version.props {version}')
PY

dotnet build "$ROOT/MSFramework.sln" -c Release --verbosity minimal
for project in MSFramework MSFramework.AspNetCore MSFramework.Ef MSFramework.Ef.MySql MSFramework.Analyzers MSFramework.Ef.Analyzers; do
    dotnet pack "$ROOT/src/$project/$project.csproj" -c Release --no-build -o "$WORK/feed" --verbosity minimal
done
dotnet pack "$ROOT/template/MSFramework.API.Template.csproj" -c Release -o "$WORK/feed" --verbosity minimal

dotnet new install "$WORK"/feed/MSFramework.API.Template.*.nupkg --debug:custom-hive "$WORK/hive"
dotnet new msframework-api -n TemplateSmoke -o "$WORK/app" --debug:custom-hive "$WORK/hive"
dotnet restore "$WORK/app/TemplateSmoke.API.sln" --source "$WORK/feed" \
    --source https://api.nuget.org/v3/index.json --packages "$WORK/packages"
dotnet test "$WORK/app/TemplateSmoke.API.sln" -c Release --no-restore --verbosity minimal
printf 'Template verification artifacts: %s\n' "$WORK"
