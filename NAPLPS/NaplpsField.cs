// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS;

public struct NaplpsField
{
    public Vector3 Origin { get; set; } = new Vector3(0f, 0f, 0f);

    public Vector3 Dimensions { get; set; } = new Vector3(1f, 1f, 1f);

    public NaplpsField(Vector3 origin, Vector3 dimensions)
    {
        Origin = origin;
        Dimensions = dimensions;
    }

    public override string ToString()
    {
        // string display this class

        return $"{Origin}, {Dimensions}";
    }
}
