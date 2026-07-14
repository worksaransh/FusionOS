#!/usr/bin/env bash
# Backup/DR (audit DevOps finding — there was no backup story at all: no
# script, no schedule, no documented recovery point/time objective). Takes a
# single compressed pg_dump of every FusionOS schema and writes it, timestamped,
# to ./backups/ (gitignored — these are data, not source). Works against the
# docker-compose Postgres by default, or any reachable instance via
# FUSIONOS_PG_HOST/PORT/USER/DB env vars — same connection shape as
# ConnectionStrings__Postgres everywhere else in this repo.
#
# Usage:
#   ./scripts/backup-postgres.sh                  # dump to backups/fusionos-<timestamp>.dump
#   FUSIONOS_PG_HOST=prod-db.internal ./scripts/backup-postgres.sh
#
# Retention: keeps the most recent KEEP_BACKUPS (default 14) local dumps and
# deletes older ones. This script only produces a local file — actually
# shipping it offsite (S3/Azure Blob/etc.) and running this on a schedule
# against a real production database is a follow-up once one exists; see
# docs/DISASTER_RECOVERY.md for the RPO/RTO this is meant to satisfy and what
# is still missing to get there.
set -euo pipefail
cd "$(dirname "$0")/.."

PGHOST="${FUSIONOS_PG_HOST:-localhost}"
PGPORT="${FUSIONOS_PG_PORT:-5432}"
PGUSER="${FUSIONOS_PG_USER:-fusionos}"
PGDATABASE="${FUSIONOS_PG_DATABASE:-fusionos}"
export PGPASSWORD="${FUSIONOS_PG_PASSWORD:-fusionos}"
KEEP_BACKUPS="${KEEP_BACKUPS:-14}"

mkdir -p backups
timestamp=$(date -u +%Y%m%dT%H%M%SZ)
outfile="backups/fusionos-${timestamp}.dump"

echo "Dumping ${PGDATABASE}@${PGHOST}:${PGPORT} -> ${outfile}"
pg_dump \
  --host="$PGHOST" \
  --port="$PGPORT" \
  --username="$PGUSER" \
  --dbname="$PGDATABASE" \
  --format=custom \
  --compress=9 \
  --file="$outfile"

echo "Backup written: $outfile ($(du -h "$outfile" | cut -f1))"

# Prune anything past the retention window.
mapfile -t existing < <(ls -1t backups/fusionos-*.dump 2>/dev/null || true)
if (( ${#existing[@]} > KEEP_BACKUPS )); then
  for old in "${existing[@]:$KEEP_BACKUPS}"; do
    echo "Pruning old backup: $old"
    rm -f "$old"
  done
fi
