// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class NoopCommand : NaplpsCommand
{
    public NoopCommand() : base(null, 0x00, null) { } // Special Command, to deliniate no operation commands in tables
}
