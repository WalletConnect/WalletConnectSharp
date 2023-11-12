using System.Reflection;
using WalletConnectSharp.Common.Logging;
using WalletConnectSharp.Platform;

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

    public IDeeplinkable AddDeepLinkable<T>() where T : IDeeplinkable, new()
    {
        var linker = new T();
        AddDeepLinkable(linker);
        return linker;
    }

    public void AddDeepLinkable(IDeeplinkable deeplinkable)
    {
        _deeplinkables.Add(deeplinkable);
    }

    public void RemoveLinkable<T>() where T : IDeeplinkable
    {
        _deeplinkables.RemoveAll(dl => dl.GetType() == typeof(T));
    }

    public string BuildLink(string wcUri, string targetChainId)
    {
        return BuildLink(wcUri, new[] { targetChainId }).FirstOrDefault() ?? throw new Exception("No linker could build the link");
    }

    public IEnumerable<string> BuildLink(string wcUri, string[] targetChains)
    {
        return from linker in _deeplinkables
            orderby linker.Priority descending
            where targetChains.Any(linker.SupportsChain) 
            select linker.TransformUri(wcUri)
            into url 
            where !string.IsNullOrWhiteSpace(url) 
            select url;
    }

    public async Task TryOpen(string wcUri, string[] targetChains)
    {
        var links = BuildLink(wcUri, targetChains);
        foreach (var link in links)
        {
            try
            {
                await DevicePlatform.OpenUrl(link);
                break;
            }
            catch (Exception e)
            {
                WCLogger.LogError(e);
            }
        }
    }
}
