using Building;
using Crafting;
using Durango.Logic.Statistics;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using Messages;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class QuestItemWidget : UIWidget
{
	[SerializeField]
	private UISprite _frameSprite;

	[SerializeField]
	private UISprite _checkSprite;

	[SerializeField]
	private ItemIconTex _itemIconTexture;

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private UILabel _countLabel;

	[SerializeField]
	private UILabel _typeLabel;

	private string _tooltip;

	public void SetItem(Messages.RewardItem item, bool finished)
	{
		_itemIconTexture.SetIcon(item);
		string text = LocalizeUtil.FormatLevel(item.Level);
		_levelLabel.text = text;
		_countLabel.text = item.Count.ToString();
		_levelLabel.gameObject.SetActive(value: true);
		_countLabel.gameObject.SetActive(value: true);
		_typeLabel.transform.parent.gameObject.SetActive(value: false);
		Prototype itemPrototype = PrototypeYaml.GetItemPrototype(item.PrototypeId, item.Level);
		_tooltip = $"{((itemPrototype != null) ? itemPrototype.Name.ToString() : item.PrototypeId)} {text}";
		SetFinished(finished);
	}

	public void SetRecipe(string recipeId, bool finished)
	{
		Crafting.Recipe recipe = GameSystem<RecipeSystem>.Instance().GetRecipe(recipeId);
		_itemIconTexture.SetIcon((recipe != null) ? recipe.Icon : string.Empty);
		_typeLabel.text = T._("제작법");
		_levelLabel.gameObject.SetActive(value: false);
		_countLabel.gameObject.SetActive(value: false);
		_typeLabel.transform.parent.gameObject.SetActive(value: true);
		_tooltip = ((recipe != null) ? recipe.Name : recipeId);
		SetFinished(finished);
	}

	public void SetBlueprint(string blueprintId, bool finished)
	{
		Building.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(blueprintId);
		_itemIconTexture.SetIcon((blueprint != null) ? blueprint.Icon : string.Empty);
		_typeLabel.text = T._("제작법");
		_levelLabel.gameObject.SetActive(value: false);
		_countLabel.gameObject.SetActive(value: false);
		_typeLabel.transform.parent.gameObject.SetActive(value: true);
		_tooltip = ((blueprint != null) ? blueprint.Name : blueprintId);
		SetFinished(finished);
	}

	public void SetTitle(string titleId, bool finished)
	{
		Durango.Logic.Statistics.Title title = GameSystem<StatisticsSystem>.Instance().GetTitle(titleId);
		string text = ((title != null) ? title.Name : string.Empty);
		_itemIconTexture.SetIcon("icon_autoguidegroup_title");
		_levelLabel.text = text;
		_levelLabel.gameObject.SetActive(value: true);
		_countLabel.gameObject.SetActive(value: false);
		_typeLabel.transform.parent.gameObject.SetActive(value: false);
		_tooltip = text;
		SetFinished(finished);
	}

	private void SetFinished(bool finished)
	{
		_frameSprite.gameObject.SetActive(!finished);
		_checkSprite.gameObject.SetActive(finished);
	}

	private void OnClick()
	{
		if (!string.IsNullOrEmpty(_tooltip))
		{
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			widgetTooltipControl.Set(null, _tooltip);
			widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
			widgetTooltipControl.Show(10f);
		}
	}
}
