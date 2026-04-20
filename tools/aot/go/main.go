// tools/aot/go/main.go
//
// Render a .nap file to PNG via the NAPLPS NativeAOT library. Uses purego to
// call the C ABI without cgo and without a C compiler at build time. Works on
// Linux / macOS via dlopen; on Windows via syscall.LoadLibrary (purego's
// Dlopen is Unix-only, so Windows loading is handled via the stdlib syscall
// package and the returned handle is passed to purego.RegisterLibFunc).
//
// Usage:
//     go build -o naplps_demo .
//     ./naplps_demo ../../../Examples/telidraw/hello.nap hello.png

package main

import (
	"fmt"
	"os"
	"path/filepath"
	"runtime"
	"strconv"

	"github.com/ebitengine/purego"
)

func libraryPath() string {
	dir := filepath.Join("..", "publish")
	switch runtime.GOOS {
	case "windows":
		return filepath.Join(dir, "NAPLPS.dll")
	case "darwin":
		return filepath.Join(dir, "libNAPLPS.dylib")
	default:
		return filepath.Join(dir, "libNAPLPS.so")
	}
}

var (
	naplpsVersion      func(outBuf *byte, outBufLen int32) int32
	naplpsCommandCount func(napBytes *byte, napLen int32) int32
	naplpsErrorCount   func(napBytes *byte, napLen int32) int32
	naplpsRenderPng    func(napBytes *byte, napLen int32, w, h int32, outBuf *byte, outBufLen int32) int32
)

func main() {
	if len(os.Args) < 3 {
		fmt.Fprintf(os.Stderr, "usage: %s <input.nap> <output.png> [width] [height]\n", os.Args[0])
		os.Exit(2)
	}

	inPath := os.Args[1]
	outPath := os.Args[2]
	width := int32(1024)
	height := int32(768)
	if len(os.Args) > 3 {
		if w, err := strconv.Atoi(os.Args[3]); err == nil {
			width = int32(w)
		}
	}
	if len(os.Args) > 4 {
		if h, err := strconv.Atoi(os.Args[4]); err == nil {
			height = int32(h)
		}
	}

	libPath := libraryPath()
	lib, err := loadLibrary(libPath)
	if err != nil {
		fmt.Fprintf(os.Stderr, "load %s: %v\n", libPath, err)
		os.Exit(1)
	}

	purego.RegisterLibFunc(&naplpsVersion, lib, "naplps_version")
	purego.RegisterLibFunc(&naplpsCommandCount, lib, "naplps_command_count")
	purego.RegisterLibFunc(&naplpsErrorCount, lib, "naplps_error_count")
	purego.RegisterLibFunc(&naplpsRenderPng, lib, "naplps_render_png")

	versionBuf := make([]byte, 32)
	vlen := naplpsVersion(&versionBuf[0], int32(len(versionBuf)))
	if vlen < 0 {
		fmt.Fprintf(os.Stderr, "naplps_version failed: %d\n", vlen)
		os.Exit(1)
	}
	fmt.Printf("NAPLPS library version: %s\n", string(versionBuf[:vlen]))

	nap, err := os.ReadFile(inPath)
	if err != nil {
		fmt.Fprintf(os.Stderr, "cannot read %s: %v\n", inPath, err)
		os.Exit(1)
	}
	fmt.Printf("Loaded %s (%d bytes)\n", inPath, len(nap))

	napPtr := &nap[0]
	napLen := int32(len(nap))

	nCmds := naplpsCommandCount(napPtr, napLen)
	nErrs := naplpsErrorCount(napPtr, napLen)
	fmt.Printf("Parsed %d commands, %d errors\n", nCmds, nErrs)
	if nCmds < 0 {
		fmt.Fprintln(os.Stderr, "parse failed")
		os.Exit(1)
	}

	required := naplpsRenderPng(napPtr, napLen, width, height, nil, 0)
	if required < 0 {
		fmt.Fprintf(os.Stderr, "render failed (query): %d\n", required)
		os.Exit(1)
	}

	png := make([]byte, required)
	written := naplpsRenderPng(napPtr, napLen, width, height, &png[0], required)
	if written < 0 {
		fmt.Fprintf(os.Stderr, "render failed: %d\n", written)
		os.Exit(1)
	}

	if err := os.WriteFile(outPath, png[:written], 0o644); err != nil {
		fmt.Fprintf(os.Stderr, "cannot write %s: %v\n", outPath, err)
		os.Exit(1)
	}
	fmt.Printf("Wrote %s (%d bytes, %dx%d)\n", outPath, written, width, height)
}
