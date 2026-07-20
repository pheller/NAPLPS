// C-ABI surface of the NativeAOT-published NAPLPS renderer dylib (libNAPLPS.dylib).
// See NAPLPS/NativeExports.cs. Return value = PNG byte count, or negative error code
// (-1 parse/exception, -2 buffer too small, -3 invalid input). Call once with outBufLen=0
// to query the required size, then again with a buffer of that size.
#ifndef NAPLPS_H
#define NAPLPS_H

int naplps_render_png_prodigy(const unsigned char *napBytes, int napLen,
                              int width, int height,
                              unsigned char *outBuf, int outBufLen);

int naplps_render_png(const unsigned char *napBytes, int napLen,
                      int width, int height,
                      unsigned char *outBuf, int outBufLen);

#endif
