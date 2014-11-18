
using System;
using System.Drawing;

using  MonoTouch.Foundation;
using  MonoTouch.UIKit;
using ChromecastDemo.iOSClassic;
using GoogleCast;

namespace ChromecastDemo.iOS
{
	public partial class HomeViewController : UIViewController, IGCKDeviceScannerListener
	{
		private GCKDeviceScanner deviceScanner;

		private AppDelegate appDelegate;

	
		public HomeViewController()
			: base( )
		{
		}

		public HomeViewController( IntPtr handle ) : base( handle )
		{
			appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;

			deviceScanner = appDelegate.DeviceScanner;

			if (appDelegate.DeviceScanner.Devices.Length > 0)
			{
				ShowCastButton();
			}

			NSNotificationCenter.DefaultCenter.AddObserver("DEVICE_ONLINE", (NSNotification obj) => {
				ShowCastButton();
			});
		}

		void ShowCastButton()
		{
			this.NavigationItem.RightBarButtonItem = new UIBarButtonItem( UIImage.FromFile( "Images/cast_solid_black.png" ), UIBarButtonItemStyle.Plain,  
				(s, e ) =>
				{
					UIActionSheet sheet = new UIActionSheet( "Select Device" );

					foreach( GCKDevice device in deviceScanner.Devices )
					{
						sheet.AddButton( device.FriendlyName );
					}

					sheet.ShowInView( this.View );
					sheet.Clicked += (object sender, UIButtonEventArgs ea ) =>
					{
					     appDelegate.SelectedDevice = deviceScanner.Devices[ea.ButtonIndex];

				
					};



				} );
		}

		public virtual void DeviceDidComeOnline( GCKDevice device )
		{
			ShowCastButton();
		}

		public override void DidReceiveMemoryWarning()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			
			// Perform any additional setup after loading the view, typically from a nib.
		}


	}
}

