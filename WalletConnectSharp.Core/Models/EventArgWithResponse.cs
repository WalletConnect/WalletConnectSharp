namespace WalletConnectSharp.Core.Models;

public class EventArgWithResponse<TArgs, TResponse> : EventArgs
{
    public TArgs EventRequest { get; }
    
    public TResponse EventResponse { get; set; }

    public EventArgWithResponse(TArgs eventRequest)
    {
        EventRequest = eventRequest;
    }
}
