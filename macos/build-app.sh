#!/bin/bash
# Build the full macOS NAPLPS.app: publish the Avalonia app self-contained, declare the .nap
# UTType, embed the Quick Look thumbnail + preview extensions, and sign the whole bundle.
# Prerequisites and signing setup: see macos/README.md.
set -euo pipefail
HERE="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$HERE/.." && pwd)"
APP="${1:-$HOME/Desktop/NAPLPS.app}"

# .NET SDK: honor $DOTNET, else take the first dotnet on PATH.
DOTNET="${DOTNET:-$(command -v dotnet || true)}"
if [ -z "$DOTNET" ]; then
  echo "error: dotnet not found. Install the .NET 10 SDK or set DOTNET=/path/to/dotnet" >&2
  exit 1
fi

# Host architecture -> .NET RID.
case "$(uname -m)" in
  arm64)  RID=osx-arm64 ;;
  x86_64) RID=osx-x64 ;;
  *) echo "error: unsupported architecture $(uname -m)" >&2; exit 1 ;;
esac

# Signing identity: honor $CODESIGN_ID; else use the local "NAPLPS Development" identity when it
# exists; else fall back to ad-hoc with a warning. Ad-hoc is fine for running the app itself, but
# the Quick Look host will NOT load ad-hoc extensions (see macos/README.md).
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

PUB="$(mktemp -d)"

echo "== publish app ($RID) =="
"$DOTNET" publish "$ROOT/NAPLPSApp/NAPLPSApp.csproj" -c Release -r "$RID" --self-contained true \
  -p:PublishSingleFile=false -p:PublishTrimmed=false -o "$PUB" 2>&1 | tail -1

echo "== build Quick Look extensions =="
DOTNET="$DOTNET" CODESIGN_ID="$SIGN_ID" "$HERE/quicklook/build.sh" >/dev/null

echo "== assemble bundle =="
rm -rf "$APP"
mkdir -p "$APP/Contents/MacOS" "$APP/Contents/Resources" "$APP/Contents/PlugIns"
cp -R "$PUB/." "$APP/Contents/MacOS/"
cp -R "$HERE/quicklook/build/NAPLPSQuickLook.appex" "$APP/Contents/PlugIns/"
cp -R "$HERE/quicklook/build/NAPLPSPreview.appex" "$APP/Contents/PlugIns/"

# App icon: generated from the repo asset so the bundle is self-contained on any host.
ICONTMP="$(mktemp -d)"
if sips -s format png "$ROOT/NAPLPSApp/Assets/naplps.ico" --out "$ICONTMP/base.png" >/dev/null 2>&1; then
  ISET="$ICONTMP/naplps.iconset"; mkdir -p "$ISET"
  for S in 16 32 64 128 256 512; do
    sips -z "$S" "$S" "$ICONTMP/base.png" --out "$ISET/icon_${S}x${S}.png" >/dev/null
    D=$((S * 2))
    sips -z "$D" "$D" "$ICONTMP/base.png" --out "$ISET/icon_${S}x${S}@2x.png" >/dev/null
  done
  iconutil -c icns "$ISET" -o "$APP/Contents/Resources/naplps.icns"
else
  echo "warning: could not convert NAPLPSApp/Assets/naplps.ico; app will have no icon" >&2
fi
rm -rf "$ICONTMP"

cat > "$APP/Contents/Info.plist" <<'PLIST'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0"><dict>
  <key>CFBundleName</key><string>NAPLPS</string>
  <key>CFBundleDisplayName</key><string>NAPLPS Toolbox</string>
  <key>CFBundleIdentifier</key><string>com.foxcouncil.naplps</string>
  <key>CFBundleVersion</key><string>0.0.0</string>
  <key>CFBundleShortVersionString</key><string>0.0.0</string>
  <key>CFBundleExecutable</key><string>NAPLPSApp</string>
  <key>CFBundlePackageType</key><string>APPL</string>
  <key>CFBundleIconFile</key><string>naplps</string>
  <key>CFBundleInfoDictionaryVersion</key><string>6.0</string>
  <key>LSMinimumSystemVersion</key><string>15.0</string>
  <key>NSHighResolutionCapable</key><true/>
  <key>NSPrincipalClass</key><string>NSApplication</string>
  <!-- com.foxcouncil.naplps is the one canonical identifier for NAPLPS pictures; other heads and
       platforms import the same id so .nap binds identically everywhere. Deliberately NOT
       conforming to public.image - that would let bitmap editors claim .nap; the Quick Look
       preview extension supplies the image experience and the app owns the type below. -->
  <key>UTExportedTypeDeclarations</key><array>
    <dict>
      <key>UTTypeIdentifier</key><string>com.foxcouncil.naplps</string>
      <key>UTTypeDescription</key><string>NAPLPS Picture</string>
      <key>UTTypeConformsTo</key><array><string>public.data</string><string>public.content</string></array>
      <key>UTTypeTagSpecification</key><dict>
        <key>public.filename-extension</key><array><string>nap</string><string>NAP</string></array>
      </dict>
    </dict>
  </array>
  <!-- The app owns the .nap type (LSHandlerRank Owner) so it is the default opener, not some image
       editor that happened to be in the Open With list. -->
  <key>CFBundleDocumentTypes</key><array><dict>
    <key>CFBundleTypeName</key><string>NAPLPS Picture</string>
    <key>CFBundleTypeRole</key><string>Viewer</string>
    <key>LSHandlerRank</key><string>Owner</string>
    <key>LSItemContentTypes</key><array><string>com.foxcouncil.naplps</string></array>
  </dict></array>
</dict></plist>
PLIST
/usr/libexec/PlistBuddy -c "Set :CFBundleVersion $VERSION" \
                        -c "Set :CFBundleShortVersionString $VERSION" "$APP/Contents/Info.plist"

chmod +x "$APP/Contents/MacOS/NAPLPSApp"
# The app is a .NET (CoreCLR) app: it JITs, so it must be signed WITHOUT the hardened runtime -
# hardened runtime blocks executable-memory allocation and the app would die instantly at launch.
# So: (1) deep-sign the whole app un-hardened (payload + a first pass over the extensions), then
# (2) re-sign each embedded extension WITH hardened runtime + sandbox entitlements (required for the
# Quick Look host to load it, fine since NativeAOT/no JIT), then (3) re-seal the app top-level.
codesign --force --deep --sign "$SIGN_ID" "$APP" 2>&1 | tail -1
for EXT in "$APP/Contents/PlugIns/NAPLPSQuickLook.appex" "$APP/Contents/PlugIns/NAPLPSPreview.appex"; do
  codesign --force --options runtime --sign "$SIGN_ID" "$EXT/Contents/Frameworks/libNAPLPS.dylib" 2>&1 | tail -1
  codesign --force --options runtime --sign "$SIGN_ID" --entitlements "$HERE/quicklook/NAPLPSQuickLook.entitlements" "$EXT" 2>&1 | tail -1
done
codesign --force --sign "$SIGN_ID" "$APP" 2>&1 | tail -1
codesign --verify --verbose=1 "$APP" 2>&1 | tail -1
rm -rf "$PUB"
echo "built: $APP"
