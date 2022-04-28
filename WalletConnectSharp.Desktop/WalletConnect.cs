using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nethereum.RLP;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Events;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Models.Ethereum;
using WalletConnectSharp.Core.Network;
using WalletConnectSharp.Core.Utils;
using WalletConnectSharp.Desktop.Network;
using HexByteConvertorExtensions = Nethereum.Hex.HexConvertors.Extensions.HexByteConvertorExtensions;

namespace WalletConnectSharp.Desktop
{
    public class WalletConnect : WalletConnectSession
    {
        static WalletConnect()
        {
            TransportFactory.Instance.RegisterDefaultTransport((eventDelegator) => new WebsocketTransport(eventDelegator));
        }
        
        public WalletConnect(ClientMeta clientMeta, string bridgeUrl = null, ITransport transport = null, ICipher cipher = null, int chainId = 1, EventDelegator eventDelegator = null) : base(clientMeta, bridgeUrl, transport, cipher, chainId, eventDelegator)
        {
        }

        public override async Task<string> EthSignTransaction(params TransactionData[] transaction)
        {
            //First try to sign the transaction normally
            try
            {
                return await base.EthSignTransaction(transaction);
            }
            catch (WalletException)
            {
                //Try using eth_sign
                //First encode the transaction data
                
                //TODO Why are we using params?
                foreach (var t in transaction)
                {
                    if (t.nonce == null || t.gasPrice == null || t.gas == null || t.value == null)
                    {
                        throw new ArgumentException(
                            "The following fields in the TransactionData are required: (nonce, gasPrice, gas, value, to, data)");
                    }

                    string funcTest =
                        @"[a-zA-Z0-9_]+\(((address|uint256|uint|uint8|bool|string|bytes|uint16|uint32|uint64|uint128),?)*\)";
                    Regex r = new Regex(funcTest, RegexOptions.Singleline);
                    if (r.IsMatch(t.data))
                    {
                        t.data = "0x" + Sha3Keccack.Current.CalculateHash(t.data).Substring(0, 8);
                    }
                    
                    byte[] nonce = t.nonce.ToBytesForRLPEncoding();
                    byte[] gasPrice = t.gasPrice.ToBytesForRLPEncoding();
                    byte[] gasLimit = t.gas.ToBytesForRLPEncoding();
                    byte[] to = HexByteConvertorExtensions.HexToByteArray(t.to);
                    byte[] amount = t.value.ToBytesForRLPEncoding();
                    byte[] data = HexByteConvertorExtensions.HexToByteArray(t.data);

                    byte[] rawData = RLP.EncodeList(new[]
                    {
                        nonce,
                        gasPrice,
                        gasLimit,
                        to,
                        amount,
                        data
                    });

                    

                    return await base.EthSign(t.@from,  "0x" + Sha3Keccack.Current.CalculateHash(rawData).ToHex());
                }

                throw;
            }
        }
    }
}