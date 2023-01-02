using System;
using System.Linq;

namespace WalletConnectSharp.Network.Models
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class RpcMethodAttribute : Attribute
    {
        public string MethodName { get; }

        public RpcMethodAttribute(string method)
        {
            MethodName = method;
        }
        
        public static string MethodForType<T>()
        {
            var attributes = typeof(T).GetCustomAttributes(typeof(RpcMethodAttribute), true);
            if (attributes.Length != 1)
                throw new Exception($"Type {typeof(T).FullName} has no WcMethod attribute!");

            var method = attributes.Cast<RpcMethodAttribute>().First().MethodName;

            return method;
        }
    }
}