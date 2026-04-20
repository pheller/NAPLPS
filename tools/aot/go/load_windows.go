//go:build windows

package main

import "syscall"

// loadLibrary returns an OS handle to pass to purego.RegisterLibFunc. On
// Windows, purego.Dlopen isn't provided so we use syscall.LoadLibrary directly.
func loadLibrary(path string) (uintptr, error) {
	h, err := syscall.LoadLibrary(path)
	if err != nil {
		return 0, err
	}
	return uintptr(h), nil
}
