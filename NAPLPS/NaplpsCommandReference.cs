namespace NAPLPS;

public class NaplpsCommandReference
{
    public Type CommandType { get; }

    public object[] Parameters { get; }

    public NaplpsCommandReference(Type commandType, params object[] parameters)
    {
        CommandType = commandType;
        Parameters = parameters;
    }
}
