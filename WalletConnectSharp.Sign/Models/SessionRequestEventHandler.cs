using System;
using System.Threading.Tasks;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models.Engine.Methods;

namespace WalletConnectSharp.Sign.Models
{
    public class SessionRequestEventHandler<T, TR> : TypedEventHandler<T, TR>
    { 
        public new static TypedEventHandler<T, TR> GetInstance(IEngine engine)
        {
            var context = engine.Client.Context;
            
            if (_instances.ContainsKey(context)) return _instances[context];

            var _instance = new SessionRequestEventHandler<T, TR>(engine);
            
            _instances.Add(context, _instance);

            return _instance;
        }
        
        protected SessionRequestEventHandler(IEngine engine) : base(engine)
        {
        }

        protected override TypedEventHandler<T, TR> BuildNew(IEngine _ref, Func<RequestEventArgs<T, TR>, bool> requestPredicate, Func<ResponseEventArgs<TR>, bool> responsePredicate)
        {
            return new SessionRequestEventHandler<T, TR>(_ref)
            {
                requestPredicate = requestPredicate,
                responsePredicate = responsePredicate
            };
        }

        protected override void Setup()
        {
            var wrappedRef = TypedEventHandler<SessionRequest<T>, TR>.GetInstance(_ref);
            
            wrappedRef.OnRequest += WrappedRefOnOnRequest;
            wrappedRef.OnResponse += WrappedRefOnOnResponse;
        }

        private Task WrappedRefOnOnResponse(ResponseEventArgs<TR> e)
        {
            return base.ResponseCallback(e.Topic, e.Response);
        }

        private Task WrappedRefOnOnRequest(RequestEventArgs<SessionRequest<T>, TR> e)
        {
            //Set inner request id to match outer request id
            e.Request.Params.Request.Id = e.Request.Id;
            
            return base.RequestCallback(e.Topic, e.Request.Params.Request);
        }
    }
}