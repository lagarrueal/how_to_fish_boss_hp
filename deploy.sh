#!/usr/bin/env bash
# Build the plugin and copy it to the Windows game folder.
#
#   ./deploy.sh
#
# Override the target without editing this file:
#   WIN_HOST=alexandre@192.168.1.50 ./deploy.sh
#
set -euo pipefail

WIN_HOST="${WIN_HOST:-win}"
# Note the doubled folder name - Steam's install dir contains a second "How to Fish".
GAME_DIR="${GAME_DIR:-C:/Program Files (x86)/Steam/steamapps/common/How to Fish/How to Fish}"
DLL="bin/Release/netstandard2.1/HowToFish.BossHpText.dll"

cd "$(dirname "$0")"

export DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
export PATH="$DOTNET_ROOT:$PATH"

echo "==> building"
dotnet build -c Release --nologo

echo "==> deploying to $WIN_HOST"
# scp speaks SFTP, so the remote path is taken literally and never reaches a
# remote shell - spaces need no escaping beyond bash's own quoting.
scp "$DLL" "$WIN_HOST:$GAME_DIR/BepInEx/plugins/"

echo "==> done - relaunch the game"
