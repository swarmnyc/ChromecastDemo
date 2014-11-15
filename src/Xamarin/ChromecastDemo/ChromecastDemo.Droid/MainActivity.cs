using System;

using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Gms.Cast;
using System.Security.Cryptography;
using Android.Support.V4.View;
using Android.Support.V7.Media;
using Android.Support.V7.App;
using Android.Nfc;
using Android.App;
using Android.Util;
using Android.Gms.Common.Apis;

namespace ChromecastDemo.Droid
{
	[Activity( 
		Label = "Chromecast Demo",
		MainLauncher = true,
		Icon = "@drawable/icon" )]
	public class MainActivity : ActionBarActivity, IGoogleApiClientConnectionCallbacks, IGoogleApiClientOnConnectionFailedListener
	{
		int count = 1;

		MediaRouter mediaRouter;

		MediaRouteSelector routeSelector;

		RouteCallback routeCallback;

		CastDevice castDevice;

		IGoogleApiClient m_googleApiClient;


		protected override void OnCreate( Bundle bundle )
		{
			base.OnCreate( bundle );

			// Set our view from the "main" layout resource
			SetContentView( Resource.Layout.Main );


			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button>( Resource.Id.btn_start_default );
			
			button.Click += delegate
			{
				// TODO Go to default Player
			};

			// Create a Media Router
			mediaRouter = MediaRouter.GetInstance( this );

			// Create a Route Selector. This is hooked up during later (onResume)
			routeSelector = new Android.Support.V7.Media.MediaRouteSelector.Builder().AddControlCategory( 
				CastMediaControlIntent.CategoryForCast( CastMediaControlIntent.DefaultMediaReceiverApplicationId ) ).Build();

			routeCallback = new RouteCallback();

			routeCallback.RouteSelected += (object sender, MediaRouteEventArgs e ) =>
			{
				castDevice = CastDevice.GetFromBundle( e.RouteInfo.Extras );
				String routeId = e.RouteInfo.Id;
				Log.Debug( "MainActivity", string.Format( "onRouteSelected : routeId = {0}", routeId ) );

				Connect();
			};
		}

		public override bool OnPrepareOptionsMenu( IMenu menu )
		{
			// Create Cast Button and hook it up to the Route Seletor

			MenuInflater.Inflate( Resource.Menu.menu, menu );

			var menuItem = menu.FindItem( Resource.Id.media_route_menu_item );
			var mediaRouteActionProvider = new Android.Support.V7.App.MediaRouteActionProvider( this );
			MenuItemCompat.SetActionView( menuItem, new Android.Support.V7.App.MediaRouteButton( this ) );
			MenuItemCompat.SetActionProvider( menuItem, mediaRouteActionProvider );

			var temp = MenuItemCompat.GetActionProvider( menuItem );
			mediaRouteActionProvider.RouteSelector = routeSelector;

			return base.OnPrepareOptionsMenu( menu );
		}

		public override bool OnCreateOptionsMenu( IMenu menu )
		{


			return base.OnCreateOptionsMenu( menu );
		}

		protected override void OnResume()
		{
			mediaRouter.AddCallback(
				routeSelector, routeCallback, MediaRouter.CallbackFlagRequestDiscovery
			);
			base.OnResume();
		}

		public void OnConnected( Bundle connectionHint )
		{

		}

		public void OnConnectionSuspended( int cause )
		{

		}

		public void OnConnectionFailed( Android.Gms.Common.ConnectionResult result )
		{

		}

		void Android.Gms.Common.IGooglePlayServicesClientOnConnectionFailedListener.OnConnectionFailed( Android.Gms.Common.ConnectionResult result )
		{

		}

		public void Connect()
		{
			if ( m_googleApiClient == null )
			{
				CastClass.CastOptions.Builder apiOptionsBuilder;
				apiOptionsBuilder = CastClass.CastOptions.InvokeBuilder(
					castDevice, 
					new CastListener() 
				);
				apiOptionsBuilder.SetVerboseLoggingEnabled( true );

				m_googleApiClient = new GoogleApiClientBuilder( this ).AddApi(
					CastClass.Api, apiOptionsBuilder.Build()
				)
				.AddConnectionCallbacks( this )
				.AddOnConnectionFailedListener( this )

				.Build();
			}
			m_googleApiClient.Connect();
		}

		public class CastListener : CastClass.Listener
		{
			public override void OnApplicationDisconnected( int statusCode )
			{
				base.OnApplicationDisconnected( statusCode );
			}

			public override void OnApplicationStatusChanged()
			{
				base.OnApplicationStatusChanged();
			}

			public override void OnVolumeChanged()
			{
				base.OnVolumeChanged();
			}
		}

		public class RouteCallback : MediaRouter.Callback
		{

			public event EventHandler<MediaRouteEventArgs> RouteSelected;


			public override void OnRouteRemoved( MediaRouter router, MediaRouter.RouteInfo route )
			{
				base.OnRouteRemoved( router, route );
			}

			public override void OnRouteSelected( MediaRouter router, MediaRouter.RouteInfo route )
			{
				if ( null != RouteSelected )
				{
					RouteSelected( this, new MediaRouteEventArgs() { MediaRouter = router, RouteInfo = route } );
				}

				base.OnRouteSelected( router, route );
			}

			public override void OnRouteUnselected( MediaRouter router, MediaRouter.RouteInfo route )
			{
				base.OnRouteUnselected( router, route );
			}

			public override void OnRouteVolumeChanged( MediaRouter router, MediaRouter.RouteInfo route )
			{
				base.OnRouteVolumeChanged( router, route );
			}
		}

		public class MediaRouteEventArgs : EventArgs
		{
			public MediaRouter MediaRouter { get; set; }

			public MediaRouter.RouteInfo RouteInfo { get; set; }
		}

	}


}


