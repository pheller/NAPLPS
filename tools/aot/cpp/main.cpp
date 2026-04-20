// tools/aot/cpp/main.cpp
//
// Read a .nap file, render it to PNG via the NAPLPS library, write the PNG.
// Usage:
//     naplps_demo <input.nap> <output.png> [width] [height]

#include "naplps.h"

#include <cstdio>
#include <cstdlib>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

namespace
{
    std::vector<uint8_t> read_file(const std::string& path)
    {
        std::ifstream f(path, std::ios::binary | std::ios::ate);
        if (!f) { throw std::runtime_error("cannot open " + path); }
        auto len = f.tellg();
        f.seekg(0);
        std::vector<uint8_t> buf(static_cast<size_t>(len));
        if (!f.read(reinterpret_cast<char*>(buf.data()), len)) { throw std::runtime_error("read failed: " + path); }
        return buf;
    }

    void write_file(const std::string& path, const std::vector<uint8_t>& buf)
    {
        std::ofstream f(path, std::ios::binary);
        if (!f) { throw std::runtime_error("cannot write " + path); }
        f.write(reinterpret_cast<const char*>(buf.data()), static_cast<std::streamsize>(buf.size()));
    }
}

int main(int argc, char** argv)
{
    if (argc < 3)
    {
        std::cerr << "usage: " << argv[0] << " <input.nap> <output.png> [width] [height]\n";
        return 2;
    }

    const std::string in_path = argv[1];
    const std::string out_path = argv[2];
    int32_t width  = (argc > 3) ? std::atoi(argv[3]) : 1024;
    int32_t height = (argc > 4) ? std::atoi(argv[4]) : 768;

    try
    {
        uint8_t version[32] = {0};
        int32_t vlen = naplps_version(version, sizeof(version));
        if (vlen < 0) { std::cerr << "naplps_version failed: " << vlen << "\n"; return 1; }
        std::cout << "NAPLPS library version: " << reinterpret_cast<const char*>(version) << "\n";

        const auto nap = read_file(in_path);
        std::cout << "Loaded " << in_path << " (" << nap.size() << " bytes)\n";

        int32_t cmd_count = naplps_command_count(nap.data(), static_cast<int32_t>(nap.size()));
        int32_t err_count = naplps_error_count(nap.data(), static_cast<int32_t>(nap.size()));
        std::cout << "Parsed " << cmd_count << " commands, " << err_count << " errors\n";
        if (cmd_count < 0) { std::cerr << "parse failed\n"; return 1; }

        // Query required buffer size.
        int32_t required = naplps_render_png(nap.data(), static_cast<int32_t>(nap.size()), width, height, nullptr, 0);
        if (required < 0) { std::cerr << "render failed (query): " << required << "\n"; return 1; }

        std::vector<uint8_t> png(static_cast<size_t>(required));
        int32_t written = naplps_render_png(nap.data(), static_cast<int32_t>(nap.size()), width, height, png.data(), required);
        if (written < 0) { std::cerr << "render failed: " << written << "\n"; return 1; }
        png.resize(static_cast<size_t>(written));

        write_file(out_path, png);
        std::cout << "Wrote " << out_path << " (" << written << " bytes, " << width << "x" << height << ")\n";
        return 0;
    }
    catch (const std::exception& e)
    {
        std::cerr << "error: " << e.what() << "\n";
        return 1;
    }
}
