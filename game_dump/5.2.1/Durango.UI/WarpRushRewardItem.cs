using System;
using Durango.Logic;
using Durango.Logic.Item;
using Durango.Logic.Shop;
using Durango.UI.Control;
using Durango.UI.Popup;
using JetBrains.Annotations;
using L10N;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class WarpRushRewardItem : MonoBehaviour
{
	public Action Clicked;

	[SerializeField]
	private ItemIconTex _icon;

	[SerializeField]
	private UILabel _count;

	[SerializeField]
	private UILabel _level;

	[SerializeField]
	private GameObject _checked;

	[SerializeField]
	private GameObject _effect;

	[SerializeField]
	private GlitteringDots _glitteringEffect;

	[SerializeField]
	private GameObject _lockObject;

	[SerializeField]
	private UIWidget _itemInfoWidget;

	private bool _enabled;

	private bool _isActivateEffect;

	private WarpRushReward _reward;

	private void OnEnable()
	{
		_enabled = true;
		ShowEffects(_isActivateEffect);
	}

	private void OnDisable()
	{
		_enabled = false;
	}

	private void OnClick()
	{
		if (Clicked != null)
		{
			Clicked();
		}
	}

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
		FillIcon(reward, _icon);
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

	public void SetScrollView(UIScrollView scrollView)
	{
		base.gameObject.GetComponent<UIDragScrollView>().scrollView = scrollView;
	}

	public static void FillIcon([NotNull] WarpRushReward reward, ItemIconTex tex)
	{
		if (reward.Currency != null)
		{
			tex.SetIcon(Inventory.GetIcon(reward.Currency.Type));
		}
		else if (reward.Item != null)
		{
			tex.SetIcon(reward.Item.PrototypeId, reward.Item.Level);
		}
		else if (!string.IsNullOrEmpty(reward.Recipe))
		{
			tex.SetIcon(GameSystem<RecipeSystem>.Instance().GetRecipe(reward.Recipe)?.Icon);
		}
		else if (!string.IsNullOrEmpty(reward.BlueprintId))
		{
			tex.SetIcon(GameSystem<RecipeSystem>.Instance().GetBlueprint(reward.BlueprintId)?.Icon);
		}
		else if (!string.IsNullOrEmpty(reward.Title))
		{
			tex.SetIcon("icon_autoguidegroup_title");
		}
		else if (reward.Voucher != null)
		{
			tex.SetIcon(reward.Voucher.Id);
		}
	}

	private void ShowEffects(bool value)
	{
		_isActivateEffect = value;
		if (_enabled)
		{
			_effect.SetActive(value);
			if (value)
			{
				_glitteringEffect.Play();
			}
			else
			{
				_glitteringEffect.Hide();
			}
		}
	}

	public void SetState(WarpRushSystem.RewardState state, bool isForbidden)
	{
		switch (state)
		{
		case WarpRushSystem.RewardState.Available:
			_checked.SetActive(value: false);
			_itemInfoWidget.alpha = 1f;
			ShowEffects(value: true);
			break;
		case WarpRushSystem.RewardState.Received:
			_checked.SetActive(value: true);
			_itemInfoWidget.alpha = 0.6f;
			ShowEffects(value: false);
			break;
		case WarpRushSystem.RewardState.Locked:
			_checked.SetActive(value: false);
			_itemInfoWidget.alpha = 0.5f;
			ShowEffects(value: false);
			break;
		default:
			throw new ArgumentOutOfRangeException("state", state, null);
		}
		_lockObject.gameObject.SetActive(isForbidden);
	}

	public void ShowTooltip()
	{
		_reward.GetTooltip(out var title, out var comment);
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.Set(title, comment, 640);
		widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
		widgetTooltipControl.Show(base.gameObject, Vector2.zero, 10f);
		if (_lockObject.gameObject.activeSelf)
		{
			Durango.Logic.Shop.Commodity commodity = GameSystem<ShopSystem>.Instance().GetCommodity(Singleton<Yaml.WarpRushRewards>.Instance.CashRewardCommodityId);
			if (commodity != null)
			{
				UIManager.SystemMsg(T._("상점에서 {0:을} 구매하신 후 획득 할 수 있어요", commodity.Title));
			}
		}
	}
}
