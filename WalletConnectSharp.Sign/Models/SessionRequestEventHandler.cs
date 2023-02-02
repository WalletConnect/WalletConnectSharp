using System;
using System.Threading.Tasks;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models.Engine.Methods;

namespace WalletConnectSharp.Sign.Models
{
    /// <summary>
    /// A sub-class of <see cref="TypedEventHandler{T,TR}"/> that fixes complex nesting issue with
    /// SessionRequest<T>. The purpose of this class is to un-nest the SessionRequest<T> object
    /// </summary>
    /// <typeparam name="T">The type of the session request</typeparam>
    /// <typeparam name="TR">The type of the response for the session request</typeparam>
    public class SessionRequestEventHandler<T, TR> : TypedEventHandler<T, TR>
    { 
        /// <summary>
        /// Get a singleton instance of this class for the given <see cref="IEngine"/> context. The context
        /// string of the given <see cref="IEngine"/> will be used to determine the singleton instance to
        /// return (or if a new one needs to be created). Beware that multiple <see cref="IEngine"/> instances
        /// with the same context string will share the same event handlers.
        /// </summary>
        /// <param name="engine">The engine this singleton instance is for, and where the context string will
        /// be read from</param>
        /// <returns>The singleton instance to use for request/response event handlers</returns>
        public static new TypedEventHandler<T, TR> GetInstance(ICore engine)
        {
            var context = engine.Context;
            
            if (_instances.ContainsKey(context)) return _instances[context];

            var _instance = new SessionRequestEventHandler<T, TR>(engine);
            
            _instances.Add(context, _instance);

            return _instance;
        }
        
        protected SessionRequestEventHandler(ICore engine) : base(engine)
        {
        }

        protected override TypedEventHandler<T, TR> BuildNew(ICore _ref, Func<RequestEventArgs<T, TR>, bool> requestPredicate, Func<ResponseEventArgs<TR>, bool> responsePredicate)
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
