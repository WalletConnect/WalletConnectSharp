using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models;

public class BatchFetchMessagesResponse
{
    public class ReceivedMessage
    {
        [JsonProperty("topic")]
        public string Topic;
        
        [JsonProperty("message")]
        public string Message;
        
        [JsonProperty("publishedAt")]
        public long PublishedAt;
        
        [JsonProperty("tag")]
        public long Tag;
    }
    
    [JsonProperty("messages")]
    public ReceivedMessage[] Messages;
        
    [JsonProperty("hasMore")]
    public bool HasMore;
}
