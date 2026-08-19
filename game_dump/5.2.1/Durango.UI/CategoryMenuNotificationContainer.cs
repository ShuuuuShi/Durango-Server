using System.Collections.Generic;
using System.Linq;
using Durango.Logic;
using Durango.Logic.Notification;

namespace Durango.UI;

public class CategoryMenuNotificationContainer
{
	private class CategoryMenuNotification : INotificationable
	{
		private readonly MenuType _menu;

		private readonly Container _notification = new Container();

		public Notification Notification => _notification;

		public CategoryMenuNotification(MenuType type)
		{
			_menu = type;
			Refresh();
		}

		public void Refresh()
		{
			_notification.BeginSetting();
			_notification.ClearChild();
			foreach (MenuType item in from c in MenuContainer.GetChildren(_menu)
				where GameSystem<MenuSystem>.Instance().IsEnabled(c)
				select c)
			{
				INotificationable notificationable = MenuHelper.GetNotificationable(item);
				if (notificationable != null)
				{
					_notification.AddChild(notificationable);
				}
			}
			_notification.EndSetting();
		}
	}

	private class MenuTypeComparer : IEqualityComparer<MenuType>
	{
		bool IEqualityComparer<MenuType>.Equals(MenuType x, MenuType y)
		{
			return x == y;
		}

		int IEqualityComparer<MenuType>.GetHashCode(MenuType obj)
		{
			return obj.GetHashCode();
		}
	}

	private readonly Dictionary<MenuType, CategoryMenuNotification> _dict = new Dictionary<MenuType, CategoryMenuNotification>(new MenuTypeComparer());

	public INotificationable Get(MenuType type)
	{
		return GetOrCreate(type);
	}

	public void Clear()
	{
		_dict.Clear();
	}

	public void Refresh()
	{
		foreach (MenuType item in MenuContainer.FirstDepthMenus.Where(MenuContainer.HasChildren))
		{
			GetOrCreate(item).Refresh();
		}
	}

	private CategoryMenuNotification GetOrCreate(MenuType type)
	{
		CategoryMenuNotification categoryMenuNotification = _dict.Get(type);
		if (categoryMenuNotification == null)
		{
			categoryMenuNotification = new CategoryMenuNotification(type);
			_dict[type] = categoryMenuNotification;
		}
		return categoryMenuNotification;
	}
}
