namespace Deeplinker;

public interface IDeeplinkable
{
    bool SupportsChain(string chainId);

    string TransformUri(string universalUri);
}
