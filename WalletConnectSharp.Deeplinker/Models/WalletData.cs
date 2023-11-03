using Newtonsoft.Json;

namespace WalletConnectSharp.Deeplinker.Models;

public class WalletData
{
    [JsonProperty("id")]
    public string Id;

    [JsonProperty("name")]
    public string Name;

    [JsonProperty("slug")]
    public string Slug;

    [JsonProperty("description")]
    public string Description;

    [JsonProperty("homepage")]
    public string Homepage;

    [JsonProperty("chains")]
    public string[] Chains;

    [JsonProperty("versions")]
    public string[] Versions;

    [JsonProperty("sdks")]
    public string[] Sdks;

    [JsonProperty("app_type")]
    public string AppType;

    [JsonProperty("image_id")]
    public string ImageId;

    [JsonProperty("image_url")]
    public ImageUrls Images;

    [JsonProperty("app")]
    public WalletAppData App;

    [JsonProperty("injected")]
    public List<WalletInjectionData> Injected;

    [JsonProperty("mobile")]
    public WalletLinkData Mobile;

    [JsonProperty("desktop")]
    public WalletLinkData Desktop;

    [JsonProperty("supported_standards")]
    public List<WalletStandard> SupportedStandards;

    [JsonProperty("metadata")]
    public WalletMetadata Metadata;

    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt;
}
