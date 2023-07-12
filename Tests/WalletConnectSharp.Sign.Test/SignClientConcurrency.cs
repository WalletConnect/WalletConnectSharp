using Newtonsoft.Json;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Events;
using WalletConnectSharp.Events.Model;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine.Events;
using WalletConnectSharp.Sign.Models.Engine.Methods;
using WalletConnectSharp.Sign.Test.Shared;
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

        public class TestResults
        {
            public long pairingLatencyMs;
            public long handshakeLatencyMs;
            public bool connected;
        }

        public class TestEmitData
        {
            [JsonProperty("name")]
            public string Name { get; set; }
            
            [JsonProperty("data")]
            public string Data { get; set; }
        }
        
        private ITestOutputHelper _output;

        public SignClientConcurrency(ITestOutputHelper output)
        {
            this._output = output;
        }

        [Fact, Trait("Category", "concurrency")]
        public async void TestConcurrentClients() => await _TestConcurrentClients().WithTimeout(TimeSpan.FromMinutes(20));

        private int[][] BatchArray(int[] array, int size)
        {
            List<int[]> results = new List<int[]>();
            for (int i = 0; i < array.Length; i += size)
            {
                var batch = array.Skip(i).Take(size).ToArray();
                results.Add(batch);
            }

            return results.ToArray();
        }

        private async Task<SignClientFixture> InitTwoClients()
        {
            var fixture = new SignClientFixture(false);
            await fixture.Init();
            await Task.Delay(500);
            return fixture;
        }

        private async Task DeleteClients(SignClientFixture clients)
        {
            await Task.Delay(500);
            foreach (var client in new[] { clients.ClientA, clients.ClientB })
            {
                if (client == null)
                    continue;

                // TODO Remove event data
                if (client.Core.Relayer.Connected)
                {
                    await client.Core.Relayer.TransportClose();
                }
            }
        }
        
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
            var testEventParams = new EventData<string>() { Name = SignTestValues.TestEvents[0], Data = "" };
            
            Task ProcessMessages(TestPairings data, int clientIndex)
            {
                var clients = data.clients;
                var sessionA = data.sessionA;
                
                var eventPayload = new SessionEvent<string>()
                {
                    ChainId = SignTestValues.TestEthereumChain, 
                    Event = testEventParams,
                    Topic = sessionA.Topic,
                };
                
                messagesReceived.Insert(clientIndex, new List<SessionEvent>());

                TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();

                ISignClient[] clientsArr = new[] { clients.ClientA, clients.ClientB };

                var namespacesBefore = sessionA.Namespaces;
                var namespacesAfter = new Namespaces(namespacesBefore)
                {
                    { 
                        "eip9001", new Namespace() {
                            Accounts = new []{ "eip9001:1:0x000000000000000000000000000000000000dead" },
                            Methods = new []{ "eth_sendTransaction" },
                            Events = new []{ "accountsChanged" }
                        }
                    }
                };

                Task Emit(ISignClient client)
                {
                    return client.Emit(sessionA.Topic, testEventParams, SignTestValues.TestEthereumChain);
                }

                void CheckAllMessagesProcessed()
                {
                    if (messagesReceived[clientIndex].Count >= TestValues.MessagesPerClient)
                    {
                        task.TrySetResult(true);
                    }
                }

                foreach (var client in clientsArr)
                {
                    client.On<SessionEvent>(EngineEvents.SessionPing, (sender, @event) =>
                    {
                        Assert.Equal(sessionA.Topic, @event.EventData.Topic);
                        messagesReceived[clientIndex].Add(@event.EventData);
                        CheckAllMessagesProcessed();
                    });

                    client.On<EmitEvent<string>>(EngineEvents.SessionEvent, (sender, @event) =>
                    {
                        Assert.Equal(testEventParams.Data, @event.EventData.Params.Event.Data);
                        Assert.Equal(eventPayload.Topic, @event.EventData.Topic);
                        messagesReceived[clientIndex].Add(@event.EventData);
                        CheckAllMessagesProcessed();
                    });

                    client.On<SessionUpdateEvent>(EngineEvents.SessionUpdate, (sender, @event) =>
                    {
                        Assert.Equal(client.Session.Get(sessionA.Topic).Namespaces, namespacesAfter);
                        messagesReceived[clientIndex].Add(@event.EventData);
                        CheckAllMessagesProcessed();
                    });
                }

                async void SendMessages()
                {
                    Random random = new Random();
                    for (int i = 0; i < TestValues.MessagesPerClient; i++)
                    {
                        var client = (int)Math.Floor(random.NextDouble() * clientsArr.Length);
                        await Emit(clientsArr[client]);
                        await Task.Delay(10);
                    }
                }
                
                SendMessages();

                return task.Task;
            }
            
            int[] arr = Enumerable.Range(0, TestValues.ClientCount+1).ToArray();
            int[][] batches = BatchArray(arr, 100);

            async Task<TestResults> ConnectClient()
            {
                var now = Clock.NowMilliseconds();
                var clients = await InitTwoClients();
                var handshakeLatencyMs = Clock.NowMilliseconds() - now;
                await Task.Delay(10);
                Assert.IsType<WalletConnectSignClient>(clients.ClientA);
                Assert.IsType<WalletConnectSignClient>(clients.ClientB);

                var sessionA = await SignTests.TestConnectMethod(clients.ClientA, clients.ClientB);
                pairings.Add(new TestPairings()
                {
                    clients = clients,
                    sessionA = sessionA
                });
                var pairingLatencyMs = Clock.NowMilliseconds() - now;
                return new TestResults()
                {
                    connected = true, handshakeLatencyMs = handshakeLatencyMs, pairingLatencyMs = pairingLatencyMs
                };
            }

            foreach (int[] batch in batches)
            {
                var connections = (await Task.WhenAll(
                    batch.Select(async delegate(int i)
                    {
                        try
                        {
                            return await ConnectClient().WithTimeout(120000);
                        }
                        catch (TimeoutException)
                        {
                            Log($"Client {i} hung up");
                            return new TestResults()
                            {
                                connected = false, handshakeLatencyMs = -1, pairingLatencyMs = -1
                            };
                        }
                    })
                )).Where(t => t.connected).ToList();

                var averagePairingLatency = connections.Select(c => c.pairingLatencyMs)
                    .Aggregate((a, b) => a + b) / connections.Count;
                var averageHandhsakeLatency = connections.Select(c => c.handshakeLatencyMs)
                    .Aggregate((a, b) => a + b) / connections.Count;
                var failures = batch.Length - connections.Count;
                Log($"{connections.Count} out of {batch.Length} connected ({averagePairingLatency}ms avg pairing latency, {averageHandhsakeLatency}ms avg handshake latency");
                
                // TODO uploadLoadTestConnectionDataToCloudWatch
            }
            
            await Task.WhenAll(
                pairings.Select(async delegate(TestPairings testPairings, int i)
                {
                    await ProcessMessages(testPairings, i);
                })
            );

            foreach (var data in pairings)
            {
                var clients = data.clients;
                var sessionA = data.sessionA;

                TaskCompletionSource<bool> clientBDisconnected = new TaskCompletionSource<bool>();
                clients.ClientB.On<SessionEvent>(EngineEvents.SessionDelete, (sender, @event) =>
                {
                    Assert.Equal(sessionA.Topic, @event.EventData.Topic);
                    clientBDisconnected.TrySetResult(true);
                });

                await Task.WhenAll(
                    clientBDisconnected.Task,
                    clients.ClientA.Disconnect(sessionA.Topic, Error.FromErrorType(ErrorType.USER_DISCONNECTED))
                );

                await DeleteClients(clients);
            }
            
            heartbeatToken.Cancel();
        }

        private void Log(string message)
        {
            this._output.WriteLine(message);
        }
    }
}
