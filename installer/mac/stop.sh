#!/bin/bash
cd "$(dirname "$0")"
if [ -f fusionos.pid ]; then
  kill "$(cat fusionos.pid)" 2>/dev/null && echo "FusionOS stopped." || echo "FusionOS was not running."
  rm -f fusionos.pid
else
  pkill -f "FusionOS.Api.Host" 2>/dev/null && echo "FusionOS stopped." || echo "FusionOS was not running."
fi
