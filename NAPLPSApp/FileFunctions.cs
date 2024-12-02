// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using NAPLPS;
using NAPLPS.Drawing;

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

        using var drawCtx = new DrawContext(naplpsFile, new SixLabors.ImageSharp.Size(1024, 768));

        drawCtx.SaveAsPng(name);

        return true;
    }
}
