using System;
using MonoTouch.Foundation;
using  MonoTouch.UIKit;
using System.CodeDom.Compiler;
using ChromecastDemo.iOSClassic;
using GoogleCast;

namespace ChromecastDemo.iOS
{
	partial class CustomAppViewController : UIViewController
	{
		ChromecastRemoteAppManager remoteAppManager;

		HudControlChannel hudConrolChannel;

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

		partial void connectHUD_TouchUpInside(UIButton sender)
		{
			hudConrolChannel = new HudControlChannel();
			remoteAppManager.AddChannel(hudConrolChannel);
		}

		partial void showHUD_TouchUpInside(UIButton sender)
		{
			if (null != hudConrolChannel)
			{
				hudConrolChannel.SendTextMessage("show");
			}
		}

		partial void hideHUD_TouchUpInside(UIButton sender)
		{
			if (null != hudConrolChannel)
			{
				hudConrolChannel.SendTextMessage("hide");
			}
		}

		public class HudControlChannel : GCKCastChannel
		{
			static string  CHANNEL_NAME = "urn:x-cast:com.google.devrel.custom";
			public HudControlChannel(  ) : base( CHANNEL_NAME )
			{
			}
			
		}
	}


}
