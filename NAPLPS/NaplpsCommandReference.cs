// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS;

public class NaplpsCommandReference
{
    const string OperandTypeString = "OperandType";
    const string OperandCountString = "OperandCount";

    public Type CommandType { get; }

    public List<object> Parameters { get; private set; }

    public NaplpsOperandType OperandType
    {
        get
        {
            if (CommandType == null)
            {
                return NaplpsOperandType.None;
            }

            var field = CommandType.GetField(OperandTypeString) ?? CommandType.BaseType?.GetField(OperandTypeString);

            if (field == null)
            {
                return NaplpsOperandType.None;
            }

            var value = field.GetValue(null);

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

            var field = CommandType.GetField(OperandCountString) ?? CommandType.BaseType?.GetField(OperandCountString);

            if (field == null)
            {
                return 0;
            }

            var value = field.GetValue(null);

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
