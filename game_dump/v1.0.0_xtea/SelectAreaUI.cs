using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectAreaUI : KSingleton<SelectAreaUI>
{
	public struct AreaStruct
	{
		public Point2 Pos;

		public Point2 Size;

		public Color Color;

		public string Comment;

		public Action OnSelect;

		public TileColorFunc TileColorFunc;

		public int TileCount => Size.x * Size.y;
	}

	public delegate Color TileColorFunc(Point2 tile);

	[SerializeField]
	private UIPanel _panel;

	[SerializeField]
	private UIPanel _overPanel;

	[SerializeField]
	private ListObjectPool _tiles;

	[SerializeField]
	private ListObjectPool _areas;

	[SerializeField]
	private ListObjectPool _selectWindow;

	[SerializeField]
	private Texture2D _tileTexture;

	private IList<AreaStruct> _areaStructs;

	private bool _isShow;

	protected override void OnAwake()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		((Component)_panel).gameObject.SetActive(false);
		((Component)_overPanel).transform.parent = ((Component)KSingleton<UIManager>.Instance().UIRoot).transform;
		((Component)_overPanel).transform.localScale = Vector3.one;
		((Component)_overPanel).transform.localRotation = Quaternion.identity;
		NGUITools.SetLayer(((Component)_overPanel).gameObject, UIManager.UILayer);
		((Component)_overPanel).gameObject.SetActive(false);
		_tiles.Init(InitTileSprite);
		_selectWindow.Init(InitSelectWindow);
	}

	private void InitTileSprite(GameObject obj)
	{
		UIWidget component = obj.GetComponent<UIWidget>();
		component.onPostFill = (UIWidget.OnPostFillCallback)Delegate.Combine(component.onPostFill, new UIWidget.OnPostFillCallback(OnTilePostFill));
	}

	private void OnTilePostFill(UIWidget widget, int bufferOffset, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		if (_areaStructs == null)
		{
			return;
		}
		GameObject gameObject = ((Component)widget).gameObject;
		int num = -1;
		int i = 0;
		for (int count = _tiles.Count; i < count; i++)
		{
			if ((Object)(object)gameObject == (Object)(object)_tiles[i])
			{
				num = i;
				break;
			}
		}
		if (num == -1 || _areaStructs.Count <= num)
		{
			return;
		}
		AreaStruct areaStruct = _areaStructs[num];
		Vector3 val = widget.localCorners[0];
		int j = 0;
		Vector2 val4 = default(Vector2);
		for (int num2 = verts.size / 4; j < num2; j++)
		{
			Color val2 = areaStruct.Color * widget.color;
			if (areaStruct.TileColorFunc != null)
			{
				Vector3 val3 = Vector3.zero;
				for (int k = 0; k < 4; k++)
				{
					val3 += verts[j * 4 + k];
				}
				val3 /= 4f;
				val3 -= val;
				((Vector2)(ref val4))._002Ector(val3.x / (float)widget.width, val3.y / (float)widget.height);
				Point2 tile = areaStruct.Pos + new Point2(new Vector2((float)areaStruct.Size.x * val4.x, (float)areaStruct.Size.y * val4.y));
				val2 *= areaStruct.TileColorFunc(tile);
			}
			for (int l = 0; l < 4; l++)
			{
				cols[j * 4 + l] = val2;
			}
		}
	}

	private void InitSelectWindow(GameObject obj)
	{
		DefaultSelectableButton componentInChildren = obj.GetComponentInChildren<DefaultSelectableButton>(true);
		if ((Object)(object)componentInChildren != (Object)null)
		{
			componentInChildren.Clicked = OnClickSelectButton;
		}
	}

	private void OnClickSelectButton()
	{
		if (_areaStructs == null)
		{
			return;
		}
		GameObject gameObject = ((Component)Selectable.Current).gameObject;
		int num = -1;
		int i = 0;
		for (int count = _selectWindow.Count; i < count; i++)
		{
			if (gameObject.transform.IsChildOf(_selectWindow[i].transform))
			{
				num = i;
				break;
			}
		}
		if (num != -1 && _areaStructs[num].OnSelect != null)
		{
			_areaStructs[num].OnSelect();
		}
	}

	public void ShowGrids(Vector2 worldTile, TileColorFunc tileColor = null, int areaSize = 0, int areaOffset = 0)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		Point2 point = new Point2(worldTile);
		if (areaSize == 0)
		{
			Point2 point2 = new Point2(16, 16);
			AreaStruct areaStruct = default(AreaStruct);
			areaStruct.Pos = point - point2 / 2;
			areaStruct.Size = point2;
			areaStruct.Color = Color.white;
			areaStruct.TileColorFunc = tileColor;
			AreaStruct areaStruct2 = areaStruct;
			Show(new AreaStruct[1] { areaStruct2 });
			return;
		}
		Point2 point3 = new Point2(16, 16);
		Vector2 val = (point - point3 / 2).ToVector2();
		val /= (float)areaSize;
		Point2 point4 = new Point2(Mathf.FloorToInt(val.x), Mathf.FloorToInt(val.y)) * areaSize;
		Vector2 val2 = (point + point3 / 2).ToVector2();
		val2 /= (float)areaSize;
		Point2 point5 = new Point2(Mathf.CeilToInt(val2.x), Mathf.CeilToInt(val2.y)) * areaSize;
		point3 = new Point2(point5.x - point4.x, point5.y - point4.y) / areaSize;
		AreaStruct[] array = new AreaStruct[point3.x * point3.y];
		for (int i = 0; i < point3.x; i++)
		{
			for (int j = 0; j < point3.y; j++)
			{
				ref AreaStruct reference = ref array[i + j * point3.x];
				reference = new AreaStruct
				{
					Pos = point4 + new Point2(i, j) * areaSize,
					Size = Point2.one * areaSize,
					Color = Color.white,
					TileColorFunc = tileColor
				};
			}
		}
		Show(array);
	}

	public void Show(IList<AreaStruct> areas)
	{
		_areaStructs = areas;
		int size = KUtility.GetSize(areas);
		_selectWindow.Set(size);
		_areas.Set(size);
		_tiles.Set(size);
		for (int i = 0; i < size; i++)
		{
			Show(areas[i], _tiles[i].GetComponent<UITexture>(), _areas[i].GetComponent<UITexture>(), _selectWindow[i]);
		}
		((Component)_panel).gameObject.SetActive(true);
		((Component)_overPanel).gameObject.SetActive(true);
		if (!_isShow)
		{
			((MonoBehaviour)this).StartCoroutine(CoUpdate());
		}
	}

	private void Show(AreaStruct area, UITexture tileWidget, UITexture areaSprite, GameObject selectWindow)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		Vector2 tilePosition = area.Pos.ToVector2() + area.Size.ToVector2() * 0.5f;
		Vector3 position = TerrainA6.TilePositionToClientPosition(tilePosition);
		int num = area.Size.x * 200;
		int num2 = area.Size.y * 200;
		tileWidget.mainTexture = (Texture)(object)_tileTexture;
		((Component)tileWidget).transform.position = position;
		Vector3 one = Vector3.one;
		one.x = 200f / (float)((Texture)_tileTexture).width;
		one.y = 200f / (float)((Texture)_tileTexture).height;
		((Component)tileWidget).transform.localScale = one;
		tileWidget.width = Mathf.RoundToInt((float)num / one.x);
		tileWidget.height = Mathf.RoundToInt((float)num2 / one.y);
		((Component)areaSprite).transform.position = position;
		areaSprite.width = num;
		areaSprite.height = num2;
		if (string.IsNullOrEmpty(area.Comment))
		{
			selectWindow.gameObject.SetActive(false);
			return;
		}
		selectWindow.transform.position = position;
		DefaultSelectableButton component = selectWindow.GetComponent<DefaultSelectableButton>();
		component.Text = area.Comment;
		UIUtility.UpdateAnchors(((Component)component).transform);
		selectWindow.gameObject.SetActive(true);
	}

	public void Hide()
	{
		_areaStructs = null;
		((Component)_panel).gameObject.SetActive(false);
		((Component)_overPanel).gameObject.SetActive(false);
		_isShow = false;
	}

	private IEnumerator CoUpdate()
	{
		_isShow = true;
		while (_isShow)
		{
			int areaCount = KUtility.GetSize(_areaStructs);
			for (int i = 0; i < areaCount; i++)
			{
				AreaStruct area = _areaStructs[i];
				Vector2 centerTile = area.Pos.ToVector2() + area.Size.ToVector2() * 0.5f;
				Vector3 centerPos = TerrainA6.TilePositionToClientPosition(centerTile);
				Vector2 uiPos = Vector2.op_Implicit(MainCamera.WorldToNGUIPos(centerPos));
				GameObject selectWindow = _selectWindow[i];
				selectWindow.transform.localPosition = Vector2.op_Implicit(uiPos);
			}
			yield return null;
		}
	}

	private int FindArea(int x, int y)
	{
		int result = -1;
		int i = 0;
		for (int size = KUtility.GetSize(_areaStructs); i < size; i++)
		{
			AreaStruct areaStruct = _areaStructs[i];
			if (areaStruct.Pos.x <= x && areaStruct.Pos.x + areaStruct.Size.x > x && areaStruct.Pos.y <= y && areaStruct.Pos.y + areaStruct.Size.y > y)
			{
				result = i;
				break;
			}
		}
		return result;
	}
}
