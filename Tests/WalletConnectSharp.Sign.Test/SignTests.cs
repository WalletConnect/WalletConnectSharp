using System;
using System.Threading.Tasks;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;
using Xunit;

namespace WalletConnectSharp.Sign.Test
{
    public class SignTests : IClassFixture<TwoClientsFixture>
    {
        private TwoClientsFixture _cryptoFixture;

        [RpcMethod("test_method"), RpcResponseOptions(Clock.ONE_MINUTE, false, 99999)]
        public class TestRequest
        {
            public int a;
            public int b;
        }

        public class TestResponse
        {
            public int result;
        }

        public WalletConnectSignClient ClientA
        {
            get
            {
                return _cryptoFixture.ClientA;
            }
        }
        
        public WalletConnectSignClient ClientB
        {
            get
            {
                return _cryptoFixture.ClientB;
            }
        }

        public SignTests(TwoClientsFixture cryptoFixture)
        {
            this._cryptoFixture = cryptoFixture;
        }

        [Fact]
        public async void TestApproveSession()
        {
            await _cryptoFixture.WaitForClientsReady();
            
            var testAddress = "0xd8dA6BF26964aF9D7eEd9e03E53415D37aA96045";
            var dappConnectOptions = new ConnectParams()
            {
                RequiredNamespaces = new RequiredNamespaces()
                {
                    {
                        "eip155", new RequiredNamespace()
                        {
                            Methods = new[]
                            {
                                "eth_sendTransaction",
                                "eth_signTransaction",
                                "eth_sign",
                                "personal_sign",
                                "eth_signTypedData",
                            },
                            Chains = new[]
                            {
                                "eip155:1"
                            },
                            Events = new[]
                            {
                                "chainChanged", "accountsChanged"
                            }
                        }
                    }
                }
            };

            var dappClient = ClientA;
            var connectData = await dappClient.Connect(dappConnectOptions);

            var walletClient = ClientB;
            var pairing = await walletClient.Pair(new PairParams()
            {
                Uri = connectData.Uri
            });

            var proposal = await pairing.FetchProposal;

            var approveData = await walletClient.Approve(proposal, testAddress);

            var sessionData = await connectData.Approval;
            await approveData.Acknowledged();
        }
        
        [Fact]
        public async void TestRejectSession()
        {
            await _cryptoFixture.WaitForClientsReady();
            
            var testAddress = "0xd8dA6BF26964aF9D7eEd9e03E53415D37aA96045";
            var dappConnectOptions = new ConnectParams()
            {
                RequiredNamespaces = new RequiredNamespaces()
                {
                    {
                        "eip155", new RequiredNamespace()
                        {
                            Methods = new[]
                            {
                                "eth_sendTransaction",
                                "eth_signTransaction",
                                "eth_sign",
                                "personal_sign",
                                "eth_signTypedData",
                            },
                            Chains = new[]
                            {
                                "eip155:1"
                            },
                            Events = new[]
                            {
                                "chainChanged", "accountsChanged"
                            }
                        }
                    }
                }
            };

            var dappClient = ClientA;
            var connectData = await dappClient.Connect(dappConnectOptions);

            var walletClient = ClientB;
            var pairing = await walletClient.Pair(new PairParams()
            {
                Uri = connectData.Uri
            });

            var proposal = await pairing.FetchProposal;

            await walletClient.Reject(proposal);

            await Assert.ThrowsAsync<WalletConnectException>(() => connectData.Approval);
        }
        
        [Fact]
        public async void TestSessionRequestResponse()
        {
            await _cryptoFixture.WaitForClientsReady();
            
            var testAddress = "0xd8dA6BF26964aF9D7eEd9e03E53415D37aA96045";
            var testMethod = "test_method";
            
            var dappConnectOptions = new ConnectParams()
            {
                RequiredNamespaces = new RequiredNamespaces()
                {
                    {
                        "eip155", new RequiredNamespace()
                        {
                            Methods = new[]
                            {
                                testMethod
                            },
                            Chains = new[]
                            {
                                "eip155:1"
                            },
                            Events = new[]
                            {
                                "chainChanged", "accountsChanged"
                            }
                        }
                    }
                }
            };

            var dappClient = ClientA;
            var connectData = await dappClient.Connect(dappConnectOptions);

            var walletClient = ClientB;
            var pairing = await walletClient.Pair(new PairParams()
            {
                Uri = connectData.Uri
            });

            var proposal = await pairing.FetchProposal;

            var approveData = await walletClient.Approve(proposal, testAddress);

            var sessionData = await connectData.Approval;
            await approveData.Acknowledged();

            var rnd = new Random();
            var a = rnd.Next(100);
            var b = rnd.Next(100);

            var testData = new TestRequest()
            {
                a = a,
                b = b,
            };

            var pending = new TaskCompletionSource<int>();
            
            // Step 1. Setup event listener for request
            
            // The wallet client will listen for the request with the "test_method" rpc method
            walletClient.Engine.SessionRequestEvents<TestRequest, TestResponse>()
                    .OnRequest += ( requestData) =>
                {
                    var request = requestData.Request;
                    var data = request.Params;

                    requestData.Response = new TestResponse()
                    {
                        result = data.a * data.b
                    };
                    
                    return Task.CompletedTask;
                };

            // The dapp client will listen for the response
            // Normally, we wouldn't do this and just rely on the return value
            // from the dappClient.Engine.Request function call (the response Result or throws an Exception)
            // We do it here for the sake of testing
            dappClient.Engine.SessionRequestEvents<TestRequest, TestResponse>()
                .FilterResponses((r) => r.Topic == sessionData.Topic)
                .OnResponse += (responseData) =>
            {
                var response = responseData.Response;
                
                var data = response.Result;

                pending.SetResult(data.result);

                return Task.CompletedTask;
            };
            
            // 2. Send the request from the dapp client
            var responseReturned = await dappClient.Engine.Request<TestRequest, TestResponse>(sessionData.Topic, testData);
            
            // 3. Wait for the response from the event listener
            var eventResult = await pending.Task.WithTimeout(TimeSpan.FromSeconds(5));
            
            Assert.Equal(eventResult, a * b);
            Assert.Equal(eventResult, testData.a * testData.b);
            Assert.Equal(eventResult, responseReturned.result);
        }
    }
}