// Quick Look data-based preview extension: renders a .nap to a PNG and hands it back as image
// content, so the spacebar/Quick Look window shows the picture and scales it with the window (like a
// JPEG/GIF) instead of the generic metadata panel. The QLPreviewReply / QLPreviewProvider data API
// lives in QuickLookUI, and a data-based provider subclasses QLPreviewProvider + QLPreviewingController.
import QuickLookUI
import UniformTypeIdentifiers
import CoreGraphics
import Foundation

final class PreviewProvider: QLPreviewProvider, QLPreviewingController {
    func providePreview(for request: QLFilePreviewRequest,
                        completionHandler handler: @escaping (QLPreviewReply?, Error?) -> Void) {
        do {
            let data = try Data(contentsOf: request.fileURL)
            guard let png = NaplpsRender.renderPNG(data) else {
                handler(nil, NSError(domain: "com.foxcouncil.naplps", code: 1,
                                     userInfo: [NSLocalizedDescriptionKey: "NAPLPS render failed"]))
                return
            }
            let size = CGSize(width: CGFloat(NaplpsRender.deviceWidth),
                              height: CGFloat(NaplpsRender.deviceHeight))
            let reply = QLPreviewReply(dataOfContentType: .png, contentSize: size) { (_: QLPreviewReply) in
                return png
            }
            handler(reply, nil)
        } catch {
            handler(nil, error)
        }
    }
}
