#!/usr/bin/env bash
set -euo pipefail
: "${IMAGE_NAME:?请设置含标签的目标镜像名称}"
docker build -t "$IMAGE_NAME" .
docker push "$IMAGE_NAME"
