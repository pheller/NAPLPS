// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS;

public class NaplpsCommandReference
{
    const string OperandTypeString = "OperandType";
    const string OperandCountString = "OperandCount";

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

            var aField = CommandType.GetField(OperandTypeString) ?? CommandType.BaseType?.GetField(OperandTypeString);

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

            var aField = CommandType.GetField(OperandCountString) ?? CommandType.BaseType?.GetField(OperandCountString);

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

    public NaplpsCommandReference(Type commandType, params object[] parameters)
    {
        CommandType = commandType;
        Parameters = [.. parameters];
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
