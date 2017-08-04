using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IDTO.DispatcherPortal.Models
{
    public class TConnRequestViewModel
    {
        public string hi { get; set; }

        public string SelectedItemId { get; set; }
     //   public List<SelectListItem> HourListItems = new List<SelectListItem>()
     //              {
     //                  new SelectListItem() { Text = "1 Hr", Value = "1" },
                      
     //   new SelectListItem() { Text = "2 Hr", Value = "2" },
     //new SelectListItem() { Text = "3 Hr", Value = "3" },
     //  new SelectListItem() { Text = "12 Hr", Value = "12" } ,
     //  new SelectListItem() { Text = "1 Day", Value = "24" }
     //              };

        public IEnumerable<SelectListItem> HourListItems { get; set; }


        public IEnumerable<Rows> RequestRows{ get; set; }
        public class Rows
        {
            public string InboundVehicle { get; set; }
            public string TConnectStopCode { get; set; }
            public int AcceptedWaitTime { get; set; }
            public string Status { get; set; }
            public DateTime EstimatedTimeArrival { get; set; }
            public int RequestedHoldMinutes { get; set; }


        }
    }
}