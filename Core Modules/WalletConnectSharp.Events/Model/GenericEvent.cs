using System;

namespace WalletConnectSharp.Events.Model
{
    /// <summary>
    /// A generic implementation of the IEvent interface. Given a event data type T, store the data in-memory
    /// in the EventData property
    /// </summary>
    /// <typeparam name="T">The event data type to store</typeparam>
    public class GenericEvent<T> : IEvent<T>
    {
        public T EventData { get; private set; }

        public void SetData(T response)
        {
            if (EventData != null && !EventData.Equals(default(T)))
            {
                throw new ArgumentException("Response was already set");
            }
            
            EventData = response;
        }
    }
}