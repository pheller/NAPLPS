// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

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
}
