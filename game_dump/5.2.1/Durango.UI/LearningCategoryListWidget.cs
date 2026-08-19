using System;
using Durango.Logic.LearningGuide;
using Durango.Logic.Notification;
using Durango.UI.Control;
using Durango.Utils.Extensions;
using NestedPrefab;

namespace Durango.UI;

public class LearningCategoryListWidget : NestedPrefabLinker
{
	private IconTabList _tabList;

	public AdviceCategory SelectedCategory { get; private set; }

	public Durango.Logic.Notification.Type NotificationType { get; private set; }

	public bool NotificationOn { get; private set; }

	private IconTabList TabList
	{
		get
		{
			if (_tabList == null)
			{
				InitalizeTabList();
			}
			return _tabList;
		}
	}

	public event Action<AdviceCategory> SelectionChanged;

	private void InitalizeTabList()
	{
		_tabList = base.Object.GetComponent<IconTabList>();
		_tabList.Clicked += OnClickCategoryListItem;
		AdviceCategory[] adviceCategories = GameSystem<StatisticsSystem>.Instance().AdviceCategories;
		_tabList.BeginLoad();
		int i = 0;
		for (int size = KUtility.GetSize(adviceCategories); i < size; i++)
		{
			AdviceCategory adviceCategory = adviceCategories[i];
			_tabList.Add(adviceCategory.Icon, adviceCategory.Name.ToString());
		}
		_tabList.EndLoad();
	}

	public void SetSelectedCategory(AdviceCategory category)
	{
		if (SelectedCategory != category)
		{
			SelectedCategory = category;
			int index = GameSystem<StatisticsSystem>.Instance().AdviceCategories.IndexOf(category);
			TabList.Select(index);
			if (this.SelectionChanged != null)
			{
				this.SelectionChanged(SelectedCategory);
			}
		}
	}

	public void RefreshNotification()
	{
		Durango.Logic.Notification.Type type = Durango.Logic.Notification.Type.Normal;
		bool notificationOn = false;
		AdviceCategory[] adviceCategories = GameSystem<StatisticsSystem>.Instance().AdviceCategories;
		int i = 0;
		for (int size = KUtility.GetSize(adviceCategories); i < size; i++)
		{
			GameSystem<StatisticsSystem>.Instance().GetAdviceCategoryNotification(adviceCategories[i].Id, out var on, out var type2);
			TabList.SetNotification(i, on, type2);
			if (on)
			{
				if (type < type2)
				{
					type = type2;
				}
				notificationOn = true;
			}
		}
		NotificationType = type;
		NotificationOn = notificationOn;
	}

	private void OnClickCategoryListItem(int index)
	{
		AdviceCategory[] adviceCategories = GameSystem<StatisticsSystem>.Instance().AdviceCategories;
		if (index >= 0 && index < KUtility.GetSize(adviceCategories))
		{
			SetSelectedCategory(adviceCategories[index]);
		}
	}
}
