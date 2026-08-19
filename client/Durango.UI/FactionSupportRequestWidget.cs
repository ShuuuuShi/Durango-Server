using System;
using Durango.Logic.Faction;
using Durango.Logic.Item;
using Durango.Network;
using Durango.UI.Control;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class FactionSupportRequestWidget : UIWidget
{
	[SerializeField]
	private GameObject _infoContainer;

	[SerializeField]
	private GameObject _emptyContainer;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UIWidget _requiredItemWidget;

	[SerializeField]
	private UILabel _requiredItemLabel;

	[SerializeField]
	private Durango.UI.Control.ItemIconWidget _requiredItemIcon;

	[SerializeField]
	private UIWidget _rewardsContainer;

	[SerializeField]
	private FactionSupportRequestRewardListWidget _rewardsBase;

	[SerializeField]
	private UIWidget _timerWidget;

	[SerializeField]
	private UILabel _timerLabel;

	[SerializeField]
	private SelectableButton _requestButton;

	[SerializeField]
	private RectLayoutComponent _layout;

	private FactionSupportRequestRewardListWidget _rewardsWidget;

	private FactionSupportRequestRewardListWidget _randomRewardsWidget;

	private SupportRequest _supportRequest;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_rewardsWidget = _rewardsBase;
			GameObject go = _rewardsContainer.gameObject;
			_randomRewardsWidget = go.AddChild(_rewardsBase.gameObject).GetComponent<FactionSupportRequestRewardListWidget>();
			_rewardsWidget.SetAnchor(go, 0f, 0.5f, 1f, 1f);
			_randomRewardsWidget.SetAnchor(go, 0f, 0f, 1f, 0.5f);
			_requestButton.CanClickWhenDisabled = true;
			SelectableButton requestButton = _requestButton;
			requestButton.Clicked = (Action)Delegate.Combine(requestButton.Clicked, new Action(OnRequestClicked));
		}
	}

	public void UpdateLayout(int w, int h)
	{
		Init();
		SetDimensions(w, h);
		_layout.UpdateLayout(w, h);
		_rewardsWidget.UpdateLayout();
		_randomRewardsWidget.UpdateLayout();
	}

	public void Set(SupportRequest request)
	{
		Init();
		_supportRequest = request;
		_infoContainer.gameObject.SetActive(value: true);
		_emptyContainer.gameObject.SetActive(value: false);
		string text = request.Name;
		if (request.MaxCount > 0)
		{
			text += $" ({request.RemainCount}/{request.MaxCount})";
		}
		_titleLabel.text = text;
		_rewardsWidget.Set(T._("확정 보상"), request.Rewards, request.FriendshipPointReward);
		_randomRewardsWidget.Set(T._("획득 가능"), request.RandomRewards, 0);
		bool flag = GameSystem<FactionSystem>.Instance().GetFaction(request.FactionType)?.IsSupportRequestAvailable() ?? false;
		flag &= request.IsAvailable();
		if (request.Duration > 0)
		{
			_timerLabel.SetText(T._("[icon=icon_timer] {0} 소요", TimedeltaFormatter.Format(request.Duration)));
			_timerLabel.SetEnable<UITweener>(enable: false);
			_timerLabel.alpha = 1f;
			_requestButton.Text = ((request.Fee.Amount <= 0) ? T._("요청") : T._("요청 {0}", Durango.Logic.Item.Inventory.CurrencyFormat(request.Fee.Amount, request.Fee.Currency)));
			_requestButton.Disabled = !flag;
			_requestButton.ClearEffect();
			_requestButton.SetClickSound(UISound.ClickType.ButtonHighlight);
			_timerWidget.gameObject.SetActive(value: true);
		}
		else
		{
			_timerLabel.text = string.Empty;
			_requestButton.Text = ((request.Fee.Amount <= 0) ? T._("교환") : T._("교환 {0}", Durango.Logic.Item.Inventory.CurrencyFormat(request.Fee.Amount, request.Fee.Currency)));
			_requestButton.ClearEffect();
			_requestButton.SetClickSound(UISound.ClickType.ButtonHighlight);
			_requestButton.Disabled = !flag;
			_timerWidget.gameObject.SetActive(value: false);
		}
		if (request.RequiredItem.HasValue)
		{
			_requiredItemWidget.gameObject.SetActive(value: true);
			bool flag2 = UIManager.IsPortraitWidget(base.gameObject);
			_requiredItemLabel.alignment = ((!flag2) ? NGUIText.Alignment.Left : NGUIText.Alignment.Center);
			Pair<Item, int> value = request.RequiredItem.Value;
			_requiredItemIcon.Set(new ItemData(value.Item1), value.Item2);
		}
		else
		{
			_requiredItemWidget.gameObject.SetActive(value: false);
		}
	}

	public Transform GetButtonTransformIfRequestAvailable()
	{
		return (!_requestButton.Disabled) ? _requestButton.transform : null;
	}

	public void SetEmpty()
	{
		Init();
		_infoContainer.gameObject.SetActive(value: false);
		_emptyContainer.gameObject.SetActive(value: true);
	}

	private void OnRequestClicked()
	{
		if (_requestButton.Disabled)
		{
			Durango.Logic.Faction.Faction faction = GameSystem<FactionSystem>.Instance().GetFaction(_supportRequest.FactionType);
			if (faction != null)
			{
				double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
				double num = Math.Max(0.0, faction.SupportRequestAvailableAt - predictedServerTime);
				double val = Math.Max(0.0, GameSystem<FactionSystem>.Instance().SupportRequestsEndAt - predictedServerTime);
				string comment = ((!_supportRequest.IsAvailable()) ? T._("요청 가능 횟수를 모두 사용하였습니다. {0} 뒤 지원 요청 가능합니다.", TimedeltaFormatter.Format(Math.Max(num, val))) : T._("{0} 뒤 지원 요청 가능합니다.", TimedeltaFormatter.Format(num)));
				UIManager.SystemMsg(comment);
			}
		}
		else
		{
			GameSystem<FactionSystem>.Instance().SendFactionSupportRequest(_supportRequest.RequestId);
		}
	}
}
