
// Copyright 2014 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this.file except in compliance with the License.
// You may obtain a copy of the License at
//
// http(//www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using GoogleCast;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MonoTouch.Foundation;
using MonoTouch.CoreFoundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.NetworkExtension;

namespace ChromecastDemo.iOSClassic
{

	public interface ChromecastControllerDelegate{
	/**
 * Called when chromecast devices are discoverd on the network.
 */
		void didDiscoverDeviceOnNetwork();

	/**
 * Called when connection to the device was established.
 *
 * @param device The device to which the connection was established.
 */
		void didConnectToDevice(GCKDevice device);

	/**
 * Called when connection to the device was closed.
 */
		void didDisconnect();

	/**
 * Called when the playback state of media on the device changes.
 */
		void didReceiveMediaStateChange();

	/**
 * Called to display the modal device view controller from the cast icon.
 */
	void shouldDisplayModalDeviceController();

	/**
 * Called to display the remote media playback view controller.
 */
		void shouldPresentPlaybackController();

	}


	public class ChromecastDeviceController : NSObject, IGCKDeviceScannerListener,
	IGCKDeviceManagerDelegate,
	IGCKMediaControlChannelDelegate 
	{

	

		static string kReceiverAppID = @"4F8B3483";  //Replace with your app id


		UIImage _btnImage;
			UIImage _btnImageConnected;
			DispatchQueue _queue;


		public GCKMediaControlChannel mediaControlChannel;
		public GCKApplicationMetadata applicationMetadata;
		public GCKDevice selectedDevice;

		public bool deviceMuted;
		public bool isReconnecting;
		public List<object> idleStateToolbarButtons;
		public List<object> playStateToolbarButtons;
		public List<object> pauseStateToolbarButtons;
		public UIImageView toolbarThumbnailImage;
		public NSUrl toolbarThumbnailURL;
		public UILabel toolbarTitleLabel;
		public UILabel toolbarSubTitleLabel;
//		public GCKMediaTextTrackStyle textTrackStyle;

		/** The device scanner used to detect devices on the network. */
		public GCKDeviceScanner deviceScanner;

		/** The device manager used to manage conencted chromecast device. */
		public GCKDeviceManager deviceManager;

		/** Get the friendly name of the device. */
		public string deviceName;

		/** Length of the media loaded on the device. */
		public double streamDuration;

		/** Current playback position of the media loaded on the device. */
		public double streamPosition;

		/** The media player state of the media on the device. */
		public GCKMediaPlayerState playerState;

		/** The media information of the loaded media on the device. */
		public GCKMediaInformation mediaInformation;

		/** The UIBarButtonItem denoting the chromecast device. */
		public UIBarButtonItem chromecastBarButton;

		/** The controllerDelegate attached to this.controller. */
		public ChromecastControllerDelegate controllerDelegate;

		/** The volume the device is currently at **/
		public float deviceVolume;

		/** Map of track identifier NSNumber to NSNumber boolean for enabled/disabled. */
		public NSMutableDictionary selectedTrackByIdentifier;



		public ChromecastDeviceController()
		{

				this.isReconnecting = false;

				// Initialize device scanner
			this.deviceScanner = new GCKDeviceScanner();
				// Create filter criteria to only show devices that can run your app
				GCKFilterCriteria filterCriteria = new GCKFilterCriteria();
				filterCriteria = GCKFilterCriteria.FromAvailableApplicationWithId(kReceiverAppID);

				// Add the criteria to the scanner to only show devices that can run your app.
				// This allows you to publish your app to the Apple App store before before publishing in Cast console.
				// Once the app is published in Cast console the cast icon will begin showing up on ios devices.
				// If an app is not published in the Cast console the cast icon will only appear for whitelisted dongles
//				this.deviceScanner.setfilterCriteria = filterCriteria;

				// Initialize UI controls for navigation bar and tool bar.
			this.initControls();

				// Queue used for loading thumbnails.
			_queue = new DispatchQueue("com.google.sample.Chromecast");


		}

		public bool isConnected() {
			return this.deviceManager.IsConnectedToApp;
		}

		public bool isPlayingMedia()
		{
			return this.deviceManager.IsConnected && null != this.mediaControlChannel &&
			null != this.mediaControlChannel.MediaStatus && ( this.playerState == GCKMediaPlayerState.Playing ||
			this.playerState == GCKMediaPlayerState.Paused ||
			this.playerState == GCKMediaPlayerState.Buffering );
		}

		public void  performScan(bool start) {
			if (start) {
				//NSLog(@"Start Scan");
				this.deviceScanner.AddListener(this);
				this.deviceScanner.StartScan();
			} else {
				//NSLog(@"Stop Scan");
				this.deviceScanner.StopScan();
				this.deviceScanner.RemoveListener(this);
			}
		}

		public void  connectToDevice(GCKDevice device) {
			//NSLog(@"Device address( %@(%d", device.ipAddress, (unsigned int) device.servicePort);
			this.selectedDevice = device;

			NSDictionary info = NSBundle.MainBundle.InfoDictionary;
			string appIdentifier = info.ObjectForKey( NSString.CreateNative( "CFBundleIdentifier")).ToString();
			this.deviceManager = new GCKDeviceManager(this.selectedDevice.clientPackageName(appIdentifier));
			// tODO
			this.deviceManager.Delegate = this;
			this.deviceManager.Connect();

			// Start animating the cast connect images.
			UIButton chromecastButton = (UIButton )this.chromecastBarButton.CustomView;
			chromecastButton.TintColor = UIColor.White;
			chromecastButton.ImageView.AnimationImages = new UIImage[] { 
				new UIImage(@"icon_cast_on0.png"), new UIImage(@"icon_cast_on1.png"),
				new UIImage(@"icon_cast_on2.png"), new UIImage(@"icon_cast_on1.png") };
			chromecastButton.ImageView.AnimationDuration = 2;
			chromecastButton.ImageView.StartAnimating();
		}

			public void  disconnectFromDevice() {
			//NSLog(@"Disconnecting device(%@", this.selectedDevice.friendlyName);
			// We're not going to stop the applicaton in case we're not the last client.
			this.deviceManager.LeaveApplication();
			// If you want to force application to stop, uncomment below.
			//this.deviceManager stopApplication;
			this.deviceManager.Disconnect();
		}

		public void  updateToolbarForViewController(UIViewController viewController) {
			this.updateToolbarStateIn(viewController);
		}

		public void  updateStatsFromDevice() {
			if (this.isConnected() && null != this.mediaControlChannel && this.mediaControlChannel.mediaStatus) {
				_streamPosition = this.mediaControlChannel.ApproximateStreamPosition();
				_streamDuration = this.mediaControlChannel.MediaStatus.MediaInformation.StreamDuration;

				_playerState = this.mediaControlChannel.MediaStatus.PlayerState;
				_mediaInformation = this.mediaControlChannel.MediaStatus.MediaInformation;
				if (!this.selectedTrackByIdentifier) {
					this.zeroSelectedTracks();
				}
			}
		}

		public void  setDeviceVolume(float deviceVolume) {
			this.deviceManager.SetVolume(deviceVolume);
		}

		public void  changeVolumeIncrease(bool goingUp) {
			float idealVolume = this.deviceVolume + (goingUp ? 0.1 : ( -0.1));
			idealVolume = MIN(1.0, MAX(0.0, idealVolume));

			this.deviceManager.SetVolume(idealVolume);
		}

		public void  setPlaybackPercent( float newPercent ){
			newPercent = MAX(MIN(1.0, newPercent), 0.0);

			NSTimeInterval newTime = newPercent * _streamDuration;
			if (_streamDuration > 0 && this.isConnected) {
				this.mediaControlChannel.seekToTimeInterval(newTime);
			}
		}

	public void  pauseCastMedia(bool shouldPause) {
			if (this.isConnected && null != this.mediaControlChannel && null != this.mediaControlChannel.MediaStatus) {
				if (shouldPause) {
					this.mediaControlChannel.Pause();
				} else {
					this.mediaControlChannel.Play();
				}
			}
		}

																	public void  stopCastMedia() {
			if (this.isConnected && this.mediaControlChannel && this.mediaControlChannel.mediaStatus) {
				//NSLog(@"Telling cast media control channel to stop");
				this.mediaControlChannel stop;
			}
		}

		 

		public void  deviceManagerDidConnect(GCKDeviceManager deviceManager) {

			if(!this.isReconnecting) {
				this.deviceManager.LaunchApplication(kReceiverAppID);
			} else {
				NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults;
				string lastSessionID = defaults.valueForKey(@"lastSessionID");
				this.deviceManager.joinApplication(kReceiverAppID, lastSessionID);
			}
				this.updateCastIconButtonStates();
		}



				public void  DidConnectToCastApplication(GCKDeviceManager deviceManager, 
		GCKApplicationMetadata applicationMetadata, 
		string sessionID,
					bool launchedApplication) {

			this.isReconnecting = false;
			this.mediaControlChannel = GCKMediaControlChannel();
			this.mediaControlChannel.Delegate = this;
			this.deviceManager.AddChannel(this.mediaControlChannel);
			this.mediaControlChannel requestStatus;

			this.applicationMetadata = applicationMetadata;
			this.updateCastIconButtonStates();

//			if (this.controllerDelegate. respondsToSelector(@selector(didConnectToDevice()) {
			this.controllerDelegate. didConnectToDevice(this.selectedDevice);
//			}

			// Store sessionID in case of restart
			NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults;
			defaults.SetValueForKey(sessionID, @"lastSessionID");
			defaults.SetValueForKey(this.selectedDevice.deviceID, @"lastDeviceID");
			defaults.Synchronize();
		}

		public void  DidFailToConnectToApplication(GCKDeviceManager deviceManager, NSError error) 
		{
			if(this.isReconnecting && error.Codecode == GCKErrorCode.ApplicationNotRunning) {
				// Expected error when unable to reconnect to previous session after another
				// application has been running
				this.isReconnecting = false;
			} else {
				this.showError(error.description);
			}

			this.updateCastIconButtonStates();
		}

		public void  DidFailToConnect( GCKDeviceManager deviceManager, GCKError error)
		{
			this.showError(error.description);

			this.deviceDisconnectedForgetDevice(true);
			this.updateCastIconButtonStates();
		}

		public void didDisconnect( GCKDeviceManager deviceManager , GCKError error)
		{
			//NSLog(@"Received notification that device disconnected");

			// Network errors are displayed in the suspend code.
			if (error && error.code != GCKErrorCode.NetworkError) {
				this.showError(error.description);
			}

			// Forget the device except when the error is a connectivity related, such a WiFi problem.
			this.deviceDisconnectedForgetDevice(!this.isRecoverableError(error));
			this.updateCastIconButtonStates();

		}

		public void  DidDisconnectFromApplication(GCKDeviceManager deviceManager, NSError error) 
		{
			//NSLog(@"Received notification that app disconnected");

			if (error) {
				//NSLog(@"Application disconnected with error( %@", error);
			}

			// Forget the device except when the error is a connectivity related, such a WiFi problem.
			this.deviceDisconnectedForgetDevice(!this.isRecoverableError(error));
			this.updateCastIconButtonStates();
		}

		public bool isRecoverableError(NSError error) {
			if (!error) {
				return false;
			}

			return (error.Code == GCKErrorCode.NetworkError ||
				error.Code == GCKErrorCode.Timeout ||
				error.Code == GCKErrorCode.AppDidEnterBackground);
		}

		public void  deviceDisconnectedForgetDevice(bool clear)
		{
			this.mediaControlChannel = null;
			_playerState = 0;
			_mediaInformation = null;
			this.selectedDevice = null;

//			if (this.controllerDelegate respondsToSelector(@selector(didDisconnect)) {
			this.controllerDelegate.didDisconnect();
//			}

			if (clear) {
				this.clearPreviousSession();
			}
		}

		public void  clearPreviousSession() {
			NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults;
			defaults.RemoveObject(@"lastDeviceID");
			defaults.Synchronize();
		}

		public void  didReceiveApplicationMetadata (GCKDeviceManager deviceManager, GCKApplicationMetadata applicationMetadata)
		{
			this.applicationMetadata = applicationMetadata;
		}

		public void  volumeDidChangeToLevel(GCKDeviceManager deviceManager, float volumeLevel, bool isMuted)
		{
			_deviceVolume = volumeLevel;
			this.deviceMuted = isMuted;

			// Fire off a notification, so no matter what controller we are in, we can show the volume
			// slider
			NSNotificationCenter.DefaultCenter.PostNotificationName(@"Volume changed", this);
		}

//		public void  didSuspendConnectionWithReason(GCKDeviceManager deviceManager, GCKConnectionSuspendReason reason)
//		{
//			if (reason == GCKConnectionSuspendReasonAppBackgrounded) {
//				//NSLog(@"Connection Suspended( App Backgrounded");
//			} else {
//				this.showError(@"Connection Suspended( Network");
//				this.deviceDisconnectedForgetDevice(false);
//				// Update cast icons on next runloop so all cast objects have time to update.
//				// TODO
////				this.PerformSelector(@Selector(updateCastIconButtonStates) withObject(null afterDelay(0)));
//			}
//		}

		public void  deviceManagerDidResumeConnection(GCKDeviceManager deviceManager, bool rejoinedApplication) {
			//NSLog(@"Connection Resumed. App Rejoined( %@", rejoinedApplication ? @"true" ( @"false");
			// Update cast icons on next runloop so all cast objects have time to update.
			// TODO
//			this.performSelector(@selector(updateCastIconButtonStates) withObject(null afterDelay(0;
		}

		 
		public void  deviceDidComeOnline( GCKDevice device) 
		{
			//NSLog(@"device found - %@", device.friendlyName);

			NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults;
			string* lastDeviceID = defaults.ValueForKey(@"lastDeviceID");
			if(lastDeviceID != null && device.deviceID == lastDeviceID){
				this.isReconnecting = true;
				this.connectToDevice(device);
			}

			// Trigger an update in the next run loop so we pick up the updated devices array.
			// TODO
//			this.performSelector(@selector(updateCastIconButtonStates) withObject(null afterDelay(0))';
//			if (this.controllerDelegate respondsToSelector(@selector(didDiscoverDeviceOnNetwork)) {
			this.controllerDelegate.didDiscoverDeviceOnNetwork();
//			}
		}

		public void  deviceDidGoOffline(GCKDevice device) {
			//NSLog(@"device went offline - %@", device.friendlyName);
			// Trigger an update in the next run loop so we pick up the updated devices array.
			// TODO
//			this.performSelector(@selector(updateCastIconButtonStates) withObject(null afterDelay(0;
		}

		 

		public void  DidCompleteLoad(GCKMediaControlChannel mediaControlChannel, int sessionID ){
			_mediaControlChannel = mediaControlChannel;
		}

		public void  mediaControlChannelDidUpdateStatus(GCKMediaControlChannel mediaControlChannel) {
			this.updateStatsFromDevice();
			//NSLog(@"Media control channel status changed");
			_mediaControlChannel = mediaControlChannel;
			this.updateTrackSelectionFromActiveTracks(_mediaControlChannel.mediaStatus.activeTrackIDs) ;
//			if (this.controllerDelegate respondsToSelector(@selector(didReceiveMediaStateChange)) {
			this.controllerDelegate.didReceiveMediaStateChange();
//			}
		}

		public void  mediaControlChannelDidUpdateMetadata( GCKMediaControlChannel mediaControlChannel) {
			//NSLog(@"Media control channel metadata changed");
			_mediaControlChannel = mediaControlChannel;
			this.updateStatsFromDevice;

//			if (this.controllerDelegate respondsToSelector(@selector(didReceiveMediaStateChange)) {
				this.controllerDelegate.didReceiveMediaStateChange();
//			}
		}

		public bool loadMedia(NSUrl url,
			NSUrl thumbnailURL,
		string title,
		string subtitle,
		string mimeType,
			List<object> tracks,
		double startTime, bool autoPlay) 
		{
			if (!this.deviceManager || !this.deviceManager.isConnected) {
				return false;
			}
			// Reset selected tracks.
			this.selectedTrackByIdentifier = null;
			GCKMediaMetadata metadata = new GCKMediaMetadata();
			if (title) {
				metadata.SetString(title ,GCKMetadataKey.Title);
			}

			if (subtitle) {
				metadata.SetString(subtitle, GCKMetadataKey.Subtitle);
			}

			if (thumbnailURL) {
				metadata.AddImage(new GCKImage(thumbnailURL ,200 ,100));
			}

			GCKMediaInformation mediaInformation =
				new GCKMediaInformation(url, GCKMediaStreamType.None, mimeType, metadata, 0//,tracks, this.textTrackStyle
					,null);

			this.mediaControlChannel.LoadMedia(mediaInformation, autoPlay ,startTime);

			return true;
		}


//		public GCKMediaTextTrackStyle TextTrackStyle {
//			get
//			{
//			if ( null == _textTrackStyle) {
//				// createDefault will use the system captions style via the MediaAccessibility framework
//				// in iOS 7 and above. For apps which support iOS 6 you may want to implement a Settings
//				// bundle and customise a GCKMediaTextTrackStyle manually on those systems.
//				_textTrackStyle = GCKMediaTextTrackStyle createDefault;
//			}
//			return _textTrackStyle;
//			}
//		}

		 

		public void  showError(string errorDescription) {
			//NSLog(@"Received error( %@", errorDescription);
			UIAlertView alert = new UIAlertView (
				@"Cast Error"
			,@"An error occurred. Make sure your Chromecast is powered up and connected to the network."
			, null
			, @"OK");
			alert.Show();
		}

		public string getDeviceName () {
			if (this.selectedDevice == null)
				return @"";
			return this.selectedDevice.FriendlyName;
		}

		public void  initControls() {
			// Create chromecast bar button.
			_btnImage = new UIImage(@"icon_cast_off.png");
			_btnImageConnected = new UIImage(@"icon_cast_on_filled.png");

			UIButton chromecastButton = new UIButton(UIButtonType.System);
			chromecastButton.TouchDown += (object sender, EventArgs e) => 
			{
				chooseDevice();
			};

		

			chromecastButton.frame = CGRectMake(0, 0, _btnImage.Size.Width, _btnImage.Size.Height);
			chromecastButton.SetImage(_btnImage,  UIControlState.Normal);
			chromecastButton.Hidden = true;

			_chromecastBarButton = new UIBarButtonItem(chromecastButton);

			// Create toolbar buttons for the mini player.
			CGRect frame = CGRectMake(0, 0, 49, 37);
			_toolbarThumbnailImage =
				new UIImageView(new UIImage(@"video_thumb_mini.png"));
			_toolbarThumbnailImage.frame = frame;
			_toolbarThumbnailImage.contentMode = UIViewContentMode.ScaleAspectFit;
			UIButton someButton =  UIButton (frame);
			someButton.AddSubview(_toolbarThumbnailImage);

			someButton.TouchUpInside += (object sender, EventArgs e) => {
				showMedia();
			};


			someButton.ShowsTouchWhenHighlighted = true;
			UIBarButtonItem thumbnail = new UIBarButtonItem(someButton);

			UIButton btn = new  UIButton(UIButtonType.Custom);
			btn.Frame =  new System.Drawing.RectangleF(0, 0, 200, 45);
			_toolbarTitleLabel = new UILabel(new System.Drawing.RectangleF(0, 0, 185, 30));
			_toolbarTitleLabel.backgroundColor = UIColor.Clear;
			_toolbarTitleLabel.font = UIFont.SystemFontOfSize(17);
			_toolbarTitleLabel.text = @"This is the title";
			_toolbarTitleLabel.autoresizingMask = UIViewAutoresizing.FlexibleWidth;
			_toolbarTitleLabel.textColor = UIColor.Black;
			btn.addSubview(_toolbarTitleLabel);

			_toolbarSubTitleLabel = new UILabel (new System.Drawing.RectangleF(0, 15, 185, 30));
			_toolbarSubTitleLabel.backgroundColor = UIColor.Clear;
			_toolbarSubTitleLabel.font = UIFont.systemFontOfSize(14);
			_toolbarSubTitleLabel.text = @"This is the sub";
			_toolbarSubTitleLabel.autoresizingMask = UIViewAutoresizing.FlexibleWidth;
			_toolbarSubTitleLabel.textColor = UIColor.Gray;
			btn.addSubview(_toolbarSubTitleLabel);
			btn.TouchUpInside += (object sender, EventArgs e) => {
				showMedia();
			};

			UIBarButtonItem titleBtn = new UIBarButtonItem(btn);

			UIBarButtonItem flexibleSpaceLeft =
				new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace,null, null);
					
			UIBarButtonItem playButton =
				new UIBarButtonItem (UIBarButtonSystemItem.Play, (object sender, EventArgs e) => {
					playMedia();
				});
			playButton.TintColor = UIColor.Black;

			UIBarButtonItem pauseButton =
				new UIBarButtonItem(UIBarButtonSystemItem.Pause, (object sender, EventArgs e) => {
					pauseMedia();
				});
					
			pauseButton.TintColor = UIColor.Black;
			// TODO
//			_idleStateToolbarButtons = new List arrayWithObjects(thumbnail, titleBtn, flexibleSpaceLeft, null;
//			_playStateToolbarButtons =
//				List arrayWithObjects(thumbnail, titleBtn, flexibleSpaceLeft, pauseButton, null;
//			_pauseStateToolbarButtons =
//				List arrayWithObjects(thumbnail, titleBtn, flexibleSpaceLeft, playButton, null;
		}

		public void  chooseDevice( object sender) {
//			if (this.controllerDelegate respondsToSelector(@selector(shouldDisplayModalDeviceController)) {
			_controllerDelegate.shouldDisplayModalDeviceController();
//			}
		}

		public void  updateCastIconButtonStates() {
			// Hide the button if there are no devices found.
			UIButton chromecastButton = (UIButton )this.chromecastBarButton.customView;
			if (this.deviceScanner.Devices.Length == 0) {
				chromecastButton.Hidden = true;
			} else {
				chromecastButton.hidden = false;
				if (this.deviceManager && this.deviceManager.isConnectedToApp) {
					chromecastButton.imageView stopAnimating;
					// Hilight with yellow tint color.
					chromecastButton.TintColor = (UIColo.Yellow);
					chromecastButton.setImage(_btnImageConnected, UIControlState.Normal);

				} else {
					// Remove the highlight.
					chromecastButton.TintColor = null;
					chromecastButton.SetImage(_btnImage, UIControlStateNormal);
				}
			}
		}

		public void  updateToolbarStateIn(UIViewController viewController) {
			// Ignore this.view controller if it is not visible.
			if (!(viewController.isViewLoaded && null == viewController.view.window)) {
				return;
			}
			// Get the playing status.
			if (this.isPlayingMedia) {
				viewController.navigationController.toolbarHidden = false;
			} else {
				viewController.navigationController.toolbarHidden = true;
				return;
			}

			// Update the play/pause state.
			if (this.playerState == GCKMediaPlayerStateUnknown ||
				this.playerState == GCKMediaPlayerStateIdle) {
				viewController.toolbarItems = this.idleStateToolbarButtons;
			} else {
				bool playing = (this.playerState == GCKMediaPlayerStatePlaying ||
					this.playerState == GCKMediaPlayerStateBuffering);
				if (playing) {
					viewController.toolbarItems = this.playStateToolbarButtons;
				} else {
					viewController.toolbarItems = this.pauseStateToolbarButtons;
				}
			}

			// Update the title.
			this.toolbarTitleLabel.text = this.mediaInformation.metadata.stringForKey(GCKMetadataKey.Title);
			this.toolbarSubTitleLabel.text =
				this.mediaInformation.metadata.stringForKey(GCKMetadataKey.Subtitle);

			// Update the image.
			GCKImage img = this.mediaInformation.metadata.images.objectAtIndex(0);
			if (img.Url == this.toolbarThumbnailURL) {
				return;
			}

			//Loading thumbnail async

			// TODO 
//			dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^{
//				UIImage image = UIImage imageWithData(SimpleImageFetcher getDataFromImageURL(img.URL;
//
//				dispatch_async(dispatch_get_main_queue(), ^{
//					this.toolbarThumbnailURL = img.URL;
//					this.toolbarThumbnailImage.image = image;
//				});
//			});
		}

		public void  playMedia() {
			this.pauseCastMedia(false);
		}

		public void  pauseMedia() {
			this.pauseCastMedia(true);
		}

		public void  showMedia() {
//			if (this.controllerDelegate respondsToSelector(@selector(shouldPresentPlaybackController)) {
			this.controllerDelegate.shouldPresentPlaybackController();
//			}
		}


		public void  updateActiveTracks() {
//			NSMutableArray tracks = NSMutableArray arrayWithCapacity(this.selectedTrackByIdentifier count;
//			NSEnumerator enumerator = this.selectedTrackByIdentifier keyEnumerator;
//			NSNumber key;
//			while ((key = enumerator nextObject)) {
//				if (this.selectedTrackByIdentifier objectForKey(key boolValue) {
//					tracks addObject(key;
//				}
//			}
//			this.mediaControlChannel setActiveTrackIDs(tracks;
		}

		public void  updateTrackSelectionFromActiveTracks(List<object> activeTracks) {
//			if (_mediaControlChannel.mediaStatus.activeTrackIDs count == 0) {
//				this.zeroSelectedTracks;
//			}
//
//			NSEnumerator enumerator = this.selectedTrackByIdentifier keyEnumerator;
//			NSNumber key;
//			while ((key = enumerator nextObject)) {
//				this.selectedTrackByIdentifier
//					setObject(NSNumber numberWithBool(activeTracks containsObject(key
//					forKey(key;
//			}
		}

		public void  zeroSelectedTracks() {
			// Disable tracks.
//			this.selectedTrackByIdentifier =
//				NSMutableDictionary dictionaryWithCapacity(this.mediaInformation.mediaTracks count;
//			NSNumber nope = NSNumber numberWithBool(false;
//			for (GCKMediaTrack track in this.mediaInformation.mediaTracks) {
//				this.selectedTrackByIdentifier setObject(nope
//					forKey(NSNumber numberWithInteger(track.identifier;
//			}
		}

	}
}

