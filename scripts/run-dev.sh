#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
command -v dotnet >/dev/null || { echo 'Install .NET 10 SDK / add dotnet to PATH.' >&2; exit 1; }
mkdir -p artifacts/data artifacts/logs
dotnet build ShoesShop.slnx --nologo -m:2
pids=()
cleanup() { for pid in "${pids[@]}"; do kill "$pid" 2>/dev/null || true; done; }
trap cleanup EXIT INT TERM
for entry in 'Razor RazorShoesShop 5101' 'Blazor BlazorShoesShop 5102' 'MVC MVCShoesShop 5103'; do
  read -r folder project port <<< "$entry"
  if [[ "${1:-}" == '--sqlite' ]]; then
    ASPNETCORE_ENVIRONMENT=Development DatabaseProvider=Sqlite ConnectionStrings__DefaultConnection="Data Source=$PWD/artifacts/data/$project.db" \
      dotnet run --no-build --no-launch-profile --project "$folder/$project" --urls "http://127.0.0.1:$port" >"artifacts/logs/$project.log" 2>&1 &
  else
    ASPNETCORE_ENVIRONMENT=Development dotnet run --no-build --no-launch-profile --project "$folder/$project" --urls "http://127.0.0.1:$port" >"artifacts/logs/$project.log" 2>&1 &
  fi
  pids+=("$!")
done
for port in 5101 5102 5103; do
  ready=false
  for attempt in {1..60}; do
    if curl --fail --silent "http://127.0.0.1:$port/Products" >/dev/null; then ready=true; break; fi
    sleep 1
  done
  if [[ "$ready" != true ]]; then echo "Startup failed on port $port; inspect artifacts/logs." >&2; exit 1; fi
  echo "Application on port $port passed catalog readiness check."
done
wait
