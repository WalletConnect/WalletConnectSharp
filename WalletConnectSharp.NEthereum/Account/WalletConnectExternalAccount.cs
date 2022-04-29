using System;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using Nethereum.JsonRpc.Client;
using Nethereum.Signer;
using Nethereum.Signer.Crypto;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Models.Ethereum;
using WalletConnectSharp.Core.Utils;

namespace WalletConnectSharp.NEthereum.Account
{
    public class WalletConnectExternalAccount : EthExternalSignerBase
    {
        public static readonly string[] EthSignWallets = new[]
        {
            "metamask",
            "trust"
        };
        
        private WalletConnectSession _session;
        private IClient _client;
        
        public WalletConnectExternalAccount(WalletConnectSession session, IClient client)
        {
            this._session = session;
            this._client = client;
        }

        public override Task<string> GetAddressAsync()
        {
            return Task.FromResult(_session.Accounts[0]);
        }
        
        protected override Task<byte[]> GetPublicKeyAsync()
        {
            throw new NotImplementedException("Not implemented interface to retrieve the public key from WalletConnect");
        }

        protected override async Task<ECDSASignature> SignExternallyAsync(byte[] hash)
        {
            var hashStr = "0x" + hash.ToHex();
            var request = new EthSign(_session.Accounts[0], hashStr);

            var response = await _session.Send<EthSign, EthResponse>(request);

            return ECDSASignatureFactory.ExtractECDSASignature(response.Result);
        }

        public override Task SignAsync(LegacyTransactionChainId transaction)
        {
            return SignHashTransactionAsync(transaction);
        }

        public override Task SignAsync(Transaction1559 transaction)
        {
            return SignHashTransactionAsync(transaction);
        }

        public override bool CalculatesV { get; protected set; } = true;

        public override bool Supported1559
        {
            get
            {
                return _session.ChainId == 1;
            }
        }

        public override ExternalSignerTransactionFormat ExternalSignerTransactionFormat { get; protected set; } =
            ExternalSignerTransactionFormat.Hash;

        public override Task SignAsync(LegacyTransaction transaction)
        {
            return SignHashTransactionAsync(transaction);
        }
    }
}