using Durango.Logic;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using L10N;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class WarpRushLobby : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private SelectableButton _enterButton;

	[SerializeField]
	private UILabel _periodLabel;

	[SerializeField]
	private UILabel _leftDaysLabel;

	[SerializeField]
	private UILabel _playCountLabel;

	[SerializeField]
	private UILabel _tipsLabel;

	[SerializeField]
	private SelectableButton _helpButton;

	[SerializeField]
	private UILabel _queueCountLabel;

	[SerializeField]
	private UIWidget _centerWidget;

	[SerializeField]
	private UIWidget _brianWidget;

	[SerializeField]
	private RectLayoutComponent _rectLayout;

	private string[] _tips;

	private int _tipIndex;

	void IUIInitializable.Init()
	{
		_enterButton.Clicked = EnterButton_Clicked;
		_helpButton.Text = T._("도움말");
		_helpButton.Clicked = HelpButton_Clicked;
		GameSystem<WarpRushSystem>.Instance().EntreeInfoUpdated += WarpRushSystem_EntreeInfoUpdated;
		GameSystem<WarpRushSystem>.Instance().IsInEntreeQueueChanged += WarpRushSystem_IsInEntreeQueueChanged;
		GameSystem<InventorySystem>.Instance().WalletUpdated += WarpRushSystem_IsInEntreeQueueChanged;
		GameSystem<SeasonSystem>.Instance().SeasonUpdated += SeasonSystem_SeasonUpdated;
		UIEventListener.Get(_periodLabel.gameObject).onClick = delegate(GameObject go)
		{
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			widgetTooltipControl.Set(null, T._("해당 기간동안 워프 러시 시즌이 진행됩니다. \n시즌이 지나가면 기록과 보상 목록이 초기화 됩니다."));
			widgetTooltipControl.Show(go, Vector2.zero, 60f);
		};
		_tips = new string[3]
		{
			T._("게임을 완료하기 전에는 퇴장할 수 없습니다."),
			T._("게임을 완료하면 기존에 있었던 섬이 아닌, 귀환 지점으로 이동하게 됩니다."),
			T._("탐사대에 사람이 다 모이지 않아도, 일정 시간 후 자동으로 출발합니다.")
		};
		_tipsLabel.SetText(new SyncString(delegate(out string text, out float period)
		{
			text = _tips[_tipIndex++ % _tips.Length];
			period = 5f;
		}));
		_playCountLabel.gameObject.SetActive(value: false);
		_periodLabel.gameObject.SetActive(value: false);
		_leftDaysLabel.gameObject.SetActive(value: false);
		WarpRushSystem_IsInEntreeQueueChanged();
		SeasonSystem_SeasonUpdated();
	}

	private void EnterButton_Clicked()
	{
		if (GameSystem<WarpRushSystem>.Instance().IsInEntreeQueue)
		{
			GameSystem<WarpRushSystem>.Instance().DequeueWarpRushEntry();
			return;
		}
		if (InventorySystem.Wallet.GetVoucherCount(Yaml.Util.Singleton<Constants>.Instance.Season2.Voucher.Id) > 0)
		{
			GameSystem<WarpRushSystem>.Instance().EnqueueWarpRushEntry();
			return;
		}
		string commodityId = Yaml.Util.Singleton<Constants>.Instance.Season2.Voucher.CommodityId;
		UIManager.FindScript<ShopGroup>().Open(commodityId, select: true);
	}

	private void HelpButton_Clicked()
	{
		CardNewsPopup cardNewsPopup = UIManager.Popup.Tooltip<CardNewsPopup>();
		if (cardNewsPopup.Load("warp_rush_help"))
		{
			cardNewsPopup.Show();
		}
	}

	private void WarpRushSystem_IsInEntreeQueueChanged()
	{
		bool isInEntreeQueue = GameSystem<WarpRushSystem>.Instance().IsInEntreeQueue;
		string id = Yaml.Util.Singleton<Constants>.Instance.Season2.Voucher.Id;
		bool flag = InventorySystem.Wallet.GetVoucherCount(id) > 0;
		_queueCountLabel.gameObject.SetActive(isInEntreeQueue);
		_enterButton.Text = (isInEntreeQueue ? T._("등록 취소") : ((!flag) ? T._("입장권 구입") : string.Format("{0}  [preset=round_box? [icon={1}]  1  ]", T._("탐사대 등록"), id)));
		_enterButton.SetStyle(isInEntreeQueue ? PresetButton.Style.Flat : PresetButton.Style.Solid);
	}

	private void SeasonSystem_SeasonUpdated()
	{
		Season? warpRushSeason = WarpRushSystem.GetWarpRushSeason();
		if (warpRushSeason.HasValue)
		{
			string dateString = Times.GetDateString(warpRushSeason.Value.Since, warpRushSeason.Value.Until, "{0:m} {0:HH:mm}", useClientTime: true);
			_periodLabel.gameObject.SetActive(value: true);
			_periodLabel.text = $"{dateString} [icon=icon_question_big]";
			_leftDaysLabel.gameObject.SetActive(value: true);
			_leftDaysLabel.SetText(WarpRushGroup.GetDateLimitSyncString(warpRushSeason.Value.Until, "[preset=rect_box?{0}]"));
		}
	}

	private void OnEnable()
	{
		_rectLayout.UpdateLayout();
		if (UIManager.IsPortraitScreen)
		{
			_brianWidget.rightAnchor.absolute = 0;
			_brianWidget.topAnchor.absolute = -100;
		}
		else
		{
			_brianWidget.rightAnchor.absolute = -63;
			_brianWidget.topAnchor.absolute = 41;
		}
		UIUtility.UpdateAnchors(base.transform);
	}

	private void WarpRushSystem_EntreeInfoUpdated(S02EntreeInfo info)
	{
		_queueCountLabel.text = T._("대기 인원  [FFFFFF26]I[-]  <em>{0}</em>[FFFFFF7F]/[-]{1}", info.QueueCount, OptionSystem.GetWarpRushEntryCount());
	}
}
