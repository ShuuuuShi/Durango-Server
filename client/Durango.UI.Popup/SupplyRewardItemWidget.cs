using Durango.UI.Control;
using JetBrains.Annotations;
using UnityEngine;
using Yaml;

namespace Durango.UI.Popup;

public class SupplyRewardItemWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _level;

	[SerializeField]
	private UILabel _count;

	[SerializeField]
	private ItemIconTex _icon;

	private WarpRushReward _reward;

	public void Set([NotNull] WarpRushReward reward)
	{
		_reward = reward;
		int level = reward.GetLevel();
		if (level > 0)
		{
			_level.text = LocalizeUtil.FormatLevel(level);
			_level.gameObject.SetActive(value: true);
		}
		else
		{
			_level.gameObject.SetActive(value: false);
		}
		WarpRushRewardItem.FillIcon(reward, _icon);
		int count = reward.GetCount();
		if (count > 0)
		{
			_count.text = count.ToString();
			_count.gameObject.SetActive(value: true);
		}
		else
		{
			_count.gameObject.SetActive(value: false);
		}
	}

	private void OnClick()
	{
		ShowInfo();
	}

	private void ShowInfo()
	{
		_reward.GetTooltip(out var title, out var comment);
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.Set(title, comment, 640);
		widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
		widgetTooltipControl.Show(base.gameObject, Vector2.zero, 10f);
	}
}
