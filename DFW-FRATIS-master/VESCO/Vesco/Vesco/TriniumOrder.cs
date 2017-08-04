using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vesco
{
    public class TriniumOrder
    {
        public String OrderNumber { get; set; }
        public String DispatchCategoryCode { get; set; }
        public String Sts { get; set; }
        public String PickupName { get; set; }
        public String PickupAddress { get; set; }
        public String PickupCity { get; set; }
        public String PickupState { get; set; }
        public String PickupZip { get; set; }
        public String DeliverName { get; set; }
        public String DeliverAddress { get; set; }
        public String DeliverCity { get; set; }
        public String DeliverState { get; set; }
        public String DeliverZip { get; set; }
        public String LegType { get; set; }
        public String LegNumber { get; set; }
        public Double ScheduledPickupDate { get; set; }
        public Double ScheduledPickupTimeFrom { get; set; }
        public Double ScheduledPickupTimeTo { get; set; }
        public Double ScheduledDeliverDate { get; set; }
        public Double ScheduledDeliverTimeFrom { get; set; }
        public Double ScheduledDeliverTimeTo { get; set; }
        public String Ssl { get; set; }
        public String Size { get; set; }
        public String Type { get; set; }
        public Boolean Ll { get; set; }
        public Boolean Haz { get; set; }
        public Boolean Overweight { get; set; }
        public String SupplierCode { get; set; }
        public String ContainerNumber { get; set; }
        public String DispatchSequence { get; set; }
    }
}
