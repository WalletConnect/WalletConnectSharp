using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Accounts;
using Nethereum.RPC.NonceServices;
using Nethereum.RPC.TransactionManagers;
using WalletConnectSharp.Core;

namespace WalletConnectSharp.NEthereum.Account
{
    public class WalletConnectAccount : IAccount
    {
        private WalletConnectSession session;

        public string Address
        {
            get
            {
                return session.Accounts[0];
            }
        }

        public ITransactionManager TransactionManager { get; }
        public INonceService NonceService { get; set; }

        public WalletConnectAccount(WalletConnectSession session, IClient client, bool allowEthSign = false)
        {
            this.session = session;
            this.TransactionManager = new WalletConnectTransactionManager(client, session, this, allowEthSign);
            this.NonceService = new InMemoryNonceService(session.Accounts[0], client);
        }
    }
}