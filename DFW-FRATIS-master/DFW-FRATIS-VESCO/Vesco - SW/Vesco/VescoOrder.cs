using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vesco
{
    public class VescoOrder
    {
        public String Job { get; set; }
        public String Sequence { get; set; }
        public String StopAction { get; set; }
        public String Location { get; set; }
        public String Address { get; set; }
        public String StopDelay { get; set; }
        public String WindowStart { get; set; }
        public String WindowEnd { get; set; }
        public String Hazardous { get; set; }
        public String Oversized { get; set; }
        public String Overweight { get; set; }
        public String OriginalLegNumber { get; set; }
        public String ContainerNumber { get; set; }
        public String SteamshipLine { get; set; }
        public String LiveLoad { get; set; }
        public String OriginalLegType { get; set; }
        public String DispatcherSequence { get; set; }
        public Double OriginalScheduledPickupDate { get; set; }
        public Double OriginalScheduledPickupTimeFrom { get; set; }
        public Double OriginalScheduledPickupTimeTo { get; set; }
        public Double OriginalScheduledDeliverDate { get; set; }
        public Double OriginalScheduledDeliverTimeFrom { get; set; }
        public Double OriginalScheduledDeliverTimeTo { get; set; }
        public DateTime RunDate { get; set; }

        public void addCarryOvers(TriniumOrder assOrder) 
        {
            this.Hazardous = assOrder.Haz.ToString();
            this.Overweight = assOrder.Overweight.ToString();
            this.OriginalLegNumber = assOrder.LegNumber;
            this.ContainerNumber = assOrder.ContainerNumber;
            this.SteamshipLine = assOrder.Ssl;
            this.LiveLoad = assOrder.Ll.ToString();
            this.OriginalLegType = assOrder.LegType;
            this.DispatcherSequence = assOrder.DispatchSequence;
            this.StopDelay = "15";
            this.OriginalScheduledPickupDate = assOrder.ScheduledPickupDate;
            this.OriginalScheduledPickupTimeFrom = assOrder.ScheduledPickupTimeFrom;
            this.OriginalScheduledPickupTimeTo = assOrder.ScheduledPickupTimeTo;
            this.OriginalScheduledDeliverDate = assOrder.ScheduledDeliverDate;
            this.OriginalScheduledDeliverTimeFrom = assOrder.ScheduledDeliverTimeFrom;
            this.OriginalScheduledDeliverTimeTo = assOrder.ScheduledDeliverTimeTo;
        }
    }
}
