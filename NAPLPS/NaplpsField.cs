// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS;

public struct NaplpsField(Vector3 origin, Vector3 dimensions)
{
    public Vector3 Origin { get; set; } = origin;

    public Vector3 Dimensions { get; set; } = dimensions;
}
