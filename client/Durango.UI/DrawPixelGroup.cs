using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using Durango.Utils.Extensions;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class DrawPixelGroup : UIBase, IUIInitializable
{
	[Serializable]
	public class AutoSaveData
	{
		public byte[][] Pixels;

		public int Width;

		public int Height;

		public string EntityId;

		public string DrawnSource;
	}

	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private KScrollView _menuScroll;

	[SerializeField]
	private SelectableWidget _pageButton;

	[SerializeField]
	private ColorSelectorWidget _colorSelector;

	[SerializeField]
	private DrawableCanvas _drawableCanvas;

	[SerializeField]
	private SelectableButton _okButton;

	[SerializeField]
	private UIWidget _warningTextWidget;

	[SerializeField]
	private UILabel _warningTextLabel;

	[SerializeField]
	private GameObject _nextFrameButton;

	[SerializeField]
	private GameObject _prevFrameButton;

	[SerializeField]
	private UILabel _frameCountLabel;

	[SerializeField]
	private SelectableButton _searchUrlButton;

	[SerializeField]
	private RectLayoutComponent _toolDetailLayout;

	[SerializeField]
	private DrawPixelToolDetail _toolDetail;

	[SerializeField]
	private UIPanel _canvasPanel;

	private List<Texture2D> _textures = new List<Texture2D>();

	private int _width;

	private int _height;

	private int _maxFrame;

	private string _entityId;

	private Action<List<Texture2D>, Action<bool>> _onResult;

	private Color[] _colors;

	private readonly List<ToolDatum> _toolData = new List<ToolDatum>();

	private int _frameIndex;

	private string _exitWarning;

	private ICoroutineBinder _autoSaveCoroutine;

	public void Init()
	{
		_pageButton.Clicked = _drawableCanvas.ClearCanvas;
		_titleWidget.Object.OnBack += UITitle_OnBack;
		_titleWidget.Object.OnClose += UITitle_OnClose;
		_colorSelector.ColorChanged += OnColorChanged;
		_drawableCanvas.Clicked += OnSelectPixel;
		_okButton.Clicked = FinishPainting;
		_searchUrlButton.gameObject.SetActive(Debug.isDebugBuild);
		_searchUrlButton.Clicked = OnClickSearchUrlButton;
		UIEventListener uIEventListener = UIEventListener.Get(_nextFrameButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickNextFrame));
		UIEventListener uIEventListener2 = UIEventListener.Get(_prevFrameButton);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnClickPrevFrame));
		foreach (ToolType value in Enum.GetValues(typeof(ToolType)))
		{
			string key = LocalizeUtil.GetKey(value);
			_toolData.Add(ToolDatum.Create(value, IconMap.Get(key)));
		}
		_menuScroll.Nodes.BeginLoad();
		List<DrawPixelListWidget> comps = new List<DrawPixelListWidget>();
		int i = 0;
		for (int size = KUtility.GetSize(_toolData); i < size; i++)
		{
			ToolDatum data = _toolData[i];
			DrawPixelListWidget comp = _menuScroll.Nodes.GetNext().GetComponent<DrawPixelListWidget>();
			comp.Set(data, delegate
			{
				MenuClicked(comps, comp, data);
			});
			comps.Add(comp);
		}
		_menuScroll.Nodes.EndLoad();
		_menuScroll.ResetPosition();
		_warningTextWidget.gameObject.SetActive(value: false);
		SetChildrenActive(activated: false);
		base.OnOpenSucceed += delegate
		{
			GameSystem<InputSystem>.Instance().On(InputCommand.GestureZoom, _drawableCanvas.OnGestureZoomProcess);
			GameSystem<InputSystem>.Instance().On(InputCommand.GestureTwoFingerDrag, _drawableCanvas.OnGestureMoveProcess);
		};
		base.OnCloseSucceed += delegate
		{
			GameSystem<InputSystem>.Instance().Off(InputCommand.GestureZoom, _drawableCanvas.OnGestureZoomProcess);
			GameSystem<InputSystem>.Instance().Off(InputCommand.GestureTwoFingerDrag, _drawableCanvas.OnGestureMoveProcess);
		};
	}

	protected override void OnScreenResized()
	{
		base.OnScreenResized();
		_menuScroll.ResetPosition();
	}

	private string GetFilePath(string entityId)
	{
		return $"DrawPixel/{entityId}";
	}

	public void Set(int width, int height, string entitiyId, int maxFrame, string tableKey, string drawBoardLocalizedName, string exitWarning, Action<List<Texture2D>, Action<bool>> onResult)
	{
		_titleWidget.Object.SetTitle(drawBoardLocalizedName);
		_width = width;
		_height = height;
		_entityId = entitiyId;
		_maxFrame = maxFrame;
		_onResult = onResult;
		_exitWarning = exitWarning;
		_colors = ColorTableLoader.GetAll(tableKey);
		if (Open())
		{
			_drawableCanvas.Opened();
			_menuScroll.Reposition(resetPosition: true, tween: false);
			_colorSelector.Set(_colors, _colors.Random(), OnSelectColor);
			SetWarningText(null);
			if (!TryLoadData())
			{
				SetEmptyCanvas();
			}
			ClickTool(ToolType.Pen);
			_toolData.Find((ToolDatum elem) => elem.Tool == ToolType.Grid).IsSelected = false;
			ClickTool(ToolType.Grid);
			this.StartCoroutine(ref _autoSaveCoroutine, AutoSaveSequence());
		}
	}

	private IEnumerator AutoSaveSequence()
	{
		while (true)
		{
			if (_drawableCanvas.IsRequiringSave)
			{
				_drawableCanvas.IsRequiringSave = false;
				SaveData();
			}
			yield return new WaitForSeconds(3f);
		}
	}

	private void SaveData()
	{
		using FileStream fileStream = AppData.OpenFile(GetFilePath(_entityId), FileMode.Create);
		AutoSaveData autoSaveData = new AutoSaveData();
		autoSaveData.Width = _width;
		autoSaveData.Height = _height;
		autoSaveData.Pixels = _textures.Select((Texture2D elem) => elem.GetRawTextureData()).ToArray();
		autoSaveData.EntityId = _entityId;
		AutoSaveData graph = autoSaveData;
		if (fileStream != null)
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			binaryFormatter.Serialize(fileStream, graph);
			fileStream.Close();
		}
	}

	private bool TryLoadData()
	{
		using FileStream fileStream = AppData.OpenFile(GetFilePath(_entityId), FileMode.Open);
		if (fileStream == null)
		{
			return false;
		}
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		AutoSaveData autoSaveData;
		try
		{
			autoSaveData = binaryFormatter.Deserialize(fileStream) as AutoSaveData;
		}
		catch
		{
			return false;
		}
		if (autoSaveData == null || autoSaveData.Pixels.Length == 0 || autoSaveData.EntityId != _entityId)
		{
			return false;
		}
		UIManager.SystemMsg(T._("임시 저장한 그림으로 복구하였습니다."));
		ApplyTextures(autoSaveData.Pixels);
		SetFrame(0);
		return true;
	}

	private void ClickTool(ToolType type)
	{
		int i = 0;
		for (int size = KUtility.GetSize(_toolData); i < size; i++)
		{
			if (type == _toolData[i].Tool)
			{
				DrawPixelListWidget component = _menuScroll.GetNode(i).GetComponent<DrawPixelListWidget>();
				if (component != null && component.Button.Clicked != null)
				{
					component.Button.Clicked();
				}
				break;
			}
		}
	}

	public void OnColorChanged(Color changedColor)
	{
		_toolDetail.UpdateColor(_drawableCanvas.CurrentTool, changedColor);
	}

	private void MenuClicked(IList<DrawPixelListWidget> toggleObjs, DrawPixelListWidget targetObj, ToolDatum selectedTool)
	{
		if (selectedTool.IsRadioButton)
		{
			int i = 0;
			for (int size = KUtility.GetSize(toggleObjs); i < size; i++)
			{
				DrawPixelListWidget drawPixelListWidget = toggleObjs[i];
				selectedTool.IsSelected = drawPixelListWidget.SetToggle(selectedTool);
			}
		}
		else if (selectedTool.IsCheckBoxButton)
		{
			selectedTool.IsSelected = !selectedTool.IsSelected;
			targetObj.SetSelection(selectedTool.IsSelected);
		}
		SetToolState(selectedTool);
	}

	private void SetToolState(ToolDatum selectedTool)
	{
		switch (selectedTool.Tool)
		{
		case ToolType.Pen:
		case ToolType.Brush:
		case ToolType.Eraser:
		case ToolType.Bucket:
			_toolDetail.gameObject.SetActive(value: true);
			_toolDetail.SetStyle(selectedTool, _colorSelector);
			_drawableCanvas.CurrentColor = _colorSelector.CurrentColor;
			_drawableCanvas.CurrentTool = selectedTool;
			break;
		case ToolType.Eyedropper:
			_toolDetail.gameObject.SetActive(value: false);
			_drawableCanvas.CurrentTool = selectedTool;
			break;
		case ToolType.Grid:
		{
			bool flag = !selectedTool.IsSelected;
			_drawableCanvas.SetGridVisibility(!flag);
			break;
		}
		}
		_toolDetailLayout.UpdateLayout();
		_colorSelector.UpdateLayout(_colorSelector.Widget.height);
	}

	private void OnSelectPixel(int x, int y, Color32 selectedColor)
	{
		switch (_drawableCanvas.CurrentTool.Tool)
		{
		case ToolType.Eyedropper:
			if (_colorSelector.TrySelectColor(selectedColor))
			{
				_drawableCanvas.CurrentColor = selectedColor;
				ClickTool(_drawableCanvas.CurrentTool.PreviousDrawableTool);
			}
			break;
		case ToolType.Bucket:
			_drawableCanvas.FillBucket(x, y, _colorSelector.CurrentColor);
			break;
		}
	}

	private void OnSelectColor(int tab, Color color)
	{
		ToolType tool = _drawableCanvas.CurrentTool.Tool;
		if (tool == ToolType.Eyedropper)
		{
			OnSelectPixel(0, 0, color);
		}
		else
		{
			_drawableCanvas.CurrentColor = color;
		}
	}

	private void FinishPainting()
	{
		if (_textures == null || _textures.Count == 0 || _onResult == null)
		{
			UIBase.CloseAllUI();
		}
		else
		{
			_onResult(_textures, ConfirmFinish);
		}
	}

	private void ConfirmFinish(bool isConfirmed)
	{
		if (isConfirmed)
		{
			this.StopCoroutine(_autoSaveCoroutine);
			AppData.DeleteFile(GetFilePath(_entityId));
		}
	}

	public void SetWarningText(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			_warningTextWidget.gameObject.SetActive(value: false);
			_canvasPanel.bottomAnchor.absolute = 0;
			_canvasPanel.UpdateAnchors();
		}
		else
		{
			_warningTextWidget.gameObject.SetActive(value: true);
			_warningTextLabel.text = text;
			_canvasPanel.bottomAnchor.absolute = _warningTextWidget.height;
			_canvasPanel.UpdateAnchors();
		}
	}

	public void SetTexture(Texture2D texture, Rect uv)
	{
		_textures.Clear();
		Color32[] pixels = UIUtility.ResizeTexturePixels(texture, uv, _width, _height);
		Texture2D texture2D = DrawExtension.MakeTexture(_width, _height);
		texture2D.SetPixels32(pixels);
		texture2D.Apply();
		_textures.Add(texture2D);
		SetFrame(0);
	}

	private void SetEmptyCanvas()
	{
		_textures.Clear();
		Texture2D item = DrawExtension.MakeEmptyTexture(_width, _height);
		_textures.Add(item);
		SetFrame(0);
	}

	private void SetFrame(int index)
	{
		int count = _textures.Count;
		_frameIndex = Mathf.Clamp(index, 0, count);
		_drawableCanvas.SetCanvas(_textures[_frameIndex]);
		_prevFrameButton.SetActive(_frameIndex > 0);
		_nextFrameButton.SetActive(_maxFrame == 0 || _frameIndex < _maxFrame - 1);
		_frameCountLabel.gameObject.SetActive(count > 1);
		_frameCountLabel.text = $"{index + 1} / {count}";
	}

	private void AddEmptyFrame(bool clear)
	{
		Texture2D texture2D;
		if (clear)
		{
			texture2D = DrawExtension.MakeEmptyTexture(_width, _height);
		}
		else
		{
			texture2D = DrawExtension.MakeTexture(_width, _height);
			Texture2D texture2D2 = _textures[_textures.Count - 1];
			texture2D.SetPixels32(texture2D2.GetPixels32());
			texture2D.Apply();
		}
		_textures.Add(texture2D);
		SetFrame(_textures.Count - 1);
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
			}, new MessageBox.Button(T._("새 프레임")), new MessageBox.Button(T._("마지막 그림 복사")), T._("취소"));
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
		TextInputPopup textInputPopup = UIManager.Popup.Tooltip<TextInputPopup>();
		textInputPopup.Show(OnInsertImageUrl, "Image Url");
	}

	private void OnInsertImageUrl(string url)
	{
		if (UIUtility.IsUrl(url))
		{
			StartCoroutine(CoRequestImage(url));
			return;
		}
		using FileStream fileStream = AppData.OpenFile(url);
		if (fileStream != null)
		{
			Texture2D texture2D = new Texture2D(0, 0);
			byte[] array = new byte[fileStream.Length];
			fileStream.Read(array, 0, array.Length);
			texture2D.LoadImage(array);
			ApplyTextures(texture2D, removeSpace: false);
		}
	}

	private IEnumerator CoRequestImage(string url)
	{
		WWW requestWWW = new WWW(url);
		UIManager.ShowLoadingIcon(show: true);
		yield return requestWWW;
		UIManager.ShowLoadingIcon(show: false);
		if (requestWWW.error == null)
		{
			ApplyTextures(requestWWW.texture, removeSpace: true);
		}
	}

	private void ApplyTextures(Texture2D texture, bool removeSpace, int frameIndex = -1)
	{
		if (frameIndex < 0)
		{
			frameIndex = _frameIndex;
		}
		Color32[] pixels = texture.GetPixels32();
		int width = texture.width;
		int height = texture.height;
		Rect uv = new Rect(0f, 0f, 1f, 1f);
		if (removeSpace)
		{
			uv = UIUtility.GetNonespaceArea(pixels, width, height);
			uv.x /= width;
			uv.width /= width;
			uv.y /= height;
			uv.height /= height;
		}
		pixels = UIUtility.ResizeTexturePixels(texture, uv, _width, _height);
		Texture2D texture2D = _textures[frameIndex];
		texture2D.SetPixels32(pixels);
		texture2D.Apply();
	}

	public void ApplyTextures(byte[][] dataPixels)
	{
		_textures.Clear();
		for (int i = 0; i < dataPixels.GetLength(0); i++)
		{
			Texture2D texture2D = DrawExtension.MakeEmptyTexture(_width, _height);
			_textures.Add(texture2D);
			texture2D.LoadRawTextureData(dataPixels[i]);
			ApplyTextures(texture2D, removeSpace: false, i);
		}
	}

	public void UITitle_OnBack()
	{
		if (!_drawableCanvas.IsDrawing)
		{
			Close();
			return;
		}
		UIManager.MessageBox.Show(_exitWarning, delegate(bool ok)
		{
			if (ok)
			{
				Close();
			}
		}, T._("종료"), T._("취소"));
	}

	public void UITitle_OnClose()
	{
		if (!_drawableCanvas.IsDrawing)
		{
			UIBase.CloseAllUI();
			return;
		}
		UIManager.MessageBox.Show(_exitWarning, delegate(bool ok)
		{
			if (ok)
			{
				UIBase.CloseAllUI();
			}
		}, T._("종료"), T._("취소"));
	}
}
