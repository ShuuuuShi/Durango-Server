using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MMT;

public class MobileMovieTexture : MonoBehaviour
{
	public delegate void OnFinished(MobileMovieTexture sender);

	private const int CHANNELS = 3;

	private const string PLATFORM_DLL = "theorawrapper";

	[SerializeField]
	private string m_path;

	[SerializeField]
	private Material[] m_movieMaterials;

	[SerializeField]
	private bool m_playAutomatically = true;

	[SerializeField]
	private bool m_advance = true;

	[SerializeField]
	private int m_loopCount = -1;

	[SerializeField]
	private float m_playSpeed = 1f;

	[SerializeField]
	private bool m_scanDuration = true;

	[SerializeField]
	private bool m_seekKeyFrame;

	private IntPtr m_nativeContext = IntPtr.Zero;

	private IntPtr m_nativeTextureContext = IntPtr.Zero;

	private int m_picX;

	private int m_picY;

	private int m_yStride;

	private int m_yHeight;

	private int m_uvStride;

	private int m_uvHeight;

	private Vector2 m_uvYScale;

	private Vector2 m_uvYOffset;

	private Vector2 m_uvCrCbScale;

	private Vector2 m_uvCrCbOffset;

	private Texture2D[] m_ChannelTextures = (Texture2D[])(object)new Texture2D[3];

	private double m_elapsedTime;

	private bool m_hasFinished = true;

	public string Path
	{
		get
		{
			return m_path;
		}
		set
		{
			m_path = value;
		}
	}

	public bool AbsolutePath { get; set; }

	public Material[] MovieMaterial => m_movieMaterials;

	public bool PlayAutomatically
	{
		set
		{
			m_playAutomatically = value;
		}
	}

	public int LoopCount
	{
		get
		{
			return m_loopCount;
		}
		set
		{
			m_loopCount = value;
		}
	}

	public float PlaySpeed
	{
		get
		{
			return m_playSpeed;
		}
		set
		{
			m_playSpeed = value;
		}
	}

	public bool ScanDuration
	{
		get
		{
			return m_scanDuration;
		}
		set
		{
			m_scanDuration = value;
		}
	}

	public bool SeekKeyFrame
	{
		get
		{
			return m_seekKeyFrame;
		}
		set
		{
			m_seekKeyFrame = value;
		}
	}

	public int Width { get; private set; }

	public int Height { get; private set; }

	public float AspectRatio
	{
		get
		{
			if (m_nativeContext != IntPtr.Zero)
			{
				return GetAspectRatio(m_nativeContext);
			}
			return 1f;
		}
	}

	public double FPS
	{
		get
		{
			if (m_nativeContext != IntPtr.Zero)
			{
				return GetVideoFPS(m_nativeContext);
			}
			return 1.0;
		}
	}

	public bool IsPlaying => m_nativeContext != IntPtr.Zero && !m_hasFinished && m_advance;

	public bool Pause
	{
		get
		{
			return !m_advance;
		}
		set
		{
			m_advance = !value;
		}
	}

	public double PlayPosition
	{
		get
		{
			return m_elapsedTime;
		}
		set
		{
			if (m_nativeContext != IntPtr.Zero)
			{
				m_elapsedTime = Seek(m_nativeContext, value, m_seekKeyFrame);
			}
		}
	}

	public double Duration => (!(m_nativeContext != IntPtr.Zero)) ? 0.0 : GetDuration(m_nativeContext);

	public event OnFinished onFinished;

	public MobileMovieTexture()
	{
		Height = 0;
		Width = 0;
	}

	[DllImport("theorawrapper")]
	private static extern IntPtr CreateContext();

	[DllImport("theorawrapper")]
	private static extern void DestroyContext(IntPtr context);

	[DllImport("theorawrapper")]
	private static extern bool OpenStream(IntPtr context, string path, int offset, int size, bool pot, bool scanDuration, int maxSkipFrames);

	[DllImport("theorawrapper")]
	private static extern void CloseStream(IntPtr context);

	[DllImport("theorawrapper")]
	private static extern int GetPicWidth(IntPtr context);

	[DllImport("theorawrapper")]
	private static extern int GetPicHeight(IntPtr context);

	[DllImport("theorawrapper")]
	private static extern int GetPicX(IntPtr context);

	[DllImport("theorawrapper")]
	private static extern int GetPicY(IntPtr context);

	[DllImport("theorawrapper")]
	private static extern int GetYStride(IntPtr context);

	[DllImport("theorawrapper")]
	private static extern int GetYHeight(IntPtr context);

	[DllImport("theorawrapper")]
	private static extern int GetUVStride(IntPtr context);

	[DllImport("theorawrapper")]
	private static extern int GetUVHeight(IntPtr context);

	[DllImport("theorawrapper")]
	private static extern bool HasFinished(IntPtr context);

	[DllImport("theorawrapper")]
	private static extern double GetDecodedFrameTime(IntPtr context);

	[DllImport("theorawrapper")]
	private static extern double GetUploadedFrameTime(IntPtr context);

	[DllImport("theorawrapper")]
	private static extern double GetTargetDecodeFrameTime(IntPtr context);

	[DllImport("theorawrapper")]
	private static extern void SetTargetDisplayDecodeTime(IntPtr context, double targetTime);

	[DllImport("theorawrapper")]
	private static extern double GetVideoFPS(IntPtr context);

	[DllImport("theorawrapper")]
	private static extern float GetAspectRatio(IntPtr context);

	[DllImport("theorawrapper")]
	private static extern double Seek(IntPtr context, double seconds, bool waitKeyFrame);

	[DllImport("theorawrapper")]
	private static extern double GetDuration(IntPtr context);

	[DllImport("theorawrapper")]
	private static extern IntPtr GetNativeHandle(IntPtr context, int planeIndex);

	[DllImport("theorawrapper")]
	private static extern IntPtr GetNativeTextureContext(IntPtr context);

	[DllImport("theorawrapper")]
	private static extern void SetPostProcessingLevel(IntPtr context, int level);

	private void Start()
	{
		m_nativeContext = CreateContext();
		if (m_nativeContext == IntPtr.Zero)
		{
			Debug.LogError((object)"Unable to create Mobile Movie Texture native context");
		}
		else if (m_playAutomatically)
		{
			Play();
		}
	}

	private void OnDestroy()
	{
		DestroyTextures();
		DestroyContext(m_nativeContext);
	}

	private void Update()
	{
		if (!(m_nativeContext != IntPtr.Zero) || m_hasFinished)
		{
			return;
		}
		IntPtr nativeTextureContext = GetNativeTextureContext(m_nativeContext);
		if (nativeTextureContext != m_nativeTextureContext)
		{
			DestroyTextures();
			AllocateTexures();
			m_nativeTextureContext = nativeTextureContext;
		}
		m_hasFinished = HasFinished(m_nativeContext);
		if (!m_hasFinished)
		{
			if (m_advance)
			{
				m_elapsedTime += Time.deltaTime * Mathf.Max(m_playSpeed, 0f);
			}
		}
		else if (m_loopCount - 1 > 0 || m_loopCount == -1)
		{
			if (m_loopCount != -1)
			{
				m_loopCount--;
			}
			m_elapsedTime %= GetDecodedFrameTime(m_nativeContext);
			Seek(m_nativeContext, 0.0, waitKeyFrame: false);
			m_hasFinished = false;
		}
		else if (this.onFinished != null)
		{
			m_elapsedTime = GetDecodedFrameTime(m_nativeContext);
			this.onFinished(this);
		}
		SetTargetDisplayDecodeTime(m_nativeContext, m_elapsedTime);
	}

	public void Play()
	{
		m_elapsedTime = 0.0;
		Open();
		m_hasFinished = false;
		if ((Object)(object)MobileMovieManager.Instance == (Object)null)
		{
			((Component)this).gameObject.AddComponent<MobileMovieManager>();
		}
	}

	public void Stop()
	{
		CloseStream(m_nativeContext);
		m_hasFinished = true;
	}

	private void Open()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Invalid comparison between Unknown and I4
		string path = m_path;
		long offset = 0L;
		long length = 0L;
		if (!AbsolutePath)
		{
			RuntimePlatform platform = Application.platform;
			if ((int)platform == 11)
			{
				path = Application.dataPath;
				if (!AssetStream.GetZipFileOffsetLength(Application.dataPath, m_path, out offset, out length))
				{
					return;
				}
			}
			else
			{
				path = Application.streamingAssetsPath + "/" + m_path;
			}
		}
		if (m_nativeContext != IntPtr.Zero && OpenStream(m_nativeContext, path, (int)offset, (int)length, pot: false, m_scanDuration, 16))
		{
			Width = GetPicWidth(m_nativeContext);
			Height = GetPicHeight(m_nativeContext);
			m_picX = GetPicX(m_nativeContext);
			m_picY = GetPicY(m_nativeContext);
			m_yStride = GetYStride(m_nativeContext);
			m_yHeight = GetYHeight(m_nativeContext);
			m_uvStride = GetUVStride(m_nativeContext);
			m_uvHeight = GetUVHeight(m_nativeContext);
			CalculateUVScaleOffset();
		}
		else
		{
			Debug.LogError((object)("Unable to open movie " + m_nativeContext), (Object)(object)this);
		}
	}

	public void ReplaceMaterial(Material mat)
	{
		if (m_movieMaterials.Length > 0)
		{
			m_movieMaterials[0] = mat;
		}
	}

	private void AllocateTexures()
	{
		m_ChannelTextures[0] = Texture2D.CreateExternalTexture(m_yStride, m_yHeight, (TextureFormat)1, false, false, GetNativeHandle(m_nativeContext, 0));
		m_ChannelTextures[1] = Texture2D.CreateExternalTexture(m_uvStride, m_uvHeight, (TextureFormat)1, false, false, GetNativeHandle(m_nativeContext, 1));
		m_ChannelTextures[2] = Texture2D.CreateExternalTexture(m_uvStride, m_uvHeight, (TextureFormat)1, false, false, GetNativeHandle(m_nativeContext, 2));
		if (m_movieMaterials == null)
		{
			return;
		}
		for (int i = 0; i < m_movieMaterials.Length; i++)
		{
			Material val = m_movieMaterials[i];
			if ((Object)(object)val != (Object)null)
			{
				SetTextures(val);
			}
		}
	}

	public void SetTextures(Material material)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		material.SetTexture("_YTex", (Texture)(object)m_ChannelTextures[0]);
		material.SetTexture("_CbTex", (Texture)(object)m_ChannelTextures[1]);
		material.SetTexture("_CrTex", (Texture)(object)m_ChannelTextures[2]);
		material.SetTextureScale("_YTex", m_uvYScale);
		material.SetTextureOffset("_YTex", m_uvYOffset);
		material.SetTextureScale("_CbTex", m_uvCrCbScale);
		material.SetTextureOffset("_CbTex", m_uvCrCbOffset);
	}

	public void RemoveTextures(Material material)
	{
		material.SetTexture("_YTex", (Texture)null);
		material.SetTexture("_CbTex", (Texture)null);
		material.SetTexture("_CrTex", (Texture)null);
	}

	private void CalculateUVScaleOffset()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		float num = Width;
		float num2 = Height;
		float num3 = m_picX;
		float num4 = m_picY;
		float num5 = m_yStride;
		float num6 = m_yHeight;
		float num7 = m_uvStride;
		float num8 = m_uvHeight;
		m_uvYScale = new Vector2(num / num5, 0f - num2 / num6);
		m_uvYOffset = new Vector2(num3 / num5, (num2 + num4) / num6);
		m_uvCrCbScale = default(Vector2);
		m_uvCrCbOffset = default(Vector2);
		if (m_uvStride == m_yStride)
		{
			m_uvCrCbScale.x = m_uvYScale.x;
		}
		else
		{
			m_uvCrCbScale.x = num / 2f / num7;
		}
		if (m_uvHeight == m_yHeight)
		{
			m_uvCrCbScale.y = m_uvYScale.y;
			m_uvCrCbOffset = m_uvYOffset;
		}
		else
		{
			m_uvCrCbScale.y = 0f - num2 / 2f / num8;
			m_uvCrCbOffset = new Vector2(num3 / 2f / num7, (num2 + num4) / 2f / num8);
		}
	}

	private void DestroyTextures()
	{
		if (m_movieMaterials != null)
		{
			for (int i = 0; i < m_movieMaterials.Length; i++)
			{
				Material val = m_movieMaterials[i];
				if ((Object)(object)val != (Object)null)
				{
					RemoveTextures(val);
				}
			}
		}
		for (int j = 0; j < 3; j++)
		{
			if ((Object)(object)m_ChannelTextures[j] != (Object)null)
			{
				Object.Destroy((Object)(object)m_ChannelTextures[j]);
				m_ChannelTextures[j] = null;
			}
		}
	}
}
