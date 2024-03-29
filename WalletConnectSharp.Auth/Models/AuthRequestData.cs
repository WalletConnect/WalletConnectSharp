﻿using Newtonsoft.Json;
using WalletConnectSharp.Core.Models.Verify;

namespace WalletConnectSharp.Auth.Models;

public class AuthRequestData
{
    [JsonProperty("cacaoPayload")]
    public Cacao.CacaoRequestPayload CacaoPayload;
    
    [JsonProperty("requester")]
    public Requester Requester { get; set; }
}
