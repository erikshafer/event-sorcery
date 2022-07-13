namespace EventSorcery.Types;

public class UnregisteredTypeException : Exception
{
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public UnregisteredTypeException(Type type)
        : base($"Type {type.Name} is not registered in the type map")
    {
    }

    public UnregisteredTypeException(string type)
        : base($"Type name {type} is not registered in the type map")
    {
    }
}