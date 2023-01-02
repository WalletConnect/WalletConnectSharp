using System;
using System.IO;
using System.Threading.Tasks;
using WalletConnectSharp.Common;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Common.Model.Relay;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Events;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Network.Tests.Models;
using WalletConnectSharp.Network.Websocket;
using WalletConnectSharp.Tests.Common;
using Websocket.Client;
using Xunit;

namespace WalletConnectSharp.Network.Tests
{
    public class RelayTests
    {
        private static readonly JsonRpcRequest<TopicData> TEST_WAKU_REQUEST =
            new JsonRpcRequest<TopicData>(RelayProtocols.DefaultProtocol.Subscribe, new TopicData()
            {
                Topic = "ca838d59a3a3fe3824dab9ca7882ac9a2227c5d0284c88655b261a2fe85db270"
            });
        private static readonly JsonRpcRequest<TopicData> TEST_BAD_WAKU_REQUEST =
            new JsonRpcRequest<TopicData>(RelayProtocols.DefaultProtocol.Subscribe, new TopicData());
        
        private static readonly string TEST_RANDOM_HOST = "random.domain.that.does.not.exist";
        private static readonly string GOOD_WS_URL = "wss://relay.walletconnect.com/";
        private static readonly string BAD_WS_URL = "ws://" + TEST_RANDOM_HOST;

        public async Task<string> BuildGoodURL()
        {
            var crypto = new Crypto.Crypto();
            await crypto.Init();
            
            var auth = await crypto.SignJwt(GOOD_WS_URL);

            return RelayUrl.FormatRelayRpcUrl(
                RelayProtocols.Default,
                RelayConstants.Version.ToString(),
                GOOD_WS_URL,
                SDKConstants.SDK_VERSION,
                TestValues.TestProjectId,
                auth
            );
        }

        [Fact]
        public async void ConnectAndRequest()
        {
            var url = await BuildGoodURL();
            var connection = new WebsocketConnection(url);
            var provider = new JsonRpcProvider(connection);
            await provider.Connect();

            var result = await provider.Request<TopicData, string>(TEST_WAKU_REQUEST);
            
            Assert.True(result.Length > 0);
        }
        
        [Fact]
        public async void RequestWithoutConnect()
        {
            var url = await BuildGoodURL();
            var connection = new WebsocketConnection(url);
            var provider = new JsonRpcProvider(connection);

            var result = await provider.Request<TopicData, string>(TEST_WAKU_REQUEST);
            
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async void ThrowOnJsonRpcError()
        {
            var url = await BuildGoodURL();
            var connection = new WebsocketConnection(url);
            var provider = new JsonRpcProvider(connection);

            await Assert.ThrowsAsync<WalletConnectException>(() => provider.Request<TopicData, string>(TEST_BAD_WAKU_REQUEST));
        }

        [Fact]
        public async void ThrowsOnUnavailableHost()
        {
            var connection = new WebsocketConnection(BAD_WS_URL);
            var provider = new JsonRpcProvider(connection);
            
            await Assert.ThrowsAsync<TimeoutException>(() => provider.Request<TopicData, string>(TEST_WAKU_REQUEST));
        }

        [Fact]
        public async void ReconnectsWithNewProvidedHost()
        {
            var url = await BuildGoodURL();
            var connection = new WebsocketConnection(BAD_WS_URL);
            var provider = new JsonRpcProvider(connection);
            Assert.Equal(BAD_WS_URL, provider.Connection.Url);
            await provider.Connect(url);
            Assert.Equal(url, provider.Connection.Url);
            
            var result = await provider.Request<TopicData, string>(TEST_WAKU_REQUEST);
            
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async void DoesNotDoubleRegisterListeners()
        {
            var url = await BuildGoodURL();
            var connection = new WebsocketConnection(url);
            var provider = new JsonRpcProvider(connection);
            
            var expectedDisconnectCount = 3;
            var disconnectCount = 0;

            provider.On<DisconnectionInfo>("disconnect", (_, __) => disconnectCount++);

            await provider.Connect();
            await provider.Disconnect();
            await provider.Connect();
            await provider.Disconnect();
            await provider.Connect();
            await provider.Disconnect();
            
            Assert.Equal(expectedDisconnectCount, disconnectCount);
        }
    }
}