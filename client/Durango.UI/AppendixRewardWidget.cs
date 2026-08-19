using System;
using System.Collections.Generic;
using Durango.Logic.Event;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class AppendixRewardWidget : UIWidget, IUIInitializable
{
	[SerializeField]
	private UILabel _label;

	[SerializeField]
	private ListObjectPool _nodePool;

	[SerializeField]
	private UIWidget _containerWidget;

	[SerializeField]
	private RectLayoutComponent _rectLayout;

	public event Action<CalenderReward> RewardNodeClicked;

	void IUIInitializable.Init()
	{
		_nodePool.Init(delegate(GameObject obj)
		{
			CalendarNodeWidget node = obj.GetComponent<CalendarNodeWidget>();
			node.Clicked += delegate
			{
				OnClickCalendarNode(node.Reward);
			};
		});
	}

	private void Refresh()
	{
		bool flag = UIManager.IsPortraitWidget(base.gameObject);
		string arg = T._("출석 최종 보상");
		string arg2 = T._("[AD965F]받을 보상을 선택하세요!![-]");
		_label.text = ((!flag) ? $"[size=22]{arg}\n[/size][size=17]{arg2}[/size]" : $"[size=25]{arg}\n[/size][size=18]{arg2}[/size]");
		_label.height = (int)((!flag) ? (_label.printedSize.y + 40f) : (_label.printedSize.y + 30f));
		_rectLayout.UpdateLayout();
	}

	public void Set([NotNull] List<CalenderReward> appendices)
	{
		Refresh();
		_nodePool.BeginLoad();
		bool flag = UIManager.IsPortraitWidget(base.gameObject);
		int num = ((!flag) ? Mathf.Min(_containerWidget.width, _containerWidget.height / Mathf.Max(appendices.Count, 1)) : _containerWidget.height);
		int i = 0;
		for (int size = KUtility.GetSize(appendices); i < size; i++)
		{
			CalenderReward reward = appendices[i];
			CalendarNodeWidget component = _nodePool.GetNext().GetComponent<CalendarNodeWidget>();
			component.GetComponent<UIWidget>().SetDimensions(num, num);
			component.Set(reward, highlight: false);
		}
		_nodePool.EndLoad();
		UIUtility.UpdateAnchors(_containerWidget.transform);
		if (flag)
		{
			Vector3 vector = _containerWidget.localCenter;
			Vector3 zero = Vector3.zero;
			zero.x = vector.x - (float)_containerWidget.width * 0.5f + (float)(_containerWidget.width - num * _nodePool.Count) / 2f;
			zero.y = vector.y;
			UIUtility.WidgetsReposition(_nodePool, Vector3.right, zero);
		}
		else
		{
			UIUtility.WidgetsReposition(_nodePool, _containerWidget, Vector3.down);
		}
	}

	private void OnClickCalendarNode(CalenderReward reward)
	{
		if (this.RewardNodeClicked != null)
		{
			this.RewardNodeClicked(reward);
		}
	}
}
