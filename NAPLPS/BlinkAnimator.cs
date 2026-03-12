// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS;

/// <summary>
/// Manages all blink processes for a NAPLPS state, updating the live palette
/// on each tick to create palette animation effects.
/// </summary>
public class BlinkAnimator
{
    private readonly List<BlinkProcess> _processes;
    private readonly Dictionary<byte, NaplpsColor> _livePalette;
    private readonly Dictionary<byte, NaplpsColor> _originalColors;

    public BlinkAnimator(List<BlinkProcess> processes, Dictionary<byte, NaplpsColor> livePalette)
    {
        _processes = processes;
        _livePalette = livePalette;

        // Snapshot original palette colors for reset
        _originalColors = new Dictionary<byte, NaplpsColor>();
        foreach (var process in _processes)
        {
            if (_livePalette.ContainsKey(process.PaletteEntry) && !_originalColors.ContainsKey(process.PaletteEntry))
            {
                _originalColors[process.PaletteEntry] = _livePalette[process.PaletteEntry];
            }
        }
    }

    /// <summary>
    /// Whether there are any active (non-finished) blink processes.
    /// </summary>
    public bool HasActiveProcesses => _processes.Any(p => !p.IsFinished);

    /// <summary>
    /// Advances all blink processes by the given delta time.
    /// Updates the live palette entries with current colors.
    /// Returns true if any process changed state (requiring re-render).
    /// </summary>
    public bool Tick(int deltaMs)
    {
        bool anyChanged = false;

        foreach (var process in _processes)
        {
            if (process.IsFinished)
            {
                continue;
            }

            bool changed = process.Tick(deltaMs);

            if (changed)
            {
                // Update the live palette entry with the current blink color
                _livePalette[process.PaletteEntry] = process.GetCurrentColor();
                anyChanged = true;
            }
        }

        return anyChanged;
    }

    /// <summary>
    /// Restores all palette entries to their original colors.
    /// </summary>
    public void Reset()
    {
        foreach (var kvp in _originalColors)
        {
            _livePalette[kvp.Key] = kvp.Value;
        }

        foreach (var process in _processes)
        {
            process.IsOn = true;
            process.ElapsedTime = 0;
            process.CompletedCycles = 0;
        }
    }
}
