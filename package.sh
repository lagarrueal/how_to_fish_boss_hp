#!/usr/bin/env bash
# Build a release archive for Nexus / mod managers.
#
# The zip mirrors the game folder, so extracting it at the game root - or letting a
# mod manager handle it - puts the DLL in the right place.
#
set -euo pipefail
cd "$(dirname "$0")"

export DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
export PATH="$DOTNET_ROOT:$PATH"

VERSION=$(grep -oP '(?<=<Version>)[^<]+' HowToFish.BossHpText.csproj)
NAME="BossHpText-$VERSION"
STAGE="dist/$NAME"

echo "==> building $VERSION"
dotnet build -c Release --nologo

echo "==> staging"
rm -rf "$STAGE" "dist/$NAME.zip"
mkdir -p "$STAGE/BepInEx/plugins"
cp bin/Release/netstandard2.1/HowToFish.BossHpText.dll "$STAGE/BepInEx/plugins/"
cp README.md "$STAGE/"

echo "==> zipping"
(cd "$STAGE" && zip -qr "../$NAME.zip" .)
rm -rf "$STAGE"

echo "==> dist/$NAME.zip"
unzip -l "dist/$NAME.zip"
