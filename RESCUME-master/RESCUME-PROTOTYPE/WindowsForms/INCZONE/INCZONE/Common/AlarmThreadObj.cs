using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using INCZONE.VITAL;
using Phidgets;

namespace INCZONE.Common
{
    public class AlarmThreadObj
    {
        public Alarm alarm { get; set; }
        public InterfaceKit ifkit { get; set; }
        public VITALModule vitalModule { get; set; }
        public bool fiKitByPassed { get; set; }
        public bool NoVitalNeeded { get; set; }
    }
}
