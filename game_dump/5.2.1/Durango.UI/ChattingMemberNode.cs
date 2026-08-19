using Durango.Player;
using Durango.UI.Control;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class ChattingMemberNode : SelectableWidget
{
	[SerializeField]
	private UISprite _iconInvite;

	[SerializeField]
	private UILabel _labelInvite;

	[SerializeField]
	private UILabel _textName;

	[SerializeField]
	private UILabel _textLevel;

	[SerializeField]
	private UITexture _portrait;

	[SerializeField]
	private Texture2D _portraitMaskTexture;

	[SerializeField]
	private bool _isNameIncludeFreq;

	[CanBeNull]
	public string EntityId { get; private set; }

	public void Set([CanBeNull] string entityId)
	{
		Init();
		EntityId = entityId;
		RefreshActiveStates();
		SetPlayerInfo();
	}

	private void SetPlayerInfo()
	{
		if (EntityId == null)
		{
			return;
		}
		string savedEntityId = EntityId;
		Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(EntityId, delegate(PlayerInfo playerInfo)
		{
			if (!(savedEntityId != EntityId))
			{
				if (playerInfo.Valid)
				{
					_textName.text = ((!_isNameIncludeFreq) ? playerInfo.Name : playerInfo.GetNameFreq(21, "FFFFFF7F"));
					_textName.color = ((!(playerInfo.EntityId == GameManager.PlayerId)) ? Color.white : PresetColor.UIYellow);
					_textLevel.text = LocalizeUtil.FormatLevel(playerInfo.Level);
					PortraitBuilder.Argument portraitArgument = playerInfo.GetPortraitArgument();
					portraitArgument.Mask = _portraitMaskTexture;
					PortraitBuilder.Set(portraitArgument, _portrait);
				}
				else
				{
					_textName.gameObject.SetActive(value: false);
					_textLevel.gameObject.SetActive(value: false);
					_portrait.gameObject.SetActive(value: false);
				}
			}
		});
	}

	private void RefreshActiveStates()
	{
		bool flag = EntityId == null;
		if (_iconInvite != null)
		{
			_iconInvite.gameObject.SetActive(flag);
		}
		if (_labelInvite != null)
		{
			_labelInvite.gameObject.SetActive(flag);
		}
		_textName.gameObject.SetActive(!flag);
		_textLevel.gameObject.SetActive(!flag);
		_portrait.gameObject.SetActive(!flag);
	}
}
