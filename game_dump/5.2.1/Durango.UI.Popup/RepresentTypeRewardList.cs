using Durango.UI.Control;
using L10N;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Popup;

public class RepresentTypeRewardList : MonoBehaviour
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UISprite _activeGaugeBg;

	[SerializeField]
	private KScrollView _scrollList;

	[SerializeField]
	private UIWidget _selectorWidget;

	private bool _isFiratActive;

	private bool _isLastActive;

	private string _focusRewardId;

	private void Start()
	{
		_titleLabel.text = T._("전체 보상 목록");
	}

	private void Update()
	{
		if (!_isLastActive && _isFiratActive)
		{
			float currentOffset = _scrollList.CurrentOffset;
			if (currentOffset < 0f)
			{
				_activeGaugeBg.fillAmount = (0f - currentOffset) / _scrollList.ViewLength + 0.01f;
			}
			else
			{
				_activeGaugeBg.fillAmount = 0f;
			}
		}
	}

	public void FocusReward(string id)
	{
		_focusRewardId = id;
	}

	public void Set(float value, DerivedRewardData[] rewards, bool reset)
	{
		_scrollList.Nodes.BeginLoad();
		_isFiratActive = value > 0f;
		_isLastActive = false;
		int num = -1;
		int i = 0;
		for (int size = KUtility.GetSize(rewards); i < size; i++)
		{
			DerivedRewardData derivedRewardData = rewards[i];
			DerivedReward derivedReward = SingletonDict<string, DerivedReward>.Get(derivedRewardData.RewardId);
			RepresentTypeRewardNode component = _scrollList.Nodes.GetNext().GetComponent<RepresentTypeRewardNode>();
			int requiredValue = derivedRewardData.RequiredValue;
			if (derivedRewardData.RewardId == _focusRewardId)
			{
				_selectorWidget.transform.parent = component.transform;
				_selectorWidget.SetAnchor(component.gameObject, 0, 0, 0, 0);
				UIUtility.UpdateAnchors(_selectorWidget.transform);
				num = i;
			}
			int num2 = ((i <= 0) ? (-requiredValue) : rewards[i - 1].RequiredValue);
			int num3 = ((i + 1 >= size) ? (-1) : rewards[i + 1].RequiredValue);
			component.Set(requiredValue.ToString(), (derivedReward != null) ? derivedReward.ToDescription() : derivedRewardData.RewardId);
			if (value < (float)requiredValue)
			{
				int num4 = requiredValue - num2;
				component.SetGaugeRatio((num4 <= 0) ? 0.49f : ((value - (float)num2) / (float)num4 - 0.5f));
			}
			else if (num3 == -1)
			{
				component.SetGaugeRatio(1f);
				_isLastActive = true;
			}
			else
			{
				int num5 = num3 - requiredValue;
				component.SetGaugeRatio((num5 <= 0) ? 1f : ((value - (float)requiredValue) / (float)num5 + 0.5f));
			}
		}
		_scrollList.Nodes.EndLoad();
		_selectorWidget.gameObject.SetActive(num != -1);
		_activeGaugeBg.fillAmount = ((!_isLastActive) ? 0f : 1f);
		if (reset)
		{
			_scrollList.ResetPosition();
			if (num > 0)
			{
				_scrollList.MoveToVisibleArea(num, instant: true, 50f, 50f);
			}
		}
		else
		{
			_scrollList.Reposition();
		}
	}
}
