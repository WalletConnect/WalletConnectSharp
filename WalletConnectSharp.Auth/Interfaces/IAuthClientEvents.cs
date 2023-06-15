using WalletConnectSharp.Auth.Models;

namespace WalletConnectSharp.Auth.Interfaces;

public interface IAuthClientEvents
{
    event EventHandler<AuthRequest> AuthRequested;
    event EventHandler<AuthResponse> AuthResponded;
    event EventHandler<AuthErrorResponse> AuthError;
}
