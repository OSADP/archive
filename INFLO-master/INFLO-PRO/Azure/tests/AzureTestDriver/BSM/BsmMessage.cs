using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AzureTestDriver.BSM
{
    [JsonObject(MemberSerialization.OptIn)]
    public class BsmMessage : IBsmMessage
    {
        public BsmMessage(byte[] data)
        {
            this.data = data;
            m_time = DateTime.UtcNow.ToString("o");
        }

        [JsonProperty]
        public string typeid { get { return "BSM"; } }

        public byte[] data { get; private set; }

        [JsonProperty]
        public string payload { get { return string.Concat(data.Select(b => b.ToString("X2")).ToArray()); } }

        private string m_time;
        [JsonProperty]
        public string time { get { return m_time; } }
    }
}
