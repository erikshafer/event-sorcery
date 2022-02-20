using System.Reflection;

namespace EventSorcery.Types;

public static class TypeMap
{
    public static readonly TypeMapper Instance = new();

    public static string GetTypeName<T>() => Instance.GetTypeName<T>();

    public static string GetTypeName(object o) => Instance.GetTypeName(o);

    public static string GetTypeNameByType(Type type) => Instance.GetTypeNameByType(type);

    public static Type GetType(string typeName) => Instance.GetType(typeName);

    public static bool TryGetType(string typeName, out Type? type) => Instance.TryGetType(typeName, out type);

    public static void AddType<T>(string name) => Instance.AddType<T>(name);

    static void AddType(Type type, string name) => Instance.AddType(type, name);

    public static bool IsTypeRegistered<T>() => Instance.IsTypeRegistered<T>();
}

public class TypeMapper
{
    private readonly Dictionary<string, Type> _reverseMap = new();
    private readonly Dictionary<Type, string> _map = new();

    public TypeMapper()
    {
        RegisterKnownEventTypes();
    }

    public string GetTypeName<T>()
    {
        if (!_map.TryGetValue(typeof(T), out var name))
            // TODO: log
            throw new UnregisteredTypeException(typeof(T));

        return name;
    }

    public string GetTypeName(object o)
    {
        if (!_map.TryGetValue(o.GetType(), out var name))
            // TODO: log
            throw new UnregisteredTypeException(o.GetType());

        return name;
    }

    public string GetTypeNameByType(Type type)
    {
        if (!_map.TryGetValue(type, out var name))
            // TODO: log
            throw new UnregisteredTypeException(type);

        return name;
    }

    public Type GetType(string typeName)
    {
        if (!_reverseMap.TryGetValue(typeName, out var type))
            // TODO: log
            throw new UnregisteredTypeException(typeName);

        return type;
    }

    public bool TryGetType(string typeName, out Type? type)
    {
        return _reverseMap.TryGetValue(typeName, out type);
    }

    public void AddType<T>(string name)
    {
        AddType(typeof(T), name);
    }

    internal void AddType(Type type, string name)
    {
        _reverseMap[name] = type;
        _map[type] = name;
        // TODO: log
    }

    public bool IsTypeRegistered<T>()
    {
        return _map.ContainsKey(typeof(T));
    }

    public void RegisterKnownEventTypes(params Assembly[] assembliesWithEvents)
    {
        var assembliesToScan = assembliesWithEvents.Length == 0
            ? GetDefaultAssemblies()
            : assembliesWithEvents;

        foreach (var assembly in assembliesToScan) RegisterAssemblyEventTypes(assembly);

        Assembly[] GetDefaultAssemblies()
        {
            var firstLevel = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => NamePredicate(x.GetName()))
                .ToArray();
            return firstLevel
                .SelectMany(Get)
                .Concat(firstLevel)
                .Distinct().ToArray();

            IEnumerable<Assembly> Get(Assembly assembly)
            {
                var referenced = assembly.GetReferencedAssemblies().Where(NamePredicate);
                var assemblies = referenced.Select(Assembly.Load).ToList();
                return assemblies.Concat(assemblies.SelectMany(Get)).Distinct();
            }
        }

        bool NamePredicate(AssemblyName name)
        {
            return name.Name != null &&
                   !name.Name.StartsWith("System.") &&
                   !name.Name.StartsWith("Microsoft.") &&
                   !name.Name.StartsWith("netstandard");
        }
    }

    private static readonly Type AttributeType = typeof(EventTypeAttribute);

    private void RegisterAssemblyEventTypes(Assembly assembly)
    {
        var decoratedTypes = assembly.DefinedTypes.Where(
            x => x.IsClass && x.CustomAttributes.Any(a => a.AttributeType == AttributeType)
        );

        foreach (var type in decoratedTypes)
        {
            var attr = type.GetAttribute<EventTypeAttribute>()!;
            AddType(type, attr.EventType);
        }
    }

    public void EnsureTypesRegistered(IEnumerable<Type> types)
    {
        foreach (var type in types) GetTypeNameByType(type);
    }
}