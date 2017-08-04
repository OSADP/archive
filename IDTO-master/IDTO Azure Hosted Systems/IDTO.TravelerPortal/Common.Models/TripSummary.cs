namespace IDTO.TravelerPortal.Common.Models
{
    using System.Collections.Generic;
    using System;

    public class TravelerSummaryForDelete
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool InformedConsent { get; set; }
        public DateTime InformedConsentDate { get; set; }
        public DateTime DeactivatedDate { get; set; }
        public bool DefaultMobilityFlag { get; set; }
        public bool DefaultBicycleFlag { get; set; }
        public string DefaultPriority { get; set; }
        public string DefaultTimezone { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public List<Trip> Trips { get; set; }
        public int ObjectState { get; set; }
    }

    public class TripSummaryForDelete
    {
        public int Id { get; set; }
        public int TravelerId { get; set; }
        public TravelerSummaryForDelete Traveler { get; set; }
        public string Origination { get; set; }
        public string Destination { get; set; }
        public DateTime TripStartDate { get; set; }
        public DateTime TripEndDate { get; set; }
        public bool MobilityFlag { get; set; }
        public bool BicycleFlag { get; set; }
        public string PriorityCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public int ObjectState { get; set; }
    }
}
