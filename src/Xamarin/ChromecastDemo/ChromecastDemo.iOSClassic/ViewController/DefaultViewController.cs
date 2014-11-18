using System;
using  MonoTouch.Foundation;
using  MonoTouch.UIKit;
using System.CodeDom.Compiler;
using ChromecastDemo.iOSClassic;
using GoogleCast;

namespace ChromecastDemo.iOS
{
	partial class DefaultViewController : UIViewController
	{

		AppDelegate appDelegate;

		ChromecastRemoteAppManager remoteAppManager;



		public DefaultViewController (IntPtr handle) : base (handle)
		{
			appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;

			remoteAppManager = new ChromecastRemoteAppManager( appDelegate.SelectedDevice);

		}

		partial void startAppButton_TouchUpInside(UIButton sender)
		{
			remoteAppManager.StartRemoteApp("CC1AD845");
		}

		partial void playPauseButton_TouchUpInside(UIButton sender)
		{
			remoteAppManager.TooglePlayPause(sender);
		}

		partial void stopAppButton_TouchUpInside(UIButton sender)
		{
			remoteAppManager.StopRemoteApp();
		}



	}
}
