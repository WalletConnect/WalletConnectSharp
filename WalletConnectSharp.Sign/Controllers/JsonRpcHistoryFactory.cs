using System.Collections.Generic;
using System.Threading.Tasks;
using WalletConnectSharp.Core.Interfaces;

namespace WalletConnectSharp.Sign.Controllers
{
    public class JsonRpcHistoryFactory : IJsonRpcHistoryFactory
    {
        public class JsonRpcHistoryHolder<T, TR>
        {
            private static Dictionary<string, JsonRpcHistoryHolder<T, TR>> _instance = new Dictionary<string, JsonRpcHistoryHolder<T, TR>>();

            public static async Task<JsonRpcHistoryHolder<T, TR>> InstanceForContext(ICore core)
            {
                if (_instance.ContainsKey(core.Context))
                    return _instance[core.Context];

                var historyHolder = new JsonRpcHistoryHolder<T, TR>(core);
                _instance.Add(core.Context, historyHolder);
                await historyHolder.History.Init();
                return historyHolder;
            }

            public IJsonRpcHistory<T, TR> History { get; }

            private JsonRpcHistoryHolder(ICore core)
            {
                History = new JsonRpcHistory<T, TR>(core);
            }
        }

        public ICore Core { get; }

        public JsonRpcHistoryFactory(ICore core)
        {
            this.Core = core;
        }

        public async Task<IJsonRpcHistory<T, TR>> JsonRpcHistoryOfType<T, TR>()
        {
            return (await JsonRpcHistoryHolder<T, TR>.InstanceForContext(Core)).History;
        }
    }
}