﻿using System.Net;
using Newtonsoft.Json;
using WalletConnectSharp.Common.Utils;

namespace WalletConnectSharp.Core.Models.Verify;

public class Verifier
{
    public const string VerifyServer = "https://verify.walletconnect.com";
    
    public CancellationTokenSource CancellationTokenSource { get; }

    public Verifier()
    {
        this.CancellationTokenSource = new CancellationTokenSource(Clock.AsTimeSpan(Clock.FIVE_SECONDS));
    }

    public async Task<string> Resolve(string attestationId)
    {
        try
        {
            using HttpClient client = new HttpClient();
            var url = $"{VerifyServer}/attestation/{attestationId}";
            var results = await client.GetStringAsync(url);

            var verifiedContext = JsonConvert.DeserializeObject<VerifiedContext>(results);

            return verifiedContext != null ? verifiedContext.Origin : "";
        }
        catch
        {
            return "";
        }
    }
}
