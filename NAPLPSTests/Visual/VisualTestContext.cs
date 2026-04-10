// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
using NAPLPS.Drawing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace NAPLPSTests.Visual;

public record VisualTestResult(
    string RelativePath,
    string? ActualPath,
    int FrameCount,
    string? ErrorMessage
);

public static class VisualTestContext
{
    public const int CanvasWidth = 1024;
    public const int CanvasHeight = 768;

    private static readonly string[] SkipExtensions = [".jpg", ".png", ".txt", ".exe"];

    public static readonly ConcurrentDictionary<string, VisualTestResult> Results = new();

    public static string SourceDir { get; } = ResolveSourceDir();

    public static string OutputDir { get; set; } = Path.Combine(AppContext.BaseDirectory, "VisualRegression");

    public static string ActualsDir => Path.Combine(OutputDir, "Actuals");

    public static string ViewersDir => Path.Combine(OutputDir, "Viewers");

    public static string ReportPath => Path.Combine(OutputDir, "VisualRegressionReport.html");

    public static string ExamplesDir => Path.Combine(AppContext.BaseDirectory, "examples");

    public static IEnumerable<string> DiscoverExampleFiles()
    {
        return Directory.GetFiles(ExamplesDir, "*", SearchOption.AllDirectories)
            .Where(f => !SkipExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .Select(f => Path.GetRelativePath(ExamplesDir, f))
            .OrderBy(f => f);
    }

    public static string GetActualPath(string relativePath)
    {
        return Path.Combine(ActualsDir, relativePath + ".apng");
    }

    public static string GetViewerHtmlPath(string relativePath)
    {
        return Path.Combine(ViewersDir, relativePath + ".html");
    }

    public static void EnsureDirectories()
    {
        Directory.CreateDirectory(ActualsDir);
        Directory.CreateDirectory(ViewersDir);
    }

    public static void CleanOutputDirs()
    {
        if (Directory.Exists(ActualsDir))
        {
            Directory.Delete(ActualsDir, true);
        }

        if (Directory.Exists(ViewersDir))
        {
            Directory.Delete(ViewersDir, true);
        }

        EnsureDirectories();
    }

    public static Image<Rgba32> RenderApng(string exampleFilePath)
    {
        var naplps = NaplpsFormat.FromFile(exampleFilePath);

        using var drawContext = new DrawContext(naplps, new SixLabors.ImageSharp.Size(CanvasWidth, CanvasHeight));

        return drawContext.RenderToApng();
    }

    public static void GenerateViewerHtml(string relativePath, string actualPath, int frameCount, string outputPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        var actualFrames = ExtractFramesAsBase64(actualPath);

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html><head><meta charset='utf-8'>");
        sb.AppendLine($"<title>{HtmlEncode(relativePath)}</title>");
        sb.AppendLine("<style>");
        sb.AppendLine(ViewerPageCss());
        sb.AppendLine("</style></head><body>");

        var reportRelative = Path.GetRelativePath(Path.GetDirectoryName(outputPath)!, ReportPath).Replace('\\', '/');
        sb.AppendLine($"<div class='breadcrumb'><a href='{HtmlEncode(reportRelative)}'>&larr; Back to Report</a></div>");
        sb.AppendLine($"<h1>{HtmlEncode(relativePath)}</h1>");
        sb.AppendLine($"<div class='summary'>{frameCount} frame{(frameCount != 1 ? "s" : "")}</div>");

        sb.AppendLine("<script>");
        sb.AppendLine($"const frames = {JsonArrayFromList(actualFrames)};");
        sb.AppendLine("let currentFrame = 0;");
        sb.AppendLine(ViewerPageJs());
        sb.AppendLine("</script>");

        sb.AppendLine("<div class='frame-nav'>");
        sb.AppendLine("<button class='nav-btn' onclick='firstFrame()' title='Home'>|&larr;</button>");
        sb.AppendLine("<button class='nav-btn' onclick='prevFrame()' title='Left Arrow'>&larr; Prev</button>");
        sb.AppendLine("<span id='frameCounter'>Frame 1 / 1</span>");
        sb.AppendLine("<button class='nav-btn' onclick='nextFrame()' title='Right Arrow'>Next &rarr;</button>");
        sb.AppendLine("<button class='nav-btn' onclick='lastFrame()' title='End'>&rarr;|</button>");
        sb.AppendLine("</div>");

        sb.AppendLine("<div id='viewer' class='viewer'></div>");
        sb.AppendLine("<script>updateView();</script>");
        sb.AppendLine("</body></html>");

        System.IO.File.WriteAllText(outputPath, sb.ToString());
    }

    public static void GenerateReport(ConcurrentDictionary<string, VisualTestResult> results)
    {
        Directory.CreateDirectory(OutputDir);

        var sorted = results.Values.OrderBy(r => r.ErrorMessage != null ? 0 : 1).ThenBy(r => r.RelativePath).ToList();
        var rendered = sorted.Count(r => r.ErrorMessage == null);
        var errors = sorted.Count(r => r.ErrorMessage != null);
        var runId = DateTime.Now.ToString("yyyyMMddHHmmss");

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html><head><meta charset='utf-8'>");
        sb.AppendLine("<title>Visual Regression Report</title>");
        sb.AppendLine("<style>");
        sb.AppendLine(ReportCss());
        sb.AppendLine("</style></head><body>");

        sb.AppendLine("<h1>Visual Regression Report</h1>");
        sb.AppendLine($"<div class='timestamp'>Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</div>");

        sb.AppendLine("<div class='stats'>");
        sb.AppendLine($"<span class='stat rendered'>{rendered} Rendered</span>");
        if (errors > 0)
        {
            sb.AppendLine($"<span class='stat error'>{errors} Errors</span>");
        }
        sb.AppendLine($"<span class='stat total'>{sorted.Count} Total</span>");
        sb.AppendLine("</div>");

        sb.AppendLine("<div class='filters'>");
        sb.AppendLine("<button class='filter active' onclick='filterBy(\"all\")'>All</button>");
        if (errors > 0)
        {
            sb.AppendLine("<button class='filter' onclick='filterBy(\"error\")'>Errors</button>");
        }
        sb.AppendLine("<button class='filter' onclick='filterBy(\"reviewed\")'>Reviewed</button>");
        sb.AppendLine("<button class='filter' onclick='filterBy(\"unreviewed\")'>Unreviewed</button>");
        sb.AppendLine("</div>");

        sb.AppendLine("<table id='results'>");
        sb.AppendLine("<thead><tr><th>File</th><th>Frames</th><th>View</th><th>Reviewed</th></tr></thead>");
        sb.AppendLine("<tbody>");

        foreach (var result in sorted)
        {
            var fileKey = HtmlEncode(result.RelativePath).Replace("\\", "/");
            var isError = result.ErrorMessage != null;
            var statusClass = isError ? "error" : "rendered";

            sb.AppendLine($"<tr class='row {statusClass}' data-status='{statusClass}' data-file='{fileKey}'>");
            sb.AppendLine($"<td>{(isError ? "&#9888; " : "")}{HtmlEncode(result.RelativePath)}</td>");
            sb.AppendLine($"<td>{result.FrameCount}</td>");

            if (!isError)
            {
                var viewerPath = GetViewerHtmlPath(result.RelativePath);
                var viewerRelative = Path.GetRelativePath(OutputDir, viewerPath);
                sb.AppendLine($"<td><a href='{HtmlEncode(viewerRelative.Replace('\\', '/'))}'>View</a></td>");
            }
            else
            {
                sb.AppendLine($"<td class='error-msg'>{HtmlEncode(result.ErrorMessage ?? "")}</td>");
            }

            sb.AppendLine($"<td class='review-cell' data-file='{fileKey}'></td>");
            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</tbody></table>");

        sb.AppendLine("<script>");
        sb.AppendLine($"const RUN_ID = '{runId}';");
        sb.AppendLine(ReportJs());
        sb.AppendLine("</script>");
        sb.AppendLine("</body></html>");

        System.IO.File.WriteAllText(ReportPath, sb.ToString());
    }

    private static string ResolveSourceDir([CallerFilePath] string? callerPath = null)
    {
        return Path.GetDirectoryName(Path.GetDirectoryName(callerPath!))!;
    }

    private static List<string> ExtractFramesAsBase64(string apngPath)
    {
        var frames = new List<string>();

        using var image = Image.Load<Rgba32>(apngPath);

        for (int i = 0; i < image.Frames.Count; i++)
        {
            using var singleFrame = new Image<Rgba32>(image.Width, image.Height);

            singleFrame.Frames.RootFrame.ProcessPixelRows(image.Frames[i], (dst, src) =>
            {
                for (int y = 0; y < dst.Height; y++)
                {
                    src.GetRowSpan(y).CopyTo(dst.GetRowSpan(y));
                }
            });

            frames.Add(ImageToBase64(singleFrame));
        }

        return frames;
    }

    private static string ImageToBase64(Image<Rgba32> image)
    {
        using var ms = new MemoryStream();
        image.SaveAsPng(ms);
        return Convert.ToBase64String(ms.ToArray());
    }

    private static string HtmlEncode(string text)
    {
        return System.Net.WebUtility.HtmlEncode(text);
    }

    private static string JsonArrayFromList(List<string> items)
    {
        var sb = new StringBuilder("[");

        for (int i = 0; i < items.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(',');
            }

            sb.Append('"');
            sb.Append(items[i]);
            sb.Append('"');
        }

        sb.Append(']');
        return sb.ToString();
    }

    private static string ReportCss() => """
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; padding: 24px; background: #0d1117; color: #c9d1d9; }
        h1 { margin-bottom: 8px; color: #f0f6fc; }
        .timestamp { color: #8b949e; margin-bottom: 16px; }
        .stats { display: flex; gap: 12px; margin-bottom: 16px; flex-wrap: wrap; }
        .stat { padding: 6px 14px; border-radius: 6px; font-weight: 600; font-size: 14px; }
        .stat.rendered { background: #0d2818; color: #3fb950; }
        .stat.error { background: #3d1417; color: #f85149; }
        .stat.total { background: #161b22; color: #8b949e; }
        .filters { margin-bottom: 16px; display: flex; gap: 8px; }
        .filter { padding: 6px 12px; border: 1px solid #30363d; background: #161b22; color: #c9d1d9; border-radius: 6px; cursor: pointer; }
        .filter.active { background: #1f6feb; border-color: #1f6feb; color: #fff; }
        table { width: 100%; border-collapse: collapse; background: #161b22; border-radius: 8px; overflow: hidden; }
        th { text-align: left; padding: 10px 14px; background: #21262d; color: #8b949e; font-size: 13px; text-transform: uppercase; }
        td { padding: 8px 14px; border-top: 1px solid #21262d; font-size: 14px; }
        .error-msg { color: #f85149; font-size: 12px; }
        tr:hover { background: #1c2128; }
        a { color: #58a6ff; text-decoration: none; }
        a:hover { text-decoration: underline; }
        .row.hidden { display: none; }
        .review-btn { padding: 2px 8px; border: 1px solid #30363d; background: #161b22; color: #8b949e; border-radius: 4px; cursor: pointer; font-size: 12px; }
        .review-btn:hover { border-color: #58a6ff; }
        .review-btn.reviewed { background: #0d2818; color: #3fb950; border-color: #238636; }
        .review-time { color: #8b949e; font-size: 11px; display: block; margin-top: 2px; }
        """;

    private static string ReportJs() => """
        const STORAGE_KEY = 'vr_reviewed_' + RUN_ID;

        // Clean up reviewed state from previous runs
        for (let i = localStorage.length - 1; i >= 0; i--) {
            const key = localStorage.key(i);
            if (key && key.startsWith('vr_reviewed_') && key !== STORAGE_KEY) {
                localStorage.removeItem(key);
            }
        }

        function getReviewed() {
            try { return JSON.parse(localStorage.getItem(STORAGE_KEY) || '{}'); } catch { return {}; }
        }

        function setReviewed(file, timestamp) {
            const data = getReviewed();
            data[file] = timestamp;
            localStorage.setItem(STORAGE_KEY, JSON.stringify(data));
        }

        function removeReviewed(file) {
            const data = getReviewed();
            delete data[file];
            localStorage.setItem(STORAGE_KEY, JSON.stringify(data));
        }

        function toggleReview(file) {
            const reviewed = getReviewed();
            if (reviewed[file]) {
                removeReviewed(file);
            } else {
                setReviewed(file, new Date().toISOString());
            }
            renderReviewButtons();
        }

        function renderReviewButtons() {
            const reviewed = getReviewed();
            document.querySelectorAll('.review-cell').forEach(cell => {
                const file = cell.dataset.file;
                const ts = reviewed[file];
                if (ts) {
                    const date = new Date(ts);
                    const timeStr = date.toLocaleString(undefined, { month:'short', day:'numeric', hour:'2-digit', minute:'2-digit' });
                    cell.innerHTML = `<button class='review-btn reviewed' onclick='toggleReview("${file}")'>&#10004; Reviewed</button><span class='review-time'>${timeStr}</span>`;
                    cell.closest('tr').dataset.reviewed = 'true';
                } else {
                    cell.innerHTML = `<button class='review-btn' onclick='toggleReview("${file}")'>Mark Reviewed</button>`;
                    cell.closest('tr').dataset.reviewed = 'false';
                }
            });
        }

        function filterBy(status) {
            document.querySelectorAll('.filter').forEach(b => b.classList.remove('active'));
            event.target.classList.add('active');
            document.querySelectorAll('.row').forEach(row => {
                let show = false;
                if (status === 'all') show = true;
                else if (status === 'reviewed') show = row.dataset.reviewed === 'true';
                else if (status === 'unreviewed') show = row.dataset.reviewed !== 'true';
                else show = row.dataset.status === status;

                row.classList.toggle('hidden', !show);
            });
        }

        renderReviewButtons();
        """;

    private static string ViewerPageCss() => """
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; padding: 24px; background: #0d1117; color: #c9d1d9; }
        h1 { margin-bottom: 8px; color: #f0f6fc; font-size: 20px; }
        .summary { color: #8b949e; margin-bottom: 16px; font-size: 14px; }
        .breadcrumb { margin-bottom: 12px; }
        .breadcrumb a { color: #58a6ff; text-decoration: none; font-size: 14px; }
        .breadcrumb a:hover { text-decoration: underline; }
        .frame-nav { display: flex; align-items: center; gap: 8px; margin-bottom: 16px; flex-wrap: wrap; }
        .nav-btn { padding: 6px 12px; border: 1px solid #30363d; background: #161b22; color: #c9d1d9; border-radius: 6px; cursor: pointer; font-size: 13px; }
        .nav-btn:hover { border-color: #58a6ff; }
        .nav-btn:disabled { opacity: 0.4; cursor: default; border-color: #30363d; }
        #frameCounter { color: #8b949e; font-size: 14px; min-width: 120px; text-align: center; }
        .viewer { display: flex; flex-direction: column; gap: 16px; }
        .viewer img { max-width: 100%; border: 1px solid #30363d; border-radius: 4px; image-rendering: pixelated; }
        """;

    private static string ViewerPageJs() => """
        function firstFrame() { currentFrame = 0; updateView(); }
        function lastFrame() { currentFrame = frames.length - 1; updateView(); }
        function prevFrame() { if (currentFrame > 0) { currentFrame--; updateView(); } }
        function nextFrame() { if (currentFrame < frames.length - 1) { currentFrame++; updateView(); } }

        function updateView() {
            const fc = document.getElementById('frameCounter');
            fc.textContent = `Frame ${currentFrame + 1} / ${frames.length}`;

            const btns = document.querySelectorAll('.nav-btn');
            btns[0].disabled = currentFrame <= 0;
            btns[1].disabled = currentFrame <= 0;
            btns[2].disabled = currentFrame >= frames.length - 1;
            btns[3].disabled = currentFrame >= frames.length - 1;

            const viewer = document.getElementById('viewer');
            const src = `data:image/png;base64,${frames[currentFrame]}`;
            viewer.innerHTML = `<img src='${src}'>`;
        }

        document.addEventListener('keydown', function(e) {
            switch (e.key) {
                case 'ArrowLeft':
                    e.preventDefault();
                    prevFrame();
                    break;
                case 'ArrowRight':
                    e.preventDefault();
                    nextFrame();
                    break;
                case 'Home':
                    e.preventDefault();
                    firstFrame();
                    break;
                case 'End':
                    e.preventDefault();
                    lastFrame();
                    break;
            }
        });
        """;
}
