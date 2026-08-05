#!/bin/bash
# Starts the FusionOS server in the background and opens it in the default browser.
cd "$(dirname "$0")"

if [ ! -f appsettings.Production.json ]; then
  echo "First run detected — running setup.sh..."
  ./setup.sh
fi

export ASPNETCORE_ENVIRONMENT=Production
chmod +x ./FusionOS.Api.Host 2>/dev/null || true
nohup ./FusionOS.Api.Host > fusionos.log 2>&1 &
echo $! > fusionos.pid
sleep 3
open http://localhost:5000
echo "FusionOS started (pid $(cat fusionos.pid)). Logs: $(pwd)/fusionos.log"
