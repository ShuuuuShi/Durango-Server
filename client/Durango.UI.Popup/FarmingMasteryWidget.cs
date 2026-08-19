using System;
using System.Collections.Generic;
using System.Linq;
using Durango.UI.Control;
using Messages;
using UnityEngine;

namespace Durango.UI.Popup;

public class FarmingMasteryWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private UISprite _activateBorder;

	[SerializeField]
	private FarmingMasterySelectWidget[] _masteryWidgets;

	private int _level;

	public event Action<int, int> MasterySelected;

	private void Start()
	{
		GetComponent<RectLayoutComponent>().UpdateOnSizeChange();
		int i = 0;
		for (int size = KUtility.GetSize(_masteryWidgets); i < size; i++)
		{
			_masteryWidgets[i].Clicked += OnMasteryClicked;
		}
	}

	private void OnMasteryClicked(int index)
	{
		if (this.MasterySelected != null)
		{
			this.MasterySelected(_level, index);
		}
	}

	public void Set(int level, FarmingEncyclopediaData data, KeyValuePair<string, float>[][] modifiers)
	{
		_level = level;
		_levelLabel.text = LocalizeUtil.FormatLevel(level);
		int size = KUtility.GetSize(_masteryWidgets);
		int num = Mathf.Min(KUtility.GetSize(modifiers), size);
		for (int i = 0; i < num; i++)
		{
			_masteryWidgets[i].Set(i, modifiers[i].FirstOrDefault());
			_masteryWidgets[i].gameObject.SetActive(value: true);
		}
		for (int j = num; j < size; j++)
		{
			_masteryWidgets[j].gameObject.SetActive(value: false);
		}
		if (data.MasteryLevelToIndex != null && data.MasteryLevelToIndex.TryGetValue(level, out var value))
		{
			_activateBorder.gameObject.SetActive(value: true);
			for (int k = 0; k < num; k++)
			{
				_masteryWidgets[k].SetState((k != value) ? FarmingMasterySelectWidget.State.Unselected : FarmingMasterySelectWidget.State.Acquired);
			}
		}
		else if (level <= data.CurrentLevel)
		{
			_activateBorder.gameObject.SetActive(value: true);
			for (int l = 0; l < num; l++)
			{
				_masteryWidgets[l].SetState(FarmingMasterySelectWidget.State.Selectable);
			}
		}
		else
		{
			_activateBorder.gameObject.SetActive(value: false);
			for (int m = 0; m < num; m++)
			{
				_masteryWidgets[m].SetState(FarmingMasterySelectWidget.State.Locked);
			}
		}
	}
}
