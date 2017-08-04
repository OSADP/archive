using System;
using System.Threading.Tasks;

namespace IDTO.Common.Models
{
	public class RegistrationApplicant
	{
		public enum RegistrationValidationCode {  VALID, INVALID_USERNAME, INVALID_PASSWORD, INVALID_FIRSTNAME, INVALID_LASTNAME, INVALID_PASSWORD_MISMATCH}

		private string username;
		private string password;
		private string passwordVerify;
		private string firstname;
		private string lastname;

		public RegistrationApplicant(string username, string password, string passwordVerify, string firstname, string lastname)
		{
			this.username = username.Trim ();
			this.password = password.Trim ();
			this.passwordVerify = passwordVerify.Trim ();
			this.firstname = firstname.Trim ();
			this.lastname = lastname.Trim ();
		}

		public async Task<LoginResult> Register(LoginManager manager)
		{
			LoginResult result;
			RegistrationValidationCode code = VerifyRegistrationData ();

			if(code.Equals(RegistrationValidationCode.VALID)) {
				result = await manager.Register (username, password, firstname, lastname);
			} else {
				result = new LoginResult();
				result.Success = false;
				result.ErrorString = RegistrationApplicant.GetInvalidCodeMessage(code);
			}

			return result; 
		}

		public static string GetInvalidCodeMessage(RegistrationValidationCode code)
		{
			switch (code) {
			case RegistrationValidationCode.INVALID_USERNAME:
				return "You must enter a valid username.";
			case RegistrationValidationCode.INVALID_PASSWORD:
				return "You must enter a valid password.";
			case RegistrationValidationCode.INVALID_FIRSTNAME:
				return "You must enter a valid first name.";
			case RegistrationValidationCode.INVALID_LASTNAME:
				return "You must enter a valid last name.";
			case RegistrationValidationCode.INVALID_PASSWORD_MISMATCH:
				return "The entered passwords do not match.";
			default:
				return "";
			}
		}

		public RegistrationValidationCode VerifyRegistrationData()
		{
			if(IsNullBlankOrEmpty(username))
				return RegistrationValidationCode.INVALID_USERNAME;
			if (IsNullBlankOrEmpty (firstname))
				return RegistrationValidationCode.INVALID_FIRSTNAME;
			if(IsNullBlankOrEmpty(lastname))
				return RegistrationValidationCode.INVALID_LASTNAME;
			if(IsNullBlankOrEmpty(password))
				return RegistrationValidationCode.INVALID_PASSWORD;
			if(!PasswordsMatch())
				return RegistrationValidationCode.INVALID_PASSWORD_MISMATCH;
			return RegistrationValidationCode.VALID;
		}

		private bool IsNullBlankOrEmpty(string s)
		{
			if (string.IsNullOrEmpty (s))
				return true;
			return string.IsNullOrEmpty (s.Trim ());
		}
		private bool PasswordsMatch()
		{							
			return password == passwordVerify;
		}
	}
}

