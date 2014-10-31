package com.swarmnyc.chromecastdemo;

import android.content.DialogInterface;
import android.content.DialogInterface.OnCancelListener;
import android.content.Intent;
import android.content.IntentSender.SendIntentException;
import android.os.Bundle;
import android.support.v4.view.MenuItemCompat;
import android.support.v7.app.ActionBarActivity;
import android.support.v7.app.MediaRouteActionProvider;
import android.support.v7.media.MediaRouteSelector;
import android.support.v7.media.MediaRouter;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import com.google.android.gms.cast.*;
import com.google.android.gms.common.ConnectionResult;
import com.google.android.gms.common.GooglePlayServicesUtil;
import com.google.android.gms.common.api.GoogleApiClient;
import com.google.android.gms.common.api.ResultCallback;
import com.google.android.gms.common.api.Status;

import java.io.IOException;

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
	private GoogleApiClient mGoogleApiClient;

	/**
	 * Determines if the client is in a resolution state, and
	 * waiting for resolution intent to return.
	 */
	private boolean                                       mIsInResolution;
	private MediaRouter                                   mMediaRouter;
	private MediaRouteSelector                            mMediaRouteSelector;
	private CastDevice                                    mSelectedDevice;
	private android.support.v7.media.MediaRouter.Callback mMediaRouterCallback;
	private RemoteMediaPlayer                             mRemoteMediaPlayer;
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


		findViewById( R.id.btn_play ).setOnClickListener(
			new View.OnClickListener()
			{
				@Override public void onClick( final View v )
				{
					play();
				}
			}
		);

		findViewById( R.id.btn_pause ).setOnClickListener(
			new View.OnClickListener()
			{
				@Override public void onClick( final View v )
				{
					pause();
				}
			}
		);

		findViewById( R.id.btn_stop ).setOnClickListener(
			new View.OnClickListener()
			{
				@Override public void onClick( final View v )
				{
					stop();
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

		mRemoteMediaPlayer = new RemoteMediaPlayer();

		mRemoteMediaPlayer.setOnStatusUpdatedListener(
			new RemoteMediaPlayer.OnStatusUpdatedListener()
			{
				@Override
				public void onStatusUpdated()
				{
					MediaStatus mediaStatus = mRemoteMediaPlayer.getMediaStatus();
					if ( null == mediaStatus )
					{
						return;
					}


				}
			}
		);

		mRemoteMediaPlayer.setOnMetadataUpdatedListener(
			new RemoteMediaPlayer.OnMetadataUpdatedListener()
			{
				@Override
				public void onMetadataUpdated()
				{
					MediaInfo mediaInfo = mRemoteMediaPlayer.getMediaInfo();
					//					MediaMetadata metadata = mediaInfo.getMetadata();
					//					...
				}
			}
		);
	}

	private void stop()
	{
		try
		{
			mRemoteMediaPlayer.stop( mGoogleApiClient );
		}
		catch ( IOException e )
		{
			Log.e( "MainActivity", "Error in stop ([])", e );
		}
	}

	private void pause()
	{
		mRemoteMediaPlayer.requestStatus( mGoogleApiClient ).setResultCallback(
			new ResultCallback<RemoteMediaPlayer.MediaChannelResult>()
			{
				@Override public void onResult(
					final RemoteMediaPlayer.MediaChannelResult mediaChannelResult
				)
				{

					if (mediaChannelResult.getStatus().isSuccess() )
					{
						final MediaStatus mediaStatus = mRemoteMediaPlayer.getMediaStatus();

						try
						{
							if ( mediaStatus.getPlayerState() == 0 ) // paused
							{
								mRemoteMediaPlayer.play( mGoogleApiClient );
							}
							else if ( mediaStatus.getPlayerState() == 0 )
							{

								mRemoteMediaPlayer.pause( mGoogleApiClient );

							}
						}
						catch ( IOException e )
						{
							Log.e( "MainActivity", "Error in onStatusUpdated ([])", e );
						}
					}
				}
			}
		);
	}

	@Override
	public boolean onCreateOptionsMenu( Menu menu )
	{
		super.onCreateOptionsMenu( menu );
		getMenuInflater().inflate( R.menu.main, menu );
		MenuItem mediaRouteMenuItem = menu.findItem( R.id.media_route_menu_item );
		MediaRouteActionProvider mediaRouteActionProvider = (MediaRouteActionProvider) MenuItemCompat
			.getActionProvider(
			mediaRouteMenuItem
		);
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
		if ( mGoogleApiClient != null )
		{
			mGoogleApiClient.disconnect();
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
		if ( !mGoogleApiClient.isConnecting() )
		{
			mGoogleApiClient.connect();
		}
	}

	public void play()
	{
		if ( mGoogleApiClient == null )
		{
			Cast.CastOptions.Builder apiOptionsBuilder;
			apiOptionsBuilder = Cast.CastOptions.builder(
				mSelectedDevice, new Cast.Listener()
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
			).setDebuggingEnabled();

			mGoogleApiClient = new GoogleApiClient.Builder( MainActivity.this ).addApi(
				Cast.API, apiOptionsBuilder.build()
			)

				// Optionally, add additional APIs and scopes if required.
				.addConnectionCallbacks( MainActivity.this ).addOnConnectionFailedListener( MainActivity.this )
				.build();
		}
		mGoogleApiClient.connect();
	}

	/**
	 * Called when {@code mGoogleApiClient} is connected.
	 */
	@Override
	public void onConnected( Bundle connectionHint )
	{
		Log.i( TAG, "GoogleApiClient connected" );
		// TODO: Start making API requests.


		Cast.CastApi.launchApplication(
			mGoogleApiClient, CastMediaControlIntent.DEFAULT_MEDIA_RECEIVER_APPLICATION_ID, false
		).setResultCallback(
			new ResultCallback<Cast.ApplicationConnectionResult>()
			{
				@Override
				public void onResult( Cast.ApplicationConnectionResult result )
				{
					Status status = result.getStatus();
					if ( status.isSuccess() )
					{
						ApplicationMetadata applicationMetadata = result.getApplicationMetadata();
						String sessionId = result.getSessionId();
						String applicationStatus = result.getApplicationStatus();
						boolean wasLaunched = result.getWasLaunched();

						mApplicationStarted = true;

						try
						{
							Cast.CastApi.setMessageReceivedCallbacks(
								mGoogleApiClient, mRemoteMediaPlayer.getNamespace(), mRemoteMediaPlayer
							);
						}
						catch ( IOException e )
						{
							Log.e( TAG, "Exception while creating media channel", e );
						}

						mRemoteMediaPlayer.requestStatus( mGoogleApiClient ).setResultCallback(
							new ResultCallback<RemoteMediaPlayer.MediaChannelResult>()
							{
								@Override
								public void onResult( RemoteMediaPlayer.MediaChannelResult result )
								{
									if ( !result.getStatus().isSuccess() )
									{
										Log.e( TAG, "Failed to request status." );
									}

									MediaMetadata mediaMetadata = new MediaMetadata( MediaMetadata.MEDIA_TYPE_MOVIE );
									mediaMetadata.putString( MediaMetadata.KEY_TITLE, "Somya's video" );
									MediaInfo mediaInfo = new MediaInfo.Builder(
										"http://distribution.bbb3d.renderfarming" +
										".net/video/mp4/bbb_sunflower_1080p_30fps_normal.mp4"
									).setContentType( "video/mp4" )
									 .setStreamType( MediaInfo.STREAM_TYPE_BUFFERED )
									 .setMetadata(
										 mediaMetadata
									 )
									 .build();
									try
									{
										mRemoteMediaPlayer.load( mGoogleApiClient, mediaInfo, true ).setResultCallback(
											new ResultCallback<RemoteMediaPlayer.MediaChannelResult>()
											{
												@Override
												public void onResult( RemoteMediaPlayer.MediaChannelResult result )
												{
													if ( result.getStatus().isSuccess() )
													{
														Log.d( TAG, "Media loaded successfully" );
													}
													else
													{
														Log.d( TAG, "Media load failed" );
													}
												}
											}
										);
									}
									catch ( IllegalStateException e )
									{
										Log.e( TAG, "Problem occurred with media during loading", e );
									}
									catch ( Exception e )
									{
										Log.e( TAG, "Problem opening media during loading", e );
									}
								}
							}
						);

					}
				}
			}
		);


	}

	/**
	 * Called when {@code mGoogleApiClient} connection is suspended.
	 */
	@Override
	public void onConnectionSuspended( int cause )
	{
		Log.i( TAG, "GoogleApiClient connection suspended" );
		retryConnecting();
	}

	/**
	 * Called when {@code mGoogleApiClient} is trying to connect but failed.
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

	private void teardown()
	{
		Log.d( TAG, "teardown" );
		if ( mGoogleApiClient != null )
		{
			if ( mApplicationStarted )
			{
				if ( mGoogleApiClient.isConnected() || mGoogleApiClient.isConnecting() )
				{

					Cast.CastApi.stopApplication( mGoogleApiClient, mSessionId );


					mGoogleApiClient.disconnect();
				}
				mApplicationStarted = false;
			}
			mGoogleApiClient = null;
		}
		mSelectedDevice = null;
		//		mWaitingForReconnect = false;
		mSessionId = null;
	}

	private class MyMediaRouterCallback extends MediaRouter.Callback
	{

		@Override
		public void onRouteSelected( MediaRouter router, MediaRouter.RouteInfo info )
		{

			mSelectedDevice = CastDevice.getFromBundle( info.getExtras() );
			String routeId = info.getId();
			Log.d( "MainActivity", String.format( "onRouteSelected : routeId = %s", routeId ) );


		}

		@Override
		public void onRouteUnselected( MediaRouter router, MediaRouter.RouteInfo info )
		{
			teardown();
			mSelectedDevice = null;
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
