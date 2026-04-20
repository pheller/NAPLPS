// tools/aot/swift/Sources/naplps_demo/main.swift
//
// Render a .nap file to PNG via the NAPLPS NativeAOT library. CNaplps is the
// system-library target declared in Package.swift; it bridges naplps.h into
// Swift automatically.
//
// Build:  swift build -c release
// Run:    ./.build/release/naplps_demo ../../../../Examples/telidraw/hello.nap hello.png

import CNaplps
import Foundation

func main() -> Int32
{
    let args = CommandLine.arguments
    guard args.count >= 3 else
    {
        FileHandle.standardError.write("usage: \(args[0]) <input.nap> <output.png> [width] [height]\n".data(using: .utf8)!)
        return 2
    }

    let inPath = args[1]
    let outPath = args[2]
    let width: Int32 = args.count > 3 ? Int32(args[3]) ?? 1024 : 1024
    let height: Int32 = args.count > 4 ? Int32(args[4]) ?? 768 : 768

    // Version string.
    var versionBuf = [UInt8](repeating: 0, count: 32)
    let vlen = versionBuf.withUnsafeMutableBufferPointer { buf in
        naplps_version(buf.baseAddress, Int32(buf.count))
    }
    guard vlen >= 0 else
    {
        FileHandle.standardError.write("naplps_version failed: \(vlen)\n".data(using: .utf8)!)
        return 1
    }
    let versionStr = String(bytes: versionBuf[0..<Int(vlen)], encoding: .ascii) ?? "?"
    print("NAPLPS library version: \(versionStr)")

    // Load input file.
    guard let nap = try? Data(contentsOf: URL(fileURLWithPath: inPath)) else
    {
        FileHandle.standardError.write("cannot read \(inPath)\n".data(using: .utf8)!)
        return 1
    }
    print("Loaded \(inPath) (\(nap.count) bytes)")

    let result: Int32 = nap.withUnsafeBytes { napPtr -> Int32 in
        let napBase = napPtr.bindMemory(to: UInt8.self).baseAddress
        let napLen = Int32(nap.count)

        let nCmds = naplps_command_count(napBase, napLen)
        let nErrs = naplps_error_count(napBase, napLen)
        print("Parsed \(nCmds) commands, \(nErrs) errors")
        if nCmds < 0 { return 1 }

        let required = naplps_render_png(napBase, napLen, width, height, nil, 0)
        if required < 0
        {
            FileHandle.standardError.write("render failed (query): \(required)\n".data(using: .utf8)!)
            return 1
        }

        var png = [UInt8](repeating: 0, count: Int(required))
        let written = png.withUnsafeMutableBufferPointer { buf in
            naplps_render_png(napBase, napLen, width, height, buf.baseAddress, required)
        }
        if written < 0
        {
            FileHandle.standardError.write("render failed: \(written)\n".data(using: .utf8)!)
            return 1
        }

        do { try Data(png[0..<Int(written)]).write(to: URL(fileURLWithPath: outPath)) }
        catch
        {
            FileHandle.standardError.write("cannot write \(outPath): \(error)\n".data(using: .utf8)!)
            return 1
        }
        print("Wrote \(outPath) (\(written) bytes, \(width)x\(height))")
        return 0
    }

    return result
}

exit(main())
