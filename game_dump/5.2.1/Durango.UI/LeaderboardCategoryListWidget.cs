using System;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class LeaderboardCategoryListWidget : MonoBehaviour
{
	[SerializeField]
	private KScrollView _kScrollView;

	[EnumList(typeof(PunchingLeaderboardSystem.Category), false, 0, -1)]
	[SerializeField]
	private SpriteData[] _categoryIcons;

	private bool _initialized;

	private bool _isPortraitMode;

	public PunchingLeaderboardSystem.Category CurrentCategory { get; private set; }

	public event Action<PunchingLeaderboardSystem.Category> SelectionChanged;

	private void OnEnable()
	{
		bool flag = UIManager.IsPortraitWidget(base.gameObject);
		if (_isPortraitMode != flag)
		{
			_isPortraitMode = flag;
			RefreshScrollView();
		}
	}

	public void Init()
	{
		if (_initialized)
		{
			return;
		}
		_initialized = true;
		_kScrollView.Nodes.Init(delegate(GameObject obj)
		{
			obj.GetComponent<LeaderboardCategoryWidget>().Clicked = OnClickCategoryTypeItem;
		});
		CurrentCategory = PunchingLeaderboardSystem.Category.Recently;
		_kScrollView.Nodes.Clear();
		foreach (PunchingLeaderboardSystem.Category value in Enum.GetValues(typeof(PunchingLeaderboardSystem.Category)))
		{
			LeaderboardCategoryWidget leaderboardCategoryWidget = _kScrollView.Nodes.Add<LeaderboardCategoryWidget>();
			leaderboardCategoryWidget.Refresh(value, _categoryIcons[(int)value]);
			leaderboardCategoryWidget.SetPortraitMode(_isPortraitMode);
			leaderboardCategoryWidget.Selected = leaderboardCategoryWidget.Category == CurrentCategory;
		}
		_kScrollView.Reposition();
	}

	public void Select(PunchingLeaderboardSystem.Category category)
	{
		if (CurrentCategory != category)
		{
			CurrentCategory = category;
			RefreshSelectionStates();
			if (this.SelectionChanged != null)
			{
				this.SelectionChanged(CurrentCategory);
			}
		}
	}

	private void RefreshSelectionStates()
	{
		for (int i = 0; i < _kScrollView.Nodes.Count; i++)
		{
			LeaderboardCategoryWidget leaderboardCategoryWidget = _kScrollView.Nodes.Get<LeaderboardCategoryWidget>(i);
			if (leaderboardCategoryWidget != null)
			{
				leaderboardCategoryWidget.Selected = leaderboardCategoryWidget.Category == CurrentCategory;
			}
		}
	}

	private void RefreshScrollView()
	{
		for (int i = 0; i < _kScrollView.Nodes.Count; i++)
		{
			LeaderboardCategoryWidget leaderboardCategoryWidget = _kScrollView.Nodes.Get<LeaderboardCategoryWidget>(i);
			if (leaderboardCategoryWidget != null)
			{
				leaderboardCategoryWidget.SetPortraitMode(_isPortraitMode);
			}
		}
		_kScrollView.ScrollView.movement = ((!_isPortraitMode) ? UIScrollView.Movement.Vertical : UIScrollView.Movement.Horizontal);
		_kScrollView.Reposition();
	}

	private void OnClickCategoryTypeItem()
	{
		LeaderboardCategoryWidget leaderboardCategoryWidget = Selectable.Current as LeaderboardCategoryWidget;
		if (leaderboardCategoryWidget != null)
		{
			Select(leaderboardCategoryWidget.Category);
		}
	}
}
