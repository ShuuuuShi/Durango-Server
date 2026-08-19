using System;
using System.Collections.Generic;
using Durango.Logic.Event;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using Shared.Attendance;
using UnityEngine;

namespace Durango.UI;

public class WeeklyCalendarWidget : CalendarWidget, IUIInitializable
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _subTitleLabel;

	[SerializeField]
	private UILabel _warningLabel;

	[SerializeField]
	private UIWidget _calendarContainer;

	[SerializeField]
	private CalendarNodeWidget _baseNode;

	[SerializeField]
	private GameObject _touchBox;

	[SerializeField]
	private UITexture _foregroundTexture;

	[SerializeField]
	private UITexture _backgroundTexture;

	private Calendar _calendar;

	private ListObjectPool<CalendarNodeWidget> _nodes;

	void IUIInitializable.Init()
	{
		UIEventListener uIEventListener = UIEventListener.Get(_touchBox);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickTouchBox));
		_nodes = new ListObjectPool<CalendarNodeWidget>();
		_nodes.BaseObject = _baseNode;
		_nodes.Clear();
		UIManager.AddOnScreenResized(UpdateLayout);
		alpha = 0f;
	}

	public override void Set(Calendar calendar)
	{
		_calendar = calendar;
		_titleLabel.text = calendar.TitleName;
		UIUtility.UpdateAnchors(_titleLabel.transform);
		_subTitleLabel.text = Times.GetDateString(calendar.Since, calendar.Until);
		UIUtility.UpdateAnchors(_subTitleLabel.transform);
		if (_warningLabel != null)
		{
			string text = null;
			if (_calendar.Category == CategoryType.Returner)
			{
				text = T._("복귀일로부터 2주 이내에 7일 이상 접속하면 모든 상품을 수령할 수 있습니다.");
			}
			if (string.IsNullOrEmpty(text))
			{
				_warningLabel.transform.parent.gameObject.SetActive(value: false);
			}
			else
			{
				_warningLabel.transform.parent.gameObject.SetActive(value: true);
				_warningLabel.text = text;
			}
		}
		_foregroundTexture.fitAcpectRatio = 2.35f;
		SetTexture(_foregroundTexture, calendar.CharacterImageName);
		SetTexture(_backgroundTexture, calendar.BackgroundImageName);
		_calendar.GetRewards(SetRewards);
	}

	private static void SetTexture(UITexture texture, string imageName)
	{
		if (texture == null)
		{
			return;
		}
		string assetPath = "UI/Event/" + imageName + ".mat";
		Singleton<AssetBundleManager>.Instance().RequestAsset(assetPath, typeof(Material), delegate(UnityEngine.Object asset)
		{
			if (!(asset == null))
			{
				texture.material = asset as Material;
			}
		});
	}

	private void SetRewards([NotNull] List<CalenderReward> rewards, [NotNull] List<CalenderReward> appendices)
	{
		alpha = 1f;
		bool active = false;
		int i = 0;
		for (int num = 7; i < num; i++)
		{
			CalendarNodeWidget calendarNodeWidget = _nodes[i];
			if (i < rewards.Count)
			{
				calendarNodeWidget.gameObject.SetActive(value: true);
				CalenderReward reward = rewards[i];
				bool highlight = i + 1 == num || i + 1 == rewards.Count;
				calendarNodeWidget.Set(reward, highlight);
				if (reward.State == RewardState.Ready)
				{
					active = true;
				}
			}
			else
			{
				calendarNodeWidget.gameObject.SetActive(value: false);
			}
		}
		_touchBox.gameObject.SetActive(active);
	}

	private void UpdateLayout()
	{
		_nodes.Set(7);
		if (UIManager.IsPortraitWidget(base.gameObject))
		{
			Vector3[] array = _calendarContainer.localCorners;
			int w = _calendarContainer.width / 4;
			int num = _calendarContainer.height / 2;
			UIWidget[] array2 = new UIWidget[4];
			int num2 = 0;
			for (int i = 0; i < 4; i++)
			{
				UIWidget component = _nodes[num2].GetComponent<UIWidget>();
				num2++;
				component.SetDimensions(w, num);
				array2[i] = component;
			}
			UIUtility.WidgetsReposition(array2, Vector3.right, array[1] + new Vector3(0f, (float)(-num) * 0.5f));
			w = _calendarContainer.width / 3;
			UIWidget[] array3 = new UIWidget[3];
			for (int j = 0; j < 3; j++)
			{
				UIWidget component2 = _nodes[num2].GetComponent<UIWidget>();
				num2++;
				component2.SetDimensions(w, num);
				array3[j] = component2;
			}
			UIUtility.WidgetsReposition(array3, Vector3.right, array[1] + new Vector3(0f, (float)(-num) * 1.5f));
		}
		else
		{
			int w2 = _calendarContainer.width / 7;
			int h = _calendarContainer.height;
			for (int k = 0; k < _nodes.Count; k++)
			{
				_nodes[k].GetComponent<UIWidget>().SetDimensions(w2, h);
			}
			UIUtility.WidgetsReposition(_nodes, _calendarContainer, Vector3.right);
		}
	}

	private void OnClickTouchBox(GameObject obj)
	{
		TakeTodayAtendanceReward(_calendar, restore: false, delegate
		{
			if (_calendar != null)
			{
				_calendar.GetRewards(SetRewards);
			}
		});
	}

	public override CalendarNodeWidget GetNodeWidget(int index)
	{
		if (index < 0 || _nodes.Count <= index)
		{
			return null;
		}
		return _nodes[index];
	}
}
