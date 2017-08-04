using IDTO.TravelerPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDTO.TravelerPortal.Views.Search
{
    public class SearchResultsViewModel
    {
        public SearchResultsViewModel()
        {

        }
        public SearchResultsViewModel(TripSearchCriteria criteria, Plan plan)
        {
            this.TripCriteria = criteria;
            this.Plan = plan;
        }

        public TripSearchCriteria TripCriteria { get; set; }

        public Plan Plan { get; set; }
    }
}