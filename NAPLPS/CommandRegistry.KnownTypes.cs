// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using NAPLPS.Commands;

namespace NAPLPS;

/// <summary>
/// AOT-safe static registry of every <see cref="NaplpsCommand"/> subclass decorated with
/// <c>[AddCommand]</c>. The dynamic version in <see cref="CommandRegistry.Build"/> used
/// <c>Assembly.GetTypes()</c> which is trim-unsafe — under NativeAOT the trimmer can't
/// prove all command subclasses are live (they have no static references) and would strip
/// them. Listing them here as <c>typeof(...)</c> literals provides those static refs so the
/// whole command family survives trimming.
///
/// Keeping this file in sync is the cost of AOT compatibility. Add any new <c>[AddCommand]</c>
/// subclass here; the build will succeed without it, but the registry won't include the
/// command and the parser will fall back to the generic <see cref="NaplpsCommand"/>.
/// </summary>
internal static class CommandRegistryKnownTypes
{
    internal static readonly Type[] All =
    [
        typeof(ArcFilledCommand),
        typeof(ArcOutlinedCommand),
        typeof(ArcSetFilledCommand),
        typeof(ArcSetOutlinedCommand),
        typeof(AsciiCharCommand),
        typeof(BlinkCommand),
        typeof(ControlCommand),
        typeof(DefTextureCommand),
        typeof(DeleteCommand),
        typeof(DomainCommand),
        typeof(IncrementalFieldCommand),
        typeof(IncrementalLineCommand),
        typeof(IncrementalPointCommand),
        typeof(IncrementalPolygonFilledCommand),
        typeof(LineAbsoluteCommand),
        typeof(LineRelativeCommand),
        typeof(LineSetAbsoluteCommand),
        typeof(LineSetRelativeCommand),
        typeof(MosaicElementCommand),
        typeof(NumericalDataCommand),
        typeof(PointAbsoluteCommand),
        typeof(PointRelativeCommand),
        typeof(PointSetAbsoluteCommand),
        typeof(PointSetRelativeCommand),
        typeof(PolygonFilledCommand),
        typeof(PolygonOutlinedCommand),
        typeof(PolygonSetFilledCommand),
        typeof(PolygonSetOutlinedCommand),
        typeof(RectangleFilledCommand),
        typeof(RectangleOutlinedCommand),
        typeof(RectangleSetFilledCommand),
        typeof(RectangleSetOutlinedCommand),
        typeof(ResetCommand),
        typeof(SelectColorCommand),
        typeof(SetColorCommand),
        typeof(TextCommand),
        typeof(TextureCommand),
        typeof(WaitCommand),
    ];
}
