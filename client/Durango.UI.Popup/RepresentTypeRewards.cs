using L10N;
using Shared.Ability;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Popup;

public class RepresentTypeRewards : MonoBehaviour
{
	[SerializeField]
	private RepresentTypeRewardList _rewardList;

	[SerializeField]
	private GameObject _emptyPage;

	[SerializeField]
	private UILabel _emptyLabel;

	private Derived? _derived;

	private void Start()
	{
		_emptyLabel.text = T._("받을 보상이 없습니다.");
	}

	private void OnDisable()
	{
		_derived = null;
	}

	public void Set(Derived derived)
	{
		Derived? derived2 = _derived;
		bool reset = !derived2.HasValue || _derived.Value != derived;
		_derived = derived;
		DerivedRewardData[] array = SingletonDict<Derived, DerivedRewardData[]>.Get(derived);
		if (KUtility.GetSize(array) == 0)
		{
			ShowEmptyRewards();
			return;
		}
		float deriveds = GameSystem<StatisticsSystem>.Instance().GetDeriveds(derived);
		ShowRewards(deriveds, array, reset);
	}

	public void FocusReward(string id)
	{
		_rewardList.FocusReward(id);
	}

	private void ShowEmptyRewards()
	{
		_emptyPage.gameObject.SetActive(value: true);
		_rewardList.gameObject.SetActive(value: false);
	}

	private void ShowRewards(float value, DerivedRewardData[] rewards, bool reset)
	{
		_emptyPage.gameObject.SetActive(value: false);
		_rewardList.gameObject.SetActive(value: true);
		_rewardList.Set(value, rewards, reset);
	}
}
