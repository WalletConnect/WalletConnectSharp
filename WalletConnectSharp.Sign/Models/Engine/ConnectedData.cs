using System.Threading.Tasks;

namespace WalletConnectSharp.Sign.Models.Engine
{
    public class ConnectedData
    {
        public string Uri { get; set; }

        public Task<SessionStruct> Approval { get; set; }
    }
}