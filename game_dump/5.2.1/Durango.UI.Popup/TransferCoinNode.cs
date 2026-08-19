using System;
using Durango.Player;
using Durango.UI.Control;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI.Popup;

public class TransferCoinNode : UIWidget
{
	[SerializeField]
	private UIWidget _contentArea;

	[SerializeField]
	private UIWidget _loadingRingArea;

	[SerializeField]
	private SelectableWidget _button;

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _clanLabel;

	[SerializeField]
	private UITexture _portraitTexture;

	[SerializeField]
	private Texture _portraitMask;

	private string _targetPlayerId;

	public void Set([CanBeNull] string id, [CanBeNull] Action<PlayerInfo> clicked)
	{
		_contentArea.gameObject.SetActive(value: false);
		_loadingRingArea.gameObject.SetActive(value: true);
		if (_button != null)
		{
			_button.Clicked = null;
		}
		_targetPlayerId = id;
		Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(id, delegate(PlayerInfo info)
		{
			if (info != null && info.EntityId == _targetPlayerId)
			{
				SetContent(info, clicked);
			}
		});
	}

	public void SetContent([NotNull] PlayerInfo info, [CanBeNull] Action<PlayerInfo> clicked)
	{
		SetText(info.Level, info.GetNameFreq(20, PresetColor.UIMoreLightGray.ToHex()), info.ClanName);
		_contentArea.gameObject.SetActive(value: true);
		_loadingRingArea.gameObject.SetActive(value: false);
		if (_button != null)
		{
			_button.Clicked = delegate
			{
				if (clicked != null)
				{
					clicked(info);
				}
			};
		}
		_portraitTexture.gameObject.SetActive(value: true);
		PortraitBuilder.Argument portraitArgument = info.GetPortraitArgument();
		portraitArgument.Mask = _portraitMask;
		PortraitBuilder.Set(portraitArgument, _portraitTexture);
	}

	private void SetText(int level, string nameFreq, string clanName)
	{
		_levelLabel.text = T._("{0:lv:}", level);
		_nameLabel.text = nameFreq;
		bool flag = string.IsNullOrEmpty(clanName);
		_clanLabel.text = string.Format("{0} {1}", "[icon=tribalwar_icon_flag]", (!flag) ? clanName : T._("부족 미가입")).ToEncodedColor((!flag) ? PresetColor.UIMoreLightGray : PresetColor.UIDarkOrange);
	}
}
