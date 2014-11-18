using System;
using GoogleCast;
using MonoTouch.CoreImage;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace ChromecastDemo.iOSClassic
{
	public class ChromecastRemoteAppManager
	{
		public static string VIDEO_URL = "http://distribution.bbb3d.renderfarming.net/video/mp4/bbb_sunflower_1080p_30fps_normal.mp4";

		GCKDeviceManager deviceManager;

		DeviceManagerDelegate myDeviceManagerDelegate;

		public ChromecastRemoteAppManager(GCKDevice device)
		{
			var info = NSBundle.MainBundle.InfoDictionary;
			deviceManager = new GCKDeviceManager( device, info ["CFBundleIdentifier"].ToString () );

			myDeviceManagerDelegate = new DeviceManagerDelegate();
			deviceManager.Delegate = myDeviceManagerDelegate;

			deviceManager.Connect();
		}

		public void Connect()
		{
			deviceManager.Connect();
		}

		public void StartRemoteApp( string appId)
		{
			deviceManager.LaunchApplication(appId);
		}


		public void TooglePlayPause(UIButton toggleButton )
		{
			if (myDeviceManagerDelegate.MediaControlChannel.MediaStatus.PlayerState == GCKMediaPlayerState.Playing)
			{
				myDeviceManagerDelegate.MediaControlChannel.Pause();
				toggleButton.SetTitle("Play", UIControlState.Normal);
			}
			else
			{
				myDeviceManagerDelegate.MediaControlChannel.Play();
				toggleButton.SetTitle("Pause", UIControlState.Normal);
			}
		}

		public void StopRemoteApp( )
		{
			deviceManager.StopApplication();
		}


		public class DeviceManagerDelegate : GCKDeviceManagerDelegate
		{
			GCKMediaControlChannel mediaControlChannel;

			string mediaUrl
;

			public GCKMediaControlChannel MediaControlChannel
			{
				get
				{
					return mediaControlChannel;
				}
				set
				{
					mediaControlChannel = value;
				}
			}

			public override void DidConnect(GCKDeviceManager deviceManager)
			{
				Console.WriteLine ("DidConnect : {0}", deviceManager.Description);
				//				deviceManager.LaunchApplication("CC1AD845");
			}

			public override void DidConnectToCastApplication(GCKDeviceManager deviceManager, GCKApplicationMetadata applicationMetadata, string sessionId, bool launchedApplication)
			{
				Console.WriteLine ("DidConnectToCastApplication : {0}", deviceManager.Description);

				mediaControlChannel = new GCKMediaControlChannel();
				mediaControlChannel.Delegate = new GCKMediaControlChannelDelegate ();
				deviceManager.AddChannel (mediaControlChannel);

				var metadata = new GCKMediaMetadata ();
				metadata.SetString ("The Cat", GCKMetadataKey.Title);
				metadata.SetString ("soft kitty, warm kitty, little ball of fur sleepy kitty, happy kitty, purr, purr, purr.", GCKMetadataKey.Subtitle);
				metadata.AddImage (new GCKImage (new NSUrl ("http://placekitten.com/480/360"), 480, 360));

				// define Media information
				var mediaInformation = new GCKMediaInformation (VIDEO_URL,
					GCKMediaStreamType.None, "video/mp4", metadata, 0, null);

				// cast video
				mediaControlChannel.LoadMedia (mediaInformation, true, 0);
			}

			public override void DidDisconnect(GCKDeviceManager deviceManager, NSError error)
			{
				Console.WriteLine ("DidDisconnect : {0}", error.LocalizedDescription);
			}

			public override void DidDisconnectFromApplication(GCKDeviceManager deviceManager, NSError error)
			{
				Console.WriteLine ("DidDisconnectFromApplication : {0}", error);
			}

			public override void DidFailToConnect(GCKDeviceManager deviceManager, NSError error)
			{
				Console.WriteLine ("DidFailToConnect : {0}", error.LocalizedDescription);
			}

			public override void DidFailToConnectToApplication(GCKDeviceManager deviceManager, NSError error)
			{
				Console.WriteLine ("DidFailToConnectToApplication : {0}", error.LocalizedDescription);
			}

			public override void DidFailToStopApplication(GCKDeviceManager deviceManager, NSError error)
			{
				Console.WriteLine ("DidFailToStopApplication : {0}", error.LocalizedDescription);
			}

			public override void DidReceiveStatus(GCKDeviceManager deviceManager, GCKApplicationMetadata applicationMetadata)
			{
				Console.WriteLine ("DidReceiveStatus : {0}", applicationMetadata);
			}

			public override void VolumeDidChange(GCKDeviceManager deviceManager, float volumeLevel, bool isMuted)
			{

			}
		}

	}
}

