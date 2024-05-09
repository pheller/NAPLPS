// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSApp;

public static class Extensions
{

#if NET8_0_WINDOWS
    // Winforms Extensions
    public static string SizeString(this Size size) => $"{size.Width}x{size.Height}";

    public static Size StringSize(this string stringSize)
    {
        var sizeParsed = stringSize.Split("x");

        return new Size(int.Parse(sizeParsed[0]), int.Parse(sizeParsed[1]));
    }
#endif

}
