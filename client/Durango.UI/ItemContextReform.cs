using System;
using Durango.Logic.Item;
using Durango.UI.Popup;
using JetBrains.Annotations;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class ItemContextReform : ItemContextBase
{
	[SerializeField]
	private UIWidget _tooltipButton;

	[SerializeField]
	private ItemContextReformSlot _slotBase;

	private ListObjectPool<ItemContextReformSlot> _slots;

	public override void Init()
	{
		base.Init();
		_slots = new ListObjectPool<ItemContextReformSlot>();
		_slots.BaseObject = _slotBase;
		_slots.UseBase = false;
		_slots.Init(delegate(ItemContextReformSlot reformSlot)
		{
			reformSlot.Init();
		});
		base.HeaderText = T._("개조  [icon=icon_question_big]");
		_tooltipButton.width = base.HeaderTextWidth;
		UIEventListener uIEventListener = UIEventListener.Get(_tooltipButton.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate(GameObject go)
		{
			TooltipButton_Clicked(go);
		});
	}

	public void Set([NotNull] ItemData item)
	{
		_slots.BeginLoad();
		int num = 0;
		foreach (ReformSlot reformSlot in item.ReformSlots)
		{
			ItemContextReformSlot next = _slots.GetNext();
			next.Set(reformSlot);
			next.ShowUpperDotLine(num++ > 0);
		}
		_slots.EndLoad();
		if (_slots.Count > 0)
		{
			base.gameObject.SetActive(value: true);
			_body.height = (int)UIUtility.WidgetsReposition(_slots, this, Vector3.down);
		}
		else
		{
			Clear();
		}
	}

	public void Clear()
	{
		base.gameObject.SetActive(value: false);
	}

	private WidgetTooltipControl TooltipButton_Clicked(GameObject go)
	{
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.Set(T._("<em>개조 슬롯</em>"), T._("장비에 적용된 개조 제작법의 효과가 표시됩니다."), 400);
		widgetTooltipControl.Show(go, Vector2.zero, 5f);
		return widgetTooltipControl;
	}
}
