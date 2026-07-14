#!/usr/bin/env bash
# Restores a dump produced by scripts/backup-postgres.sh into a target
# database. Restoring into a fresh/empty database is strongly recommended —
# this uses pg_restore's --clean --if-exists so it can also overwrite an
# existing one, but that is destructive and asks for confirmation first.
#
# Usage:
#   ./scripts/restore-postgres.sh backups/fusionos-20260714T120000Z.dump
#   FUSIONOS_PG_DATABASE=fusionos_restore_test ./scripts/restore-postgres.sh <dump-file>
set -euo pipefail
cd "$(dirname "$0")/.."

if [[ $# -ne 1 ]]; then
  echo "Usage: $0 <path-to-dump-file>" >&2
  exit 1
fi
DUMP_FILE="$1"
if [[ ! -f "$DUMP_FILE" ]]; then
  echo "No such file: $DUMP_FILE" >&2
  exit 1
fi

PGHOST="${FUSIONOS_PG_HOST:-localhost}"
PGPORT="${FUSIONOS_PG_PORT:-5432}"
PGUSER="${FUSIONOS_PG_USER:-fusionos}"
PGDATABASE="${FUSIONOS_PG_DATABASE:-fusionos}"
export PGPASSWORD="${FUSIONOS_PG_PASSWORD:-fusionos}"

echo "About to restore '$DUMP_FILE' into ${PGDATABASE}@${PGHOST}:${PGPORT}, dropping/recreating any conflicting objects."
read -r -p "Continue? [y/N] " confirm
if [[ "$confirm" != "y" && "$confirm" != "Y" ]]; then
  echo "Aborted."
  exit 1
fi

pg_restore \
  --host="$PGHOST" \
  --port="$PGPORT" \
  --username="$PGUSER" \
  --dbname="$PGDATABASE" \
  --clean \
  --if-exists \
  --no-owner \
  "$DUMP_FILE"

echo "Restore complete."
