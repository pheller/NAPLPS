// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Helpers;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class AddCommandAttribute(int height, string name, string description) : Attribute
{
    public int Height { get; } = height;

    public string Name { get; } = name;

    public string Description { get; } = description;

    public CommandCategory Category { get; set; } = CommandCategory.Unknown;

    public string DslKeyword { get; set; } = string.Empty;
}