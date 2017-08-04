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
	[Register ("TermsViewController")]
	partial class TermsViewController
	{
		[Outlet]
		MonoTouch.UIKit.UIWebView webView { get; set; }

		[Action ("acceptTerms:")]
		partial void acceptTerms (MonoTouch.Foundation.NSObject sender);

		[Action ("cancelTerms:")]
		partial void cancelTerms (MonoTouch.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (webView != null) {
				webView.Dispose ();
				webView = null;
			}
		}
	}
}
