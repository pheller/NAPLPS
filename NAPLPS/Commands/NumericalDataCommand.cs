// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

[AddCommand(80, "Numerical Data", "Raw numeric operand byte (filler in the GeneralPDISet 0xC0-0xFF range).", Category = CommandCategory.Data, DslKeyword = "numdata")]
public class NumericalDataCommand : NaplpsCommand
{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
    public NumericalDataCommand() : base(null, 0x00, null) { } // this is just a special class to represent operands
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
}
