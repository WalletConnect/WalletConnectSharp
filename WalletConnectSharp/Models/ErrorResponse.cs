namespace WalletConnectSharp.Models
{
    public class ErrorResponse
    {
        public string message;

        public ErrorResponse(string message)
        {
            this.message = message;
        }
    }
}