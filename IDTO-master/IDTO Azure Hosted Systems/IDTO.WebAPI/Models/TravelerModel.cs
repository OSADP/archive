using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using IDTO.Entity.Models;
namespace IDTO.WebAPI.Models
{
    /// <summary>
    /// Contains fields for loading a Trip to the database.
    /// </summary>
    [DataContract]
    public class TravelerModel
    {
        /// <summary>
        /// Unique identifier created by the database after traveler is added.
        /// </summary>
        [DataMember]
        public int Id { get; set; }
        /// <summary>
        /// Unique identifier to Azure authentication.
        /// </summary>
        [DataMember]
        public string LoginId { get; set; }
       [DataMember]
        public string FirstName { get; set; }
             [DataMember]
        public string MiddleName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public string Email { get; set; }
       [DataMember]
        public string PhoneNumber { get; set; }
        [DataMember]
        public bool InformedConsent { get; set; }
         [DataMember]
        public DateTime? InformedConsentDate { get; set; }
         [DataMember]

        public bool DefaultMobilityFlag { get; set; }
        [DataMember]
        public bool DefaultBicycleFlag { get; set; }
         [DataMember]
        public string DefaultPriority { get; set; }
         [DataMember]
        public string DefaultTimezone { get; set; }
        [DataMember]
        public DateTime? CreatedDate { get; set; }

        [DataMember]
        public string PromoCode { get; set; }

       [DataMember]
        public List<TripModel> Trips { get; set; }

        public TravelerModel()
        {
        }
        public TravelerModel(Traveler t)
        {
           this.Id = t.Id;
           this.LoginId = t.LoginId;
           this.FirstName = t.FirstName;
           this.MiddleName = t.MiddleName;
           this.LastName = t.LastName;
           this.Email = t.Email;
           this.PhoneNumber = t.PhoneNumber;
           this.InformedConsent = t.InformedConsent;
           this.InformedConsentDate = t.InformedConsentDate;
           this.DefaultMobilityFlag = t.DefaultMobilityFlag;
           this.DefaultBicycleFlag = t.DefaultBicycleFlag;
           this.DefaultPriority = t.DefaultPriority;
           this.DefaultTimezone = t.DefaultTimezone;
           this.CreatedDate = t.CreatedDate;
           this.PromoCode = t.PromoCode;
        }
        /// <summary>
        /// Converts this instance to type Traveler.
        /// </summary>
        /// <returns></returns>
        public Traveler ToTraveler()
        {
            Traveler travelerEntity = new Traveler();
            travelerEntity.Id = this.Id;
            travelerEntity.LoginId = this.LoginId;
            travelerEntity.DefaultBicycleFlag = this.DefaultBicycleFlag;
            travelerEntity.DefaultMobilityFlag = this.DefaultMobilityFlag;
            travelerEntity.DefaultPriority = this.DefaultPriority;
            travelerEntity.DefaultTimezone = this.DefaultTimezone;
            travelerEntity.InformedConsentDate = this.InformedConsentDate;
            travelerEntity.InformedConsent = this.InformedConsent;
            travelerEntity.PhoneNumber = this.PhoneNumber;
            travelerEntity.Email = this.Email;
            travelerEntity.LastName = this.LastName;
            travelerEntity.MiddleName = this.MiddleName;
            travelerEntity.FirstName = this.FirstName;
            travelerEntity.CreatedDate = this.CreatedDate.HasValue ? this.CreatedDate.Value : DateTime.UtcNow;
            travelerEntity.PromoCode = this.PromoCode;

            return travelerEntity;
        }
    }
}