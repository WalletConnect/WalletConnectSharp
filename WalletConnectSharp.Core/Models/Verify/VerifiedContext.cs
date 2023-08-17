using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Verify;

public class VerifiedContext
{
    [JsonProperty("origin")]
    public string Origin;

    [JsonProperty("validation")]
    private string _validation;

    public string Validation
    {
        get
        {
            return _validation;
        }
        set
        {
            if (value != Verify.Validation.Unknown && value != Verify.Validation.Invalid &&
                value != Verify.Validation.Valid)
                throw new ArgumentException("Invalid validation value, must be one of Verify.Validation string");

            _validation = value;
        }
    }
    
    [JsonProperty("verifyUrl")]
    public string VerifyUrl { get; set; }
}
