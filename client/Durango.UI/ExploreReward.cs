using Durango.Logic.Item;
using Durango.UI.Control;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class ExploreReward : MonoBehaviour
{
	public enum RewardState
	{
		Invalid = -1,
		Locked,
		Available,
		Received
	}

	[SerializeField]
	private GameObject _rewardItem;

	[SerializeField]
	private UILabel _rewardLabel;

	[SerializeField]
	private GameObject _checked;

	[SerializeField]
	private GameObject _effect;

	[SerializeField]
	private GlitteringDots _glitteringEffect;

	[SerializeField]
	private UISprite _currencyIcon;

	[SerializeField]
	private UILabel _currencyCount;

	public void Set(RewardState state, Cost cost)
	{
		_currencyIcon.spriteName = Durango.Logic.Item.Inventory.GetIcon(cost.Currency);
		_currencyCount.text = cost.Amount.ToString();
		switch (state)
		{
		case RewardState.Locked:
			_rewardLabel.text = string.Format("[ffffff]{0}[-]", T._("탐험 완료 보상"));
			_checked.SetActive(value: false);
			_effect.SetActive(value: false);
			_glitteringEffect.Hide();
			UIEventListener.Get(_rewardItem).onClick = null;
			break;
		case RewardState.Available:
			_rewardLabel.text = string.Format("[ffd85b]{0}[-]", T._("보상 획득 가능"));
			_checked.SetActive(value: false);
			_effect.SetActive(value: true);
			_glitteringEffect.Play();
			UIEventListener.Get(_rewardItem).onClick = delegate
			{
				MapSystem.RequestFullCountPOIsReward();
			};
			break;
		case RewardState.Received:
			_rewardLabel.text = string.Format("[D4CEBEBE]{0}[-]", T._("보상 획득함"));
			_checked.SetActive(value: true);
			_effect.SetActive(value: false);
			_glitteringEffect.Hide();
			UIEventListener.Get(_rewardItem).onClick = null;
			break;
		}
	}
}
