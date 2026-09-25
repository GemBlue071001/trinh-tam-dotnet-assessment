#!/usr/bin/env bash
set -euo pipefail

if [ "$#" -ne 1 ]; then
  echo "Usage: ./add-migration.sh <MigrationName>"
  exit 1
fi

cd "$(dirname "$0")/.."

dotnet ef migrations add "$1" \
  --project ./Infrastructure/Infrastructure.csproj \
  --startup-project ./ECommerceApi/ECommerceApi.csproj \
  --context ECommerceDbContext \
  --output-dir Persistence/Migrations
