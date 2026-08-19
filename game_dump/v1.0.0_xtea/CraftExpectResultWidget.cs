using Crafting;
using L10N;
using UnityEngine;
using Yaml;

public class CraftExpectResultWidget : MonoBehaviour
{
	[SerializeField]
	private UIWidget _upperPanel;

	[SerializeField]
	private UILabel _labelTitle;

	[SerializeField]
	private UIWidget _centerPanel;

	[SerializeField]
	private UISprite _iconResult;

	[SerializeField]
	private UILabel _textName;

	[SerializeField]
	private CraftExpectResultDetailWidget _craftExpectResultDetailWidget;

	[SerializeField]
	private UIWidget _entrustTimePanel;

	[SerializeField]
	private UILabel _textEntrustTime;

	[SerializeField]
	private UIWidget _lowerPanel;

	[SerializeField]
	private Color[] _resultLevelRateTextColors;

	[SerializeField]
	private int[] _resultLevelRatePercentages;

	public void Refresh()
	{
		CraftSlotContainer slotContainer = GameSystem<ItemCraftingSystem>.Instance().SlotContainer;
		ShowNameAndIcon(slotContainer);
		ShowEntrustTime(slotContainer.Recipe);
		_craftExpectResultDetailWidget.Show(slotContainer.ExpectedResult);
	}

	private void ShowNameAndIcon(CraftSlotContainer slotContainer)
	{
		Crafting.Recipe recipe = slotContainer.Recipe;
		IExpectedResultInfo expectedResult = slotContainer.ExpectedResult;
		string icon = recipe.Icon;
		string arg = recipe.LocalizedName;
		int? resultLevel = null;
		if (expectedResult != null)
		{
			Prototype itemPrototype = PrototypeYaml.GetItemPrototype(expectedResult.Id, expectedResult.Level);
			if (itemPrototype != null)
			{
				icon = itemPrototype.icon;
				resultLevel = expectedResult.Level;
			}
			arg = expectedResult.Name;
		}
		string arg2 = CreateLevelText(resultLevel, slotContainer.GetAverageMaterialsLevel());
		UIUtility.SetSpriteName(_iconResult, icon);
		UIUtility.SetLabelText(_textName, $"{arg} {arg2}");
	}

	private string CreateLevelText(int? resultLevel, float averageMaterialLevel)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		string text = "?";
		Color c = UIManager.UIYellow;
		if (resultLevel.HasValue)
		{
			int percentage = ((!(averageMaterialLevel > 0f)) ? 100 : Mathf.CeilToInt((float)resultLevel.Value / averageMaterialLevel * 100f));
			text = resultLevel.Value.ToString();
			c = UIUtility.GetValueByPercentage(percentage, _resultLevelRatePercentages, _resultLevelRateTextColors);
		}
		return NGUIText.EncodeColor(T.Format("{0:lv:}", text), c);
	}

	private void ShowEntrustTime(Crafting.Recipe recipe)
	{
		if (recipe.Entrusts)
		{
			int num = (int)recipe.DurationWait / 60;
			int num2 = (int)recipe.DurationWait % 60;
			UIUtility.SetLabelText(_textEntrustTime, $"{num:D2}:{num2:D2}");
		}
		((Component)_entrustTimePanel).gameObject.SetActive(recipe.Entrusts);
		if (recipe.Entrusts)
		{
			_centerPanel.height = ((Component)this).GetComponent<UIWidget>().height - (_upperPanel.height + _entrustTimePanel.height + _lowerPanel.height);
		}
		else
		{
			_centerPanel.height = ((Component)this).GetComponent<UIWidget>().height - (_upperPanel.height + _lowerPanel.height);
		}
		UIUtility.ResetAnUpdateAnchors(((Component)_centerPanel).transform);
	}
}
