using System;
using Durango.Player;
using UnityEngine;

namespace Durango.UI;

public class BalloonContainer : MonoBehaviour
{
	public delegate Vector2 PositionConverter(Vector2 tilePos);

	[SerializeField]
	private AnnounceBalloon _announceBalloon;

	[SerializeField]
	[EnumList(typeof(AnnounceType), false, 0, -1)]
	private AnnounceBalloonMeta[] _announceBalloonMetaList;

	private ListObjectPool<AnnounceBalloon> _announceBalloons;

	private bool _isWorldMapMode;

	public PositionConverter TileToMapPosition { get; set; }

	public PositionConverter TileToHumanePosition { get; set; }

	private void Awake()
	{
		_announceBalloons = new ListObjectPool<AnnounceBalloon>();
		_announceBalloons.BaseObject = _announceBalloon;
		_announceBalloons.Init(null);
	}

	public void AddAnnounceBalloon(AnnounceType type, Vector2 tilePos, PlayerInfo info)
	{
		AnnounceBalloon announceBalloon = AddAnnounceBalloon(type, tilePos, info.EntityId, info.Name);
		announceBalloon.SetPortrait(info);
	}

	public void AddAnnounceBalloon(AnnounceType type, Vector2 tilePos, string entityId, string spriteName, string titleName, int spriteSize)
	{
		AnnounceBalloon announceBalloon = AddAnnounceBalloon(type, tilePos, entityId, titleName);
		announceBalloon.SetSprite(entityId, spriteName, spriteSize);
	}

	private AnnounceBalloon AddAnnounceBalloon(AnnounceType type, Vector2 tilePos, string entityId, string titleName)
	{
		AnnounceBalloonMeta meta = _announceBalloonMetaList[(int)type];
		RemoveDuplicatedBalloons(meta._duplicateType, type, entityId);
		AnnounceBalloon announceBalloon = _announceBalloons.Add();
		announceBalloon.Show(tilePos, entityId, titleName, type, meta);
		announceBalloon.SetTitleVisible(_isWorldMapMode);
		return announceBalloon;
	}

	public void RemoveAnnounceBalloons(AnnounceType type)
	{
		RemoveBalloons((AnnounceBalloon balloon) => balloon.Type == type);
	}

	public void RemoveAnnounceBalloons(AnnounceType type, string entityId)
	{
		RemoveBalloons((AnnounceBalloon balloon) => balloon.Type == type && balloon.EntityId == entityId);
	}

	public void UpdatePosition()
	{
		for (int num = _announceBalloons.Count - 1; num >= 0; num--)
		{
			AnnounceBalloon announceBalloon = _announceBalloons[num];
			if (TileToMapPosition != null)
			{
				announceBalloon.transform.localPosition = TileToMapPosition(announceBalloon.TilePosition);
			}
			announceBalloon.Process();
			if (!announceBalloon.IsShow)
			{
				_announceBalloons.Swap(num, _announceBalloons.Count - 1);
				_announceBalloons.Set(_announceBalloons.Count - 1);
			}
		}
	}

	public void SetWorldmapMode(bool isWorldmmap)
	{
		_isWorldMapMode = isWorldmmap;
		for (int i = 0; i < _announceBalloons.Count; i++)
		{
			AnnounceBalloon announceBalloon = _announceBalloons[i];
			announceBalloon.SetTitleVisible(isWorldmmap);
		}
	}

	private void RemoveDuplicatedBalloons(BalloonDuplicateType duplicateType, AnnounceType announceType, string entityId)
	{
		switch (duplicateType)
		{
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
		for (int num = _announceBalloons.Count - 1; num >= 0; num--)
		{
			AnnounceBalloon announceBalloon = _announceBalloons[num];
			if (predicate(announceBalloon))
			{
				announceBalloon.IsShow = false;
				_announceBalloons.Swap(num, _announceBalloons.Count - 1);
				_announceBalloons.Set(_announceBalloons.Count - 1);
				if (removeFirstOnly)
				{
					break;
				}
			}
		}
	}
}
