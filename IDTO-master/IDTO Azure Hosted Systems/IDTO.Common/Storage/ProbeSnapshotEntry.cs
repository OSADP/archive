using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace IDTO.Common.Storage
{
    public class ProbeSnapshotEntry : TableEntity
    {
        public ProbeSnapshotEntry(): base("_default", null) 
        {}

        public ProbeSnapshotEntry(string tripId) : base(tripId, null) 
        {}

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Speed { get; set; }
        public int Heading { get; set; }
        public DateTime PositionTimestamp { get; set; }
        public int Satellites { get; set; }
        public int Accuracy { get; set; }
        public double Altitude { get; set; }
    }
}
