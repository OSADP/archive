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
	[Register ("LoginViewController")]
	partial class LoginViewController
	{
		[Outlet]
		MonoTouch.UIKit.UIView formView { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblLogin { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtEmailAddress { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtPassword { get; set; }

		[Action ("SubmitLogin:")]
		partial void SubmitLogin (MonoTouch.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (txtEmailAddress != null) {
				txtEmailAddress.Dispose ();
				txtEmailAddress = null;
			}

			if (txtPassword != null) {
				txtPassword.Dispose ();
				txtPassword = null;
			}

			if (lblLogin != null) {
				lblLogin.Dispose ();
				lblLogin = null;
			}

			if (formView != null) {
				formView.Dispose ();
				formView = null;
			}
		}
	}
}
