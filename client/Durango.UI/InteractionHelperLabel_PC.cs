using System;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class InteractionHelperLabel_PC : InteractionHelperLabel, IUICursorChangable
{
	[SerializeField]
	private InteractionHelperLabelKey _menuKey;

	[SerializeField]
	private InteractionHelperLabelKey _rideKey;

	private bool _lockHotKey;

	public bool HotKeyPressed { get; private set; }

	protected override void OnInit()
	{
		_menuKey.SetShortcut(InputCommand.Collect, T._("링 메뉴 호출"));
		_rideKey.SetShortcut(InputCommand.Mount, T._("펫 타기"));
		GameSystem<InputSystem>.Instance().On(InputCommand.Collect, MenuKeyPressed);
		OnHovered = (Action<bool>)Delegate.Combine(OnHovered, new Action<bool>(OnHoverLabel));
	}

	private void MenuKeyPressed(InputCommandMessage message)
	{
		if (_menuKey.gameObject.activeInHierarchy)
		{
			HotKeyPressed = true;
			if (InputSystem.IsMouseButtonReversed)
			{
				OnRightClick();
			}
			else
			{
				OnClick();
			}
			HotKeyPressed = false;
		}
	}

	public void EnableHotKey(bool enable)
	{
		if (!_lockHotKey && _menuKey.gameObject.activeSelf != enable)
		{
			_menuKey.gameObject.SetActive(enable);
		}
	}

	public override void UpdateContents()
	{
		base.UpdateContents();
		_lockHotKey = false;
		bool flag = false;
		if (base.Target != null)
		{
			PetAI component = base.Target.GetComponent<PetAI>();
			if (component != null)
			{
				if (!component.IsLocalPlayersPet())
				{
					_lockHotKey = true;
					_menuKey.Activate(enable: false, enableDescription: false);
					_rideKey.Activate(enable: false, enableDescription: false);
					return;
				}
				flag = true;
			}
		}
		_menuKey.PosY = ((!flag) ? _menuKey.DefaultPosY : _rideKey.SecondaryPosY);
		_menuKey.Activate(enable: true, flag);
		_rideKey.Activate(flag, flag);
		if (flag)
		{
			if (_menuKey.DescBgWidth > _rideKey.DescBgWidth)
			{
				_rideKey.DescBgWidth = _menuKey.DescBgWidth;
			}
			else
			{
				_menuKey.DescBgWidth = _rideKey.DescBgWidth;
			}
		}
	}

	bool IUICursorChangable.IsCursorChangable()
	{
		return true;
	}

	bool IUICursorChangable.IsCursorSpecified(ref GameCursorType cursorType)
	{
		cursorType = GameCursorType.Normal;
		return false;
	}

	private void OnHoverLabel(bool isHover)
	{
		if (_immovable != null)
		{
			_immovable.Hover(isHover);
		}
	}
}
