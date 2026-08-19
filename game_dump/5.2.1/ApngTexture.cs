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

	private Color _color = Color.white;

	private float _nextFrameTimer;

	private float _transitionTimer;

	private int _frameIndex;

	private bool _isTransition;

	private bool _isUITexture;

	private UITexture _textureWidget;

	private MeshRenderer _meshRenderer;

	private Material _material;

	private ImageStruct[] _frames;

	private int _index;

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
			return _background;
		}
		set
		{
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

	public bool IsVisible
	{
		get
		{
			if (!(_textureWidget != null) || !_textureWidget.isVisible)
			{
				if (_meshRenderer != null)
				{
					return _meshRenderer.isVisible;
				}
				return false;
			}
			return true;
		}
	}

	public int FrameLength
	{
		get
		{
			if (_frames == null)
			{
				return 0;
			}
			return _frames.Length;
		}
	}

	public Color Color
	{
		get
		{
			return _color;
		}
		set
		{
			if (!(_color == value))
			{
				_color = value;
				if (_isInit)
				{
					_material.color = _color;
				}
			}
		}
	}

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		_textureWidget = GetComponent<UITexture>();
		_meshRenderer = GetComponent<MeshRenderer>();
		if (_textureWidget != null)
		{
			_material = new Material(Shader.Find("Durango/NGUI/Transparent"));
			_isUITexture = true;
			_textureWidget.material = _material;
		}
		else
		{
			Shader shader = Shader.Find("Durango/Custom/BlendingTexture");
			Material material = null;
			if (_meshRenderer != null)
			{
				material = _meshRenderer.sharedMaterial;
				if (material.shader != shader)
				{
					material = null;
				}
			}
			_material = ((!(material != null)) ? new Material(shader) : new Material(material));
		}
		if (_meshRenderer != null)
		{
			_meshRenderer.sharedMaterial = _material;
		}
		_mainTex = Shader.PropertyToID("_MainTex");
		_blendTex = Shader.PropertyToID("_BlendTex");
		_ratio = Shader.PropertyToID("_Ratio");
		_bgColor = Shader.PropertyToID("_BgColor");
	}

	public void Set([NotNull] APNG apng)
	{
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
				array[i].Rect = new Rect(frame.XOffset, height - (frame.YOffset + frame.Height), frame.Width, frame.Height);
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
		Set(array);
		_image = apng;
	}

	public void Set(IList<Texture2D> texture, float second)
	{
		ImageStruct[] array = new ImageStruct[texture.Count];
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			array[i].Texture = texture[i];
			array[i].Duration = second;
			array[i].Rect = new Rect(0f, 0f, 1f, 1f);
		}
		Set(array);
	}

	public void Set(Texture2D texture)
	{
		Set(texture, new Rect(0f, 0f, 1f, 1f));
	}

	public void Set(Texture2D texture, Rect uv)
	{
		Texture2D texture2D;
		if (uv == new Rect(0f, 0f, 1f, 1f))
		{
			texture2D = texture;
		}
		else
		{
			int width = Mathf.RoundToInt((float)texture.width * uv.width);
			int height = Mathf.RoundToInt((float)texture.height * uv.height);
			Color32[] pixels = UIUtility.ResizeTexturePixels(texture, uv, width, height);
			texture2D = new Texture2D(width, height);
			texture2D.SetPixels32(pixels);
		}
		ImageStruct[] array = new ImageStruct[1];
		array[0].Texture = texture2D;
		array[0].Rect = new Rect(0f, 0f, 1f, 1f);
		Set(array);
	}

	private void Set(IList<ImageStruct> images)
	{
		Init();
		_image = null;
		int num = _textureWidth;
		int num2 = _textureHeight;
		if (num == 0)
		{
			int num3 = 0;
			for (int i = 0; i < images.Count; i++)
			{
				num3 = Mathf.Max(images[i].Texture.width, num3);
			}
			num = num3;
		}
		if (num2 == 0)
		{
			int num4 = 0;
			for (int j = 0; j < images.Count; j++)
			{
				num4 = Mathf.Max(images[j].Texture.height, num4);
			}
			num2 = num4;
		}
		int count = images.Count;
		_frames = new ImageStruct[count];
		for (int k = 0; k < count; k++)
		{
			Rect rect = images[k].Rect;
			int num5 = Mathf.RoundToInt((float)num * rect.width);
			int num6 = Mathf.RoundToInt((float)num2 * rect.height);
			Color32[] colors = UIUtility.ResizeTexturePixels(images[k].Texture, num5, num6);
			Texture2D texture2D = new Texture2D(num, num2);
			texture2D.filterMode = FilterMode.Point;
			texture2D.wrapMode = TextureWrapMode.Clamp;
			for (int l = 0; l < texture2D.width; l++)
			{
				for (int m = 0; m < texture2D.height; m++)
				{
					texture2D.SetPixel(l, m, Color.clear);
				}
			}
			texture2D.SetPixels32(Mathf.RoundToInt(rect.x), Mathf.RoundToInt(rect.y), num5, num6, colors);
			texture2D.Apply();
			_frames[k].Texture = texture2D;
			_frames[k].Duration = images[k].Duration;
		}
		_frameIndex = 0;
		_isTransition = false;
		SetFrame(0f, refresh: true);
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
			_frameIndex = Mathf.FloorToInt(_frameIndex + 1) % _frames.Length;
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
		SetFrame(frame, refresh: false);
	}

	private void SetFrame(float frame, bool refresh)
	{
		int num = Mathf.FloorToInt(frame) % _frames.Length;
		int next = (num + 1) % _frames.Length;
		Material material = null;
		bool flag = false;
		if (_isUITexture && _textureWidget.drawCall != null)
		{
			material = _textureWidget.drawCall.dynamicMaterial;
			flag = material != null;
		}
		if (refresh || _index != num)
		{
			_index = num;
			SetMaterialFrame(_material, num, next);
			if (flag)
			{
				SetMaterialFrame(material, num, next);
			}
		}
		SetMaterialRatio(_material, frame % 1f);
		if (flag)
		{
			SetMaterialRatio(material, frame % 1f);
		}
	}

	private void SetMaterialFrame([NotNull] Material mat, int index, int next)
	{
		mat.SetTexture(_mainTex, _frames[index].Texture);
		mat.SetTexture(_blendTex, _frames[next].Texture);
		mat.SetColor(_bgColor, _background);
	}

	private void SetMaterialRatio([NotNull] Material mat, float ratio)
	{
		mat.SetFloat(_ratio, ratio);
	}
}
