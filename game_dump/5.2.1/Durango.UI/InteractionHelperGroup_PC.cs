using System;
using Durango.UI.Popup;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class InteractionHelperGroup_PC : InteractionHelperGroupBase
{
	private bool _magnifyingGlassDisabled;

	private bool _hiddenByCombat;

	public bool MagnifyingGlassDisabled
	{
		get
		{
			return _magnifyingGlassDisabled;
		}
		set
		{
			_searchButton.GetComponent<Collider>().enabled = !value;
			_searchButton.Disabled = value;
			if (value && _helperList.IsShow)
			{
				_helperList.Hide();
			}
			_magnifyingGlassDisabled = value;
		}
	}

	protected override void Start()
	{
		base.Start();
		GameSystem<InputSystem>.Instance().On(InputCommand.HelperButtonAction, OnDoHelperButtonAction);
		UIBase.UIOpened += RefreshButton;
		UIBase.UIClosed += RefreshButton;
		UIEventListener uIEventListener = UIEventListener.Get(_searchButton.gameObject);
		uIEventListener.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onHover, new UIEventListener.BoolDelegate(OnHoverSearchButton));
		GameSystem<CombatSystem>.Instance().ChangedCombatMode += OnCombatModeChanged;
	}

	private void OnDestroy()
	{
		UIBase.UIOpened -= RefreshButton;
		UIBase.UIClosed -= RefreshButton;
	}

	private void OnCombatModeChanged(bool isCombat)
	{
		if (isCombat)
		{
			if (_helperList.IsShow)
			{
				_helperList.Hide();
				_hiddenByCombat = true;
			}
		}
		else if (_hiddenByCombat)
		{
			_helperList.Show();
			_hiddenByCombat = false;
		}
	}

	private void OnDoHelperButtonAction(InputCommandMessage message)
	{
		if (!MagnifyingGlassDisabled)
		{
			ToggleHelperListVisible();
			UISound.PlayClick(UISound.ClickType.ButtonDefault);
		}
	}

	protected override void ToggleHelperListVisible()
	{
		base.ToggleHelperListVisible();
		ShowSearchButtonTooltip(show: false);
	}

	private void RefreshButton()
	{
		bool active = UIBase.CurrentUI == null || UIBase.CurrentUI.Anchor != AnchorType.Fullscreen;
		_searchButton.gameObject.SetActive(active);
	}

	private void OnHoverSearchButton(GameObject go, bool state)
	{
		ShowSearchButtonTooltip(state);
	}

	private void ShowSearchButtonTooltip(bool show)
	{
		ButtonInfoTooltip buttonInfoTooltip = UIManager.Popup.Tooltip<ButtonInfoTooltip>();
		if (!(buttonInfoTooltip == null))
		{
			buttonInfoTooltip.Sign = 1;
			if (show)
			{
				string description = T._("주변 사물의 이름 보기/끄기");
				buttonInfoTooltip.Set(InputCommand.HelperButtonAction, description);
				buttonInfoTooltip.Show(_searchButton.gameObject, new Vector2(-5f, 20f), float.MaxValue);
			}
			else
			{
				buttonInfoTooltip.Hide();
			}
		}
	}
}
