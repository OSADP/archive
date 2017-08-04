using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDTO.TravelerPortal.Models
{
    public class Plan
    {
        public long date { get; set; }
        public string FromLocation { get; set; }
        public string ToLocation { get; set; }

        public List<Itinerary> itineraries { get; set; }

        public Itinerary GetItinerary(Guid id)
        {
            return itineraries.FirstOrDefault(i => i.id == id);
        }
    }
}
