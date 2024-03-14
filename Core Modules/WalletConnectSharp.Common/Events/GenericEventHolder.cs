using System.Collections.Concurrent;

namespace WalletConnectSharp.Common.Events;

public class GenericEventHolder
{
    private readonly ConcurrentDictionary<Type, object> _dynamicTypeMapping = new();

    public EventHandlerMap<T> OfType<T>()
    {
        Type t = typeof(T);

        if (_dynamicTypeMapping.TryGetValue(t, out var value))
        {
            if (value is EventHandlerMap<T> eventHandlerMap)
            {
                return eventHandlerMap;
            }

            throw new InvalidCastException($"Stored value cannot be cast to EventHandlerMap<{typeof(T).Name}>");
        }

        var mapping = new EventHandlerMap<T>();
        _dynamicTypeMapping.TryAdd(t, mapping);

        return mapping;
    }
}
