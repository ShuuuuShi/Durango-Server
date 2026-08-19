using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class MediaPlayerCtrl : MonoBehaviour
{
	public enum MEDIAPLAYER_ERROR
	{
		MEDIA_ERROR_NOT_VALID_FOR_PROGRESSIVE_PLAYBACK = 200,
		MEDIA_ERROR_IO = -1004,
		MEDIA_ERROR_MALFORMED = -1007,
		MEDIA_ERROR_TIMED_OUT = -110,
		MEDIA_ERROR_UNSUPPORTED = -1010,
		MEDIA_ERROR_SERVER_DIED = 100,
		MEDIA_ERROR_UNKNOWN = 1
	}

	public enum MEDIAPLAYER_STATE
	{
		NOT_READY,
		READY,
		END,
		PLAYING,
		PAUSED,
		STOPPED,
		ERROR
	}

	public string m_strFileName;

	public bool m_bLoop;

	public bool m_bAutoPlay = true;

	private Texture2D m_VideoTexture;

	private Texture2D m_VideoTextureDummy;

	private MEDIAPLAYER_STATE m_CurrentState;

	private int m_iCurrentSeekPosition;

	private float m_fVolume = 1f;

	private int m_iAndroidMgrID;

	private bool m_bIsFirstFrameReady;

	private bool m_bFirst;

	private bool m_bStop;

	private bool m_bInit;

	private bool m_bPause;

	private bool m_bCheckFBO;

	private bool _supportRockchip;

	public Action OnReady;

	public Action OnEnd;

	public Action<MEDIAPLAYER_ERROR, MEDIAPLAYER_ERROR> OnVideoError;

	public Action OnVideoFirstFrameReady;

	private AndroidJavaObject javaObj;

	[DllImport("BlueDoveMediaRender")]
	private static extern void InitNDK();

	[DllImport("BlueDoveMediaRender")]
	private static extern IntPtr EasyMovieTextureRender();

	private void Awake()
	{
		_supportRockchip = SystemInfo.deviceModel.Contains("rockchip");
	}

	private void Start()
	{
		if (SystemInfo.graphicsMultiThreaded)
		{
			InitNDK();
		}
		m_iAndroidMgrID = Call_InitNDK();
		Call_SetUnityActivity();
		if (Application.dataPath.Contains(".obb"))
		{
			Call_SetSplitOBB(bValue: true, Application.dataPath);
		}
		else
		{
			Call_SetSplitOBB(bValue: false, null);
		}
		m_bInit = true;
	}

	private void OnDisable()
	{
		if (GetCurrentState() == MEDIAPLAYER_STATE.PLAYING)
		{
			Pause();
		}
	}

	private void OnEnable()
	{
		if (GetCurrentState() == MEDIAPLAYER_STATE.PAUSED)
		{
			Play();
		}
	}

	private void Update()
	{
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Expected O, but got Unknown
		if (string.IsNullOrEmpty(m_strFileName))
		{
			return;
		}
		if (!m_bFirst)
		{
			string strFileName = m_strFileName.Trim();
			Call_Load(strFileName, 0);
			Call_SetLooping(m_bLoop);
			m_bFirst = true;
		}
		if (m_CurrentState == MEDIAPLAYER_STATE.PLAYING || m_CurrentState == MEDIAPLAYER_STATE.PAUSED)
		{
			if (!m_bCheckFBO)
			{
				if (Call_GetVideoWidth() <= 0 || Call_GetVideoHeight() <= 0)
				{
					return;
				}
				if ((Object)(object)m_VideoTexture != (Object)null)
				{
					if ((Object)(object)m_VideoTextureDummy != (Object)null)
					{
						Object.Destroy((Object)(object)m_VideoTextureDummy);
						m_VideoTextureDummy = null;
					}
					m_VideoTextureDummy = m_VideoTexture;
					m_VideoTexture = null;
				}
				m_VideoTexture = new Texture2D(Call_GetVideoWidth(), Call_GetVideoHeight(), (TextureFormat)((!_supportRockchip) ? 4 : 7), false);
				((Texture)m_VideoTexture).filterMode = (FilterMode)1;
				((Texture)m_VideoTexture).wrapMode = (TextureWrapMode)1;
				Call_SetUnityTexture((int)((Texture)m_VideoTexture).GetNativeTexturePtr());
				Call_SetWindowSize();
				m_bCheckFBO = true;
			}
			Call_UpdateVideoTexture();
			m_iCurrentSeekPosition = Call_GetSeekPosition();
		}
		if (m_CurrentState != Call_GetStatus())
		{
			m_CurrentState = Call_GetStatus();
			if (m_CurrentState == MEDIAPLAYER_STATE.READY)
			{
				if (OnReady != null)
				{
					OnReady();
				}
				if (m_bAutoPlay)
				{
					Call_Play(0);
				}
				SetVolume(m_fVolume);
			}
			else if (m_CurrentState == MEDIAPLAYER_STATE.END)
			{
				if (OnEnd != null)
				{
					OnEnd();
				}
				if (m_bLoop)
				{
					Call_Play(0);
				}
			}
			else if (m_CurrentState == MEDIAPLAYER_STATE.ERROR)
			{
				OnError((MEDIAPLAYER_ERROR)Call_GetError(), (MEDIAPLAYER_ERROR)Call_GetErrorExtra());
			}
		}
		GL.InvalidateState();
	}

	private void OnError(MEDIAPLAYER_ERROR iCode, MEDIAPLAYER_ERROR iCodeExtra)
	{
		string text = iCode switch
		{
			MEDIAPLAYER_ERROR.MEDIA_ERROR_NOT_VALID_FOR_PROGRESSIVE_PLAYBACK => "MEDIA_ERROR_NOT_VALID_FOR_PROGRESSIVE_PLAYBACK", 
			MEDIAPLAYER_ERROR.MEDIA_ERROR_SERVER_DIED => "MEDIA_ERROR_SERVER_DIED", 
			MEDIAPLAYER_ERROR.MEDIA_ERROR_UNKNOWN => "MEDIA_ERROR_UNKNOWN", 
			_ => "Unknown error " + iCode, 
		} + " ";
		Debug.LogError(iCodeExtra switch
		{
			MEDIAPLAYER_ERROR.MEDIA_ERROR_IO => text + "MEDIA_ERROR_IO", 
			MEDIAPLAYER_ERROR.MEDIA_ERROR_MALFORMED => text + "MEDIA_ERROR_MALFORMED", 
			MEDIAPLAYER_ERROR.MEDIA_ERROR_TIMED_OUT => text + "MEDIA_ERROR_TIMED_OUT", 
			MEDIAPLAYER_ERROR.MEDIA_ERROR_UNSUPPORTED => text + "MEDIA_ERROR_UNSUPPORTED", 
			_ => "Unknown error " + iCode, 
		});
		if (OnVideoError != null)
		{
			OnVideoError(iCode, iCodeExtra);
		}
	}

	private void OnDestroy()
	{
		Call_UnLoad();
		if ((Object)(object)m_VideoTextureDummy != (Object)null)
		{
			Object.Destroy((Object)(object)m_VideoTextureDummy);
			m_VideoTextureDummy = null;
		}
		if ((Object)(object)m_VideoTexture != (Object)null)
		{
			Object.Destroy((Object)(object)m_VideoTexture);
		}
		Call_Destroy();
	}

	private void OnApplicationPause(bool bPause)
	{
		if (bPause)
		{
			if (m_CurrentState == MEDIAPLAYER_STATE.PAUSED)
			{
				m_bPause = true;
			}
			Call_Pause();
			return;
		}
		Call_RePlay();
		if (m_bPause)
		{
			Call_Pause();
			m_bPause = false;
		}
	}

	public MEDIAPLAYER_STATE GetCurrentState()
	{
		return m_CurrentState;
	}

	public Texture2D GetVideoTexture()
	{
		return m_VideoTexture;
	}

	public void Play()
	{
		if (m_bStop)
		{
			Call_Play(0);
			m_bStop = false;
		}
		if (m_CurrentState == MEDIAPLAYER_STATE.PAUSED)
		{
			Call_RePlay();
		}
		else if (m_CurrentState == MEDIAPLAYER_STATE.READY || m_CurrentState == MEDIAPLAYER_STATE.STOPPED || m_CurrentState == MEDIAPLAYER_STATE.END)
		{
			Call_Play(0);
		}
	}

	public void Stop()
	{
		if (m_CurrentState == MEDIAPLAYER_STATE.PLAYING)
		{
			Call_Pause();
		}
		m_bStop = true;
		m_CurrentState = MEDIAPLAYER_STATE.STOPPED;
		m_iCurrentSeekPosition = 0;
	}

	public void Pause()
	{
		if (m_CurrentState == MEDIAPLAYER_STATE.PLAYING)
		{
			Call_Pause();
		}
		m_CurrentState = MEDIAPLAYER_STATE.PAUSED;
	}

	public void Load(string strFileName)
	{
		if (GetCurrentState() != 0)
		{
			UnLoad();
		}
		m_bIsFirstFrameReady = false;
		m_bFirst = false;
		m_bCheckFBO = false;
		m_strFileName = strFileName;
		if (m_bInit)
		{
			m_CurrentState = MEDIAPLAYER_STATE.NOT_READY;
		}
	}

	public void SetVolume(float fVolume)
	{
		if (m_CurrentState == MEDIAPLAYER_STATE.PLAYING || m_CurrentState == MEDIAPLAYER_STATE.PAUSED || m_CurrentState == MEDIAPLAYER_STATE.END || m_CurrentState == MEDIAPLAYER_STATE.READY || m_CurrentState == MEDIAPLAYER_STATE.STOPPED)
		{
			m_fVolume = fVolume;
			Call_SetVolume(fVolume);
		}
	}

	public int GetSeekPosition()
	{
		if (m_CurrentState == MEDIAPLAYER_STATE.PLAYING || m_CurrentState == MEDIAPLAYER_STATE.PAUSED || m_CurrentState == MEDIAPLAYER_STATE.END)
		{
			return m_iCurrentSeekPosition;
		}
		return 0;
	}

	public void SeekTo(int iSeek)
	{
		if (m_CurrentState == MEDIAPLAYER_STATE.PLAYING || m_CurrentState == MEDIAPLAYER_STATE.READY || m_CurrentState == MEDIAPLAYER_STATE.PAUSED || m_CurrentState == MEDIAPLAYER_STATE.END || m_CurrentState == MEDIAPLAYER_STATE.STOPPED)
		{
			Call_SetSeekPosition(iSeek);
		}
	}

	public int GetDuration()
	{
		if (m_CurrentState == MEDIAPLAYER_STATE.PLAYING || m_CurrentState == MEDIAPLAYER_STATE.PAUSED || m_CurrentState == MEDIAPLAYER_STATE.END || m_CurrentState == MEDIAPLAYER_STATE.READY || m_CurrentState == MEDIAPLAYER_STATE.STOPPED)
		{
			return Call_GetDuration();
		}
		return 0;
	}

	public int GetCurrentSeekPercent()
	{
		if (m_CurrentState == MEDIAPLAYER_STATE.PLAYING || m_CurrentState == MEDIAPLAYER_STATE.PAUSED || m_CurrentState == MEDIAPLAYER_STATE.END || m_CurrentState == MEDIAPLAYER_STATE.READY)
		{
			return Call_GetCurrentSeekPercent();
		}
		return 0;
	}

	public int GetVideoWidth()
	{
		return Call_GetVideoWidth();
	}

	public int GetVideoHeight()
	{
		return Call_GetVideoHeight();
	}

	public void UnLoad()
	{
		m_bCheckFBO = false;
		Call_UnLoad();
		m_CurrentState = MEDIAPLAYER_STATE.NOT_READY;
	}

	private AndroidJavaObject GetJavaObject()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		if (javaObj == null)
		{
			javaObj = new AndroidJavaObject("com.EasyMovieTexture.EasyMovieTexture", new object[0]);
		}
		return javaObj;
	}

	private void Call_Destroy()
	{
		if (SystemInfo.graphicsMultiThreaded)
		{
			GL.IssuePluginEvent(EasyMovieTextureRender(), 5 + m_iAndroidMgrID * 10 + 7000);
		}
		else
		{
			GetJavaObject().Call("Destroy", new object[0]);
		}
	}

	private void Call_UnLoad()
	{
		if (SystemInfo.graphicsMultiThreaded)
		{
			GL.IssuePluginEvent(EasyMovieTextureRender(), 4 + m_iAndroidMgrID * 10 + 7000);
		}
		else
		{
			GetJavaObject().Call("UnLoad", new object[0]);
		}
	}

	private bool Call_Load(string strFileName, int iSeek)
	{
		if (SystemInfo.graphicsMultiThreaded)
		{
			GetJavaObject().Call("NDK_SetFileName", new object[1] { strFileName });
			GL.IssuePluginEvent(EasyMovieTextureRender(), 1 + m_iAndroidMgrID * 10 + 7000);
			Call_SetNotReady();
			return true;
		}
		GetJavaObject().Call("NDK_SetFileName", new object[1] { strFileName });
		if (GetJavaObject().Call<bool>("Load", new object[0]))
		{
			return true;
		}
		OnError(MEDIAPLAYER_ERROR.MEDIA_ERROR_UNKNOWN, MEDIAPLAYER_ERROR.MEDIA_ERROR_UNKNOWN);
		return false;
	}

	private void Call_UpdateVideoTexture()
	{
		if (!Call_IsUpdateFrame())
		{
			return;
		}
		if ((Object)(object)m_VideoTextureDummy != (Object)null)
		{
			Object.Destroy((Object)(object)m_VideoTextureDummy);
			m_VideoTextureDummy = null;
		}
		if (SystemInfo.graphicsMultiThreaded)
		{
			GL.IssuePluginEvent(EasyMovieTextureRender(), 3 + m_iAndroidMgrID * 10 + 7000);
		}
		else
		{
			GetJavaObject().Call("UpdateVideoTexture", new object[0]);
		}
		if (!m_bIsFirstFrameReady)
		{
			m_bIsFirstFrameReady = true;
			if (OnVideoFirstFrameReady != null)
			{
				OnVideoFirstFrameReady();
				OnVideoFirstFrameReady = null;
			}
		}
	}

	private void Call_SetVolume(float fVolume)
	{
		GetJavaObject().Call("SetVolume", new object[1] { fVolume });
	}

	private void Call_SetSeekPosition(int iSeek)
	{
		GetJavaObject().Call("SetSeekPosition", new object[1] { iSeek });
	}

	private int Call_GetSeekPosition()
	{
		return GetJavaObject().Call<int>("GetSeekPosition", new object[0]);
	}

	private void Call_Play(int iSeek)
	{
		GetJavaObject().Call("Play", new object[1] { iSeek });
	}

	private void Call_Reset()
	{
		GetJavaObject().Call("Reset", new object[0]);
	}

	private void Call_Stop()
	{
		GetJavaObject().Call("Stop", new object[0]);
	}

	private void Call_RePlay()
	{
		GetJavaObject().Call("RePlay", new object[0]);
	}

	private void Call_Pause()
	{
		GetJavaObject().Call("Pause", new object[0]);
	}

	private int Call_InitNDK()
	{
		return GetJavaObject().Call<int>("InitNative", new object[1] { GetJavaObject() });
	}

	private int Call_GetVideoWidth()
	{
		return GetJavaObject().Call<int>("GetVideoWidth", new object[0]);
	}

	private int Call_GetVideoHeight()
	{
		return GetJavaObject().Call<int>("GetVideoHeight", new object[0]);
	}

	private bool Call_IsUpdateFrame()
	{
		return GetJavaObject().Call<bool>("IsUpdateFrame", new object[0]);
	}

	private void Call_SetUnityTexture(int iTextureID)
	{
		GetJavaObject().Call("SetUnityTexture", new object[1] { iTextureID });
	}

	private void Call_SetWindowSize()
	{
		if (SystemInfo.graphicsMultiThreaded)
		{
			GL.IssuePluginEvent(EasyMovieTextureRender(), 2 + m_iAndroidMgrID * 10 + 7000);
		}
		else
		{
			GetJavaObject().Call("SetWindowSize", new object[0]);
		}
	}

	private void Call_SetLooping(bool bLoop)
	{
		GetJavaObject().Call("SetLooping", new object[1] { bLoop });
	}

	private void Call_SetRockchip(bool bValue)
	{
		GetJavaObject().Call("SetRockchip", new object[1] { bValue });
	}

	private int Call_GetDuration()
	{
		return GetJavaObject().Call<int>("GetDuration", new object[0]);
	}

	private int Call_GetCurrentSeekPercent()
	{
		return GetJavaObject().Call<int>("GetCurrentSeekPercent", new object[0]);
	}

	private int Call_GetError()
	{
		return GetJavaObject().Call<int>("GetError", new object[0]);
	}

	private void Call_SetSplitOBB(bool bValue, string strOBBName)
	{
		GetJavaObject().Call("SetSplitOBB", new object[2] { bValue, strOBBName });
	}

	private int Call_GetErrorExtra()
	{
		return GetJavaObject().Call<int>("GetErrorExtra", new object[0]);
	}

	private void Call_SetUnityActivity()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		AndroidJavaClass val = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject @static = ((AndroidJavaObject)val).GetStatic<AndroidJavaObject>("currentActivity");
		GetJavaObject().Call("SetUnityActivity", new object[1] { @static });
		if (SystemInfo.graphicsMultiThreaded)
		{
			GL.IssuePluginEvent(EasyMovieTextureRender(), 0 + m_iAndroidMgrID * 10 + 7000);
		}
		else
		{
			Call_InitJniManager();
		}
	}

	private void Call_SetNotReady()
	{
		GetJavaObject().Call("SetNotReady", new object[0]);
	}

	private void Call_InitJniManager()
	{
		GetJavaObject().Call("InitJniManager", new object[0]);
	}

	private MEDIAPLAYER_STATE Call_GetStatus()
	{
		return (MEDIAPLAYER_STATE)GetJavaObject().Call<int>("GetStatus", new object[0]);
	}
}
