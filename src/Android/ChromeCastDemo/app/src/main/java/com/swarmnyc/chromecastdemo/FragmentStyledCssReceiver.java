package com.swarmnyc.chromecastdemo;

import android.os.Bundle;
import android.support.v4.app.Fragment;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import com.google.android.gms.cast.*;
import com.google.android.gms.common.api.GoogleApiClient;
import com.google.android.gms.common.api.ResultCallback;
import com.google.android.gms.common.api.Status;

import java.io.IOException;

/**
 * Created by somya on 10/31/14.
 */
public class FragmentStyledCssReceiver extends Fragment
{

	private Button            m_startAppButton;
	private Button            m_pausePlayButton;
	private Button            m_stopAppButton;
	private RemoteMediaPlayer mRemoteMediaPlayer;
	private boolean           mApplicationStarted;
	private GoogleApiClient   m_googleApiClient;
	private String TAG = "DefaultReceiverFragment";


	@Override public View onCreateView(
		final LayoutInflater inflater, final ViewGroup container, final Bundle savedInstanceState
	)
	{
		return inflater.inflate( R.layout.fragment_default, container, false );
	}

	@Override public void onViewCreated( final View view, final Bundle savedInstanceState )
	{
		m_startAppButton = (Button) view.findViewById( R.id.btn_start_app );
		m_pausePlayButton = (Button) view.findViewById( R.id.btn_pause_play );
		m_stopAppButton = (Button) view.findViewById( R.id.btn_stop_app );


		m_startAppButton.setOnClickListener(
			new View.OnClickListener()
			{
				@Override public void onClick( final View v )
				{
					startStyledReciever();
				}
			}
		);

		m_pausePlayButton.setOnClickListener(
			new View.OnClickListener()
			{
				@Override public void onClick( final View v )
				{
					pauseOrPlay();
				}
			}
		);

		m_stopAppButton.setOnClickListener(
			new View.OnClickListener()
			{
				@Override public void onClick( final View v )
				{
					stopMedia();
				}
			}
		);

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

		super.onViewCreated( view, savedInstanceState );
	}

	private void stopMedia()
	{

		mRemoteMediaPlayer.stop( m_googleApiClient );
	}

	private void pauseOrPlay()
	{
		mRemoteMediaPlayer.requestStatus( m_googleApiClient ).setResultCallback(
			new ResultCallback<RemoteMediaPlayer.MediaChannelResult>()
			{
				@Override public void onResult(
					final RemoteMediaPlayer.MediaChannelResult mediaChannelResult
				)
				{

					if ( mediaChannelResult.getStatus().isSuccess() )
					{
						final MediaStatus mediaStatus = mRemoteMediaPlayer.getMediaStatus();

						if ( mediaStatus.getPlayerState() == MediaStatus.PLAYER_STATE_PAUSED ) // paused
						{
							mRemoteMediaPlayer.play( m_googleApiClient );
							m_pausePlayButton.setText( R.string.pause );
						}
						else if ( mediaStatus.getPlayerState() == MediaStatus.PLAYER_STATE_PLAYING )
						{

							mRemoteMediaPlayer.pause( m_googleApiClient );
							m_pausePlayButton.setText( R.string.play );

						}
					}
				}
			}
		);
	}


	public void startStyledReciever()
	{

		Cast.CastApi.launchApplication(
			m_googleApiClient, "F7C9F7B8", false
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
								m_googleApiClient, mRemoteMediaPlayer.getNamespace(), mRemoteMediaPlayer
							);
						}
						catch ( IOException e )
						{
							Log.e( TAG, "Exception while creating media channel", e );
						}

						mRemoteMediaPlayer.requestStatus( m_googleApiClient ).setResultCallback(
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
									mediaMetadata.putString( MediaMetadata.KEY_TITLE, "Styled video Demo" );
									MediaInfo mediaInfo = new MediaInfo.Builder(
										"http://distribution.bbb3d.renderfarming"
										+ ".net/video/mp4/bbb_sunflower_1080p_30fps_normal.mp4"
									).setContentType( "video/mp4" )
									 .setStreamType( MediaInfo.STREAM_TYPE_BUFFERED )
									 .setMetadata(
										 mediaMetadata
									 )
									 .build();
									try
									{
										mRemoteMediaPlayer.load(
											m_googleApiClient, mediaInfo, true
										).setResultCallback(
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


	public GoogleApiClient getGoogleApiClient()
	{
		return m_googleApiClient;
	}

	public void setGoogleApiClient( final GoogleApiClient googleApiClient )
	{
		m_googleApiClient = googleApiClient;
	}
}
