using UnityEngine;
using System.IO;
using System.Collections.Generic;
#if UNITY_ANDROID || UNITY_IOS
using NativeShareNamespace;
#endif

#pragma warning disable 0414
public class NativeShare
{
	public enum ShareResult { Unknown = 0, Shared = 1, NotShared = 2 };

	public delegate void ShareResultCallback( ShareResult result, string shareTarget );

#if !UNITY_EDITOR && UNITY_ANDROID
	private static AndroidJavaClass m_ajc = null;
	private static AndroidJavaClass AJC
	{
		get
		{
			if( m_ajc == null )
				m_ajc = new AndroidJavaClass( "com.yasirkula.unity.NativeShare" );

			return m_ajc;
		}
	}

	private static AndroidJavaObject m_context = null;
	private static AndroidJavaObject Context
	{
		get
		{
			if( m_context == null )
			{
				using( AndroidJavaObject unityClass = new AndroidJavaClass( "com.unity3d.player.UnityPlayer" ) )
				{
					m_context = unityClass.GetStatic<AndroidJavaObject>( "currentActivity" );
				}
			}

			return m_context;
		}
	}
#elif !UNITY_EDITOR && UNITY_IOS
	[System.Runtime.InteropServices.DllImport( "__Internal" )]
	private static extern void _NativeShare_Share( string[] files, int filesCount, string subject, string text, string link );
#endif

	private string subject = string.Empty;
	private string title = string.Empty;
	private string url = string.Empty;

	private ShareResultCallback callback;

	public NativeShare SetSubject( string subject )
	{
		this.subject = subject ?? string.Empty;
		return this;
	}

	public NativeShare SetUrl( string url )
	{
		this.url = url ?? string.Empty;
		return this;
	}

	public NativeShare SetTitle( string title )
	{
		this.title = title ?? string.Empty;
		return this;
	}

	public NativeShare SetCallback( ShareResultCallback callback )
	{
		this.callback = callback;
		return this;
	}

	public void Share()
	{
		if(subject.Length == 0 && url.Length == 0 )
		{
			Debug.LogWarning( "Share Error: attempting to share nothing!" );
			return;
		}

#if UNITY_EDITOR
		Debug.Log( "Shared!" );

		if( callback != null )
			callback( ShareResult.Shared, null );
#elif UNITY_ANDROID
		AJC.CallStatic( "Share", Context, new NSShareResultCallbackAndroid( callback ), new string[0], new string[0], new string[0], new string[0], subject, url, title );
#elif UNITY_IOS
		NSShareResultCallbackiOS.Initialize( callback );
		_NativeShare_Share(new string[0], 0, subject, "", url);
#else
		Debug.LogWarning( "NativeShare is not supported on this platform!" );
#endif
    }
}
#pragma warning restore 0414