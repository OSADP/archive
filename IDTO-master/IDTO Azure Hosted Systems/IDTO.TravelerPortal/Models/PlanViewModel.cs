using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDTO.TravelerPortal.Models
{
    public class PlanViewModel
    {
        public PlanViewModel(TripSearchCriteria criteria, Plan plan)
        {
            this.TripCriteria = criteria;
            this.Plan = plan;
        }

        public TripSearchCriteria TripCriteria { get; private set; }

        public Plan Plan { get; private set; }
    }
}