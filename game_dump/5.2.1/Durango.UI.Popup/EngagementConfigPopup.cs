using System;
using Durango.Logic;
using Durango.Network;
using Durango.UI.Control;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI.Popup;

public class EngagementConfigPopup : TooltipBase
{
	[SerializeField]
	private SelectableButton _okButton;

	[SerializeField]
	private BinaryToggleSlider _toggleButton;

	[SerializeField]
	private GameObject _rewardReceivedCover;

	public override bool DragLock => true;

	protected override void OnAwake()
	{
		SelectableButton okButton = _okButton;
		okButton.Clicked = (Action)Delegate.Combine(okButton.Clicked, new Action(Hide));
		BinaryToggleSlider toggleButton = _toggleButton;
		toggleButton.ValueChanged = (Action<bool>)Delegate.Combine(toggleButton.ValueChanged, (Action<bool>)delegate(bool agreed)
		{
			KUtility.DelayedCall(this, delegate
			{
				GameSystem<EngagementSystem>.Instance().Agreed = agreed;
			}, 0.5f);
			if (!agreed)
			{
				Connections.Frontend.Send(default(DeleteEngagementData));
			}
		});
		BinaryToggleSlider toggleButton2 = _toggleButton;
		toggleButton2.Clicked = (Action)Delegate.Combine(toggleButton2.Clicked, (Action)delegate
		{
			if (_toggleButton.Disabled)
			{
				UIManager.SystemMsg(T._("해당 시스템은 점검 중이며 이용이 불가능합니다."));
			}
		});
		GameSystem<EngagementSystem>.Instance().AgreedChanged += delegate(bool agreed)
		{
			_toggleButton.Set((!agreed) ? 0f : 1f, sendEvent: false, playAnimation: true);
		};
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		_rewardReceivedCover.SetActive(GameSystem<EngagementSystem>.Instance().EngagementRewardSent);
		_toggleButton.Set((!GameSystem<EngagementSystem>.Instance().Agreed) ? 0f : 1f);
		_toggleButton.SetDisabled(OptionSystem.IsShutdownEngagement());
	}
}
