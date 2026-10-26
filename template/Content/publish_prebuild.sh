#!/usr/bin/env bash
set -euo pipefail
: "${IMAGE_NAME:?请设置构建阶段镜像名称}"
docker build --target build -t "$IMAGE_NAME" .
