using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Network;
using Durango.UI.Control;
using Messages;
using Shared.Estate;
using UnityEngine;

namespace Durango.UI.Popup;

public class PersonalRegionAdmissionPopup : TooltipBase
{
	[SerializeField]
	private BinaryToggleSlider _toggleButton;

	[SerializeField]
	private UIWidget _notAllowed;

	[SerializeField]
	private UIWidget _allowed;

	[SerializeField]
	private ListObjectPool _nodes;

	private List<LicenseCategory> _categories;

	private List<LicenseCategory> _prevCategories;

	private readonly LicenseCategory[] _list = new LicenseCategory[3]
	{
		LicenseCategory.Friend,
		LicenseCategory.Clan,
		LicenseCategory.Other
	};

	private bool _changed;

	public override bool DragLock
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	public void Set(LicenseCategory[] categories)
	{
		_categories = ((KUtility.GetSize(categories) <= 0) ? null : categories.ToList());
		_prevCategories = ((_categories == null) ? new List<LicenseCategory>() : _categories);
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		BinaryToggleSlider toggleButton = _toggleButton;
		toggleButton.ValueRatioChanged = (Action<float>)Delegate.Combine(toggleButton.ValueRatioChanged, new Action<float>(ToggleButton_ValueRatioChanged));
		BinaryToggleSlider toggleButton2 = _toggleButton;
		toggleButton2.ValueChanged = (Action<bool>)Delegate.Combine(toggleButton2.ValueChanged, new Action<bool>(ToggleButton_ValueChanged));
		_nodes.BeginLoad();
		for (int i = 0; i < _list.Length; i++)
		{
			LicenseCategory category = _list[i];
			GameObject next = _nodes.GetNext();
			next.FindComponent<UILabel>("Label").text = LocalizeUtil.Get(category);
			CheckBoxWidget checkBoxWidget = next.FindComponent<CheckBoxWidget>("CheckBox");
			checkBoxWidget.ValueChanged = (Action<bool>)Delegate.Combine(checkBoxWidget.ValueChanged, (Action<bool>)delegate(bool value)
			{
				if (value != _categories.Contains(category))
				{
					UpdateCategory(value, category);
				}
			});
			if (i == _list.Length - 1)
			{
				next.transform.Find("Seperator").gameObject.SetActive(value: false);
			}
		}
		_nodes.EndLoad();
		UIUtility.WidgetsReposition(_nodes, _allowed, new Vector3(0f, -1f));
	}

	protected override void OnShow()
	{
		base.OnShow();
		float num = ((_categories == null) ? 0f : 1f);
		_toggleButton.Set(num);
		ToggleButton_ValueRatioChanged(num);
	}

	protected override void FillData()
	{
		for (int i = 0; i < _list.Length; i++)
		{
			LicenseCategory item = _list[i];
			_nodes[i].FindComponent<CheckBoxWidget>("CheckBox").SetValue(_categories != null && _categories.Contains(item), dispatchEvent: false);
		}
	}

	private void UpdateCategory(bool add, LicenseCategory category)
	{
		if (add)
		{
			_categories.Add(category);
		}
		else
		{
			_categories.Remove(category);
		}
		_prevCategories = _categories;
		_changed = true;
	}

	private void ToggleButton_ValueRatioChanged(float ratio)
	{
		_notAllowed.gameObject.SetActive(ratio < 1f);
		_allowed.gameObject.SetActive(ratio > 0f);
		_notAllowed.alpha = 1f - ratio;
		_allowed.alpha = ratio;
	}

	private void ToggleButton_ValueChanged(bool value)
	{
		_categories = ((!value) ? null : _prevCategories);
		Refresh();
		_changed = true;
	}

	protected override void OnHide()
	{
		base.OnHide();
		if (_changed)
		{
			Connections.Frontend.Send(new SetPersonalRegionAdmission
			{
				AdmissionCategories = ((_categories == null) ? null : _categories.ToArray())
			});
			_changed = false;
		}
	}

	protected override void OnTryConfirmOnModal()
	{
		Hide();
	}
}
