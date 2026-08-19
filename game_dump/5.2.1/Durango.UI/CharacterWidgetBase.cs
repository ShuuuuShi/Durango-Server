using System;
using Durango.Logic;
using Durango.Logic.Clan;
using Durango.Logic.Social;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class CharacterWidgetBase : UIWidget
{
	[SerializeField]
	private UITexture _portrait;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	protected UILabel _expLabel;

	[SerializeField]
	private UISprite _expGauge;

	[SerializeField]
	private UIWidget _clanWidget;

	[SerializeField]
	private UILabel _clanLabel;

	[SerializeField]
	private SelectableButton _timelineLogBtn;

	private bool _isInit;

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		UIEventListener uIEventListener = UIEventListener.Get(_portrait.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			PlayerInfoPopup.RequestShow(PlayerBehavior.LocalPlayer.EntityId, delegate(PlayerInfoPopup tooltip)
			{
				tooltip.Show(_portrait, Vector2.up * ((float)_portrait.height * (1f - _portrait.pivotOffset.y) + 30f) + Vector2.right * 10f, 3600f);
			});
		});
		SelectableButton timelineLogBtn = _timelineLogBtn;
		timelineLogBtn.Clicked = (Action)Delegate.Combine(timelineLogBtn.Clicked, (Action)delegate
		{
			UIManager.FindScript<TimelineLogGroup>().Open();
		});
		_timelineLogBtn.Text = T._("이력");
		_timelineLogBtn.SetDimensions(_timelineLogBtn.GetPreferredSize() + new Point2(20, 0));
	}

	public void Refresh()
	{
		Init();
		PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
		if (!(localPlayer == null))
		{
			PortraitBuilder.Argument portraitArgument = localPlayer.GetPortraitArgument();
			portraitArgument.Mask = null;
			portraitArgument.Emotion = PortraitEmotion.Normal;
			PortraitBuilder.Set(portraitArgument, _portrait);
			_nameLabel.text = MakeNameText(localPlayer.PlayerName, localPlayer.Freq);
			SetClan(GameSystem<ClanSystem>.Instance().PlayerClan);
			GameSystem<StatisticsSystem>.Instance().GetLevel(out var level, out var currentExp, out var currentMaxExp);
			SetExp(level, currentExp, currentMaxExp);
		}
	}

	protected virtual string MakeNameText(string playerName, int freq)
	{
		return $"{playerName}[size=20]#{freq:0000} kHz[/size]";
	}

	private void SetClan(Clan clan)
	{
		_clanWidget.gameObject.SetActive(GameSystem<MenuSystem>.Instance().IsEnabled(MenuType.Clan));
		_clanLabel.text = ((clan != null) ? T._("{0} 부족", clan.Name) : T._("부족이 없습니다"));
	}

	protected virtual void SetExp(int level, int current, int currentMax)
	{
		float num = (float)current / (float)currentMax;
		_expGauge.fillAmount = num;
		_expGauge.gameObject.SetActive(num > 0f);
	}
}
