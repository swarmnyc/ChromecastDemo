// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;

namespace ChromecastDemo.iOS
{
	[Register ("CustomAppViewController")]
	partial class CustomAppViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton connectHUD { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton hideHUD { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton pausePlayButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton showHUD { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton startAppButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton stopAppButton { get; set; }

		[Action ("connectHUD_TouchUpInside:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void connectHUD_TouchUpInside (UIButton sender);

		[Action ("hideHUD_TouchUpInside:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void hideHUD_TouchUpInside (UIButton sender);

		[Action ("pausePlayButton_TouchUpInside:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void pausePlayButton_TouchUpInside (UIButton sender);

		[Action ("showHUD_TouchUpInside:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void showHUD_TouchUpInside (UIButton sender);

		[Action ("startAppButton_TouchUpInside:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void startAppButton_TouchUpInside (UIButton sender);

		[Action ("stopAppButton_TouchUpInside:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void stopAppButton_TouchUpInside (UIButton sender);

		void ReleaseDesignerOutlets ()
		{
			if (connectHUD != null) {
				connectHUD.Dispose ();
				connectHUD = null;
			}
			if (hideHUD != null) {
				hideHUD.Dispose ();
				hideHUD = null;
			}
			if (pausePlayButton != null) {
				pausePlayButton.Dispose ();
				pausePlayButton = null;
			}
			if (showHUD != null) {
				showHUD.Dispose ();
				showHUD = null;
			}
			if (startAppButton != null) {
				startAppButton.Dispose ();
				startAppButton = null;
			}
			if (stopAppButton != null) {
				stopAppButton.Dispose ();
				stopAppButton = null;
			}
		}
	}
}
