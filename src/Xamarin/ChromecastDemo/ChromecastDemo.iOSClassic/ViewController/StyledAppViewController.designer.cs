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
	[Register ("StyledAppViewController")]
	partial class StyledAppViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton pausePlayButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton startAppButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton stopAppButton { get; set; }

		[Action ("pausePlayButton_TouchUpInside:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void pausePlayButton_TouchUpInside (UIButton sender);

		[Action ("startAppButton_TouchUpInside:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void startAppButton_TouchUpInside (UIButton sender);

		[Action ("stopAppButton_TouchUpInside:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void stopAppButton_TouchUpInside (UIButton sender);

		void ReleaseDesignerOutlets ()
		{
			if (pausePlayButton != null) {
				pausePlayButton.Dispose ();
				pausePlayButton = null;
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
