using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AzureTestDriver.BSM
{
    [JsonObject(MemberSerialization.OptIn)]
    public class BsmBundleFormatterJson : IBsmBundleFormatter
    {
        [JsonProperty]
        private string typeid { get { return "BSB"; } }
        [JsonProperty]
        private IBsmMessage[] payload { get; set; }

        public BsmBundleFormatterJson()
        {

        }

        string IBsmBundleFormatter.GetFormattedString(ICollection<IBsmMessage> bsmBundle)
        {
            this.payload = bsmBundle.ToArray();

            return JsonConvert.SerializeObject(this);
        }
    }

    
}
