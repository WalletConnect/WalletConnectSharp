namespace Deeplinker;

public interface IDeeplinkable
{
    int Priority { get; }
    
    bool SupportsChain(string chainId);

    string TransformUri(string wcUri);
}
