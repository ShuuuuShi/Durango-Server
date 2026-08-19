using Durango.Logic;
using Durango.Logic.Event;
using Durango.Logic.Notification;
using Durango.UI.Control;
using Durango.Utils.Extensions;
using L10N;
using NestedPrefab;
using Shared.Attendance;
using UnityEngine;

namespace Durango.UI;

[Uri("Event")]
public class EventGroup : UIBase, INotificationable
{
	private enum AttendanceType
	{
		Monthly,
		Weekly,
		Event
	}

	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private NestedPrefabLinker _tabLinker;

	[SerializeField]
	[EnumList(typeof(AttendanceType), false, 0, -1)]
	private CalendarWidget[] _attendanceList;

	private static bool _isRewardChecked;

	private int _selectedTab;

	private Countable _notification;

	private IconTabList _tabList;

	public Notification Notification => _notification;

	private void Awake()
	{
		_openCloseSound = UISound.GroupType.Event;
		_notification = new Countable(Type.Important);
	}

	private void Start()
	{
		_titleWidget.Object.SetTitle(T._("이벤트"));
		_tabList = _tabLinker.Object.GetComponent<IconTabList>();
		_tabList.Clicked += OnClickTab;
		base.OnOpenSucceed += OnOpened;
		GameSystem<EventSystem>.Instance().CalendarUpdated += OnCalendarUpdate;
		OnCalendarUpdate();
		SetChildrenActive(activated: false);
	}

	private void OnCalendarUpdate()
	{
		Calendar[] calendars = GameSystem<EventSystem>.Instance().Calendars;
		int size = KUtility.GetSize(calendars);
		if (size == 0)
		{
			return;
		}
		_selectedTab = Mathf.Clamp(_selectedTab, 0, size - 1);
		int num = 0;
		int? num2 = null;
		for (int i = 0; i < size; i++)
		{
			Calendar calendar = calendars[i];
			if (calendar.HasTodayReward())
			{
				_tabList.SetNotification(i, on: true, Type.Important);
				num++;
				if (!num2.HasValue)
				{
					num2 = i;
				}
			}
			else
			{
				_tabList.SetNotification(i, on: false, Type.Important);
			}
		}
		_notification.Count = num;
		if (!_isRewardChecked)
		{
			_isRewardChecked = true;
			if (num2.HasValue)
			{
				_selectedTab = num2.Value;
				Open();
			}
		}
	}

	private void OnOpened()
	{
		RefreshTabs();
		SelectTab(_selectedTab);
	}

	private void RefreshTabs()
	{
		Calendar[] calendars = GameSystem<EventSystem>.Instance().Calendars;
		_tabList.BeginLoad();
		int i = 0;
		for (int size = KUtility.GetSize(calendars); i < size; i++)
		{
			_tabList.Add(null, calendars[i].TabName);
		}
		_tabList.EndLoad();
	}

	private void OnClickTab(int index)
	{
		if (_selectedTab != index)
		{
			SelectTab(index);
		}
	}

	private void SelectTab(int index)
	{
		Calendar[] calendars = GameSystem<EventSystem>.Instance().Calendars;
		if (index < KUtility.GetSize(calendars))
		{
			_selectedTab = index;
			_tabList.Select(index);
			Calendar calendar = calendars[index];
			AttendanceType attendanceType = GetAttendanceType(calendar.Category);
			for (int i = 0; i < _attendanceList.Length; i++)
			{
				bool active = i == (int)attendanceType;
				_attendanceList[i].gameObject.SetActive(active);
			}
			UIUtility.UpdateAnchors(base.transform);
			_attendanceList[(int)attendanceType].Set(calendar);
		}
	}

	private static AttendanceType GetAttendanceType(CategoryType category)
	{
		switch (category)
		{
		case CategoryType.Event1:
		case CategoryType.Event2:
		case CategoryType.Event3:
		case CategoryType.Event4:
		case CategoryType.Event5:
			return AttendanceType.Event;
		case CategoryType.Returner:
			return AttendanceType.Weekly;
		default:
			return AttendanceType.Monthly;
		}
	}

	public Transform GetCategoryTransform(CategoryType category)
	{
		int index = -1;
		Calendar[] calendars = GameSystem<EventSystem>.Instance().Calendars;
		int i = 0;
		for (int size = KUtility.GetSize(calendars); i < size; i++)
		{
			if (calendars[i].Category == category)
			{
				index = i;
				break;
			}
		}
		IconTabWidget iconTabWidget = _tabList.Get(index);
		return (!(iconTabWidget == null)) ? iconTabWidget.transform : null;
	}

	public CategoryType GetCurrenCategoryType()
	{
		Calendar[] calendars = GameSystem<EventSystem>.Instance().Calendars;
		if (_selectedTab >= KUtility.GetSize(calendars))
		{
			return CategoryType.Monthly;
		}
		Calendar calendar = calendars[_selectedTab];
		return calendar.Category;
	}

	public Transform GetCategoryNodeWidget(int index)
	{
		CategoryType currenCategoryType = GetCurrenCategoryType();
		AttendanceType attendanceType = GetAttendanceType(currenCategoryType);
		CalendarWidget calendarWidget = _attendanceList[(int)attendanceType];
		CalendarNodeWidget nodeWidget = calendarWidget.GetNodeWidget(index);
		return (!(nodeWidget != null)) ? null : nodeWidget.transform;
	}

	protected override void DefaultUri()
	{
		base.DefaultUri();
		string argument = UriParser.GetArgument("Tab");
		if (!argument.TryEnum<AttendanceType>(out var type))
		{
			return;
		}
		Calendar[] calendars = GameSystem<EventSystem>.Instance().Calendars;
		if (calendars != null)
		{
			int num = calendars.IndexOf((Calendar x) => GetAttendanceType(x.Category) == type);
			if (num != -1)
			{
				SelectTab(num);
			}
		}
	}
}
