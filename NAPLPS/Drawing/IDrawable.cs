// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Drawing;

/// <summary>Used on every drawable shape or command.</summary>
public interface IDrawable
{
    /// <summary></summary>
    /// <param name="image"></param>
    /// <param name="state"></param>
    /// <param name="size"></param>
    void Draw(Image<Rgba32> image, NaplpsState state, Size size);
}
