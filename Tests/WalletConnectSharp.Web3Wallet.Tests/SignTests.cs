using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Newtonsoft.Json;
using WalletConnectSharp.Auth;
using WalletConnectSharp.Auth.Internals;
using WalletConnectSharp.Auth.Models;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Models.Verify;
using WalletConnectSharp.Events;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectSharp.Sign.Models.Engine.Events;
using WalletConnectSharp.Tests.Common;
using Xunit;

namespace WalletConnectSharp.Web3Wallet.Tests
{
    public class SignClientTests : IClassFixture<CryptoWalletFixture>, IAsyncLifetime
    {
        [RpcMethod("eth_signTransaction"), RpcRequestOptions(Clock.ONE_MINUTE, 99997)]
        public class EthSignTransaction : List<TransactionInput>
        {
        }

        public class ChainChangedEvent
        {
            [JsonProperty("test")]
            public string Test { get; set; }
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
        
        private static readonly RequestParams DefaultRequestParams = new RequestParams()
        {
            Aud = "http://localhost:3000/login",
            Domain = "localhost:3000",
            ChainId = "eip155:1",
            Nonce = CryptoUtils.GenerateNonce()
        };

        private static readonly RequiredNamespaces TestRequiredNamespaces = new RequiredNamespaces()
        {
            {
                "eip155", new RequiredNamespace()
                    .WithMethod("eth_signTransaction")
                    .WithChain("eip155:1")
                    .WithEvent("chainChanged")
            }
        };
        
        private static readonly Namespaces TestUpdatedNamespaces = new Namespaces()
        {
            {
                "eip155", new Namespace()
                    {
                        Methods = new []
                        {
                            "eth_signTransaction",
                            "eth_sendTransaction",
                            "personal_sign",
                            "eth_signTypedData"
                        },
                        Accounts = TestAccounts,
                        Events = TestEvents
                    }
            }
        };

        private static readonly Namespace TestNamespace = new Namespace()
        {
            Methods = new[] { "eth_signTransaction", },
            Accounts = new[] { TestAccounts[0] },
            Events = new[] { TestEvents[0] }
        };
        
        private static readonly Namespaces TestNamespaces = new Namespaces()
        {
            {
                "eip155", TestNamespace
            }
        };

        private static readonly ConnectOptions TestConnectOptions = new ConnectOptions()
            .UseRequireNamespaces(TestRequiredNamespaces);
        
        private readonly CryptoWalletFixture _cryptoWalletFixture;
        private WalletConnectCore _core;
        private WalletConnectSignClient _dapp;
        private Web3WalletClient _wallet;
        private string uriString;
        private Task<SessionStruct> sessionApproval;
        private SessionStruct session;
        
        
        public string WalletAddress
        {
            get
            {
                return _cryptoWalletFixture.WalletAddress;
            }
        }

        public string Iss
        {
            get
            {
                return _cryptoWalletFixture.Iss;
            }
        }

        public SignClientTests(CryptoWalletFixture cryptoWalletFixture)
        {
            this._cryptoWalletFixture = cryptoWalletFixture;
        }

        public async Task InitializeAsync()
        {
            _core = new WalletConnectCore(new CoreOptions()
            {
                ProjectId = TestValues.TestProjectId, RelayUrl = TestValues.TestRelayUrl,
            });
            _dapp = await WalletConnectSignClient.Init(new SignClientOptions()
            {
                ProjectId = TestValues.TestProjectId,
                Metadata = new Metadata(),
                Name = "dapp",
            });
            var connectData = await _dapp.Connect(TestConnectOptions);
            uriString = connectData.Uri ?? "";
            sessionApproval = connectData.Approval;
            
            _wallet = await Web3WalletClient.Init(_core, new Metadata(), "wallet");
            
            Assert.NotNull(_wallet);
            Assert.NotNull(_dapp);
            Assert.NotNull(_core);
            Assert.Null(_wallet.Metadata.Redirect);
            // Compiler knows ;)
            //Assert.Null(_dapp.Metadata.Redirect);
        }

        public async Task DisposeAsync()
        {
            if (_core.Relayer.Connected)
            {
                await _core.Relayer.TransportClose();
            }
        }

        [Fact, Trait("Category", "unit")]
        public async void TestShouldApproveSessionProposal()
        {
            TaskCompletionSource<bool> task1 = new TaskCompletionSource<bool>();
            _wallet.On<SessionProposalEvent>(EngineEvents.SessionProposal, async (sender, @event) =>
            {
                var id = @event.EventData.Id;
                var proposal = @event.EventData.Proposal;
                var verifyContext = @event.EventData.VerifiedContext;
                
                Assert.Equal(Validation.Unknown, verifyContext.Validation);
                session = await _wallet.ApproveSession(id, TestNamespaces);
                
                Assert.Equal(proposal.RequiredNamespaces, TestRequiredNamespaces);
                task1.TrySetResult(true);
            });

            await Task.WhenAll(
                task1.Task,
                sessionApproval,
                _wallet.Pair(uriString)
            );
        }
        
        [Fact, Trait("Category", "unit")]
        public async void TestShouldRejectSessionProposal()
        {
            var rejectionError = Error.FromErrorType(ErrorType.USER_DISCONNECTED);

            TaskCompletionSource<bool> task1 = new TaskCompletionSource<bool>();
            _wallet.On<SessionProposalEvent>(EngineEvents.SessionProposal, async (sender, @event) =>
            {
                var proposal = @event.EventData.Proposal;
                var id = @event.EventData.Id;
                Assert.Equal(TestRequiredNamespaces, proposal.RequiredNamespaces);
                await _wallet.RejectSession(id, rejectionError);
                task1.TrySetResult(true);
            });

            async Task CheckSessionReject()
            {
                try
                {
                    await sessionApproval;
                }
                catch (WalletConnectException e)
                {
                    Assert.Equal(rejectionError.Code, e.Code);
                    Assert.Equal(rejectionError.Message, e.Message);
                    return;
                }
                Assert.Fail("Session approval task did not throw exception, expected rejection");
            }
            
            await Task.WhenAll(
                task1.Task,
                CheckSessionReject(),
                _wallet.Pair(uriString)
            );
        }

        [Fact, Trait("Category", "unit")]
        public async void TestUpdateSession()
        {
            TaskCompletionSource<bool> task1 = new TaskCompletionSource<bool>();
            _wallet.On<SessionProposalEvent>(EngineEvents.SessionProposal, async (sender, @event) =>
            {
                var id = @event.EventData.Id;
                var proposal = @event.EventData.Proposal;
                var verifyContext = @event.EventData.VerifiedContext;
                
                Assert.Equal(Validation.Unknown, verifyContext.Validation);
                session = await _wallet.ApproveSession(id, TestNamespaces);
                
                Assert.Equal(proposal.RequiredNamespaces, TestRequiredNamespaces);
                task1.TrySetResult(true);
            });

            await Task.WhenAll(
                task1.Task,
                sessionApproval,
                _wallet.Pair(uriString)
            );
            
            Assert.NotEqual(TestNamespaces, TestUpdatedNamespaces);

            TaskCompletionSource<bool> task2 = new TaskCompletionSource<bool>();
            _dapp.On<SessionUpdateEvent>(EngineEvents.SessionUpdate, (sender, @event) =>
            {
                var param = @event.EventData.Params;
                Assert.Equal(TestUpdatedNamespaces, param.Namespaces);
                task2.TrySetResult(true);
            });

            await Task.WhenAll(
                task2.Task,
                _wallet.UpdateSession(session.Topic, TestUpdatedNamespaces)
            );
        }

        [Fact, Trait("Category", "unit")]
        public async void TestExtendSession()
        {
            TaskCompletionSource<bool> task1 = new TaskCompletionSource<bool>();
            _wallet.On<SessionProposalEvent>(EngineEvents.SessionProposal, async (sender, @event) =>
            {
                var id = @event.EventData.Id;
                var proposal = @event.EventData.Proposal;
                var verifyContext = @event.EventData.VerifiedContext;
                
                Assert.Equal(Validation.Unknown, verifyContext.Validation);
                session = await _wallet.ApproveSession(id, TestNamespaces);
                
                Assert.Equal(proposal.RequiredNamespaces, TestRequiredNamespaces);
                task1.TrySetResult(true);
            });

            await Task.WhenAll(
                task1.Task,
                sessionApproval,
                _wallet.Pair(uriString)
            );

            var prevExpiry = session.Expiry;
            var topic = session.Topic;
            
            // TODO Figure out if we need fake timers?
            await Task.Delay(5000);
            
            await _wallet.ExtendSession(topic);

            var updatedExpiry = _wallet.Engine.SignClient.Session.Get(topic).Expiry;
            
            Assert.True(updatedExpiry > prevExpiry);
        }

        [Fact, Trait("Category", "unit")]
        public async void TestRespondToSessionRequest()
        {
            TaskCompletionSource<bool> task1 = new TaskCompletionSource<bool>();
            _wallet.On<SessionProposalEvent>(EngineEvents.SessionProposal, async (sender, @event) =>
            {
                var id = @event.EventData.Id;
                var proposal = @event.EventData.Proposal;
                var verifyContext = @event.EventData.VerifiedContext;
                
                session = await _wallet.ApproveSession(id, new Namespaces()
                {
                    { 
                        "eip155", new Namespace()
                        {
                            Methods = TestNamespace.Methods,
                            Events = TestNamespace.Events,
                            Accounts = new []{ $"{TestEthereumChain}:{WalletAddress}" }
                        }
                    }
                });

                Assert.Equal(proposal.RequiredNamespaces, TestRequiredNamespaces);
                task1.TrySetResult(true);
            });

            await Task.WhenAll(
                task1.Task,
                sessionApproval,
                _wallet.Pair(uriString)
            );

            TaskCompletionSource<bool> task2 = new TaskCompletionSource<bool>();
            _wallet.Engine.SignClient.Engine.SessionRequestEvents<EthSignTransaction, string>()
                .OnRequest += async args =>
            {
                var id = args.Request.Id;
                var @params = args.Request;
                var verifyContext = args.VerifiedContext;
                var signTransaction = @params.Params[0];
                
                Assert.Equal(verifyContext.Validation, Validation.Unknown);

                var signature = await _cryptoWalletFixture.CryptoWallet.GetAccount(0).TransactionManager
                    .SignTransactionAsync(signTransaction);

                args.Response = signature;
                task2.TrySetResult(true);
            };

            async Task SendRequest()
            {
                var result = await _dapp.Request<EthSignTransaction, string>(session.Topic,
                    new EthSignTransaction()
                    {
                        new()
                        {
                            From = WalletAddress,
                            To = WalletAddress,
                            Data = "0x",
                            Nonce = new HexBigInteger("0x1"),
                            GasPrice = new HexBigInteger("0x020a7ac094"),
                            Gas = new HexBigInteger("0x5208"),
                            Value = new HexBigInteger("0x00")
                        }
                    }, TestEthereumChain);
                
                Assert.False(string.IsNullOrWhiteSpace(result));
            }

            await Task.WhenAll(
                task2.Task,
                SendRequest()
            );
        }

        [Fact, Trait("Category", "unit")]
        public async void TestWalletDisconnectFromSession()
        {
            TaskCompletionSource<bool> task1 = new TaskCompletionSource<bool>();
            _wallet.On<SessionProposalEvent>(EngineEvents.SessionProposal, async (sender, @event) =>
            {
                var id = @event.EventData.Id;
                var proposal = @event.EventData.Proposal;
                var verifyContext = @event.EventData.VerifiedContext;
                
                session = await _wallet.ApproveSession(id, new Namespaces()
                {
                    { 
                        "eip155", new Namespace()
                        {
                            Methods = TestNamespace.Methods,
                            Events = TestNamespace.Events,
                            Accounts = new []{ $"{TestEthereumChain}:{WalletAddress}" }
                        }
                    }
                });

                Assert.Equal(proposal.RequiredNamespaces, TestRequiredNamespaces);
                task1.TrySetResult(true);
            });

            await Task.WhenAll(
                task1.Task,
                sessionApproval,
                _wallet.Pair(uriString)
            );

            var reason = Error.FromErrorType(ErrorType.USER_DISCONNECTED);

            TaskCompletionSource<bool> task2 = new TaskCompletionSource<bool>();
            _dapp.On<SessionEvent>(EngineEvents.SessionDelete, (sender, @event) =>
            {
                Assert.Equal(session.Topic, @event.EventData.Topic);
                task2.TrySetResult(true);
            });

            await Task.WhenAll(
                task2.Task,
                _wallet.DisconnectSession(session.Topic, reason)
            );
        }
        
        [Fact, Trait("Category", "unit")]
        public async void TestDappDisconnectFromSession()
        {
            TaskCompletionSource<bool> task1 = new TaskCompletionSource<bool>();
            _wallet.On<SessionProposalEvent>(EngineEvents.SessionProposal, async (sender, @event) =>
            {
                var id = @event.EventData.Id;
                var proposal = @event.EventData.Proposal;
                var verifyContext = @event.EventData.VerifiedContext;
                
                session = await _wallet.ApproveSession(id, new Namespaces()
                {
                    { 
                        "eip155", new Namespace()
                        {
                            Methods = TestNamespace.Methods,
                            Events = TestNamespace.Events,
                            Accounts = new []{ $"{TestEthereumChain}:{WalletAddress}" }
                        }
                    }
                });

                Assert.Equal(proposal.RequiredNamespaces, TestRequiredNamespaces);
                task1.TrySetResult(true);
            });

            await Task.WhenAll(
                task1.Task,
                sessionApproval,
                _wallet.Pair(uriString)
            );

            var reason = Error.FromErrorType(ErrorType.USER_DISCONNECTED);

            TaskCompletionSource<bool> task2 = new TaskCompletionSource<bool>();
            _wallet.On<SessionEvent>(EngineEvents.SessionDelete, (sender, @event) =>
            {
                Assert.Equal(session.Topic, @event.EventData.Topic);
                task2.TrySetResult(true);
            });

            await Task.WhenAll(
                task2.Task,
                _dapp.Disconnect(session.Topic, reason)
            );
        }

        [Fact, Trait("Category", "unit")]
        public async void TestEmitSessionEvent()
        {
            TaskCompletionSource<bool> task1 = new TaskCompletionSource<bool>();
            _wallet.On<SessionProposalEvent>(EngineEvents.SessionProposal, async (sender, @event) =>
            {
                var id = @event.EventData.Id;
                var proposal = @event.EventData.Proposal;
                var verifyContext = @event.EventData.VerifiedContext;
                
                session = await _wallet.ApproveSession(id, new Namespaces()
                {
                    { 
                        "eip155", new Namespace()
                        {
                            Methods = TestNamespace.Methods,
                            Events = TestNamespace.Events,
                            Accounts = new []{ $"{TestEthereumChain}:{WalletAddress}" }
                        }
                    }
                });

                Assert.Equal(proposal.RequiredNamespaces, TestRequiredNamespaces);
                task1.TrySetResult(true);
            });

            await Task.WhenAll(
                task1.Task,
                sessionApproval,
                _wallet.Pair(uriString)
            );

            var sentData = new EventData<ChainChangedEvent>()
            {
                Name = "chainChanged",
                Data = new ChainChangedEvent()
                {
                    Test = "123"
                }
            };

            TaskCompletionSource<bool> task2 = new TaskCompletionSource<bool>();
            _dapp.HandleEventMessageType<ChainChangedEvent>(async (s, request) =>
            {
                var eventData = request.Params.Event;
                var topic = request.Params.Topic;
                Assert.Equal(session.Topic, topic);
                Assert.Equal(sentData, eventData);
                task2.TrySetResult(true);
            }, null);

            await Task.WhenAll(
                task2.Task,
                _wallet.EmitSessionEvent(session.Topic, sentData, TestRequiredNamespaces["eip155"].Chains[0])
            );
        }

        [Fact, Trait("Category", "unit")]
        public async void TestGetActiveSessions()
        {
            TaskCompletionSource<bool> task1 = new TaskCompletionSource<bool>();
            _wallet.On<SessionProposalEvent>(EngineEvents.SessionProposal, async (sender, @event) =>
            {
                var id = @event.EventData.Id;
                var proposal = @event.EventData.Proposal;
                var verifyContext = @event.EventData.VerifiedContext;
                
                session = await _wallet.ApproveSession(id, new Namespaces()
                {
                    { 
                        "eip155", new Namespace()
                        {
                            Methods = TestNamespace.Methods,
                            Events = TestNamespace.Events,
                            Accounts = new []{ $"{TestEthereumChain}:{WalletAddress}" }
                        }
                    }
                });

                Assert.Equal(proposal.RequiredNamespaces, TestRequiredNamespaces);
                task1.TrySetResult(true);
            });

            await Task.WhenAll(
                task1.Task,
                sessionApproval,
                _wallet.Pair(uriString)
            );

            var sessions = _wallet.ActiveSessions;
            Assert.NotNull(sessions);
            Assert.Single(sessions);
            Assert.Equal(session.Topic, sessions.Values.ToArray()[0].Topic);
        }

        public async void TestGetPendingSessionProposals()
        {
            TaskCompletionSource<bool> task1 = new TaskCompletionSource<bool>();
            _wallet.On<SessionProposalEvent>(EngineEvents.SessionProposal, async (sender, @event) =>
            {
                var proposals = _wallet.PendingSessionProposals;
                Assert.NotNull(proposals);
                Assert.Single(proposals);
                Assert.Equal(TestRequiredNamespaces, proposals.Values.ToArray()[0].RequiredNamespaces);
                task1.TrySetResult(true);
            });

            await Task.WhenAll(
                task1.Task,
                _wallet.Pair(uriString)
            );
        }

        public async void TestGetPendingSessionRequests()
        {
            TaskCompletionSource<bool> task1 = new TaskCompletionSource<bool>();
            _wallet.On<SessionProposalEvent>(EngineEvents.SessionProposal, async (sender, @event) =>
            {
                var id = @event.EventData.Id;
                var proposal = @event.EventData.Proposal;
                var verifyContext = @event.EventData.VerifiedContext;
                
                session = await _wallet.ApproveSession(id, new Namespaces()
                {
                    { 
                        "eip155", new Namespace()
                        {
                            Methods = TestNamespace.Methods,
                            Events = TestNamespace.Events,
                            Accounts = new []{ $"{TestEthereumChain}:{WalletAddress}" }
                        }
                    }
                });

                Assert.Equal(proposal.RequiredNamespaces, TestRequiredNamespaces);
                task1.TrySetResult(true);
            });

            await Task.WhenAll(
                task1.Task,
                sessionApproval,
                _wallet.Pair(uriString)
            );

            var requestParams = new EthSignTransaction()
            {
                new()
                {
                    From = WalletAddress,
                    To = WalletAddress,
                    Data = "0x",
                    Nonce = new HexBigInteger("0x1"),
                    GasPrice = new HexBigInteger("0x020a7ac094"),
                    Gas = new HexBigInteger("0x5208"),
                    Value = new HexBigInteger("0x00")
                }
            };
            
            TaskCompletionSource<bool> task2 = new TaskCompletionSource<bool>();
            _wallet.Engine.SignClient.Engine.SessionRequestEvents<EthSignTransaction, string>()
                .OnRequest += async args =>
            {
                // Get the pending session request, since that's what we're testing
                var pendingRequests = _wallet.PendingSessionRequests;
                var request = pendingRequests[0];
                
                var id = request.Id;
                var verifyContext = args.VerifiedContext;
                
                // Perform unsafe cast, all pending requests are stored as object type
                var signTransaction = ((EthSignTransaction)request.Parameters.Request.Params)[0];

                Assert.Equal(args.Request.Id, id);
                Assert.Equal(verifyContext.Validation, Validation.Unknown);

                var signature = await _cryptoWalletFixture.CryptoWallet.GetAccount(0).TransactionManager
                    .SignTransactionAsync(signTransaction);

                args.Response = signature;
                task2.TrySetResult(true);
            };

            async Task SendRequest()
            {
                var result = await _dapp.Request<EthSignTransaction, string>(session.Topic, 
                    requestParams, TestEthereumChain);
                
                Assert.False(string.IsNullOrWhiteSpace(result));
            }

            await Task.WhenAll(
                task2.Task,
                SendRequest()
            );
        }
    }
}
