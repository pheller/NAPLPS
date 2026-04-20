/* tools/aot/c/main.c
 *
 * Read a .nap file, call the NAPLPS library to render it as PNG, write the PNG
 * to disk. Usage:
 *
 *     naplps_demo <input.nap> <output.png> [width] [height]
 *
 * width / height default to 1024x768. Exit code 0 on success; non-zero on any
 * error (I/O, parse, library call).
 */

#include "naplps.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

static int read_file(const char* path, uint8_t** out_buf, int32_t* out_len)
{
    FILE* f = fopen(path, "rb");
    if (!f) { fprintf(stderr, "cannot open %s\n", path); return 1; }

    if (fseek(f, 0, SEEK_END) != 0) { fclose(f); return 1; }
    long len = ftell(f);
    if (len < 0) { fclose(f); return 1; }
    rewind(f);

    uint8_t* buf = (uint8_t*)malloc((size_t)len);
    if (!buf) { fclose(f); return 1; }

    size_t n = fread(buf, 1, (size_t)len, f);
    fclose(f);
    if ((long)n != len) { free(buf); return 1; }

    *out_buf = buf;
    *out_len = (int32_t)len;
    return 0;
}

static int write_file(const char* path, const uint8_t* buf, int32_t len)
{
    FILE* f = fopen(path, "wb");
    if (!f) { fprintf(stderr, "cannot write %s\n", path); return 1; }
    size_t n = fwrite(buf, 1, (size_t)len, f);
    fclose(f);
    return ((int32_t)n == len) ? 0 : 1;
}

int main(int argc, char** argv)
{
    if (argc < 3)
    {
        fprintf(stderr, "usage: %s <input.nap> <output.png> [width] [height]\n", argv[0]);
        return 2;
    }

    const char* in_path = argv[1];
    const char* out_path = argv[2];
    int32_t width = (argc > 3) ? atoi(argv[3]) : 1024;
    int32_t height = (argc > 4) ? atoi(argv[4]) : 768;

    /* Print library version so we know the DLL loaded. */
    uint8_t version[32] = {0};
    int32_t vlen = naplps_version(version, sizeof(version));
    if (vlen < 0) { fprintf(stderr, "naplps_version failed: %d\n", vlen); return 1; }
    printf("NAPLPS library version: %s\n", version);

    /* Read the input .nap file. */
    uint8_t* nap_bytes = NULL;
    int32_t nap_len = 0;
    if (read_file(in_path, &nap_bytes, &nap_len) != 0) { return 1; }
    printf("Loaded %s (%d bytes)\n", in_path, nap_len);

    /* Sanity: how many commands parsed? */
    int32_t cmd_count = naplps_command_count(nap_bytes, nap_len);
    int32_t err_count = naplps_error_count(nap_bytes, nap_len);
    printf("Parsed %d commands, %d errors\n", cmd_count, err_count);
    if (cmd_count < 0) { fprintf(stderr, "parse failed\n"); free(nap_bytes); return 1; }

    /* Query required PNG buffer size. */
    int32_t required = naplps_render_png(nap_bytes, nap_len, width, height, NULL, 0);
    if (required < 0) { fprintf(stderr, "render failed (query): %d\n", required); free(nap_bytes); return 1; }

    /* Allocate + render. */
    uint8_t* png = (uint8_t*)malloc((size_t)required);
    if (!png) { free(nap_bytes); return 1; }
    int32_t written = naplps_render_png(nap_bytes, nap_len, width, height, png, required);
    if (written < 0) { fprintf(stderr, "render failed: %d\n", written); free(png); free(nap_bytes); return 1; }

    if (write_file(out_path, png, written) != 0) { free(png); free(nap_bytes); return 1; }
    printf("Wrote %s (%d bytes, %dx%d)\n", out_path, written, width, height);

    free(png);
    free(nap_bytes);
    return 0;
}
