namespace WalletConnectSharp.Common.Utils;

public class EventUtils
{
    /// <summary>
    /// Invoke the given event handler once and then unsubscribe it.
    /// Use with abstract events. Otherwise, use the extension method as it is more efficient.
    /// </summary>
    /// <example>
    /// <code>
    /// EventUtils.ListenOnce((_, _) => WCLogger.Log("Resubscribed")),
    ///     h => this.Subscriber.Resubscribed += h,
    ///     h => this.Subscriber.Resubscribed -= h
    /// );
    /// </code>
    /// </example>
    public static Action ListenOnce(
        EventHandler handler,
        Action<EventHandler> subscribe,
        Action<EventHandler> unsubscribe)
    {
        EventHandler internalHandler = null;
        internalHandler = (sender, args) =>
        {
            unsubscribe(internalHandler);
            handler(sender, args);
        };

        subscribe(internalHandler);

        return () => unsubscribe(internalHandler);
    }

    /// <summary>
    /// Invoke the given event handler once and then unsubscribe it.
    /// Use with abstract events. Otherwise, use the extension method as it is more efficient.
    /// </summary>
    public static Action ListenOnce<TEventArgs>(
        EventHandler<TEventArgs> handler,
        Action<EventHandler<TEventArgs>> subscribe,
        Action<EventHandler<TEventArgs>> unsubscribe)
    {
        EventHandler<TEventArgs> internalHandler = null;
        internalHandler = (sender, args) =>
        {
            unsubscribe(internalHandler);
            handler(sender, args);
        };

        subscribe(internalHandler);

        return () => unsubscribe(internalHandler);
    }
}
