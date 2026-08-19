using System;
using MapData;
using Player;
using UnityEngine;

public class BalloonContainer : MonoBehaviour
{
	public delegate Vector2 PositionConverter(Vector2 tilePos);

	[SerializeField]
	private AnnounceBalloon _announceBalloon;

	private GameObjectPool<AnnounceBalloon> _announceBalloonPool;

	public PositionConverter TileToMapPosition { get; set; }

	public PositionConverter TileToHumanePosition { get; set; }

	private void Awake()
	{
		_announceBalloonPool = new GameObjectPool<AnnounceBalloon>(_announceBalloon);
	}

	public void AddAnnounceBalloon(AnnounceType type, Vector2 tilePos, PlayerInfo info)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (MapIndicatorMeta.TryGetAnnounceBalloonMeta(type, out var meta))
		{
			RemoveDuplicatedBalloons(meta._duplicateType, type, info.EntityId);
			AnnounceBalloon announceBalloon = _announceBalloonPool.Pop();
			Vector2 humanePos = ((TileToHumanePosition == null) ? Vector2.zero : TileToHumanePosition(tilePos));
			announceBalloon.Show(tilePos, humanePos, info, type, meta);
		}
	}

	public void RemoveAnnounceBalloons(AnnounceType type)
	{
		RemoveBalloons((AnnounceBalloon balloon) => balloon.Type == type);
	}

	public void RemoveAnnounceBalloons(AnnounceType type, ulong entityId)
	{
		RemoveBalloons((AnnounceBalloon balloon) => balloon.Type == type && balloon.EntityId == entityId);
	}

	public void UpdatePosition()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		for (int num = _announceBalloonPool.Count - 1; num >= 0; num--)
		{
			AnnounceBalloon announceBalloon = _announceBalloonPool[num];
			if (TileToMapPosition != null)
			{
				((Component)announceBalloon).transform.localPosition = Vector2.op_Implicit(TileToMapPosition(announceBalloon.TilePosition));
			}
			announceBalloon.Update();
			if (announceBalloon.IsHided)
			{
				_announceBalloonPool.Push(announceBalloon);
			}
		}
	}

	private void RemoveDuplicatedBalloons(BalloonDuplicateType duplicateType, AnnounceType announceType, ulong entityId)
	{
		switch (duplicateType)
		{
		case BalloonDuplicateType.Multiple:
			break;
		case BalloonDuplicateType.OnePerPerson:
			RemoveBalloons((AnnounceBalloon balloon) => balloon.Type == announceType && balloon.EntityId == entityId, removeFirstOnly: true);
			break;
		case BalloonDuplicateType.OnlyOne:
			RemoveBalloons((AnnounceBalloon balloon) => balloon.Type == announceType, removeFirstOnly: true);
			break;
		}
	}

	private void RemoveBalloons(Predicate<AnnounceBalloon> predicate, bool removeFirstOnly = false)
	{
		for (int num = _announceBalloonPool.Count - 1; num >= 0; num--)
		{
			AnnounceBalloon announceBalloon = _announceBalloonPool[num];
			if (predicate(announceBalloon))
			{
				announceBalloon.Hide();
				_announceBalloonPool.Push(announceBalloon);
				if (removeFirstOnly)
				{
					break;
				}
			}
		}
	}
}
