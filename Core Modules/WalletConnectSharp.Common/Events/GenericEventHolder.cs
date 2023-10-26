namespace WalletConnectSharp.Common.Events;

public class GenericEventHolder
{
    private Dictionary<Type, object> dynamicTypeMapping = new();

    public EventHandlerMap<T> OfType<T>()
    {
        Type t = typeof(T);

        if (dynamicTypeMapping.TryGetValue(t, out var value))
            return (EventHandlerMap<T>)value;

        var mapping = new EventHandlerMap<T>();
        dynamicTypeMapping.Add(t, mapping);

        return mapping;
    }
}
