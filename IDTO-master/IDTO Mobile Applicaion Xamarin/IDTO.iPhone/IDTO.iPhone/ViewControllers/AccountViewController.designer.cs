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
	[Register ("AccountViewController")]
	partial class AccountViewController
	{
		[Outlet]
		MonoTouch.UIKit.UIView backgroundView { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnSetPromoCode { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblAccount { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblUsername { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblVersion { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtPromoCode { get; set; }

		[Action ("LogoutAction:")]
		partial void LogoutAction (MonoTouch.Foundation.NSObject sender);

		[Action ("SetPromoCodeAction:")]
		partial void SetPromoCodeAction (MonoTouch.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (backgroundView != null) {
				backgroundView.Dispose ();
				backgroundView = null;
			}

			if (lblAccount != null) {
				lblAccount.Dispose ();
				lblAccount = null;
			}

			if (lblUsername != null) {
				lblUsername.Dispose ();
				lblUsername = null;
			}

			if (lblVersion != null) {
				lblVersion.Dispose ();
				lblVersion = null;
			}

			if (btnSetPromoCode != null) {
				btnSetPromoCode.Dispose ();
				btnSetPromoCode = null;
			}

			if (txtPromoCode != null) {
				txtPromoCode.Dispose ();
				txtPromoCode = null;
			}
		}
	}
}
