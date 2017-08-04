using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vesco
{
    class ExecutedOrder
    {
        public String DeliverTo { get; set; }
        public String DeliveryAddress { get; set; }
        public String DeliveryCity { get; set; }
        public DateTime dtDevliveryDateTime { get; set; }
        public String Driver { get; set; }

    }
}
