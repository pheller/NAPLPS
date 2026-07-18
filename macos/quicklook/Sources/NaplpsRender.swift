// Renders a NAPLPS .nap byte stream to a CGImage via the NativeAOT dylib, by way of a PNG
// round-trip. The renderer detects the system type from the stream header (A1 C8 domain
// marker -> Prodigy authentic pipeline; 0x0E -> Telidon; otherwise generic NAPLPS with the
// default palette), so thumbnails use the palette the file was authored for. Shared by the
// thumbnail (and any future preview) provider.
import CoreGraphics
import Foundation
import ImageIO
import CNaplps

enum NaplpsRender {
    static let deviceWidth: Int32 = 640
    static let deviceHeight: Int32 = 480

    static func renderPNG(_ data: Data) -> Data? {
        if data.isEmpty { return nil }
        return data.withUnsafeBytes { (raw: UnsafeRawBufferPointer) -> Data? in
            let base = raw.bindMemory(to: UInt8.self).baseAddress
            let need = naplps_render_png(base, Int32(data.count), deviceWidth, deviceHeight, nil, 0)
            if need <= 0 { return nil }
            var out = Data(count: Int(need))
            let n = out.withUnsafeMutableBytes { (ob: UnsafeMutableRawBufferPointer) -> Int32 in
                naplps_render_png(base, Int32(data.count), deviceWidth, deviceHeight,
                                  ob.bindMemory(to: UInt8.self).baseAddress, need)
            }
            return n > 0 ? out.prefix(Int(n)) : nil
        }
    }

    static func renderCGImage(_ data: Data) -> CGImage? {
        guard let png = renderPNG(data),
              let src = CGImageSourceCreateWithData(png as CFData, nil) else { return nil }
        return CGImageSourceCreateImageAtIndex(src, 0, nil)
    }
}
