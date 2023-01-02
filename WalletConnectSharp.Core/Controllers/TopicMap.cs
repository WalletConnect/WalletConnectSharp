using System;
using System.Collections.Generic;
using System.Linq;
using WalletConnectSharp.Core.Interfaces;

namespace WalletConnectSharp.Core.Controllers
{
    public class TopicMap : ISubscriberMap
    {
        private Dictionary<string, List<string>> topicMap = new Dictionary<string, List<string>>();

        public string[] Topics => topicMap.Keys.ToArray();

        public void Set(string topic, string id)
        {
            if (Exists(topic, id)) return;
            
            if (!topicMap.ContainsKey(topic))
                topicMap.Add(topic, new List<string>());

            var ids = topicMap[topic];
            ids.Add(id);
        }

        public string[] Get(string topic)
        {
            if (!topicMap.ContainsKey(topic))
                return Array.Empty<string>();

            return topicMap[topic].ToArray();
        }

        public bool Exists(string topic, string id)
        {
            var ids = Get(topic);
            return ids.Contains(id);
        }

        public void Delete(string topic, string id = null)
        {
            if (!topicMap.ContainsKey(topic))
                return;

            if (id == null)
            {
                topicMap.Remove(topic);
                return;
            }

            if (!Exists(topic, id))
                return;

            var ids = topicMap[topic];
            ids.Remove(id);

            if (ids.Count == 0)
                topicMap.Remove(topic);
        }

        public void Clear()
        {
            topicMap.Clear();
        }
    }
}