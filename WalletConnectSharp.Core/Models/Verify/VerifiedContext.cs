using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Verify;

public class VerifiedContext
{
    [JsonProperty("origin")]
    public string Origin;

    [JsonProperty("validation")]
    private string _validation;

    public string ValidationString => _validation;

    public Validation Validation
    {
        get
        {
            return FromString();
        }
        set
        {

            _validation = AsString(value);
        }
    }
    
    [JsonProperty("verifyUrl")]
    public string VerifyUrl { get; set; }

    private Validation FromString()
    {
        switch (ValidationString.ToLowerInvariant())
        {
            case "VALID":
                return Validation.Valid;
            case "INVALID":
                return Validation.Invalid;
            default:
                return Validation.Unknown;
        }
    }

    private string AsString(Validation str)
    {
        switch (str)
        {
            case Validation.Invalid:
                return "INVALID";
            case Validation.Valid:
                return "VALID";
            default:
                return "UNKNOWN";
        }
    }
}
