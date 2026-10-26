#!/usr/bin/env bash
set -euo pipefail
: "${ENV_FILE:?请设置包含数据库、JWT、APP_API_TOKEN 的外部环境文件路径，不得提交该文件}"
docker build -f Dockerfile.Development -t template-api:dev .
docker run --rm --name template-api --env-file "$ENV_FILE" -p 8080:8080 template-api:dev
