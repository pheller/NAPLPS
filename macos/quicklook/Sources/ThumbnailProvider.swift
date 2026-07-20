// Quick Look thumbnail extension: renders a .nap file to its authentic Prodigy image and draws it
// aspect-fit into the requested thumbnail. macOS uses this for Finder thumbnails and, when there is
// no dedicated preview extension, for the spacebar Quick Look panel too.
import QuickLookThumbnailing
import CoreGraphics

final class ThumbnailProvider: QLThumbnailProvider {
    override func provideThumbnail(for request: QLFileThumbnailRequest,
                                   _ handler: @escaping (QLThumbnailReply?, Error?) -> Void) {
        guard let data = try? Data(contentsOf: request.fileURL),
              let image = NaplpsRender.renderCGImage(data) else {
            handler(nil, nil)
            return
        }

        let source = CGSize(width: CGFloat(NaplpsRender.deviceWidth),
                            height: CGFloat(NaplpsRender.deviceHeight))
        let maxSize = request.maximumSize
        let fit = min(maxSize.width / source.width, maxSize.height / source.height)
        let size = CGSize(width: max(1, source.width * fit),
                          height: max(1, source.height * fit))

        // The drawing block's CGContext is backed at contextSize * request.scale device pixels with a
        // pixel-space CTM (contextSize is in points). Drawing at point size would fill only the lower
        // 1/scale corner (the classic Retina "thumbnail in the bottom-left quadrant" bug), so scale the
        // draw rect by request.scale to cover the whole backing.
        let pixels = CGSize(width: size.width * request.scale,
                            height: size.height * request.scale)

        let reply = QLThumbnailReply(contextSize: size) { (ctx: CGContext) -> Bool in
            ctx.interpolationQuality = .none            // preserve the hard-edged NAPLPS pixels
            ctx.draw(image, in: CGRect(origin: .zero, size: pixels))
            return true
        }
        handler(reply, nil)
    }
}
