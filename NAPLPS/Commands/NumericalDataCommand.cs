namespace NAPLPS.Commands;

public class NumericalDataCommand : NaplpsCommand
{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
    public NumericalDataCommand() : base(null, 0x00, null) { } // this is just a special class to represent operands
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
}
