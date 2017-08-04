// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace IDTO.iPhone
{
	[Register ("RegisterViewController")]
	partial class RegisterViewController
	{
		[Outlet]
		MonoTouch.UIKit.UIView formView { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblCreateAccount { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtEmailAddress { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtFirstName { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtLastName { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtPassword { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtVerifyPassword { get; set; }

		[Action ("SubmitRegistration:")]
		partial void SubmitRegistration (MonoTouch.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (txtEmailAddress != null) {
				txtEmailAddress.Dispose ();
				txtEmailAddress = null;
			}

			if (txtFirstName != null) {
				txtFirstName.Dispose ();
				txtFirstName = null;
			}

			if (txtLastName != null) {
				txtLastName.Dispose ();
				txtLastName = null;
			}

			if (txtPassword != null) {
				txtPassword.Dispose ();
				txtPassword = null;
			}

			if (txtVerifyPassword != null) {
				txtVerifyPassword.Dispose ();
				txtVerifyPassword = null;
			}

			if (lblCreateAccount != null) {
				lblCreateAccount.Dispose ();
				lblCreateAccount = null;
			}

			if (formView != null) {
				formView.Dispose ();
				formView = null;
			}
		}
	}
}
