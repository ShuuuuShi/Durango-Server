using JetBrains.Annotations;
using MapData;
using UnityEngine;

public class MapIndicator : MonoBehaviour
{
	private UIWidget _widget;

	private Transform _target;

	private Point2 _tile = -Point2.one;

	public UIWidget Widget => (!((Object)(object)_widget == (Object)null)) ? _widget : (_widget = ((Component)this).GetComponent<UIWidget>());

	public ulong Id { get; private set; }

	public IndicatorType Type { get; private set; }

	public string Tooltip { get; private set; }

	public IndicatorVisibleType VisibleType { get; set; }

	public virtual bool FixedScale => true;

	public void Set(ulong id, IndicatorType type)
	{
		Id = id;
		Type = type;
	}

	public void SetTarget([NotNull] GameObject target)
	{
		_target = target.transform;
		_tile = -Point2.one;
	}

	public void SetTarget(Point2 tile)
	{
		_tile = tile;
		_target = null;
		_target = null;
	}

	public void SetTooltip(string text)
	{
		Tooltip = text;
	}

	public void SetVisible(bool isVisible)
	{
		Widget.alpha = ((!isVisible) ? 0f : 1f);
	}

	public Vector3 GetPosition()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_target != (Object)null)
		{
			return _target.position;
		}
		if (_tile.x >= 0 && _tile.y >= 0)
		{
			return TerrainA6.TilePositionToClientPosition(_tile, tileCenter: true);
		}
		return Vector3.zero;
	}

	public Vector2 GetTile()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_target != (Object)null)
		{
			return TerrainA6.ClientPositionToTilePosition(_target.position);
		}
		if (_tile.x >= 0 && _tile.y >= 0)
		{
			return _tile.ToVector2() + Vector2.one * 0.5f;
		}
		return Vector2.zero;
	}

	public virtual bool IsValid()
	{
		if ((Object)(object)_target != (Object)null)
		{
			return true;
		}
		if (_tile.x >= 0 && _tile.y >= 0)
		{
			return true;
		}
		return false;
	}

	public virtual void OnUpdate()
	{
	}
}
