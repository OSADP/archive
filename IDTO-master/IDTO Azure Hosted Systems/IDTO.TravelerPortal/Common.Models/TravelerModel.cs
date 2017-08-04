namespace IDTO.TravelerPortal.Common.Models
{
    using System.Collections.Generic;
    using System;

    public class TravelerModel
    {
        /// <summary>
        /// Unique identifier created by the database after traveler is added.
        /// </summary>

        public int Id { get; set; }
        
        public string LoginId { get; set; }

        public string FirstName { get; set; }
     
        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public bool InformedConsent { get; set; }
 
        public DateTime? InformedConsentDate { get; set; }
 
        public bool DefaultMobilityFlag { get; set; }

        public bool DefaultBicycleFlag { get; set; }
 
        public string DefaultPriority { get; set; }
 
        public string DefaultTimezone { get; set; }

        public List<Trip> Trips { get; set; }

        public DateTime CreatedDate { get; set; }

        public string PromoCode { get; set; }

        public TravelerModel()
        {
        }
    }
}