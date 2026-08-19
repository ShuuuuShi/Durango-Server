using System;
using Durango.Logic;
using Durango.Network;
using Durango.Utils;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class MinimapGroupBase : UIBase
{
	[SerializeField]
	private GameObject _minimapTouchBox;

	[SerializeField]
	private Transform _attachDock;

	[SerializeField]
	private UILabel _bottomLabel;

	protected virtual void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(_minimapTouchBox);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			UISound.PlayClick(UISound.ClickType.ButtonDefault);
			OpenWorldMap();
		});
		AttachMapContext();
		WorldMapGroup worldMapGroup = UIManager.FindScript<WorldMapGroup>();
		if (worldMapGroup != null)
		{
			worldMapGroup.OnCloseSucceed += AttachMapContext;
		}
		_bottomLabel.gameObject.SetActive(value: false);
		GameSystem<WarpRushSystem>.Instance().GameStarted += RefreshWarpRushTimeLabel;
		GameSystem<WarpRushSystem>.Instance().DayChanged += RefreshWarpRushTimeLabel;
		GameSystem<PvpIslandSystem>.Instance().GameStarted += RefreshPvpIslandTimeLabel;
	}

	public Transform GetTouchTransform()
	{
		return _minimapTouchBox.transform;
	}

	private void OpenWorldMap()
	{
		WorldMapGroup worldMapGroup = UIManager.FindScript<WorldMapGroup>();
		if (!(worldMapGroup == null))
		{
			worldMapGroup.Open();
		}
	}

	private void AttachMapContext()
	{
		Singleton<MapContext>.Instance().Attach(worldMapMode: false, _attachDock);
	}

	private void RefreshWarpRushTimeLabel()
	{
		WarpRushSystem warpRushSystem = GameSystem<WarpRushSystem>.Instance();
		_bottomLabel.text = ((warpRushSystem.DaysPassed > 0) ? T._("<em>{0}일 차</em>", warpRushSystem.DaysPassed) : T._("<em>준비 기간</em>"));
		_bottomLabel.gameObject.SetActive(value: true);
		_bottomLabel.UpdateAnchors();
	}

	private void RefreshPvpIslandTimeLabel()
	{
		double firstPlayerEnteredAt = GameSystem<PvpIslandSystem>.Instance().TimeInfo.FirstPlayerEnteredAt;
		_bottomLabel.SetText(new SyncString(delegate(out string text, out float period)
		{
			double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
			double num = predictedServerTime - firstPlayerEnteredAt;
			text = TimedeltaFormatter.ColonFormat(num);
			period = (float)num % 1f;
		}));
		_bottomLabel.gameObject.SetActive(value: true);
		_bottomLabel.UpdateAnchors();
	}
}
