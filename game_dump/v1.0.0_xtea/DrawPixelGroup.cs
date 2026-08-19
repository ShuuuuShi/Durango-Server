using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using APNGLib;
using L10N;
using UnityEngine;

public class DrawPixelGroup : UIBase
{
	[SerializeField]
	private UITitleWidget _titleWidget;

	[SerializeField]
	private ColorSelectorWidget _colorSelector;

	[SerializeField]
	private DrawableCanvas _drawableCanvas;

	[SerializeField]
	private DefaultSelectableButton _okButton;

	[SerializeField]
	private DefaultSelectableButton _clearButton;

	[SerializeField]
	private DefaultSelectableButton _undoButton;

	[SerializeField]
	private GameObject _nextFrameButton;

	[SerializeField]
	private GameObject _prevFrameButton;

	[SerializeField]
	private UILabel _frameCountLabel;

	[SerializeField]
	private DefaultSelectableButton _searchUrlButton;

	private List<Texture2D> _textures = new List<Texture2D>();

	private int _width;

	private int _height;

	private int _maxFrame;

	private Action<List<Texture2D>> _onResult;

	private Color[] _colors;

	private int _frameIndex;

	private void Awake()
	{
		((Component)_searchUrlButton).gameObject.SetActive(Debug.isDebugBuild);
	}

	private void Start()
	{
		base.OnOpenSucceed += OpenSucceed;
		_titleWidget.OnBack += Close;
		_titleWidget.OnClose += base.ForceClose;
		_okButton.Clicked = Submit;
		_clearButton.Clicked = delegate
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			_colorSelector.SelectColor(Color.clear);
			_drawableCanvas.Color = Color.clear;
		};
		_undoButton.Clicked = delegate
		{
			_drawableCanvas.Undo();
		};
		_searchUrlButton.Clicked = OnClickSearchUrlButton;
		UIEventListener uIEventListener = UIEventListener.Get(_nextFrameButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickNextFrame));
		UIEventListener uIEventListener2 = UIEventListener.Get(_prevFrameButton);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnClickPrevFrame));
		OnClose();
	}

	public void Open(int width, int height, int maxFrame, Action<List<Texture2D>> onResult, Color[] colors)
	{
		_width = width;
		_height = height;
		_maxFrame = maxFrame;
		_onResult = onResult;
		_colors = colors;
		Open();
	}

	private void OpenSucceed()
	{
		Reset();
	}

	private void Reset()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		_colorSelector.Set(_colors, Color.clear, OnSelectColor);
		_drawableCanvas.Color = Color.clear;
		_drawableCanvas.IsDrawing = false;
		SetEmptyCanvas();
	}

	private void OnSelectColor(int tab, Color color)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_drawableCanvas.Color = color;
	}

	private void Submit()
	{
		ForceClose();
		if (_textures != null && _textures.Count != 0 && _onResult != null)
		{
			_onResult(_textures);
		}
	}

	public void SetCanvas(APNG apng, bool removeSpace)
	{
		if (apng.IsAnimated)
		{
			int frameCount = apng.FrameCount;
			Texture2D[] array = (Texture2D[])(object)new Texture2D[frameCount];
			for (int i = 0; i < frameCount; i++)
			{
				Stream stream = apng.ToStream(i);
				byte[] array2 = new byte[stream.Length];
				stream.Read(array2, 0, array2.Length);
				array[i] = MakeTexture();
				array[i].LoadImage(array2);
			}
			SetTextures(array, removeSpace);
		}
		else
		{
			Texture2D val = MakeTexture();
			Stream stream2 = apng.DefaultImageToStream();
			byte[] array3 = new byte[stream2.Length];
			stream2.Read(array3, 0, array3.Length);
			val.LoadImage(array3);
			SetTexture(val, removeSpace);
		}
	}

	public void SetTexture(Texture2D texture, bool removeSpace)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		_textures.Clear();
		Color32[] pixels = texture.GetPixels32();
		int width = ((Texture)texture).width;
		int height = ((Texture)texture).height;
		Rect nonespaceArea = default(Rect);
		((Rect)(ref nonespaceArea))._002Ector(0f, 0f, 1f, 1f);
		if (removeSpace)
		{
			nonespaceArea = UIUtility.GetNonespaceArea(pixels, width, height);
			((Rect)(ref nonespaceArea)).x = ((Rect)(ref nonespaceArea)).x / (float)width;
			((Rect)(ref nonespaceArea)).width = ((Rect)(ref nonespaceArea)).width / (float)width;
			((Rect)(ref nonespaceArea)).y = ((Rect)(ref nonespaceArea)).y / (float)height;
			((Rect)(ref nonespaceArea)).height = ((Rect)(ref nonespaceArea)).height / (float)height;
		}
		pixels = UIUtility.ResizeTexturePixels(texture, nonespaceArea, _width, _height);
		Texture2D val = MakeTexture(_width, _height);
		val.SetPixels32(pixels);
		val.Apply();
		_textures.Add(val);
		SetFrame(0);
	}

	public void SetTextures(IList<Texture2D> textures, bool removeSpace)
	{
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		_textures.Clear();
		Rect uv = default(Rect);
		((Rect)(ref uv))._002Ector(0f, 0f, 1f, 1f);
		if (removeSpace)
		{
			float num = 1f;
			float num2 = 0f;
			float num3 = 1f;
			float num4 = 0f;
			int i = 0;
			for (int size = KUtility.GetSize(textures); i < size; i++)
			{
				Texture2D val = textures[i];
				int width = ((Texture)val).width;
				int height = ((Texture)val).height;
				Color32[] pixels = val.GetPixels32();
				Rect nonespaceArea = UIUtility.GetNonespaceArea(pixels, width, height);
				((Rect)(ref nonespaceArea)).x = ((Rect)(ref nonespaceArea)).x / (float)width;
				((Rect)(ref nonespaceArea)).width = ((Rect)(ref nonespaceArea)).width / (float)width;
				((Rect)(ref nonespaceArea)).y = ((Rect)(ref nonespaceArea)).y / (float)height;
				((Rect)(ref nonespaceArea)).height = ((Rect)(ref nonespaceArea)).height / (float)height;
				num = Mathf.Min(num, ((Rect)(ref nonespaceArea)).xMin);
				num2 = Mathf.Max(num2, ((Rect)(ref nonespaceArea)).xMax);
				num3 = Mathf.Min(num3, ((Rect)(ref nonespaceArea)).yMin);
				num4 = Mathf.Max(num4, ((Rect)(ref nonespaceArea)).yMax);
			}
			((Rect)(ref uv))._002Ector(num, num4, num2 - num, num4 - num3);
		}
		int j = 0;
		for (int size2 = KUtility.GetSize(textures); j < size2; j++)
		{
			Texture2D texture = textures[j];
			Color32[] pixels2 = UIUtility.ResizeTexturePixels(texture, uv, _width, _height);
			Texture2D val2 = MakeTexture(_width, _height);
			val2.SetPixels32(pixels2);
			val2.Apply();
			_textures.Add(val2);
		}
		SetFrame(0);
	}

	private void SetEmptyCanvas()
	{
		_textures.Clear();
		Texture2D item = MakeEmptyTexture(_width, _height);
		_textures.Add(item);
		SetFrame(0);
	}

	private void SetFrame(int index)
	{
		int count = _textures.Count;
		_frameIndex = Mathf.Clamp(index, 0, count);
		_drawableCanvas.SetCanvas(_textures[_frameIndex]);
		_prevFrameButton.SetActive(_frameIndex > 0);
		((Component)_frameCountLabel).gameObject.SetActive(count > 1);
		_nextFrameButton.SetActive(_maxFrame == 0 || _frameIndex < _maxFrame - 1);
		_frameCountLabel.text = $"{index + 1} / {count}";
	}

	private void AddEmptyFrame(bool clear)
	{
		Texture2D val;
		if (clear)
		{
			val = MakeEmptyTexture(_width, _height);
		}
		else
		{
			val = MakeTexture(_width, _height);
			Texture2D val2 = _textures[_textures.Count - 1];
			val.SetPixels32(val2.GetPixels32());
			val.Apply();
		}
		_textures.Add(val);
		SetFrame(_textures.Count - 1);
	}

	private Texture2D MakeTexture(int width = 0, int height = 0)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		Texture2D val = new Texture2D(width, height);
		((Texture)val).filterMode = (FilterMode)0;
		((Texture)val).wrapMode = (TextureWrapMode)1;
		return val;
	}

	private Texture2D MakeEmptyTexture(int width, int height)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = MakeTexture(width, height);
		Color32[] array = (Color32[])(object)new Color32[_width * _height];
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			ref Color32 reference = ref array[i];
			reference = new Color32((byte)0, (byte)0, (byte)0, (byte)0);
		}
		val.SetPixels32(array);
		val.Apply();
		return val;
	}

	private void OnClickNextFrame(GameObject obj)
	{
		if (_frameIndex == _textures.Count - 1)
		{
			UIManager.MessageBox.Show(T._("새 프레임을 추가하시겠습니까?"), delegate(int index)
			{
				switch (index)
				{
				case 0:
					AddEmptyFrame(clear: true);
					break;
				case 1:
					AddEmptyFrame(clear: false);
					break;
				}
			}, T._("새 프레임"), T._("마지막 그림 복사"), T._("취소"));
		}
		else
		{
			SetFrame(_frameIndex + 1);
		}
	}

	private void OnClickPrevFrame(GameObject obj)
	{
		SetFrame(_frameIndex - 1);
	}

	private void OnClickSearchUrlButton()
	{
		TextInputWidget textInput = UIManager.Popup.TextInput;
		textInput.Show(OnInsertImageUrl, "Image Url");
	}

	private void OnInsertImageUrl(string url)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (UIUtility.IsUrl(url))
		{
			((MonoBehaviour)this).StartCoroutine(CoRequestImage(url));
			return;
		}
		using FileStream fileStream = KFileUtil.GetFileStream(url);
		Texture2D val = new Texture2D(0, 0);
		byte[] array = new byte[fileStream.Length];
		fileStream.Read(array, 0, array.Length);
		val.LoadImage(array);
		DrawCurrentCanvas(val, removeSpace: false);
	}

	private IEnumerator CoRequestImage(string url)
	{
		WWW requestWWW = new WWW(url);
		UIManager.ShowLoadingIcon(show: true);
		yield return requestWWW;
		UIManager.ShowLoadingIcon(show: false);
		if (requestWWW.error == null)
		{
			DrawCurrentCanvas(requestWWW.texture, removeSpace: true);
		}
	}

	private void DrawCurrentCanvas(Texture2D texture, bool removeSpace)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Color32[] pixels = texture.GetPixels32();
		int width = ((Texture)texture).width;
		int height = ((Texture)texture).height;
		Rect nonespaceArea = default(Rect);
		((Rect)(ref nonespaceArea))._002Ector(0f, 0f, 1f, 1f);
		if (removeSpace)
		{
			nonespaceArea = UIUtility.GetNonespaceArea(pixels, width, height);
			((Rect)(ref nonespaceArea)).x = ((Rect)(ref nonespaceArea)).x / (float)width;
			((Rect)(ref nonespaceArea)).width = ((Rect)(ref nonespaceArea)).width / (float)width;
			((Rect)(ref nonespaceArea)).y = ((Rect)(ref nonespaceArea)).y / (float)height;
			((Rect)(ref nonespaceArea)).height = ((Rect)(ref nonespaceArea)).height / (float)height;
		}
		pixels = UIUtility.ResizeTexturePixels(texture, nonespaceArea, _width, _height);
		Texture2D val = _textures[_frameIndex];
		val.SetPixels32(pixels);
		val.Apply();
	}
}
