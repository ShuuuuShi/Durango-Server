using System.Collections.Generic;
using System.IO;
using APNGLib;
using JetBrains.Annotations;
using UnityEngine;

public class ApngTexture : MonoBehaviour
{
	private struct ImageStruct
	{
		public Texture2D Texture;

		public float Duration;

		public Rect Rect;
	}

	[SerializeField]
	private int _textureWidth;

	[SerializeField]
	private int _textureHeight;

	[SerializeField]
	private Color _background;

	[SerializeField]
	private float _transitionPeriod;

	private float _nextFrameTimer;

	private float _transitionTimer;

	private int _frameIndex;

	private bool _isTransition;

	private UITexture _textureWidget;

	private MeshRenderer _meshRenderer;

	private Material _material;

	private ImageStruct[] _frames;

	private int _index = -1;

	private APNG _image;

	private static int _mainTex;

	private static int _blendTex;

	private static int _ratio;

	private static int _bgColor;

	private bool _isInit;

	public int TextureWidth
	{
		get
		{
			return _textureWidth;
		}
		set
		{
			_textureWidth = value;
		}
	}

	public int TextureHeight
	{
		get
		{
			return _textureHeight;
		}
		set
		{
			_textureHeight = value;
		}
	}

	public Color Background
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _background;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_background = value;
		}
	}

	public float TransitionPeriod
	{
		get
		{
			return _transitionPeriod;
		}
		set
		{
			_transitionPeriod = value;
		}
	}

	public bool IsVisible => ((Object)(object)_textureWidget != (Object)null && _textureWidget.isVisible) || ((Object)(object)_meshRenderer != (Object)null && ((Renderer)_meshRenderer).isVisible);

	public int FrameLength => (_frames != null) ? _frames.Length : 0;

	private void Init()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		if (!_isInit)
		{
			_isInit = true;
			Material material = new Material(Shader.Find("Custom/BlendingTexture"));
			_material = material;
			_textureWidget = ((Component)this).GetComponent<UITexture>();
			if ((Object)(object)_textureWidget != (Object)null)
			{
				_textureWidget.material = _material;
			}
			_meshRenderer = ((Component)this).GetComponent<MeshRenderer>();
			if ((Object)(object)_meshRenderer != (Object)null)
			{
				((Renderer)_meshRenderer).sharedMaterial = _material;
			}
			_mainTex = Shader.PropertyToID("_MainTex");
			_blendTex = Shader.PropertyToID("_BlendTex");
			_ratio = Shader.PropertyToID("_Ratio");
			_bgColor = Shader.PropertyToID("_BgColor");
		}
	}

	public void Set([NotNull] APNG apng)
	{
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Expected O, but got Unknown
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Expected O, but got Unknown
		if (_image == apng)
		{
			return;
		}
		ImageStruct[] array;
		if (apng.IsAnimated)
		{
			int frameCount = apng.FrameCount;
			array = new ImageStruct[frameCount];
			int width = (int)apng.Width;
			int height = (int)apng.Height;
			for (int i = 0; i < frameCount; i++)
			{
				Frame frame = apng.GetFrame(i);
				array[i].Duration = frame.Seconds;
				array[i].Rect = new Rect((float)frame.XOffset, (float)(height - (frame.YOffset + frame.Height)), (float)frame.Width, (float)frame.Height);
				array[i].Rect = UIUtility.DivideRect(array[i].Rect, width, height);
				Stream stream = apng.ToStream(i);
				byte[] array2 = new byte[stream.Length];
				stream.Read(array2, 0, array2.Length);
				array[i].Texture = new Texture2D(0, 0);
				array[i].Texture.LoadImage(array2);
			}
		}
		else
		{
			array = new ImageStruct[1];
			Stream stream2 = apng.DefaultImageToStream();
			byte[] array3 = new byte[stream2.Length];
			stream2.Read(array3, 0, array3.Length);
			array[0].Texture = new Texture2D(0, 0);
			array[0].Texture.LoadImage(array3);
			array[0].Rect = new Rect(0f, 0f, 1f, 1f);
		}
		Set((IList<ImageStruct>)array);
		_image = apng;
	}

	public void Set(IList<Texture2D> texture, float second)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		ImageStruct[] array = new ImageStruct[texture.Count];
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			array[i].Texture = texture[i];
			array[i].Duration = second;
			array[i].Rect = new Rect(0f, 0f, 1f, 1f);
		}
		Set((IList<ImageStruct>)array);
	}

	public void Set(Texture2D texture)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		ImageStruct[] array = new ImageStruct[1];
		array[0].Texture = new Texture2D(0, 0);
		array[0].Texture = texture;
		array[0].Rect = new Rect(0f, 0f, 1f, 1f);
		Set((IList<ImageStruct>)array);
	}

	private void Set(IList<ImageStruct> images)
	{
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Expected O, but got Unknown
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		Init();
		_image = null;
		int num = _textureWidth;
		int num2 = _textureHeight;
		if (num == 0)
		{
			int num3 = 0;
			for (int i = 0; i < images.Count; i++)
			{
				num3 = Mathf.Max(((Texture)images[i].Texture).width, num3);
			}
			num = num3;
		}
		if (num2 == 0)
		{
			int num4 = 0;
			for (int j = 0; j < images.Count; j++)
			{
				num4 = Mathf.Max(((Texture)images[j].Texture).height, num4);
			}
			num2 = num4;
		}
		int count = images.Count;
		_frames = new ImageStruct[count];
		for (int k = 0; k < count; k++)
		{
			Rect rect = images[k].Rect;
			int num5 = Mathf.RoundToInt((float)num * ((Rect)(ref rect)).width);
			int num6 = Mathf.RoundToInt((float)num2 * ((Rect)(ref rect)).height);
			Color32[] array = UIUtility.ResizeTexturePixels(images[k].Texture, num5, num6);
			Texture2D val = new Texture2D(num, num2);
			((Texture)val).filterMode = (FilterMode)0;
			((Texture)val).wrapMode = (TextureWrapMode)1;
			for (int l = 0; l < ((Texture)val).width; l++)
			{
				for (int m = 0; m < ((Texture)val).height; m++)
				{
					val.SetPixel(l, m, Color.clear);
				}
			}
			val.SetPixels32(Mathf.RoundToInt(((Rect)(ref rect)).x), Mathf.RoundToInt(((Rect)(ref rect)).y), num5, num6, array);
			val.Apply();
			_frames[k].Texture = val;
			_frames[k].Duration = images[k].Duration;
		}
		_frameIndex = 0;
		_isTransition = false;
		SetFrame(0f);
	}

	private void Update()
	{
		PlayRoutine();
	}

	private void PlayRoutine()
	{
		if (FrameLength <= 1)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		bool flag = false;
		if (_isTransition)
		{
			_transitionTimer -= deltaTime;
			if (_transitionTimer <= 0f)
			{
				flag = true;
			}
		}
		else if (_nextFrameTimer > 0f)
		{
			_nextFrameTimer -= deltaTime;
		}
		else if (_transitionPeriod > 0f)
		{
			_isTransition = true;
			_transitionTimer = _transitionPeriod + _nextFrameTimer;
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			_frameIndex = Mathf.FloorToInt((float)(_frameIndex + 1)) % _frames.Length;
			_isTransition = false;
			SetFrame(_frameIndex);
			_nextFrameTimer += _frames[_frameIndex].Duration;
		}
		else if (_transitionTimer > 0f && IsVisible)
		{
			float num = Mathf.Clamp01(_transitionTimer / _transitionPeriod);
			SetFrame((float)_frameIndex + 1f - num);
		}
	}

	public void SetFrame(float frame)
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.FloorToInt(frame) % _frames.Length;
		int num2 = (num + 1) % _frames.Length;
		if (_index != num)
		{
			_index = num;
			_material.SetTexture(_mainTex, (Texture)(object)_frames[num].Texture);
			_material.SetTexture(_blendTex, (Texture)(object)_frames[num2].Texture);
			_material.SetColor(_bgColor, _background);
		}
		_material.SetFloat(_ratio, frame % 1f);
	}
}
