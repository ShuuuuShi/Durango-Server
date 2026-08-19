using System.Collections.Generic;
using StatisticsData;
using UnityEngine;

public class AutoGuideTitleSelectPage : MonoBehaviour
{
	[SerializeField]
	private AutoGuideTitleSelectWidget _selectWidget;

	[SerializeField]
	private AutoGuideTitleDetailWidget _detailWidget;

	private bool _initialized;

	public void Show(bool visible)
	{
		((Component)this).gameObject.SetActive(visible);
		if (!visible)
		{
			return;
		}
		Initialize();
		List<Title> list = new List<Title>();
		Title[] titles = GameSystem<StatisticsSystem>.Instance().Titles;
		for (int i = 0; i < titles.Length; i++)
		{
			if (titles[i].ForAdvisor)
			{
				list.Add(titles[i]);
			}
		}
		list.Sort((Title title1, Title title2) => (title1.ExptectedLevelOfAchieved == title2.ExptectedLevelOfAchieved) ? string.CompareOrdinal(title1.Name, title2.Name) : ((title1.ExptectedLevelOfAchieved > title2.ExptectedLevelOfAchieved) ? 1 : (-1)));
		_selectWidget.Setup(list);
	}

	private void Initialize()
	{
		if (!_initialized)
		{
			_selectWidget.Selected += SelectWidget_Selected;
			_detailWidget.ConfirmButtonClicked += DetailWidget_ConfirmButtonClicked;
			_initialized = true;
		}
	}

	private void SelectWidget_Selected()
	{
		_detailWidget.Set(_selectWidget.SelectedTitle);
	}

	private void DetailWidget_ConfirmButtonClicked()
	{
		if (_selectWidget.SelectedTitle != null)
		{
			GameSystem<AutoGuideSystem>.Instance().SelectTitle(_selectWidget.SelectedTitle.Id);
		}
	}
}
