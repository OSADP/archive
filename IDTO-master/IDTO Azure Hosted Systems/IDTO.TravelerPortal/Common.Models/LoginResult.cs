namespace IDTO.TravelerPortal.Common.Models
{
    public class LoginResult
    {
        public bool Success { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string UserToken { get; set; }
        public string ErrorString { get; set; }

    }
}