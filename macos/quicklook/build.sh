#!/bin/bash
# Build the NAPLPS Quick Look extensions (thumbnail + data-based preview) as .appex bundles: publish
# the NativeAOT renderer dylib once, compile each Swift extension against it, package + sign. Run from
# build-app.sh to embed them in the app. Prerequisites and signing setup: see macos/README.md.
set -euo pipefail
HERE="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$HERE/../.." && pwd)"

# .NET SDK: honor $DOTNET, else take the first dotnet on PATH.
DOTNET="${DOTNET:-$(command -v dotnet || true)}"
if [ -z "$DOTNET" ]; then
  echo "error: dotnet not found. Install the .NET 10 SDK or set DOTNET=/path/to/dotnet" >&2
  exit 1
fi

# Host architecture -> .NET RID + Swift target.
case "$(uname -m)" in
  arm64)  RID=osx-arm64; SWIFT_ARCH=arm64 ;;
  x86_64) RID=osx-x64;   SWIFT_ARCH=x86_64 ;;
  *) echo "error: unsupported architecture $(uname -m)" >&2; exit 1 ;;
esac
SWIFT_TARGET="$SWIFT_ARCH-apple-macos15.0"

# Signing identity: honor $CODESIGN_ID; else use the local "NAPLPS Development" identity when it
# exists; else fall back to ad-hoc with a warning. The Quick Look host will NOT load an ad-hoc
# signed extension, so ad-hoc builds are compile-only (see macos/README.md to set up an identity).
if [ -n "${CODESIGN_ID:-}" ]; then
  SIGN_ID="$CODESIGN_ID"
elif security find-identity -v -p codesigning 2>/dev/null | grep -q "NAPLPS Development"; then
  SIGN_ID="NAPLPS Development"
else
  SIGN_ID="-"
  echo "warning: no signing identity (set CODESIGN_ID); ad-hoc signing - Quick Look will not load the extensions" >&2
fi

# Bundle version comes from the library csproj so it cannot drift from the code.
VERSION="$(sed -n 's/.*<InformationalVersion>\([^<]*\).*/\1/p' "$ROOT/NAPLPS/NAPLPS.csproj" | head -1)"
VERSION="${VERSION:-0.0.0}"

BUILD="$HERE/build"
rm -rf "$BUILD"; mkdir -p "$BUILD"

echo "== publish NativeAOT renderer dylib ($RID) =="
"$DOTNET" publish "$ROOT/NAPLPS/NAPLPS.csproj" -r "$RID" -c Release \
  /p:NativeLib=Shared /p:PublishAot=true -o "$BUILD/native" 2>&1 | tail -1
cp "$BUILD/native/NAPLPS.dylib" "$BUILD/libNAPLPS.dylib"
install_name_tool -id @rpath/libNAPLPS.dylib "$BUILD/libNAPLPS.dylib"

# build_appex <module-name> <info.plist> <provider.swift>
build_appex() {
  local mod="$1" plist="$2" provider="$3"
  local appex="$BUILD/$mod.appex"
  echo "== compile $mod.appex ($SWIFT_TARGET) =="
  mkdir -p "$appex/Contents/MacOS" "$appex/Contents/Frameworks"
  cp "$BUILD/libNAPLPS.dylib" "$appex/Contents/Frameworks/"
  cp "$plist" "$appex/Contents/Info.plist"
  /usr/libexec/PlistBuddy -c "Set :CFBundleShortVersionString $VERSION" "$appex/Contents/Info.plist"
  xcrun swiftc \
    -module-name "$mod" \
    -target "$SWIFT_TARGET" \
    -I "$HERE/CNaplps" \
    "$HERE/Sources/NaplpsRender.swift" "$provider" \
    -framework QuickLook -framework QuickLookUI -framework QuickLookThumbnailing -framework CoreGraphics \
    -framework ImageIO -framework AppKit -framework Foundation \
    -L "$BUILD" -lNAPLPS \
    -Xlinker -rpath -Xlinker @loader_path/../Frameworks \
    -Xlinker -e -Xlinker _NSExtensionMain \
    -o "$appex/Contents/MacOS/$mod"
  codesign --force --options runtime --sign "$SIGN_ID" "$appex/Contents/Frameworks/libNAPLPS.dylib"
  codesign --force --options runtime --sign "$SIGN_ID" --entitlements "$HERE/NAPLPSQuickLook.entitlements" "$appex"
  codesign --verify --verbose=1 "$appex" 2>&1 | tail -1
  echo "built: $appex"
}

build_appex NAPLPSQuickLook "$HERE/Info.plist"         "$HERE/Sources/ThumbnailProvider.swift"
build_appex NAPLPSPreview    "$HERE/Info-preview.plist" "$HERE/Sources/PreviewProvider.swift"
