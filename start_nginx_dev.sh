#!/usr/bin/env sh
docker run --network=host  -v=${PWD}/nginx_dev.conf:/etc/nginx/nginx.conf:ro -p 8880:8880  nginx:1.25.5-alpine
