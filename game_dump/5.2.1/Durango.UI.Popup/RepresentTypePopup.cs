using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Shared.Ability;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Popup;

public class RepresentTypePopup : TooltipBase
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private GameObject _helpButton;

	[SerializeField]
	private UIWidget _scrollViewContainer;

	[SerializeField]
	private KScrollView _scrollView;

	[SerializeField]
	private RepresentTypeRewards _rewards;

	private RectLayoutComponent _layout;

	private bool _reset;

	private RepresentType? _type;

	private readonly List<Derived> _deriveds = new List<Derived>();

	private Derived? _selected;

	private bool _hasReward;

	public override bool DragLock => true;

	protected override void OnAwake()
	{
		base.OnAwake();
		_layout = GetComponent<RectLayoutComponent>();
		_scrollView.Nodes.Init(delegate(GameObject obj)
		{
			Selectable component = obj.GetComponent<Selectable>();
			component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnSelectDerived));
		});
		ResetArguments();
		UIEventListener uIEventListener = UIEventListener.Get(_helpButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate(GameObject go)
		{
			OnClickHelp(go);
		});
		GameSystem<StatisticsSystem>.Instance().StatisticsUpdated += OnUpdateStatistics;
	}

	protected override void OnHide()
	{
		base.OnHide();
		ResetArguments();
	}

	private void ResetArguments()
	{
		_type = null;
		_reset = true;
		_selected = null;
		_rewards.FocusReward(null);
	}

	private void OnUpdateStatistics()
	{
		if (base.IsVisible)
		{
			MarkAsChanged();
		}
	}

	public void Set(RepresentType type)
	{
		_type = type;
	}

	public bool Derived(Derived derived)
	{
		_selected = derived;
		RepresentType? type = _type;
		if (type.HasValue)
		{
			return false;
		}
		RepresentType[] array = Enums<RepresentType>.All();
		foreach (RepresentType representType in array)
		{
			Dictionary<Derived, float> dictionary = Yaml.Util.Singleton<Constants>.Instance.RepresentAbilities.Get(representType);
			if (dictionary != null && dictionary.ContainsKey(derived))
			{
				Set(representType);
				return true;
			}
		}
		return false;
	}

	public bool FocusReward(string rewardId)
	{
		_rewards.FocusReward(rewardId);
		Derived? selected = _selected;
		if (selected.HasValue)
		{
			return false;
		}
		foreach (KeyValuePair<Derived, DerivedRewardData[]> item in SingletonDict<Shared.Ability.Derived, DerivedRewardData[]>.Instance)
		{
			if (item.Value != null && item.Value.Any((DerivedRewardData r) => r.RewardId == rewardId) && Derived(item.Key))
			{
				return true;
			}
		}
		return false;
	}

	protected override void FillData()
	{
		RepresentType? type = _type;
		if (!type.HasValue)
		{
			_type = RepresentType.CraftingPower;
		}
		RepresentType value = _type.Value;
		_titleLabel.text = T._("{0} 상세정보", value.GetName());
		if (value == RepresentType.CraftingPower)
		{
			_helpButton.gameObject.SetActive(value: true);
		}
		else
		{
			_helpButton.gameObject.SetActive(value: false);
		}
		Dictionary<Derived, float> dictionary = Yaml.Util.Singleton<Constants>.Instance.RepresentAbilities.Get(value);
		_hasReward = false;
		_deriveds.Clear();
		_scrollView.Nodes.BeginLoad();
		if (dictionary != null)
		{
			_deriveds.AddRange(dictionary.Keys);
			_deriveds.Sort();
			foreach (Derived derived in _deriveds)
			{
				GameObject next = _scrollView.Nodes.GetNext();
				UILabel component = next.transform.Find("Key").GetComponent<UILabel>();
				UILabel component2 = next.transform.Find("Value").GetComponent<UILabel>();
				component.text = derived.GetName();
				component2.text = ((int)GameSystem<StatisticsSystem>.Instance().GetDeriveds(derived)).ToString();
				if (KUtility.GetSize(SingletonDict<Shared.Ability.Derived, DerivedRewardData[]>.Get(derived)) > 0)
				{
					_hasReward = true;
				}
			}
		}
		_scrollView.Nodes.EndLoad();
		if (_hasReward)
		{
			Derived? selected = _selected;
			if (!selected.HasValue)
			{
				_selected = _deriveds.FirstOrDefault();
			}
			_rewards.gameObject.SetActive(value: true);
		}
		else
		{
			_selected = null;
			_rewards.gameObject.SetActive(value: false);
		}
		SelectDerived(_selected);
	}

	protected override void UpdateLayout()
	{
		int safeWidth = UIManager.SafeWidth;
		int safeHeight = UIManager.SafeHeight;
		if (_hasReward)
		{
			safeWidth = Mathf.Min(850, safeWidth - 100);
			_scrollViewContainer.width = Mathf.Min(300, (int)((float)safeWidth * 0.4f));
		}
		else
		{
			safeWidth = 540;
			_scrollViewContainer.width = safeWidth;
		}
		safeHeight = Mathf.Min(740, safeHeight - 120);
		_layout.UpdateLayout(safeWidth, safeHeight);
		UIUtility.UpdateAnchors(base.transform);
		if (_reset)
		{
			_scrollView.ResetPosition();
		}
		else
		{
			_scrollView.Reposition();
		}
		_reset = false;
	}

	private void SelectDerived(Derived? derived)
	{
		_selected = derived;
		for (int i = 0; i < _scrollView.Nodes.Count; i++)
		{
			bool selected = derived.HasValue && derived.Value == _deriveds[i];
			_scrollView.Nodes[i].GetComponent<Selectable>().Selected = selected;
		}
		if (_hasReward && derived.HasValue)
		{
			_rewards.Set(derived.Value);
		}
	}

	private void OnSelectDerived()
	{
		if (_hasReward)
		{
			int num = _scrollView.Nodes.IndexOf(Selectable.Current.gameObject);
			if (num != -1)
			{
				SelectDerived(_deriveds[num]);
			}
		}
	}

	private WidgetTooltipControl OnClickHelp(GameObject obj)
	{
		RepresentType? type = _type;
		if (!type.HasValue)
		{
			return null;
		}
		WidgetTooltipControl widgetTooltipControl = null;
		if (_type.Value == RepresentType.CraftingPower)
		{
			widgetTooltipControl = UIManager.Popup.FindTooltip<WidgetTooltipControl>();
			using (Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop())
			{
				StringBuilder value = reusable.Value;
				value.AppendLine(T._("제작 능력을 올리기 위해서는"));
				value.Append("<br>10</br>");
				value.Append("<li>");
				value.Append(T._("적절한 장비와 칭호를 착용하세요."));
				value.Append("</li>");
				value.Append("<br>10</br>");
				value.Append("<li>");
				value.Append(T._("관련 연습 스킬을 습득하세요."));
				value.Append("</li>");
				value.Append("<br>10</br>");
				value.Append("<li>");
				value.Append(T._("관련 연구소를 건설하세요."));
				value.Append("</li>");
				value.Append("<br>10</br>");
				value.Append("<li>");
				value.Append(T._("장비 개조 후, 기술지원을 받으세요."));
				value.Append("</li>");
				value.Append("<br>10</br>");
				value.Append("<li>");
				value.Append(T._("능력치를 올려주는 음식을 먹으세요."));
				value.Append("</li>");
				widgetTooltipControl.Set(null, value.ToString(), 500);
			}
			widgetTooltipControl.Direction = TooltipDirection.Vertical;
			widgetTooltipControl.Show(_helpButton.gameObject, Vector2.zero, 10f);
		}
		return widgetTooltipControl;
	}
}
