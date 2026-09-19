using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

static class Globals
{
    public static Dictionary<string, Value> Values { get; } = new Dictionary<string, Value>();
    public static KindOperations KindOperations { get; } = new KindOperations();
    public static Position ProtocalPosition = new Position(0, 0, "<protocol>");

    public static Value GetGlobal(string name, Position position)
    {
        return Gaurder.Gaurd(position, () =>
        {
            if (Values.TryGetValue(name, out Value value))
                return value;

            throw new Error($"'{name}' does not exist", position);
        });
    }

    [UnconditionalSuppressMessage(
    "Trimming",
    "IL2026:RequiresUnreferencedCode",
    Justification = "Mineol intentionally discovers native types with reflection.")]

    [UnconditionalSuppressMessage(
    "Trimming",
    "IL2072:UnrecognizedReflectionPattern",
    Justification = "Mineol native implementations are expected to have parameterless constructors.")]


#pragma warning disable CA2255 // The 'ModuleInitializer' attribute should not be used in libraries
    [ModuleInitializer]
#pragma warning restore CA2255 // The 'ModuleInitializer' attribute should not be used in libraries
    public static void LoadNatives()
    {
        Type nativeType = typeof(INative);

        Type[] types = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type =>
                nativeType.IsAssignableFrom(type) &&
                !type.IsInterface &&
                !type.IsAbstract)
            .ToArray();

        List<INative> natives = new List<INative>();

        foreach (Type type in types)
            if (Activator.CreateInstance(type) is INative native)
                natives.Add(native);

        foreach (INative native in natives)
            foreach (string kind in native.Kinds)
            {
                if (ValueKind.NameToId.TryGetValue(kind, out int kindId))
                    if (ValueKind.Get(kindId).IsProtected)
                        throw new InvalidOperationException($"{kind} is a protected type and cant be overwritten");

                ValueKind.Register(kind, false);
            }

        foreach (INative native in natives)
        {
            native.Register(Values);
            native.RegiserKindOperations(KindOperations);
        }

        RecordOperations.Register(KindOperations);

        // foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        // {
        //     foreach (Type type in assembly.GetTypes())
        //     {
        //         if (type != typeof(Exception) && type != typeof(Error) && typeof(Exception).IsAssignableFrom(type))
        //             ErrorType.Register(type);
        //     }
        // }
    }

}