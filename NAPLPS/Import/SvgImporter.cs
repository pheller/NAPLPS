// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;

namespace NAPLPS.Import;

/// <summary>
/// Minimal SVG → Telidraw converter. Parses `<svg viewBox="x y w h">` to establish the
/// source coordinate space, then walks `<path d="...">` elements and converts a subset
/// of path commands (M, L, H, V, Z) into Telidraw `move` / `line` statements. Quadratic
/// and cubic bezier curves are approximated with straight-line segments.
///
/// Out of scope for this minimal port: transforms, stroke styles, gradients, text, raster
/// images, most bezier fidelity. The goal is a usable first-cut for simple SVG exports
/// (logos, line drawings) — not a full SVG renderer.
/// </summary>
public static class SvgImporter
{
    /// <summary>
    /// Convert SVG XML to a Telidraw `.td` source string. The SVG's viewBox is mapped to
    /// NAPLPS normalized coords (X in [0,1], Y in [0,0.75]). Y is flipped (SVG y-down →
    /// NAPLPS y-up).
    /// </summary>
    public static string ToTelidraw(string svgXml)
    {
        var doc = new XmlDocument();
        doc.LoadXml(svgXml);

        var root = doc.DocumentElement ?? throw new System.InvalidOperationException("Missing <svg> root");

        // Establish source coord space. Prefer viewBox; fall back to width/height.
        float vbX = 0, vbY = 0, vbW = 100, vbH = 100;
        var vb = root.GetAttribute("viewBox");
        if (!string.IsNullOrWhiteSpace(vb))
        {
            var parts = vb.Split([' ', ','], System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 4)
            {
                vbX = ParseFloat(parts[0]);
                vbY = ParseFloat(parts[1]);
                vbW = ParseFloat(parts[2]);
                vbH = ParseFloat(parts[3]);
            }
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("// Imported from SVG");
        sb.AppendLine("#coord fractions");
        sb.AppendLine();
        sb.AppendLine("color 7");

        // Walk every <path> element and convert its d="" attribute.
        var paths = root.GetElementsByTagName("path");
        foreach (XmlNode node in paths)
        {
            if (node is not XmlElement el) { continue; }
            var d = el.GetAttribute("d");
            if (string.IsNullOrWhiteSpace(d)) { continue; }

            ConvertPath(sb, d, vbX, vbY, vbW, vbH);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static float ParseFloat(string s) =>
        float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : 0;

    /// <summary>
    /// Convert a single path d="..." into Telidraw statements. Supports M/m (moveto),
    /// L/l (lineto), H/h (horizontal lineto), V/v (vertical lineto), Z/z (closepath).
    /// Bezier commands (C/c, Q/q, S/s, T/t, A/a) fall back to a straight line to their
    /// end-point — visually approximate, good enough for first import.
    /// </summary>
    private static void ConvertPath(System.Text.StringBuilder sb, string d, float vbX, float vbY, float vbW, float vbH)
    {
        // Tokenize: split on letters (commands) and numbers (params).
        var tokens = Regex.Matches(d, @"[a-zA-Z]|-?\d*\.?\d+(?:[eE][-+]?\d+)?");
        float penX = 0, penY = 0, startX = 0, startY = 0;
        char cmd = 'M';
        int i = 0;

        while (i < tokens.Count)
        {
            var tok = tokens[i].Value;
            if (char.IsLetter(tok[0]))
            {
                cmd = tok[0];
                i++;
                continue;
            }

            switch (cmd)
            {
                case 'M':
                case 'm':
                {
                    float x = ParseFloat(tokens[i].Value);
                    float y = ParseFloat(tokens[i + 1].Value);
                    if (cmd == 'm') { x += penX; y += penY; }
                    sb.AppendLine($"move {MapX(x, vbX, vbW)} {MapY(y, vbY, vbH)}");
                    penX = startX = x;
                    penY = startY = y;
                    i += 2;
                    cmd = cmd == 'M' ? 'L' : 'l';  // subsequent coord pairs after M are implicit L
                    break;
                }
                case 'L':
                case 'l':
                {
                    float x = ParseFloat(tokens[i].Value);
                    float y = ParseFloat(tokens[i + 1].Value);
                    if (cmd == 'l') { x += penX; y += penY; }
                    sb.AppendLine($"line {MapX(x, vbX, vbW)} {MapY(y, vbY, vbH)}");
                    penX = x; penY = y;
                    i += 2;
                    break;
                }
                case 'H':
                case 'h':
                {
                    float x = ParseFloat(tokens[i].Value);
                    if (cmd == 'h') { x += penX; }
                    sb.AppendLine($"line {MapX(x, vbX, vbW)} {MapY(penY, vbY, vbH)}");
                    penX = x;
                    i += 1;
                    break;
                }
                case 'V':
                case 'v':
                {
                    float y = ParseFloat(tokens[i].Value);
                    if (cmd == 'v') { y += penY; }
                    sb.AppendLine($"line {MapX(penX, vbX, vbW)} {MapY(y, vbY, vbH)}");
                    penY = y;
                    i += 1;
                    break;
                }
                case 'Z':
                case 'z':
                {
                    sb.AppendLine($"line {MapX(startX, vbX, vbW)} {MapY(startY, vbY, vbH)}");
                    penX = startX; penY = startY;
                    break;
                }
                case 'C': case 'c': case 'S': case 's':
                {
                    // Cubic bezier — skip control points, draw straight line to endpoint.
                    int stride = cmd is 'C' or 'c' ? 6 : 4;
                    if (i + stride - 1 >= tokens.Count) { return; }
                    float x = ParseFloat(tokens[i + stride - 2].Value);
                    float y = ParseFloat(tokens[i + stride - 1].Value);
                    if (char.IsLower(cmd)) { x += penX; y += penY; }
                    sb.AppendLine($"line {MapX(x, vbX, vbW)} {MapY(y, vbY, vbH)}   // cubic-bezier approximated");
                    penX = x; penY = y;
                    i += stride;
                    break;
                }
                case 'Q': case 'q': case 'T': case 't':
                {
                    int stride = cmd is 'Q' or 'q' ? 4 : 2;
                    if (i + stride - 1 >= tokens.Count) { return; }
                    float x = ParseFloat(tokens[i + stride - 2].Value);
                    float y = ParseFloat(tokens[i + stride - 1].Value);
                    if (char.IsLower(cmd)) { x += penX; y += penY; }
                    sb.AppendLine($"line {MapX(x, vbX, vbW)} {MapY(y, vbY, vbH)}   // quad-bezier approximated");
                    penX = x; penY = y;
                    i += stride;
                    break;
                }
                case 'A': case 'a':
                {
                    // Arc — skip rx, ry, rotation, large-arc, sweep flags; draw line to endpoint.
                    // NAPLPS arcs take mid+end not SVG's rx/ry+flags, so conversion is non-trivial.
                    if (i + 6 >= tokens.Count) { return; }
                    float x = ParseFloat(tokens[i + 5].Value);
                    float y = ParseFloat(tokens[i + 6].Value);
                    if (cmd == 'a') { x += penX; y += penY; }
                    sb.AppendLine($"line {MapX(x, vbX, vbW)} {MapY(y, vbY, vbH)}   // svg-arc approximated");
                    penX = x; penY = y;
                    i += 7;
                    break;
                }
                default:
                    // Unknown command — advance past it.
                    i++;
                    break;
            }
        }
    }

    private static string MapX(float x, float vbX, float vbW) =>
        ((x - vbX) / vbW).ToString("0.####", CultureInfo.InvariantCulture);

    /// <summary>Map SVG y (y-down) to NAPLPS y (y-up within 0..0.75).</summary>
    private static string MapY(float y, float vbY, float vbH) =>
        (0.75f - (y - vbY) / vbH * 0.75f).ToString("0.####", CultureInfo.InvariantCulture);
}
