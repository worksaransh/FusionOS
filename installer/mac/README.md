# FusionOS for macOS

This is a self-contained build of the FusionOS server (it also serves the web app itself —
no separate install needed) — not a signed `.pkg`/`.dmg`. macOS will flag it as from an
unidentified developer the first time; that's expected for an unsigned/unnotarized build,
not a sign of a broken download.

## Requirements

- macOS 12+ (Apple Silicon or Intel — use the zip matching your Mac).
- A running PostgreSQL server. If you don't have one: `brew install postgresql@16 && brew services start postgresql@16`, then `createuser fusionos -P` and `createdb fusionos -O fusionos` (matches this package's setup defaults).

## Install

1. Unzip the download.
2. Open Terminal, `cd` into the unzipped folder.
3. First time only, restore executable permissions (zip doesn't preserve them) and clear the
   Gatekeeper quarantine flag (unsigned build):
   `chmod +x FusionOS.Api.Host launch.sh stop.sh setup.sh && xattr -dr com.apple.quarantine .`
4. `./launch.sh` — runs setup on first launch (asks for your Postgres connection details),
   then starts FusionOS and opens it in your browser at `http://localhost:5000`.

## Stopping / restarting

- `./stop.sh` stops the server.
- `./launch.sh` starts it again (setup only runs once — delete `appsettings.Production.json`
  to redo it, e.g. to change the database connection).

## Uninstall

Run `./stop.sh`, then just delete the folder — nothing is installed outside it, and your
PostgreSQL database is never touched by this.
