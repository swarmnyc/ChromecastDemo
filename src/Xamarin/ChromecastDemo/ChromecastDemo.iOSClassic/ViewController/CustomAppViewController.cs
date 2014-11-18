using System;
using MonoTouch.Foundation;
using  MonoTouch.UIKit;
using System.CodeDom.Compiler;
using ChromecastDemo.iOSClassic;

namespace ChromecastDemo.iOS
{
	partial class CustomAppViewController : UIViewController
	{
		ChromecastRemoteAppManager remoteAppManager;

		public CustomAppViewController (IntPtr handle) : base (handle)
		{
			remoteAppManager = new ChromecastRemoteAppManager( (UIApplication.SharedApplication.Delegate as AppDelegate).SelectedDevice);

		}

		partial void startAppButton_TouchUpInside(UIButton sender)
		{
			remoteAppManager.StartRemoteApp("F650276D");

		}

		partial void pausePlayButton_TouchUpInside(UIButton sender)
		{
			remoteAppManager.TooglePlayPause(sender);

		}

		partial void stopAppButton_TouchUpInside(UIButton sender)
		{
			remoteAppManager.StopRemoteApp();
		}
	}
}
