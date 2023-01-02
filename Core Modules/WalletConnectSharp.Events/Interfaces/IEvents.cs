using System;
using WalletConnectSharp.Events.Model;

namespace WalletConnectSharp.Events.Interfaces
{
    /// <summary>
    /// An interface that represents a class that triggers events that can be listened to.
    /// </summary>
    public interface IEvents
    {
        EventDelegator Events { get; }
    }
}