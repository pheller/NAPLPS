/* naplps.h — C header for the NAPLPS NativeAOT library.
 *
 * Link against NAPLPS.dll (Windows), libNAPLPS.so (Linux), or libNAPLPS.dylib
 * (macOS) produced by:
 *
 *     dotnet publish NAPLPS/NAPLPS.csproj -c Release -r <rid> --property:PublishAot=true
 *
 * where <rid> is one of win-x64, linux-x64, osx-x64, osx-arm64, linux-arm64.
 *
 * All functions are thread-safe; each call builds its own internal render state.
 *
 * Error codes (negative return values):
 *   -1  Parse error or exception.
 *   -2  Output buffer too small. Call again with a larger buffer.
 *   -3  Invalid input (null pointer or non-positive length).
 */

#ifndef NAPLPS_H
#define NAPLPS_H

#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

#ifdef _WIN32
    #define NAPLPS_IMPORT __declspec(dllimport)
#else
    #define NAPLPS_IMPORT
#endif

/* Render a NAPLPS byte stream to a PNG image and copy the PNG bytes into out_buf.
 * Returns the PNG byte count on success, or a negative error code.
 *
 * To query the required buffer size, call with out_buf=NULL and out_buf_len=0;
 * the return value is the byte count needed. Then allocate a buffer of that size
 * and call again.
 */
NAPLPS_IMPORT int32_t naplps_render_png(
    const uint8_t* nap_bytes,
    int32_t nap_len,
    int32_t width,
    int32_t height,
    uint8_t* out_buf,
    int32_t out_buf_len);

/* Return the count of parsed NAPLPS commands in the byte stream, or a negative
 * error code on parse failure. Useful for sanity-checking that a file loaded.
 */
NAPLPS_IMPORT int32_t naplps_command_count(
    const uint8_t* nap_bytes,
    int32_t nap_len);

/* Return the count of parse errors recorded during load. Zero means a clean parse.
 */
NAPLPS_IMPORT int32_t naplps_error_count(
    const uint8_t* nap_bytes,
    int32_t nap_len);

/* Write the library version string (ASCII, null-terminated) into out_buf.
 * Returns the written length excluding the terminator, or a negative error code.
 * Call with out_buf=NULL, out_buf_len=0 to get the required size (including terminator).
 */
NAPLPS_IMPORT int32_t naplps_version(uint8_t* out_buf, int32_t out_buf_len);

#ifdef __cplusplus
}
#endif

#endif /* NAPLPS_H */
