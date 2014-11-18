using System;
using  MonoTouch.Foundation;
using  MonoTouch.UIKit;
using System.CodeDom.Compiler;
using ChromecastDemo.iOSClassic;

namespace ChromecastDemo.iOS
{
	partial class StyledAppViewController : UIViewController
	{
		ChromecastRemoteAppManager remoteAppManager;

		public StyledAppViewController (IntPtr handle) : base (handle)
		{


			remoteAppManager = new ChromecastRemoteAppManager( (UIApplication.SharedApplication.Delegate as AppDelegate).SelectedDevice);
		}

		partial void startAppButton_TouchUpInside(UIButton sender)
		{
			remoteAppManager.StartRemoteApp("F7C9F7B8");
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
