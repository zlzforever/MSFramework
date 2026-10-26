#!/bin/bash
# 任一步骤失败立即退出
set -euo pipefail
export NUGET_SERVER=$PT_NUGET
export NUGET_KEY=$PT_GITEA_NUGET_KEY
sh publish.sh
