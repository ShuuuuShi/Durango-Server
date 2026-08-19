using System;
using Durango.Logic;
using Durango.Network;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class PvpIslandSurvivorCountWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _descriptionLabel;

	[SerializeField]
	private UISprite _progressSprite;

	[SerializeField]
	private UISprite _progressSpriteBg;

	[SerializeField]
	private TweenerPlayer _tweener;

	private void Awake()
	{
		base.gameObject.SetActive(GameManager.Region.IsPvpIsland());
		GameSystem<PvpIslandSystem>.Instance().PlayerCountUpdated += FillSurvivorCount;
		UIEventListener uIEventListener = UIEventListener.Get(base.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			string text = T._("<em>생존자의 수</em> 입니다. <em>최후의 1명</em>이 남을때까지 난투가 진행됩니다.");
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			widgetTooltipControl.Set(null, text, 400);
			widgetTooltipControl.AutoPosition = true;
			widgetTooltipControl.Show(4f);
		});
		_tweener.gameObject.SetActive(value: false);
		_tweener.ResetToFirst();
		FillSurvivorCount(-1);
	}

	private void FillSurvivorCount(int playerCount)
	{
		bool flag = playerCount < 0;
		bool num = playerCount > 3;
		_descriptionLabel.text = ((!flag) ? playerCount.ToString() : "-");
		_progressSprite.fillAmount = ((!flag) ? ((float)playerCount / (float)GameSystem<PvpIslandSystem>.Instance().TotalPlayerCount) : 1f);
		if (num || flag)
		{
			_progressSprite.color = PresetColor.UIYellow;
			_progressSpriteBg.color = new Color32(111, 71, 5, byte.MaxValue);
			_descriptionLabel.color = PresetColor.UIYellow;
			_tweener.gameObject.SetActive(value: false);
			_tweener.ResetToFirst();
		}
		else
		{
			_progressSprite.color = new Color32(242, 53, 4, byte.MaxValue);
			_progressSpriteBg.color = new Color32(108, 22, 3, byte.MaxValue);
			_descriptionLabel.color = PresetColor.UILightRed;
			_tweener.gameObject.SetActive(value: true);
			_tweener.Play();
		}
	}

	[ExposedInEditor("3인 이하")]
	private void TestAlertCounter()
	{
		base.gameObject.SetActive(value: true);
		Connections.Frontend.PushPacket(new S02PVPStatus
		{
			RemainSurvivorCount = 16
		});
		Connections.Frontend.PushPacket(new S02PVPStatus
		{
			RemainSurvivorCount = 5
		});
	}

	[ExposedInEditor("4인 이상")]
	private void TestNormalCounter()
	{
		base.gameObject.SetActive(value: true);
		Connections.Frontend.PushPacket(new S02PVPStatus
		{
			RemainSurvivorCount = 7
		});
		Connections.Frontend.PushPacket(new S02PVPStatus
		{
			RemainSurvivorCount = 2
		});
	}
}
