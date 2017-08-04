using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Repository;

namespace IDTO.Entity.Models
{
    public class Traveler : EntityBase
    {
        //primary key
        public int Id { get; set; }
        [Required, MaxLength(80, ErrorMessage = "LoginId must be between 1 and 80 characters"), MinLength(1)]
        public string LoginId { get; set; }
        //Required attribute is used to validate the model. It specifies that the Name property must be a non-empty string.
        [Required,MaxLength(50, ErrorMessage="FirstName must be 50 characters or less"),MinLength(1)]
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        [Required, MaxLength(50, ErrorMessage = "LastName must be between 2 and 50 characters"), MinLength(2)]
        public string LastName { get; set; }
        [Required, MaxLength(255, ErrorMessage = "Email must be between 7 and 255 characters"), MinLength(7)]
        public string Email { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]//TODO: MVC checks 'Required' too somehow and the user would not enter this, will this mess up UI?
        public DateTime CreatedDate { get; set; }
        [Required]
        public bool InformedConsent { get; set; }
        public DateTime? InformedConsentDate { get; set; }
        public DateTime? DeactivatedDate { get; set; }
        [Required]
        public bool DefaultMobilityFlag { get; set; }
        [Required]
        public bool DefaultBicycleFlag { get; set; }
        public string DefaultPriority { get; set; }
        public string DefaultTimezone { get; set; }
        [Required]
        public DateTime ModifiedDate { get; set; }
         [MaxLength(20)]
        public string ModifiedBy { get; set; }

        [MaxLength(50, ErrorMessage = "PromoCode must be between 2 and 50 characters"), MinLength(2)]
        public string PromoCode { get; set; }

        public DateTime? LastContactedDate { get; set; }
        // Navigation property
        public virtual ICollection<Trip> Trips { get; set; }
    }
}
