using Durango.Logic.Clusters;
using Durango.Player;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class PlayerSlotNode : SelectableWidget
{
	public enum SlotType
	{
		[T.EnumName("캐릭터")]
		HasPlayer,
		[T.EnumName("빈 슬롯")]
		Empty,
		[T.EnumName("캐릭터 슬롯 구매 필요")]
		Locked
	}

	[SerializeField]
	private GameObject _iconObject;

	[SerializeField]
	private GameObject _plusSprite;

	[SerializeField]
	private GameObject _lockSprite;

	[SerializeField]
	private UITexture _portraitTexture;

	[SerializeField]
	private GameObject _redCircle;

	[SerializeField]
	private UILabel _playerNameLabel;

	[SerializeField]
	private UILabel _levelAndKhzLabel;

	[SerializeField]
	private UILabel _clanLabel;

	[SerializeField]
	private UILabel _infoLabel;

	[SerializeField]
	private Texture _portraitMask;

	public SlotType Type { get; private set; }

	public Durango.Logic.Clusters.PlayerInfo PlayerInfo { get; private set; }

	public string PlayerEntityId
	{
		get
		{
			if (PlayerInfo != null)
			{
				return PlayerInfo.PlayerEntityId;
			}
			return null;
		}
	}

	public void Set(SlotType slotType, Durango.Logic.Clusters.PlayerInfo info)
	{
		Type = slotType;
		PlayerInfo = info;
		bool flag = slotType == SlotType.HasPlayer && info != null;
		_playerNameLabel.gameObject.SetActive(flag);
		_redCircle.SetActive(flag && info.IsSoftDeleted);
		_levelAndKhzLabel.gameObject.SetActive(value: false);
		_clanLabel.gameObject.SetActive(value: false);
		_portraitTexture.gameObject.SetActive(value: false);
		_iconObject.SetActive(!flag);
		_plusSprite.SetActive(slotType == SlotType.Empty);
		_lockSprite.SetActive(slotType == SlotType.Locked);
		_infoLabel.gameObject.SetActive(!flag);
		if (flag)
		{
			_playerNameLabel.text = info.PlayerName;
			Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(info.PlayerEntityId, delegate(Durango.Player.PlayerInfo playerInfo)
			{
				_levelAndKhzLabel.gameObject.SetActive(value: true);
				_clanLabel.gameObject.SetActive(value: true);
				_portraitTexture.gameObject.SetActive(value: true);
				_levelAndKhzLabel.text = T._("{0:lv:} [icon=bg_line_height] {1}", info.PlayerLevel, playerInfo.GetFreq(20));
				bool flag2 = !string.IsNullOrEmpty(playerInfo.ClanName);
				string text = string.Format("{0} {1}", "icon_popup_player_clan".ToEncodedIcon(), (!flag2) ? T._("부족 없음") : playerInfo.ClanName);
				_clanLabel.text = ((!flag2) ? text.ToEncodedColor("D4CEBEFF") : text);
				PortraitBuilder.Argument portraitArgument = playerInfo.GetPortraitArgument();
				portraitArgument.Mask = _portraitMask;
				PortraitBuilder.Set(portraitArgument, _portraitTexture);
			});
		}
		else
		{
			_infoLabel.text = slotType.GetName();
		}
	}
}
