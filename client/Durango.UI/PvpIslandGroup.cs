using Durango.Logic;
using Durango.Network;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

[Uri("PvpIsland")]
public class PvpIslandGroup : UIBase
{
	private const string ScoreFormat = "[FFFFFFB4]{0}<br>18</br>[-][size=32][b]{1}[/b][/size]<br>18</br>[size=18][preset=round_box? {2} ][/size]";

	[SerializeField]
	private SelectableButton _guideButton;

	[SerializeField]
	private UISpriteLabel _winCount;

	[SerializeField]
	private UISpriteLabel _playCount;

	[SerializeField]
	private UISpriteLabel _totalKill;

	[SerializeField]
	private UISpriteLabel _averageKill;

	[SerializeField]
	private UILabel _waitingCount;

	[SerializeField]
	private SelectableButton _enterButton;

	[SerializeField]
	private GameObject _winCountHighlight;

	[SerializeField]
	private RectLayoutComponent _layout;

	private void Awake()
	{
		_openCloseSound = UISound.GroupType.Default;
		_enterButton.Clicked = EnterButton_Clicked;
		_guideButton.Clicked = delegate
		{
			PvpIslandGuidePopup pvpIslandGuidePopup = UIManager.Popup.Tooltip<PvpIslandGuidePopup>();
			pvpIslandGuidePopup.Show();
		};
		GameSystem<WarpRushSystem>.Instance().EntreeInfoUpdated += WarpRushSystem_EntreeInfoUpdated;
		GameSystem<WarpRushSystem>.Instance().IsInEntreeQueueChanged += WarpRushSystem_IsInEntreeQueueChanged;
		GameSystem<WarpRushSystem>.Instance().LobbyInfoUpdated += delegate(S02LobbyInfo info)
		{
			bool flag = info.WinRank.HasValue && info.WinRank.Value.Item2 >= 1f;
			_winCountHighlight.SetActive(flag);
			UISpriteLabel winCount = _winCount;
			string title = T._("승리횟수");
			Pair<int, float>? winRank = info.WinRank;
			bool isEmphasis = flag;
			SetScore(winCount, title, winRank, isInteger: true, isEmphasis);
			SetScore(_playCount, T._("참여횟수"), info.PlayRank);
			SetScore(_totalKill, T._("처단한 스파이"), info.KillRank);
			SetScore(_averageKill, T._("평균 킬"), info.AverageKillRank, isInteger: false);
		};
		SetChildrenActive(activated: false);
		base.OnOpenSucceed += delegate
		{
			GameSystem<WarpRushSystem>.Instance().RequestLobbyInfo();
			_layout.UpdateLayout();
		};
		WarpRushSystem_IsInEntreeQueueChanged();
	}

	private void SetScore(UISpriteLabel target, string title, Pair<int, float>? info, bool isInteger = true, bool isEmphasis = false)
	{
		string text = ((!info.HasValue) ? " - " : info.Value.Item1.ToString("N0", T.Culture));
		string text2 = ((!info.HasValue) ? " - " : ((!isInteger) ? info.Value.Item2.ToString("F1", T.Culture) : info.Value.Item2.ToString("N0", T.Culture)));
		if (isEmphasis)
		{
			text = $"<em>{text}</em>";
			text2 = $"<em>{text2}</em>";
		}
		target.text = string.Format("[FFFFFFB4]{0}<br>18</br>[-][size=32][b]{1}[/b][/size]<br>18</br>[size=18][preset=round_box? {2} ][/size]", title, text2, T._("{0}위", text));
	}

	private void EnterButton_Clicked()
	{
		if (GameSystem<WarpRushSystem>.Instance().IsInEntreeQueue)
		{
			GameSystem<WarpRushSystem>.Instance().DequeueWarpRushEntry();
		}
		else
		{
			GameSystem<WarpRushSystem>.Instance().EnqueueWarpRushEntry();
		}
	}

	private void WarpRushSystem_EntreeInfoUpdated(S02EntreeInfo info)
	{
		_waitingCount.SetText(new SyncString(delegate(out string text, out float period)
		{
			bool flag = OptionSystem.GetS02WaitingQueueMin() <= info.QueueCount;
			double num = info.DepartureAt - Connections.Frontend.GetPredictedServerTime();
			string arg = T._("대기 인원  [FFFFFF3C]<bar/>[-]  <em>{0}</em>[FFFFFF7F]/[-]{1}", info.QueueCount, OptionSystem.GetWarpRushEntryCount());
			string arg2 = string.Format("{0}  [FFFFFF3C]<bar/>[-]  <em>{1}</em>", T._("남은 시간"), (!flag) ? " - " : TimedeltaFormatter.Format(num));
			text = $"{arg}        {arg2}";
			int num2 = TimedeltaFormatter.CurrentMinUnit();
			period = (float)(num % (double)num2);
		}));
	}

	private void WarpRushSystem_IsInEntreeQueueChanged()
	{
		bool isInEntreeQueue = GameSystem<WarpRushSystem>.Instance().IsInEntreeQueue;
		_waitingCount.gameObject.SetActive(isInEntreeQueue);
		_enterButton.Text = ((!isInEntreeQueue) ? T._("난투섬 입장 등록") : T._("등록 취소"));
		_enterButton.SetStyle(isInEntreeQueue ? PresetButton.Style.Flat : PresetButton.Style.Solid);
	}
}
