using Durango.UI.Control;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class FactionSupportRequestRewardListWidget : UIWidget
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UIWidget _rewardsContainer;

	[SerializeField]
	private KScrollView _scroll;

	[SerializeField]
	private RectLayoutComponent _layout;

	private bool _resetFlag;

	protected override void OnDisable()
	{
		base.OnDisable();
		_resetFlag = false;
	}

	public void UpdateLayout()
	{
		UpdateAnchors();
		int num = Mathf.Min(80, base.height - 40);
		_rewardsContainer.height = num;
		_layout.UpdateLayout(base.width, base.height);
		_titleLabel.alignment = ((!UIManager.IsPortraitWidget(base.gameObject)) ? NGUIText.Alignment.Left : NGUIText.Alignment.Center);
		foreach (GameObject node in _scroll.Nodes)
		{
			node.GetComponent<UIWidget>().SetDimensions(num, num);
		}
		UIUtility.UpdateAnchors(base.transform);
		if (!base.isVisible)
		{
			_resetFlag = false;
		}
		_scroll.Reposition(!_resetFlag, _resetFlag);
		_resetFlag = true;
	}

	public void Set(string title, SupportRewards rewards, int friendshipPointReward)
	{
		_titleLabel.text = title;
		int size = KUtility.GetSize(rewards.Items);
		int size2 = KUtility.GetSize(rewards.Moneys);
		int num = size + size2;
		_scroll.Nodes.BeginLoad();
		if (friendshipPointReward > 0)
		{
			GameObject next = _scroll.Nodes.GetNext();
			FactionSupportRequestRewardWidget component = next.GetComponent<FactionSupportRequestRewardWidget>();
			component.Set(friendshipPointReward);
		}
		for (int i = 0; i < num; i++)
		{
			GameObject next2 = _scroll.Nodes.GetNext();
			FactionSupportRequestRewardWidget component2 = next2.GetComponent<FactionSupportRequestRewardWidget>();
			if (i < size)
			{
				component2.Set(rewards.Items[i]);
			}
			else
			{
				component2.Set(rewards.Moneys[i - size]);
			}
		}
		_scroll.Nodes.EndLoad();
	}
}
