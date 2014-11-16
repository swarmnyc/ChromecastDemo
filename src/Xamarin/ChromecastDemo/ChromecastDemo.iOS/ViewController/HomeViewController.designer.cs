// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;

namespace ChromecastDemo.iOS
{
	[Register ("HomeViewController")]
	partial class HomeViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton customAppButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton defaultAppButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton styledAppButton { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (customAppButton != null) {
				customAppButton.Dispose ();
				customAppButton = null;
			}
			if (defaultAppButton != null) {
				defaultAppButton.Dispose ();
				defaultAppButton = null;
			}
			if (styledAppButton != null) {
				styledAppButton.Dispose ();
				styledAppButton = null;
			}
		}
	}
}
