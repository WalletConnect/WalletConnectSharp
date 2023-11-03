using System.Reflection;

namespace Deeplinker;

public class DeeplinkerFactory
{
    public static readonly DeeplinkerFactory Instance = new DeeplinkerFactory();
    private readonly MethodInfo BuilderMethod;
    private List<IDeeplinkable> _deeplinkables = null!;
    
    private DeeplinkerFactory()
    {
        BuilderMethod = GetType().GetMethod("BuildLinkable") ?? throw new InvalidOperationException("Cannot find builder method in DeeplinkerFactory");
        
        RefreshDeeplinkables();
    }

    public IList<IDeeplinkable> Deeplinkables
    {
        get
        {
            return _deeplinkables.AsReadOnly();
        }
    }

    public void RefreshDeeplinkables()
    {
        var inheritedTypes = from assembly in AppDomain.CurrentDomain.GetAssemblies()
            from type in assembly.GetTypes()
            where type != typeof(object) && typeof(IDeeplinkable).IsSubclassOf(type)
            select type;

        _deeplinkables = inheritedTypes.Select(BuildGenericLinkable).ToList();
    }

    private IDeeplinkable BuildGenericLinkable(Type t)
    {
        var genericMethod = BuilderMethod.MakeGenericMethod(t) ?? throw new InvalidOperationException($"Cannot create generic builder method for type {t.FullName}");
        return (IDeeplinkable)(genericMethod.Invoke(this, new object[] { }) ?? throw new InvalidOperationException($"Cannot build deeplinkable for type {t.FullName}"));
    }

    public IDeeplinkable BuildLinkable<T>() where T : IDeeplinkable, new()
    {
        return new T();
    }
}
