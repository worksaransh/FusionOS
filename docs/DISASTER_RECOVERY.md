# Disaster Recovery — FusionOS

Status: minimal, honest baseline. This is what exists today, not an aspirational target.

## What this covers

- **Postgres data** (every module's schema — Core, Inventory, Warehouse, Procurement, Sales, Finance, and whichever of the other nine gain real tables later) via `scripts/backup-postgres.sh` (`pg_dump --format=custom`, compressed) and `scripts/restore-postgres.sh` (`pg_restore --clean --if-exists`).
- Backups are local files under `backups/` (gitignored) with a default 14-backup retention window, pruned automatically by the backup script.

## What this does not cover yet

- **Automated scheduling.** The backup script must be run manually or wired into whatever scheduler the actual deployment target uses (cron, a Kubernetes CronJob, etc.) — there is no GitHub Actions cron job doing this, because there is no reachable production database for CI to reach.
- **Offsite/durable storage.** Dumps land on local disk only. Shipping them to S3/Azure Blob/GCS (and encrypting them at rest) is a follow-up once a real deployment target exists.
- **Kafka.** Integration events are transient (03_SYSTEM_ARCHITECTURE.md §4.2) and are not backed up; the outbox pattern (`OutboxDispatcher`) means Postgres is the durable source of truth for anything Kafka carries, so restoring Postgres is sufficient to recover event-sourced state, not Kafka's topic log itself.
- **Redis.** Purely a cache — nothing stored there is not recoverable from Postgres, so it is intentionally excluded from backup scope.

## Recovery point / time objectives (target, not yet measured)

- **RPO:** as good as the last successful backup. With the default 14-backup retention and manual/cron-driven runs, this is only as tight as however often the script actually runs — decide and document a real interval (hourly/daily) once there is a real environment to run it against.
- **RTO:** one `restore-postgres.sh` run against a fresh Postgres instance, plus however long it takes to point the API host at it. Not yet measured against a realistic data volume — do that before relying on this number.

## Restore drill (do this before trusting any of the above)

```bash
./scripts/backup-postgres.sh
FUSIONOS_PG_DATABASE=fusionos_restore_drill createdb -h localhost -U fusionos fusionos_restore_drill
FUSIONOS_PG_DATABASE=fusionos_restore_drill ./scripts/restore-postgres.sh backups/fusionos-<timestamp>.dump
```

Then point a throwaway API host config at `fusionos_restore_drill` and confirm the app actually reads real data back out. Nobody has run this drill yet — treat the RPO/RTO numbers above as unverified until someone does.
