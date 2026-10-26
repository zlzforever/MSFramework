#!/bin/bash
# 任一步骤失败立即退出
set -euo pipefail
export NUGET_SERVER=https://api.nuget.org/v3/index.json
sh publish.sh