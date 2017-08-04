using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfloWebRole
{
    public class TimV2VTableEntity : Microsoft.WindowsAzure.Storage.Table.TableEntity
    {
        public TimV2VTableEntity(InfloCommon.Models.TimMessage message)
        {
            this.PartitionKey = "";
            this.RowKey = Guid.NewGuid().ToString();
            this.message = message.payload;
        }

        public string message { get; set; }
    }
}