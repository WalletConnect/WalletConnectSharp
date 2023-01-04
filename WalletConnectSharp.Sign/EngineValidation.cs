using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WalletConnectSharp.Common;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectSharp.Sign.Models.Engine.Methods;

namespace WalletConnectSharp.Sign
{
    public partial class Engine
    {
        
        private void IsInitialized()
        {
            if (!_initialized)
            {
                throw WalletConnectException.FromType(ErrorType.NOT_INITIALIZED, Name);
            }
        }
        
        async Task IEnginePrivate.IsValidConnect(ConnectOptions options)
        {
            if (options == null)
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID, $"Connect() params: {JsonConvert.SerializeObject(options)}");

            var pairingTopic = options.PairingTopic;
            var requiredNamespaces = options.RequiredNamespaces;
            var relays = options.Relays;

            if (pairingTopic != null)
                await IsValidPairingTopic(pairingTopic);
        }

        async Task IsValidPairingTopic(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID,
                    $"pairing topic should be a string {topic}");

            if (!this.Client.Pairing.Keys.Contains(topic))
                throw WalletConnectException.FromType(ErrorType.NO_MATCHING_KEY,
                    $"pairing topic doesn't exist {topic}");

            if (Clock.IsExpired(this.Client.Pairing.Get(topic).Expiry.Value))
            {
                await PrivateThis.DeletePairing(topic);
                throw WalletConnectException.FromType(ErrorType.EXPIRED, $"pairing topic: {topic}");
            }
        }

        async Task IsValidSessionTopic(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID,
                    $"session topic should be a string {topic}");
            
            if (!this.Client.Session.Keys.Contains(topic))
                throw WalletConnectException.FromType(ErrorType.NO_MATCHING_KEY,
                    $"session topic doesn't exist {topic}");
            
            if (Clock.IsExpired(this.Client.Session.Get(topic).Expiry.Value))
            {
                await PrivateThis.DeleteSession(topic);
                throw WalletConnectException.FromType(ErrorType.EXPIRED, $"session topic: {topic}");
            }
        }

        async Task IsValidProposalId(long id)
        {
            if (!this.Client.Proposal.Keys.Contains(id))
                throw WalletConnectException.FromType(ErrorType.NO_MATCHING_KEY,
                    $"proposal id doesn't exist {id}");
            
            if (Clock.IsExpired(this.Client.Proposal.Get(id).Expiry.Value))
            {
                await PrivateThis.DeleteProposal(id);
                throw WalletConnectException.FromType(ErrorType.EXPIRED, $"proposal id: {id}");
            }
        }

        async Task IsValidSessionOrPairingTopic(string topic)
        {
            if (this.Client.Session.Keys.Contains(topic)) await this.IsValidSessionTopic(topic);
            else if (this.Client.Pairing.Keys.Contains(topic)) await this.IsValidPairingTopic(topic);
            else if (string.IsNullOrWhiteSpace(topic))
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID,
                    $"session or pairing topic should be a string {topic}");
            else
            {
                throw WalletConnectException.FromType(ErrorType.NO_MATCHING_KEY,
                    $"session or pairing topic doesn't exist {topic}");
            }
        }

        private bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            
            try
            {
                new Uri(url);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        async Task IEnginePrivate.IsValidPair(PairParams pairParams)
        {
            if (pairParams == null)
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID, $"pair() params: {pairParams}");
            }

            if (!IsValidUrl(pairParams.Uri))
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID, $"pair() uri: {pairParams.Uri}");
        }

        async Task IEnginePrivate.IsValidSessionSettleRequest(SessionSettle settle)
        {
            if (settle == null)
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID, $"onSessionSettleRequest() params: {settle}");
            }

            var relay = settle.Relay;
            var controller = settle.Controller;
            var namespaces = settle.Namespaces;
            var expiry = settle.Expiry;
            if (relay.Protocol != null && string.IsNullOrWhiteSpace(relay.Protocol))
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID,
                    $"OnSessionSettleRequest() relay protocol should be a string");
            }

            if (string.IsNullOrWhiteSpace(controller.PublicKey))
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID,
                    "OnSessionSettleRequest controller public key should be a string");
            }

            var validNamespacesError = IsValidNamespaces(namespaces, "OnSessionSettleRequest()");
            if (validNamespacesError != null)
                throw validNamespacesError.ToException();

            if (Clock.IsExpired(expiry))
                throw WalletConnectException.FromType(ErrorType.EXPIRED, "OnSessionSettleRequest()");
        }

        async Task IEnginePrivate.IsValidApprove(ApproveParams @params)
        {
            if (@params == null)
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID, $"approve() params: {@params}");
            }

            var id = @params.Id;
            var namespaces = @params.Namespaces;
            var relayProtocol = @params.RelayProtocol;
            
            await IsValidProposalId(id);
            var proposal = this.Client.Proposal.Get(id);

            var validNamespacesError = IsValidNamespaces(namespaces, "approve()");
            if (validNamespacesError != null)
                throw validNamespacesError.ToException();

            var conformingNamespacesError = IsConformingNamespaces(proposal.RequiredNamespaces, namespaces, "update()");
            if (conformingNamespacesError != null)
                throw conformingNamespacesError.ToException();

            if (relayProtocol != null)
            {
                if (string.IsNullOrWhiteSpace(relayProtocol))
                    throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID,
                        $"approve() relayProtocol: {relayProtocol}");
            }
        }

        async Task IEnginePrivate.IsValidReject(RejectParams @params)
        {
            if (@params == null)
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID, $"reject() params: {@params}");
            }

            var id = @params.Id;
            var reason = @params.Reason;

            await IsValidProposalId(id);

            if (reason == null || string.IsNullOrWhiteSpace(reason.Message))
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID,
                    $"reject() reason: ${JsonConvert.SerializeObject(reason)}");
            }
        }

        async Task IEnginePrivate.IsValidUpdate(UpdateParams @params)
        {
            if (@params == null)
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID, $"update() params: {@params}");
            }

            var topic = @params.Topic;
            var namespaces = @params.Namespaces;

            await IsValidSessionTopic(topic);

            var session = this.Client.Session.Get(topic);

            var validNamespaceError = IsValidNamespaces(namespaces, "update()");
            if (validNamespaceError != null)
                throw validNamespaceError.ToException();

            var conformingNamespacesError = IsConformingNamespaces(session.RequiredNamespaces, namespaces, "update()");

            if (conformingNamespacesError != null)
                throw conformingNamespacesError.ToException();
        }

        async Task IEnginePrivate.IsValidExtend(ExtendParams @params)
        {
            if (@params == null)
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID, $"extend() params: {@params}");
            }

            var topic = @params.Topic;

            await IsValidSessionTopic(topic);
        }

        async Task IEnginePrivate.IsValidRequest<T>(RequestParams<T> @params)
        {
            if (@params == null)
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID, $"request() params: {@params}");
            }

            var topic = @params.Topic;
            var request = @params.Request;
            var chainId = @params.ChainId;

            await IsValidSessionTopic(topic);

            var session = this.Client.Session.Get(topic);
            var namespaces = session.Namespaces;
            if (!IsValidNamespacesChainId(namespaces, chainId))
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID, $"request() chainId: {chainId}");
            }

            if (request == null || string.IsNullOrWhiteSpace(request.Method))
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID,
                    $"request() ${JsonConvert.SerializeObject(request)}");
            }

            var validMethods = GetNamespacesMethodsForChainId(namespaces, chainId);
            if (!validMethods.Contains(request.Method))
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID,
                    $"request() method: {request.Method}");
        }

        async Task IEnginePrivate.IsValidRespond<T>(RespondParams<T> @params)
        {
            if (@params == null)
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID, $"respond() params: {@params}");
            }

            var topic = @params.Topic;
            var response = @params.Response;

            await IsValidSessionTopic(topic);

            if (response == null || (response.Result == null && response.Error == null))
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID,
                    $"respond() response: ${JsonConvert.SerializeObject(response)}");
            }
        }

        async Task IEnginePrivate.IsValidPing(PingParams @params)
        {
            if (@params == null)
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID, $"ping() params: {@params}");
            }

            var topic = @params.Topic;

            await IsValidSessionOrPairingTopic(topic);
        }

        private List<string> GetNamespacesEventsForChainId(Namespaces namespaces, string chainId)
        {
            var events = new List<string>();
            foreach (var ns in namespaces.Values)
            {
                var chains = GetAccountsChains(ns.Accounts);
                if (chains.Contains(chainId)) events.AddRange(ns.Events);
                if (ns.Extension == null) continue;
                foreach (var ext in ns.Extension)
                {
                    var extChains = GetAccountsChains(ext.Accounts);
                    if (extChains.Contains(chainId)) events.AddRange(ext.Events);
                }
            }

            return events;
        }

        async Task IEnginePrivate.IsValidEmit<T>(EmitParams<T> @params)
        {
            if (@params == null)
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID, $"emit() params: {@params}");
            }

            var topic = @params.Topic;
            var @event = @params.Event;
            var chainId = @params.ChainId;

            await IsValidSessionTopic(topic);
            var session = this.Client.Session.Get(topic);
            var namespaces = session.Namespaces;

            if (!IsValidNamespacesChainId(namespaces, chainId))
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID, $"emit() chainId: {chainId}");
            }

            if (@event == null || string.IsNullOrWhiteSpace(@event.Name))
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID, $"emit() event: {JsonConvert.SerializeObject(@event)}");

            if (!GetNamespacesEventsForChainId(namespaces, chainId).Contains(@event.Name))
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID,
                    $"emit() event: {JsonConvert.SerializeObject(@event)}");
            }
        }

        async Task IEnginePrivate.IsValidDisconnect(DisconnectParams @params)
        {
            if (@params == null)
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID, $"disconnect() params: {@params}");
            }

            var topic = @params.Topic;

            await IsValidSessionOrPairingTopic(topic);
        }

        private bool IsValidAccountId(string account)
        {
            if (!string.IsNullOrWhiteSpace(account) && account.Contains(":"))
            {
                var split = account.Split(":");
                if (split.Length == 3)
                {
                    var chainId = split[0] + ":" + split[1];
                    return !string.IsNullOrWhiteSpace(split[2]) && IsValidChainId(chainId);
                }
            }
            return false;
        }

        private ErrorResponse IsValidAccounts(string[] accounts, string context)
        {
            ErrorResponse error = null;
            foreach (var account in accounts)
            {
                if (error != null)
                    break;

                if (!IsValidAccountId(account))
                {
                    error = ErrorResponse.FromErrorType(ErrorType.UNSUPPORTED_ACCOUNTS, $"{context}, account {account} should be a string and conform to 'namespace:chainId:address' format");
                }
            }

            return error;
        }

        private ErrorResponse IsValidExtension(Namespace ns, string method)
        {
            if (ns.Extension != null)
            {
                if (ns.Extension.Length == 0)
                    return ErrorResponse.FromErrorType(ErrorType.MISSING_OR_INVALID, $"{method} extension should be an array of namespaces, or omitted");
            }

            return null;
        }

        private ErrorResponse IsValidNamespaceAccounts(Namespaces namespaces, string method)
        {
            ErrorResponse error = null;
            foreach (var ns in namespaces.Values)
            {
                if (error != null) break;

                var validAccountsError = IsValidAccounts(ns.Accounts, $"{method} namespace");
                var validExtensionError = IsValidExtension(ns, method);
                if (validAccountsError != null)
                {
                    error = validAccountsError;
                }
                else if (validExtensionError != null)
                {
                    error = validAccountsError;
                } 
                else if (ns.Extension != null)
                {
                    foreach (var ext in ns.Extension)
                    {
                        if (error != null)
                            break;

                        var extValidAccountsError = IsValidAccounts(ext.Accounts, $"{method} extension");
                        if (extValidAccountsError != null)
                        {
                            error = extValidAccountsError;
                        }
                    }
                }
            }

            return error;
        }

        private ErrorResponse IsValidNamespaces(Namespaces namespaces, string method)
        {
            ErrorResponse error = null;
            if (namespaces != null)
            {
                var validAccountsError = IsValidNamespaceAccounts(namespaces, method);
                if (validAccountsError != null)
                {
                    error = validAccountsError;
                }
            }
            else
            {
                error = ErrorResponse.FromErrorType(ErrorType.MISSING_OR_INVALID, $"{method}, namespaces should be an object with data");
            }

            return error;
        }

        private List<string> GetNamespacesMethodsForChainId(Namespaces namespaces, string chainId)
        {
            var methods = new List<string>();
            foreach (var ns in namespaces.Values)
            {
                var chains = GetAccountsChains(ns.Accounts);
                if (chains.Contains(chainId)) methods.AddRange(ns.Methods);
                if (ns.Extension == null) continue;
                foreach (var ext in ns.Extension)
                {
                    var extChains = GetAccountsChains(ext.Accounts);
                    if (extChains.Contains(chainId)) methods.AddRange(ext.Methods);
                }
            }

            return methods;
        }

        private bool IsValidChainId(string chainId)
        {
            if (chainId.Contains(":"))
            {
                var split = chainId.Split(":");
                return split.Length == 2;
            }

            return false;
        }

        private List<string> GetNamespacesChains(Namespaces namespaces)
        {
            List<string> chains = new List<string>();
            foreach (var ns in namespaces.Values)
            {
                chains.AddRange(GetAccountsChains(ns.Accounts));
                if (ns.Extension == null) continue;
                foreach (var ext in ns.Extension)
                {
                    chains.AddRange(GetAccountsChains(ext.Accounts));
                }
            }

            return chains;
        }

        private bool IsValidNamespacesChainId(Namespaces namespaces, string chainId)
        {
            if (!IsValidChainId(chainId)) return false;
            var chains = GetNamespacesChains(namespaces);
            if (!chains.Contains(chainId)) return false;

            return true;
        }

        private ErrorResponse IsConformingNamespaces(RequiredNamespaces requiredNamespaces, Namespaces namespaces,
            string context)
        {
            ErrorResponse error = null;
            var requiredNamespaceKeys = requiredNamespaces.Keys.ToArray();
            var namespaceKeys = namespaces.Keys.ToArray();
            
            if (!HasOverlap(requiredNamespaceKeys, namespaceKeys))
                error = ErrorResponse.FromErrorType(ErrorType.NON_CONFORMING_NAMESPACES, $"{context} namespaces keys don't satisfy requiredNamespaces");
            else
            {
                foreach (var key in requiredNamespaceKeys)
                {
                    if (error != null)
                        break;

                    var requiredNamespaceChains = requiredNamespaces[key].Chains;
                    var namespaceChains = GetAccountsChains(namespaces[key].Accounts);

                    if (!HasOverlap(requiredNamespaceChains, namespaceChains))
                    {
                        error = ErrorResponse.FromErrorType(ErrorType.NON_CONFORMING_NAMESPACES, $"{context} namespaces accounts don't satisfy requiredNamespaces chains for {key}");
                    } 
                    else if (!HasOverlap(requiredNamespaces[key].Methods, namespaces[key].Methods))
                    {
                        error = ErrorResponse.FromErrorType(ErrorType.NON_CONFORMING_NAMESPACES, $"{context} namespaces methods don't satisfy requiredNamespaces methods for {key}");
                    }
                    else if (!HasOverlap(requiredNamespaces[key].Events, namespaces[key].Events))
                    {
                        error = ErrorResponse.FromErrorType(ErrorType.NON_CONFORMING_NAMESPACES, $"{context} namespaces events don't satisfy requiredNamespaces events for {key}");
                    }
                    else if (requiredNamespaces[key].Extension != null &&
                             requiredNamespaces[key].Extension.Length > 0 && namespaces[key].Extension == null)
                    {
                        error = ErrorResponse.FromErrorType(ErrorType.NON_CONFORMING_NAMESPACES, $"{context} namespaces extension doesn't satisfy requiredNamespaces extension for {key}");
                    }
                    else if (requiredNamespaces[key].Extension != null &&
                             requiredNamespaces[key].Extension.Length > 0 && namespaces[key].Extension != null &&
                             namespaces[key].Extension.Length > 0)
                    {
                        foreach (var ext in requiredNamespaces[key].Extension)
                        {
                            var extMethods = ext.Methods;
                            var extEvents = ext.Events;
                            var extChains = ext.Chains;

                            if (error != null)
                                break;

                            var isOverlap = namespaces[key].Extension.Any(ns =>
                            {
                                var accChains = GetAccountsChains(ns.Accounts);
                                return HasOverlap(extChains, accChains) &&
                                       HasOverlap(extEvents, ns.Events) &&
                                       HasOverlap(extMethods, ns.Methods);
                            });

                            if (!isOverlap)
                            {
                                error = ErrorResponse.FromErrorType(ErrorType.NON_CONFORMING_NAMESPACES, $"{context} namespaces extension doesn't satisfy requiredNamespaces extension for {key}");
                            }
                        }
                    }
                }
            }

            return error;
        }
        
        private bool HasOverlap(string[] a, string[] b)
        {
            var matches = a.Where(x => b.Contains(x));
            return matches.Count() == a.Length;
        }

        private string[] GetAccountsChains(string[] accounts)
        {
            List<string> chains = new List<string>();
            foreach (var account in accounts)
            {
                var values = account.Split(":");
                var chain = values[0];
                var chainId = values[1];
                
                chains.Add($"{chain}:{chainId}");
            }

            return chains.ToArray();
        }

        private bool IsSessionCompatible(SessionStruct session, FindParams @params)
        {
            var requiredNamespaces = @params.RequiredNamespaces;
            var compatible = true;

            var sessionKeys = session.Namespaces.Keys.ToArray();
            var paramsKeys = requiredNamespaces.Keys.ToArray();

            if (!HasOverlap(paramsKeys, sessionKeys)) return false;

            foreach (var key in sessionKeys)
            {
                var value = session.Namespaces[key];
                var accounts = value.Accounts;
                var methods = value.Methods;
                var events = value.Events;
                var extension = value.Extension;
                var chains = GetAccountsChains(accounts);
                var requiredNamespace = requiredNamespaces[key];

                if (!HasOverlap(requiredNamespace.Chains, chains) ||
                    !HasOverlap(requiredNamespace.Methods, methods) ||
                    !HasOverlap(requiredNamespace.Events, events))
                    compatible = false;

                if (compatible && extension != null && extension.Length > 0)
                {
                    foreach (var extensionNamespace in extension)
                    {
                        var extAccounts = extensionNamespace.Accounts;
                        var extMethods = extensionNamespace.Methods;
                        var extEvents = extensionNamespace.Events;
                        var extChains = GetAccountsChains(extAccounts);
                        var overlap = false;

                        if (requiredNamespace.Extension != null)
                            overlap = requiredNamespace.Extension.Any(re =>
                                HasOverlap(re.Chains, extChains) && HasOverlap(re.Methods, extMethods) &&
                                HasOverlap(re.Events, extEvents));

                        if (!overlap) compatible = false;
                    }
                }
            }

            return compatible;
        }
    }
}