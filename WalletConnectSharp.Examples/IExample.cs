using System.Threading.Tasks;

namespace WalletConnectSharp.Examples
{
    public interface IExample
    {
        string Name { get; }

        Task Execute(string[] args);
    }
}