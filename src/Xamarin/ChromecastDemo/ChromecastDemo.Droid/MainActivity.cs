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

namespace ChromecastDemo.Droid
{
	[Activity( 
		Label = "Chromecast Demo",
		MainLauncher = true,
		Icon = "@drawable/icon")]
	public class MainActivity : ActionBarActivity
	{
		int count = 1;

		MediaRouter mediaRouter;

		MediaRouteSelector routeSelector;

		RouteCallback routeCallback;

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


			mediaRouter = MediaRouter.GetInstance(this);

			routeSelector = new Android.Support.V7.Media.MediaRouteSelector.Builder().AddControlCategory( 
				CastMediaControlIntent.CategoryForCast( CastMediaControlIntent.DefaultMediaReceiverApplicationId ) ).Build();

			routeCallback = new RouteCallback();
		}

		public override bool OnPrepareOptionsMenu(IMenu menu)
		{
			MenuInflater.Inflate(Resource.Menu.menu, menu);

			var menuItem = menu.FindItem(Resource.Id.media_route_menu_item);
			var mediaRouteActionProvider = new Android.Support.V7.App.MediaRouteActionProvider( this );
			MenuItemCompat.SetActionView(menuItem, new Android.Support.V7.App.MediaRouteButton(this));
			MenuItemCompat.SetActionProvider(menuItem, mediaRouteActionProvider);

		//	var temp = MenuItemCompat.GetActionProvider(menuItem);


			mediaRouteActionProvider.RouteSelector = routeSelector;

			return base.OnPrepareOptionsMenu(menu);
		}

		public override bool OnCreateOptionsMenu(IMenu menu)
		{


			return base.OnCreateOptionsMenu(menu);
		}

		protected override void OnResume()
		{

			mediaRouter.AddCallback(
				routeSelector, routeCallback, MediaRouter.CallbackFlagRequestDiscovery
			);
			base.OnResume();
		}

		public class RouteCallback : MediaRouter.Callback
		{
			public override void OnRouteRemoved(MediaRouter router, MediaRouter.RouteInfo route)
			{
				base.OnRouteRemoved(router, route);
			}

			public override void OnRouteSelected(MediaRouter router, MediaRouter.RouteInfo route)
			{
				base.OnRouteSelected(router, route);
			}

			public override void OnRouteUnselected(MediaRouter router, MediaRouter.RouteInfo route)
			{
				base.OnRouteUnselected(router, route);
			}

			public override void OnRouteVolumeChanged(MediaRouter router, MediaRouter.RouteInfo route)
			{
				base.OnRouteVolumeChanged(router, route);
			}
		}

	}


}


