#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")/.."

dotnet ef database update \
  --project ./Infrastructure/Infrastructure.csproj \
  --startup-project ./ECommerceApi/ECommerceApi.csproj \
  --context ECommerceDbContext
