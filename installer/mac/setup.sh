#!/bin/bash
# One-time setup: asks for PostgreSQL connection details, generates a random
# JWT signing key, and writes appsettings.Production.json next to this script.
# Re-run any time to change the connection settings.
set -e
cd "$(dirname "$0")"

echo "FusionOS setup"
echo "=============="
read -p "Postgres host [localhost]: " PG_HOST
PG_HOST=${PG_HOST:-localhost}
read -p "Postgres port [5432]: " PG_PORT
PG_PORT=${PG_PORT:-5432}
read -p "Database name [fusionos]: " PG_DB
PG_DB=${PG_DB:-fusionos}
read -p "Username [fusionos]: " PG_USER
PG_USER=${PG_USER:-fusionos}
read -s -p "Password: " PG_PASS
echo

SIGNING_KEY=$(LC_ALL=C tr -dc 'A-Za-z0-9' < /dev/urandom | head -c 48)

cat > appsettings.Production.json <<EOF
{
  "Deployment": { "AutoMigrate": true },
  "ConnectionStrings": { "Postgres": "Host=$PG_HOST;Port=$PG_PORT;Database=$PG_DB;Username=$PG_USER;Password=$PG_PASS" },
  "Jwt": { "Issuer": "fusionos", "Audience": "fusionos-clients", "SigningKey": "$SIGNING_KEY" }
}
EOF

chmod +x FusionOS.Api.Host launch.sh stop.sh 2>/dev/null || true

echo "Wrote appsettings.Production.json. Run ./launch.sh to start FusionOS."
