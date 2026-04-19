// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.Diagnostics.CodeAnalysis;

namespace NAPLPS;

public class NaplpsCommandReference
{
    const string OperandTypeString = "OperandType";
    const string OperandCountString = "OperandCount";

    // AOT: preserve the static `OperandType` / `OperandCount` fields + public constructors
    // of every assigned CommandType across the trim pass. The Activator path in
    // NaplpsFormat.AddCommand instantiates this Type with runtime-resolved ctor args; the
    // reflection accessors below read two named static fields. Declaring both member kinds
    // here means any Type flowing in from our static NCR tables (C0Set/C1Set/...) keeps
    // its constructors AND those two fields under `IsTrimmable=true`.
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicConstructors)]
    public Type CommandType { get; }

    public List<object> Parameters { get; private set; }

    public string Name => CommandType.Name.Replace("Command", string.Empty);

    public NaplpsOperandType OperandType
    {
        get
        {
            if (CommandType == null)
            {
                return NaplpsOperandType.None;
            }

            var aField = CommandType.GetField(OperandTypeString) ?? GetBaseTypeField(CommandType, OperandTypeString);

            if (aField == null)
            {
                return NaplpsOperandType.None;
            }

            var value = aField.GetValue(null);

            if (value is NaplpsOperandType operandType)
            {
                return operandType;
            }

            return NaplpsOperandType.None;
        }
    }

    public int OperandCount
    {
        get
        {
            if (CommandType == null)
            {
                return 0;
            }

            var aField = CommandType.GetField(OperandCountString) ?? GetBaseTypeField(CommandType, OperandCountString);

            if (aField == null)
            {
                return 0;
            }

            var value = aField.GetValue(null);

            if (value is int count)
            {
                return count;
            }

            return 0;
        }
    }

    public NaplpsCommandReference(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicConstructors)]
        Type commandType,
        params object[] parameters)
    {
        CommandType = commandType;
        Parameters = [.. parameters];
    }

    /// <summary>
    /// AOT helper: walk up the BaseType chain looking for a public static field. The command
    /// class hierarchy only has a few levels (NaplpsCommand → GeometricDrawingCommandBase →
    /// FillableGeometricDrawingCommandBase → concrete), and both base classes declare
    /// `OperandType` publicly, so this loop terminates within 3 iterations max. The
    /// suppression is safe because those base types are referenced statically from
    /// NaplpsState's NCR tables (C0Set / C1Set / PrimaryCharacterSet / GeneralPDISet /
    /// MosaicSet / SupplementaryCharacterSet) so their public fields are always kept.
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Base types are statically referenced by NCR tables; public fields survive trimming.")]
    private static System.Reflection.FieldInfo? GetBaseTypeField(Type t, string name)
    {
        var baseType = t.BaseType;
        while (baseType != null)
        {
            var field = baseType.GetField(name);
            if (field != null) { return field; }
            baseType = baseType.BaseType;
        }
        return null;
    }

    public override string ToString()
    {
        if (CommandType == typeof(ControlCommand))
        {
            return $"REF: {CommandType} | {(NaplpsControlCommands)Parameters[0]}";
        }

        return $"REF: {CommandType}";
    }
}
