// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Helpers;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class AddCommandAttribute(int height, string name, string description) : Attribute
{
    public int Height { get; } = height;

    public string Name { get; } = name;

    public string Description { get; } = description;
}