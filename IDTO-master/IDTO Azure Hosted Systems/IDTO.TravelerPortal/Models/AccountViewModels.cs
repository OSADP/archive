using System.ComponentModel.DataAnnotations;
using IDTO.TravelerPortal.Common.Models;
using IDTO.TravelerPortal.Common;

namespace IDTO.TravelerPortal.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class AccountLoginWidgetViewModel
    {
        public UserData UserData { get; set; }
    }
    


    public class RegisterViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LocalPasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class AccountInfoModel
    {

        public AccountInfoModel()
        {
        }

        public AccountInfoModel(TravelerModel traveler)
        {
            this.PromoCodeUpdated = false;
            this.PromoCodeValid = false;
            this.PromoCode = traveler.PromoCode;
            this.Email = traveler.Email;
            this.DefaultBicycleFlag = traveler.DefaultBicycleFlag;
            this.DefaultMobilityFlag = traveler.DefaultMobilityFlag;
        }

        public bool PromoCodeUpdated { get; set; }
        public bool PromoCodeValid { get; set; }

        [Display(Name = "PIN Code")]
        public string PromoCode { get; set; }
        public string Email { get; set; }

        [Display(Name = "Wheelchair Accessible")]
        public bool DefaultMobilityFlag { get; set; }

        [Display(Name = "Bike Rack Required")]
        public bool DefaultBicycleFlag { get; set; }
    }

    public class AccountViewModel
    {
        public TravelerModel traveler { get; set; }
        public LocalPasswordModel localPasswordModel { get; set; }
        public AccountInfoModel accountInfoModel { get; set; }
    }

}
