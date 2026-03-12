// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS;

public class NaplpsSequence(NaplpsState state, NaplpsCommand command)
{
    public NaplpsState State { get; set; } = state;

    public NaplpsCommand Command { get; set; } = command;

    public void Deconstruct(out NaplpsCommand command, out NaplpsState state)
    {
        command = Command;
        state = State;
    }

    public override string ToString()
    {
        return $"{Command}:|:{State}";
    }
}
