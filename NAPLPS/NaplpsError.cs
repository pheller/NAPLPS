// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS;

public enum NaplpsErrorSeverity
{
    Warning,
    Error,
}

public enum NaplpsErrorType
{
    UnknownOpcode,
    CommandInstantiationFailed,
    UnexpectedEndOfStream,
    InvalidCommand,
}

public class NaplpsError(NaplpsErrorSeverity severity, NaplpsErrorType type, string message, byte? opcode = null, long? streamPosition = null)
{
    public NaplpsErrorSeverity Severity { get; } = severity;

    public NaplpsErrorType Type { get; } = type;

    public string Message { get; } = message;

    public byte? Opcode { get; } = opcode;

    public long? StreamPosition { get; } = streamPosition;

    public override string ToString()
    {
        var result = $"[{Severity}:{Type}] {Message}";

        if (Opcode.HasValue)
        {
            result += $" (opcode: 0x{Opcode.Value:X2})";
        }

        if (StreamPosition.HasValue)
        {
            result += $" (position: {StreamPosition.Value})";
        }

        return result;
    }
}
