using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models.Engine;

namespace WalletConnectSharp.Sign.Models
{
    public class TypedEventHandler<T, TR>
    {
        protected static Dictionary<string, TypedEventHandler<T, TR>> _instances = new Dictionary<string,TypedEventHandler<T,TR>>();

        protected Func<RequestEventArgs<T, TR>, bool> requestPredicate;
        protected Func<ResponseEventArgs<TR>, bool> responsePredicate;
        protected IEngine _ref;

        public static TypedEventHandler<T, TR> GetInstance(IEngine engine)
        {
            var context = engine.Client.Context;
            
            if (_instances.ContainsKey(context)) return _instances[context];

            var _instance = new TypedEventHandler<T, TR>(engine);
            
            _instances.Add(context, _instance);

            return _instance;
        }
            
        public delegate Task RequestMethod<TRequestArgs, TResponseArgs>(RequestEventArgs<TRequestArgs, TResponseArgs> e);
        public delegate Task ResponseMethod<TResponseArgs>(ResponseEventArgs<TResponseArgs> e);

        private event RequestMethod<T, TR> _onRequest;
        private event ResponseMethod<TR> _onResponse;
        private object _eventLock = new object();
        private int _activeCount;
        
        
        public event RequestMethod<T, TR> OnRequest
        {
            add
            {
                lock (_eventLock)
                {
                    _onRequest += value;

                    if (_activeCount == 0)
                    {
                        Setup();
                    }

                    _activeCount++;
                }
            }
            remove
            {
                lock (_eventLock)
                {
                    _onRequest -= value;

                    _activeCount--;

                    if (_activeCount == 0)
                    {
                        Teardown();
                    }
                }
            }
        }

        public event ResponseMethod<TR> OnResponse
        {
            add
            {
                lock (_eventLock)
                {
                    _onResponse += value;

                    if (_activeCount == 0)
                    {
                        Setup();
                    }

                    _activeCount++;
                }
            }
            remove
            {
                lock (_eventLock)
                {
                    _onResponse -= value;

                    _activeCount--;

                    if (_activeCount == 0)
                    {
                        Teardown();
                    }
                }
            }
        }

        protected TypedEventHandler(IEngine engine)
        {
            _ref = engine;
        }
        
        public virtual TypedEventHandler<T, TR> FilterRequests(Func<RequestEventArgs<T, TR>, bool> predicate)
        {
            var finalPredicate = predicate;
            if (this.requestPredicate != null)
                finalPredicate = (rea) => this.requestPredicate(rea) && predicate(rea);

            return BuildNew(_ref, finalPredicate, responsePredicate);
        }

        public virtual TypedEventHandler<T, TR> FilterResponses(Func<ResponseEventArgs<TR>, bool> predicate)
        {
            var finalPredicate = predicate;
            if (this.responsePredicate != null)
                finalPredicate = (rea) => this.responsePredicate(rea) && predicate(rea);

            return BuildNew(_ref, requestPredicate, finalPredicate);
        }

        protected virtual TypedEventHandler<T, TR> BuildNew(IEngine _ref, Func<RequestEventArgs<T, TR>, bool> requestPredicate,
            Func<ResponseEventArgs<TR>, bool> responsePredicate)
        {
            return new TypedEventHandler<T, TR>(_ref)
            {
                requestPredicate = requestPredicate,
                responsePredicate = responsePredicate
            };
        }

        protected virtual void Setup()
        {
            _ref.HandleMessageType<T, TR>(RequestCallback, ResponseCallback);
        }

        protected virtual void Teardown()
        {
            // TODO Unsubscribe from HandleMessageType<T, TR> from above
        }

        protected virtual Task ResponseCallback(string arg1, JsonRpcResponse<TR> arg2)
        {
            var rea = new ResponseEventArgs<TR>(arg2, arg1);
            return responsePredicate != null && !responsePredicate(rea) ? Task.CompletedTask :
                _onResponse != null ? _onResponse(rea) : Task.CompletedTask;
        }

        protected virtual async Task RequestCallback(string arg1, JsonRpcRequest<T> arg2)
        {
            var rea = new RequestEventArgs<T, TR>(arg1, arg2);

            if (requestPredicate != null && !requestPredicate(rea)) return;
            if (_onRequest == null) return;

            await _onRequest(rea);

            if (rea.Response != null || rea.Error != null)
            {
                await _ref.Respond<T, TR>(new RespondParams<TR>()
                {
                    Response = new JsonRpcResponse<TR>()
                    {
                        Error = rea.Error,
                        Id = arg2.Id,
                        Result = rea.Response
                    },
                    Topic = arg1
                });
            }
        }
    }
}