// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS;

/// <summary>
/// Represents a blink animation process that cycles a palette entry between colors.
/// </summary>
public class BlinkProcess
{
    /// <summary>Palette entry being animated</summary>
    public byte PaletteEntry { get; set; }

    /// <summary>Color to blink from (the "on" color)</summary>
    public NaplpsColor BlinkFromColor { get; set; }

    /// <summary>Color to blink to (the "off" color, typically black)</summary>
    public NaplpsColor BlinkToColor { get; set; }

    /// <summary>Duration of "on" phase in units (implementation dependent)</summary>
    public int OnInterval { get; set; }

    /// <summary>Duration of "off" phase in units</summary>
    public int OffInterval { get; set; }

    /// <summary>Initial delay before blinking starts</summary>
    public int StartDelay { get; set; }

    /// <summary>Number of cycles (0 = infinite)</summary>
    public int CycleCount { get; set; }

    /// <summary>Phase: 0 to 7 for gradual transitions</summary>
    public int Phase { get; set; }

    /// <summary>Current state of the blink (true = showing BlinkFromColor)</summary>
    public bool IsOn { get; set; } = true;

    /// <summary>Time elapsed in current phase</summary>
    public int ElapsedTime { get; set; }

    /// <summary>Number of completed blink cycles</summary>
    public int CompletedCycles { get; set; }

    /// <summary>Whether this process has finished all its cycles</summary>
    public bool IsFinished => CycleCount > 0 && CompletedCycles >= CycleCount;

    /// <summary>Base time unit in milliseconds (~60Hz frame, NAPLPS spec dependent)</summary>
    private const int TimeUnit = 16;

    /// <summary>
    /// Advances the blink animation by the given delta time.
    /// Returns true if the blink state changed (requiring a re-render).
    /// </summary>
    public bool Tick(int deltaMs)
    {
        if (IsFinished)
        {
            return false;
        }

        ElapsedTime += deltaMs;

        // Handle start delay
        if (StartDelay > 0)
        {
            int delayMs = StartDelay * TimeUnit;

            if (ElapsedTime < delayMs)
            {
                return false;
            }

            ElapsedTime -= delayMs;
            StartDelay = 0;
        }

        // Calculate interval in ms
        int intervalMs = (IsOn ? OnInterval : OffInterval) * TimeUnit;

        if (intervalMs <= 0)
        {
            intervalMs = TimeUnit;
        }

        if (ElapsedTime >= intervalMs)
        {
            ElapsedTime -= intervalMs;
            IsOn = !IsOn;

            if (!IsOn)
            {
                CompletedCycles++;
            }

            return true;
        }

        // For phase-based gradual transitions (Phase 1-7), we may need intermediate updates
        if (Phase > 0)
        {
            return true; // Always update for gradual transitions
        }

        return false;
    }

    /// <summary>
    /// Gets the current color based on blink state and phase.
    /// Phase 0: hard toggle between from/to colors.
    /// Phase 1-7: gradual interpolation.
    /// </summary>
    public NaplpsColor GetCurrentColor()
    {
        if (Phase == 0)
        {
            return IsOn ? BlinkFromColor : BlinkToColor;
        }

        // Gradual transition: interpolate based on elapsed time and phase
        int intervalMs = (IsOn ? OnInterval : OffInterval) * TimeUnit;

        if (intervalMs <= 0)
        {
            intervalMs = TimeUnit;
        }

        float progress = Math.Clamp((float)ElapsedTime / intervalMs, 0f, 1f);

        // Phase determines the interpolation curve (1=linear, higher=more gradual)
        float t = IsOn ? (1f - progress) : progress;

        // Apply phase as a power curve for gradual transitions
        float phaseFactor = Phase / 7f;
        t = MathF.Pow(t, 1f + phaseFactor);

        return NaplpsColor.Lerp(BlinkFromColor, BlinkToColor, t);
    }
}
