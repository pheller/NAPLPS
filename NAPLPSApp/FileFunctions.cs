// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using NAPLPS;
using NAPLPS.Commands;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using Color = SixLabors.ImageSharp.Color;
using PointF = SixLabors.ImageSharp.PointF;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using Pens = SixLabors.ImageSharp.Drawing.Processing.Pens;
using Brushes = SixLabors.ImageSharp.Drawing.Processing.Brushes;
using System.Diagnostics;
using NAPLPSApp.Drawing;

namespace NAPLPSApp;

/// <summary>Various functions to perform on NaplpsFormat objects</summary>
public static class FileFunctions
{
    /// <summary></summary>
    /// <param name="naplpsFile"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool Convert(NaplpsFormat naplpsFile, string name)
    {
        if (!naplpsFile.IsValid)
        {
            return false;
        }

        using var drawCtx = new DrawContext(naplpsFile, new System.Drawing.Size(1024, 768));

        drawCtx.SaveAsPng(name);

        return true;
    }
}
