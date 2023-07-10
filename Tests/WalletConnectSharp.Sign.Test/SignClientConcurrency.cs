using Newtonsoft.Json;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Events;
using WalletConnectSharp.Events.Model;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine.Events;
using WalletConnectSharp.Sign.Models.Engine.Methods;
using WalletConnectSharp.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace WalletConnectSharp.Sign.Test
{

    public class SignClientConcurrency
    {
        public class TestPairings
        {
            public SignClientFixture clients;
            public SessionStruct sessionA;
        }

        public class TestEmitData
        {
            [JsonProperty("name")]
            public string Name { get; set; }
            
            [JsonProperty("data")]
            public string Data { get; set; }
        }
        
        private static readonly string TestEthereumAddress = "0x3c582121909DE92Dc89A36898633C1aE4790382b";
        private static readonly string TestEthereumChain = "eip155:1";
        private static readonly string TestArbitrumChain = "eip155:42161";
        private static readonly string TestAvalancheChain = "eip155:43114";

        private static readonly string[] TestAccounts = new[]
        {
            $"{TestEthereumChain}:{TestEthereumAddress}", $"{TestArbitrumChain}:{TestEthereumAddress}",
            $"{TestAvalancheChain}:{TestEthereumAddress}"
        };

        private static readonly string[] TestEvents = new[] { "chainChanged", "accountsChanged" };

        private ITestOutputHelper _output;

        public SignClientConcurrency(ITestOutputHelper output)
        {
            this._output = output;
        }

        [Fact, Trait("Category", "integration")]
        public async void TestConcurrentClients() => await _TestConcurrentClients().WithTimeout(TimeSpan.FromMinutes(20));

        private async Task _TestConcurrentClients()
        {
            List<TestPairings> pairings = new List<TestPairings>();
            List<List<SessionEvent>> messagesReceived = new List<List<SessionEvent>>();

            CancellationTokenSource heartbeatToken = new CancellationTokenSource();

#pragma warning disable CS4014
            Task.Run(async delegate
#pragma warning restore CS4014
            {
                while (!heartbeatToken.Token.IsCancellationRequested)
                {
                    Log($"initialized pairs - {pairings.Count}");
                    
                    await Task.Delay(TestValues.HeartbeatInterval);
                }
            }, heartbeatToken.Token);
            
            // TODO Do stuff
            var testEventParams = new EventData<string>() { Name = TestEvents[0], Data = "" };
            
            Task ProcessMessages(TestPairings data, int clientIndex)
            {
                var clients = data.clients;
                var sessionA = data.sessionA;
                
                var eventPayload = new SessionEvent<string>()
                {
                    ChainId = TestEthereumChain, 
                    Event = testEventParams,
                    Topic = sessionA.Topic,
                };
                
                messagesReceived.Insert(clientIndex, new List<SessionEvent>());

                TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();

                ISignClient[] clientsArr = new[] { clients.ClientA, clients.ClientB };

                void CheckAllMessagesProcessed()
                {
                    if (messagesReceived[clientIndex].Count >= TestValues.MessagesPerClient)
                    {
                        task.TrySetResult(true);
                    }
                }

                foreach (var client in clientsArr)
                {
                    client.On<SessionEvent>(EngineEvents.SessionPing, delegate(object sender, GenericEvent<SessionEvent> @event)
                    {
                        Assert.Equal(sessionA.Topic, @event.EventData.Topic);
                        messagesReceived[clientIndex].Add(@event.EventData);
                        CheckAllMessagesProcessed();
                    });
                    
                    client.On<EmitEvent<string>>(EngineEvents.SessionEvent, (sender, @event) =>
                    {
                        Assert.Equal(testEventParams.Data, @event.EventData.Params.Event.Data);
                        Assert.Equal(eventPayload.Topic, @event.EventData.Topic);
                        messagesReceived[clientIndex].Add(@eve);
                    })
                }

                return task.Task;
            }
            
            heartbeatToken.Cancel();
        }

        private void Log(string message)
        {
            this._output.WriteLine(message);
        }
    }
}
