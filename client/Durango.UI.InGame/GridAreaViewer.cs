using System.Collections.Generic;
using Durango.Render.Camera;
using Durango.Terrain;
using Durango.UI.Control;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI.InGame;

public class GridAreaViewer : Singleton<GridAreaViewer>
{
	public enum LayerType
	{
		Upper,
		Bottom
	}

	[SerializeField]
	private UIPanel _panel;

	[SerializeField]
	private UIPanel _overPanel;

	[SerializeField]
	private UITexture _texture;

	[SerializeField]
	private SelectableButton _baseButton;

	[SerializeField]
	private int _upperQueue;

	[SerializeField]
	private int _bottomQueue;

	[SerializeField]
	private TweenAlpha _tweenAlpha;

	private readonly List<int> _buttonIndexes = new List<int>();

	private IList<GridAreaBase> _areaStructs;

	private ListObjectPool<SelectableButton> _buttons;

	private void Start()
	{
		_panel.gameObject.SetActive(value: false);
		Transform transform = _overPanel.transform;
		transform.parent = Singleton<UIManager>.Instance().UIRoot.transform;
		transform.localScale = Vector3.one;
		transform.localRotation = Quaternion.identity;
		NGUITools.SetLayer(_overPanel.gameObject, LayerHelper.UILayer);
		_overPanel.gameObject.SetActive(value: false);
		_buttons = new ListObjectPool<SelectableButton>();
		_buttons.BaseObject = _baseButton;
		_buttons.UseBase = true;
		_buttons.Init(InitSelectWindow);
		_texture.mainTexture = Texture2D.whiteTexture;
		_texture.fillGeometry = false;
		_texture.SetDimensions(9600, 9600);
		Hide();
	}

	private void LateUpdate()
	{
		for (int i = 0; i < _buttonIndexes.Count; i++)
		{
			GridAreaBase gridAreaBase = _areaStructs[_buttonIndexes[i]];
			Vector3 world = Util.TilePositionToClientPosition(gridAreaBase.CenterTile);
			Vector2 vector = MainCamera.WorldToNGUIPos(world);
			SelectableButton selectableButton = _buttons[i];
			selectableButton.transform.localPosition = vector;
		}
		if (_tweenAlpha.isActiveAndEnabled)
		{
			FillGridTexture();
		}
	}

	public void FillGridTexture()
	{
		UIGeometry geometry = _texture.geometry;
		geometry.Clear();
		int i = 0;
		for (int size = KUtility.GetSize(_areaStructs); i < size; i++)
		{
			GridAreaBase gridAreaBase = _areaStructs[i];
			gridAreaBase.Draw(geometry, _panel.alpha);
		}
		_texture.MarkAsChanged();
	}

	private void InitSelectWindow(SelectableButton btn)
	{
		btn.Clicked = OnClickSelectButton;
		UIEventListener.Get(btn.gameObject).onDrag = UIManager.IgnoreUIDrag;
	}

	private void OnClickSelectButton()
	{
		if (_areaStructs != null)
		{
			int num = _buttons.IndexOf((SelectableButton)Selectable.Current);
			int num2 = ((num != -1) ? _buttonIndexes[num] : (-1));
			if (num2 != -1 && _areaStructs[num2].OnSelect != null)
			{
				_areaStructs[num2].OnSelect(_areaStructs[num2].Tile);
			}
		}
	}

	public Point2 GetTileOffset()
	{
		Point2 centerChunkCoords = Singleton<TerrainBase>.Instance().CenterChunkCoords;
		Point2 result = default(Point2);
		result.x = (centerChunkCoords.x - 1) * 16;
		result.y = (centerChunkCoords.y - 1) * 16;
		return result;
	}

	public void Show(IList<GridAreaBase> areas, LayerType layerType = LayerType.Bottom, bool tweenAlpha = false)
	{
		Show(areas, null, layerType, tweenAlpha);
	}

	public void Show(IList<GridAreaBase> areas, int? floor, LayerType layerType, bool tweenAlpha)
	{
		_areaStructs = areas;
		Vector3 vector = Vector3.up * (floor.HasValue ? ((float)(floor.Value * 200)) : 0f);
		Vector3 position = Util.TilePositionToClientPosition(GetTileOffset()) + vector;
		_panel.transform.position = position;
		_panel.renderQueue = UIPanel.RenderQueue.StartAt;
		_panel.startingRenderQueue = ((layerType != 0) ? _bottomQueue : _upperQueue);
		_panel.alpha = 1f;
		FillGridTexture();
		_buttonIndexes.Clear();
		int count = 0;
		int i = 0;
		for (int size = KUtility.GetSize(areas); i < size; i++)
		{
			GridAreaBase gridAreaBase = areas[i];
			if (gridAreaBase.HasButton())
			{
				_buttonIndexes.Add(i);
				SelectableButton orAdd = _buttons.GetOrAdd(count++);
				Vector3 position2 = Util.TilePositionToClientPosition(gridAreaBase.CenterTile) + vector;
				orAdd.transform.position = position2;
				orAdd.SetStyle(gridAreaBase.ButtonStyle);
				orAdd.Color = ((!(gridAreaBase.ButtonColor.a > 0f)) ? Color.white : gridAreaBase.ButtonColor);
				orAdd.Text = gridAreaBase.ButtonText;
			}
		}
		_buttons.Set(count);
		_panel.gameObject.SetActive(value: true);
		_overPanel.gameObject.SetActive(value: true);
		base.enabled = true;
		if (tweenAlpha)
		{
			_tweenAlpha.PlayForward();
		}
		LateUpdate();
	}

	public void Hide()
	{
		_areaStructs = null;
		_panel.gameObject.SetActive(value: false);
		_overPanel.gameObject.SetActive(value: false);
		base.enabled = false;
		_tweenAlpha.enabled = false;
	}
}
