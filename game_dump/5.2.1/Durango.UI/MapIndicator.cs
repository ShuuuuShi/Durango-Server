using System;
using Durango.Logic.Map;
using Durango.Terrain;
using UnityEngine;

namespace Durango.UI;

public class MapIndicator : MonoBehaviour
{
	[Flags]
	public enum HideFlag
	{
		None = 0,
		Reveal = 1,
		Minimap = 2,
		Zoom = 4,
		Type = 8,
		EntityVisible = 0x10,
		Member = 0x20
	}

	public enum Refresh
	{
		ClanInfoUpdated,
		PlayerClanChanged,
		LevelChanged,
		MapModeChanged
	}

	private HideFlag _hideFlag;

	private Transform _target;

	protected Point2 Tile = -Point2.one;

	private UIWidget _widget;

	public UIWidget Widget
	{
		get
		{
			if (_widget == null)
			{
				return _widget = GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public string Id { get; private set; }

	public IndicatorType Type { get; private set; }

	public string Tooltip { get; private set; }

	public bool CheckReveal { get; protected set; }

	public bool IsHidden { get; private set; }

	public bool StickToBoundary { get; protected set; }

	public virtual float VisibleZoom => 0f;

	public void Set(string id, IndicatorType type)
	{
		Id = id;
		Type = type;
	}

	public void SetTarget(GameObject target)
	{
		if (target != null)
		{
			_target = target.transform;
			Tile = -Point2.one;
		}
		else
		{
			_target = null;
		}
	}

	public void SetTarget(Point2 tile)
	{
		Tile = tile;
		_target = null;
	}

	public void SetTooltip(string text)
	{
		Tooltip = text;
	}

	public void ToggleHideFlag(HideFlag flag, bool hide)
	{
		if (hide)
		{
			_hideFlag |= flag;
		}
		else
		{
			_hideFlag &= ~flag;
		}
		bool flag2 = _hideFlag != HideFlag.None;
		if (IsHidden != flag2)
		{
			IsHidden = flag2;
			base.gameObject.SetActive(!IsHidden);
			OnHide(IsHidden);
		}
	}

	public Vector2 GetTile()
	{
		if (_target != null)
		{
			return Util.ClientPositionToTilePosition(_target.position);
		}
		if (Tile.x >= 0 && Tile.y >= 0)
		{
			return Tile.ToVector2() + Vector2.one * 0.5f;
		}
		return Vector2.zero;
	}

	public bool IsValid()
	{
		if (_target != null)
		{
			return true;
		}
		if (Tile.x >= 0)
		{
			return Tile.y >= 0;
		}
		return false;
	}

	public virtual void OnInitialized()
	{
		_hideFlag = HideFlag.None;
		_target = null;
		IsHidden = false;
	}

	public virtual void OnUpdate()
	{
	}

	public virtual void OnRefresh(Refresh type)
	{
	}

	protected virtual void OnHide(bool isHide)
	{
	}
}
