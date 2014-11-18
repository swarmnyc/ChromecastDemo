package com.swarmnyc.chromecastdemo;

import android.content.DialogInterface;
import android.content.DialogInterface.OnCancelListener;
import android.content.Intent;
import android.content.IntentSender.SendIntentException;
import android.os.Bundle;
import android.support.v4.app.FragmentTransaction;
import android.support.v4.view.MenuItemCompat;
import android.support.v7.app.ActionBarActivity;
import android.support.v7.app.MediaRouteActionProvider;
import android.support.v7.media.MediaRouteSelector;
import android.support.v7.media.MediaRouter;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import com.google.android.gms.cast.Cast;
import com.google.android.gms.cast.CastDevice;
import com.google.android.gms.cast.CastMediaControlIntent;
import com.google.android.gms.common.ConnectionResult;
import com.google.android.gms.common.GooglePlayServicesUtil;
import com.google.android.gms.common.api.GoogleApiClient;

public class MainActivity extends ActionBarActivity
	implements GoogleApiClient.ConnectionCallbacks, GoogleApiClient.OnConnectionFailedListener
{

	private static final String TAG = "MainActivity";

	private static final String KEY_IN_RESOLUTION = "is_in_resolution";

	/**
	 * Request code for auto Google Play Services error resolution.
	 */
	protected static final int REQUEST_CODE_RESOLUTION = 1;

	/**
	 * Google API client.
	 */
	private GoogleApiClient m_googleApiClient;

	/**
	 * Determines if the client is in a resolution state, and
	 * waiting for resolution intent to return.
	 */
	private boolean                                       mIsInResolution;
	private MediaRouter                                   mMediaRouter;
	private MediaRouteSelector                            mMediaRouteSelector;
	private CastDevice                                    m_castDevice;
	private android.support.v7.media.MediaRouter.Callback mMediaRouterCallback;
	private boolean                                       mApplicationStarted;
	private String                                        mSessionId;

	/**
	 * Called when the activity is starting. Restores the activity state.
	 */
	@Override
	protected void onCreate( Bundle savedInstanceState )
	{
		super.onCreate( savedInstanceState );

		setContentView( R.layout.activity_main );


		findViewById( R.id.btn_start_default ).setOnClickListener(
			new View.OnClickListener()
			{
				@Override public void onClick( final View v )
				{
					final FragmentDefaultReceiver fragment = new FragmentDefaultReceiver();
					fragment.setGoogleApiClient( m_googleApiClient );
					getSupportFragmentManager().beginTransaction().setTransition(
						FragmentTransaction.TRANSIT_FRAGMENT_OPEN
					).add( R.id.container_main, fragment ).addToBackStack( "DEFAULT" ).commit();
				}
			}
		);

		findViewById( R.id.btn_start_styled_receiver ).setOnClickListener(
			new View.OnClickListener()
			{
				@Override public void onClick( final View v )
				{
					final FragmentStyledCssReceiver fragment = new FragmentStyledCssReceiver();
					fragment.setGoogleApiClient( m_googleApiClient );
					getSupportFragmentManager().beginTransaction().setTransition(
						FragmentTransaction.TRANSIT_FRAGMENT_OPEN
					).add( R.id.container_main, fragment ).addToBackStack( "STYLED_CSS" ).commit();
				}
			}
		);

		findViewById( R.id.btn_custom_app_demo ).setOnClickListener(
			new View.OnClickListener()
			{
				@Override public void onClick( final View v )
				{
					final FragmentCustomReceiver fragment = new FragmentCustomReceiver();
					fragment.setGoogleApiClient( m_googleApiClient );
					getSupportFragmentManager().beginTransaction().setTransition(
						FragmentTransaction.TRANSIT_FRAGMENT_OPEN
					).add( R.id.container_main, fragment ).addToBackStack( "CUSTOM" ).commit();
				}
			}
		);

		findViewById( R.id.btn_tic_tac_toe_app_demo ).setOnClickListener(
			new View.OnClickListener()
			{
				@Override public void onClick( final View v )
				{
					final FragmentTicTacToeSender fragment = new FragmentTicTacToeSender();
					fragment.setGoogleApiClient( m_googleApiClient );
					getSupportFragmentManager().beginTransaction().setTransition(
						FragmentTransaction.TRANSIT_FRAGMENT_OPEN
					).add( R.id.container_main, fragment ).addToBackStack( "TIC_TAC_TOE" ).commit();
				}
			}
		);

		if ( savedInstanceState != null )
		{
			mIsInResolution = savedInstanceState.getBoolean( KEY_IN_RESOLUTION, false );
		}

		mMediaRouter = MediaRouter.getInstance( getApplicationContext() );

		// Select a chromecast that supports a default receiver
		mMediaRouteSelector = new MediaRouteSelector.Builder().addControlCategory(
			CastMediaControlIntent.categoryForCast( CastMediaControlIntent.DEFAULT_MEDIA_RECEIVER_APPLICATION_ID )
		).build();

		mMediaRouterCallback = new MyMediaRouterCallback();


	}


	@Override
	public boolean onCreateOptionsMenu( Menu menu )
	{
		super.onCreateOptionsMenu( menu );

		getMenuInflater().inflate( R.menu.main, menu );
		MenuItem mediaRouteMenuItem = menu.findItem( R.id.media_route_menu_item );
		MediaRouteActionProvider mediaRouteActionProvider = (MediaRouteActionProvider) MenuItemCompat
			.getActionProvider(mediaRouteMenuItem);
		mediaRouteActionProvider.setRouteSelector( mMediaRouteSelector );

		return true;
	}

	/**
	 * Called when the Activity is made visible.
	 * A connection to Play Services need to be initiated as
	 * soon as the activity is visible. Registers {@code ConnectionCallbacks}
	 * and {@code OnConnectionFailedListener} on the
	 * activities itself.
	 */
	@Override
	protected void onStart()
	{
		super.onStart();

	}

	@Override
	protected void onResume()
	{
		super.onResume();
		mMediaRouter.addCallback(
			mMediaRouteSelector, mMediaRouterCallback, MediaRouter.CALLBACK_FLAG_REQUEST_DISCOVERY
		);
	}

	@Override
	protected void onPause()
	{
		if ( isFinishing() )
		{
			mMediaRouter.removeCallback( mMediaRouterCallback );
		}
		super.onPause();
	}


	/**
	 * Called when activity gets invisible. Connection to Play Services needs to
	 * be disconnected as soon as an activity is invisible.
	 */
	@Override
	protected void onStop()
	{
		if ( m_googleApiClient != null )
		{
			m_googleApiClient.disconnect();
		}
		super.onStop();
	}

	/**
	 * Saves the resolution state.
	 */
	@Override
	protected void onSaveInstanceState( Bundle outState )
	{
		super.onSaveInstanceState( outState );
		outState.putBoolean( KEY_IN_RESOLUTION, mIsInResolution );
	}

	/**
	 * Handles Google Play Services resolution callbacks.
	 */
	@Override
	protected void onActivityResult( int requestCode, int resultCode, Intent data )
	{
		super.onActivityResult( requestCode, resultCode, data );
		switch ( requestCode )
		{
			case REQUEST_CODE_RESOLUTION:
				retryConnecting();
				break;
		}
	}

	private void retryConnecting()
	{
		mIsInResolution = false;
		if ( !m_googleApiClient.isConnecting() )
		{
			m_googleApiClient.connect();
		}
	}

	/**
	 * Called when {@code m_googleApiClient} is connected.
	 */
	@Override
	public void onConnected( Bundle connectionHint )
	{
		Log.i( TAG, "GoogleApiClient connected" );
		// TODO: Start making API requests.


	}

	/**
	 * Called when {@code m_googleApiClient} connection is suspended.
	 */
	@Override
	public void onConnectionSuspended( int cause )
	{
		Log.i( TAG, "GoogleApiClient connection suspended" );
		retryConnecting();
	}

	/**
	 * Called when {@code m_googleApiClient} is trying to connect but failed.
	 * Handle {@code result.getResolution()} if there is a resolution
	 * available.
	 */
	@Override
	public void onConnectionFailed( ConnectionResult result )
	{
		Log.i( TAG, "GoogleApiClient connection failed: " + result.toString() );
		if ( !result.hasResolution() )
		{
			// Show a localized error dialog.
			GooglePlayServicesUtil.getErrorDialog(
				result.getErrorCode(), this, 0, new OnCancelListener()
				{
					@Override
					public void onCancel( DialogInterface dialog )
					{
						retryConnecting();
					}
				}
			).show();
			return;
		}
		// If there is an existing resolution error being displayed or a resolution
		// activity has started before, do nothing and wait for resolution
		// progress to be completed.
		if ( mIsInResolution )
		{
			return;
		}
		mIsInResolution = true;
		try
		{
			result.startResolutionForResult( this, REQUEST_CODE_RESOLUTION );
		}
		catch ( SendIntentException e )
		{
			Log.e( TAG, "Exception while starting resolution activity", e );
			retryConnecting();
		}
	}

	public void connect()
	{
		if ( m_googleApiClient == null )
		{
			Cast.CastOptions.Builder apiOptionsBuilder;
			apiOptionsBuilder = Cast.CastOptions.builder(
				m_castDevice, new Cast.Listener()
				{
					@Override public void onApplicationStatusChanged()
					{
						Log.d( "MainActivity", "onApplicationStatusChanged ()" );
					}

					@Override
					public void onApplicationDisconnected( final int statusCode )
					{
						Log.d(
							"MainActivity", String.format(
								"onApplicationDisconnected ([%s])", statusCode
							)
						);
						super.onApplicationDisconnected( statusCode );
					}

					@Override public void onVolumeChanged()
					{
						super.onVolumeChanged();
					}
				}
			).setVerboseLoggingEnabled( true );

			m_googleApiClient = new GoogleApiClient.Builder( MainActivity.this ).addApi(
				Cast.API, apiOptionsBuilder.build()
			)

				// Optionally, add additional APIs and scopes if required.
				.addConnectionCallbacks( MainActivity.this ).addOnConnectionFailedListener( MainActivity.this )
				.build();
		}
		m_googleApiClient.connect();
	}

	private void teardown()
	{
		Log.d( TAG, "teardown" );
		if ( m_googleApiClient != null )
		{
			if ( mApplicationStarted )
			{
				if ( m_googleApiClient.isConnected() || m_googleApiClient.isConnecting() )
				{

					Cast.CastApi.stopApplication( m_googleApiClient, mSessionId );


					m_googleApiClient.disconnect();
				}
				mApplicationStarted = false;
			}
			m_googleApiClient = null;
		}
		m_castDevice = null;
		//		mWaitingForReconnect = false;
		mSessionId = null;
	}

	public CastDevice getCastDevice()
	{
		return m_castDevice;
	}

	private class MyMediaRouterCallback extends MediaRouter.Callback
	{

		@Override
		public void onRouteSelected( MediaRouter router, MediaRouter.RouteInfo info )
		{

			m_castDevice = CastDevice.getFromBundle( info.getExtras() );
			String routeId = info.getId();
			Log.d( "MainActivity", String.format( "onRouteSelected : routeId = %s", routeId ) );

			connect();
		}

		@Override
		public void onRouteUnselected( MediaRouter router, MediaRouter.RouteInfo info )
		{
			teardown();
			m_castDevice = null;
		}

		@Override public void onRouteAdded(
			final MediaRouter router, final MediaRouter.RouteInfo route
		)
		{
			Log.d( "MainActivity", String.format( "onRouteAdded ([%s, %s])", router, route ) );
			super.onRouteAdded( router, route );
		}

		@Override public void onRouteChanged(
			final MediaRouter router, final MediaRouter.RouteInfo route
		)
		{
			Log.d( "MainActivity", String.format( "onRouteChanged ([%s, %s])", router, route ) );
			super.onRouteChanged( router, route );
		}
	}


}
