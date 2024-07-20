#!/usr/bin/env bash
docker-compose up -d --build || exit 2
id=$(docker build frontend-test -q) || exit 3
docker run --network=host -e FRONTEND_TEST_URL="http://localhost:8580/" "$id"
