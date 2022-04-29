using System.Linq;
using System.Threading.Tasks;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.JsonRpc.Client;
using Nethereum.RLP;
using Nethereum.RPC.Accounts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.TransactionManagers;
using Nethereum.Util;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Models.Ethereum;
using WalletConnectSharp.NEthereum.Model;

namespace WalletConnectSharp.NEthereum.Account
{
    public class WalletConnectTransactionManager : TransactionManager
    {
        public static readonly string[] EthSignWallets = new[]
        {
            "metamask",
            "trust"
        };

        
        private WalletConnectSession _session;
        private IAccount _account;
        public WalletConnectTransactionManager(IClient client, WalletConnectSession session, IAccount account) : base(client)
        {
            _session = session;
            _account = account;
        }

        public override async Task<string> SignTransactionAsync(TransactionInput transaction)
        {
            if (EthSignWallets.Contains(_session.WalletMetadata.Name.ToLower()))
            {
                //MetaMask does not support eth_signTransaction
                //Therefore, we'll use eth_sign for now

                if (transaction.Nonce == null)
                {
                    var nextNonce = await _account.NonceService.GetNextNonceAsync();
                    transaction.Nonce = nextNonce;
                }

                if (transaction.Gas == null)
                {
                    var estimatedGas = await EstimateGasAsync(transaction);
                    transaction.Gas = estimatedGas;
                }

                if (transaction.GasPrice == null)
                {
                    var estimatedGasPrice = await GetGasPriceAsync(transaction);
                    transaction.GasPrice = estimatedGasPrice;
                }

                byte[] nonce = transaction.Nonce.Value.ToBytesForRLPEncoding();
                byte[] gasPrice = transaction.GasPrice.Value.ToBytesForRLPEncoding();
                byte[] gasLimit = transaction.Gas.Value.ToBytesForRLPEncoding();
                byte[] to = HexByteConvertorExtensions.HexToByteArray(transaction.To);
                byte[] amount = transaction.Value.Value.ToBytesForRLPEncoding();
                byte[] data = HexByteConvertorExtensions.HexToByteArray(transaction.Data);

                byte[] rawData = RLP.EncodeList(new[]
                {
                    nonce,
                    gasPrice,
                    gasLimit,
                    to,
                    amount,
                    data
                });

                var hash = "0x" + Sha3Keccack.Current.CalculateHash(rawData).ToHex();

                var request = new EthSign(_account.Address, hash);

                var response = await _session.Send<EthSign, EthResponse>(request);

                return response.Result;
            }
            else
            {
                var request = new NEthSignTransaction(transaction);

                var response = await _session.Send<NEthSignTransaction, EthResponse>(request);

                return response.Result;
            }
        }
    }
}