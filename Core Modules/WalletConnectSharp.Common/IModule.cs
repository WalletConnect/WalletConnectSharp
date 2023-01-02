using System;

namespace WalletConnectSharp.Common
{
    // TODO Make IModule implement IDisposable, enforce everywhere
    public interface IModule /* : IDisposable */
    {
        string Name { get; }
        
        string Context { get; }
    }
}