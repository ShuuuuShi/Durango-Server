using System;
using Durango.Logic;
using Durango.Render.Camera;
using Durango.UI.Popup;
using L10N;
using UnityEngine;

namespace Durango.UI.Control;

public class HoverShortcutViewer : MonoBehaviour
{
	private enum ShortcutType
	{
		None,
		Normal,
		Menu
	}

	[SerializeField]
	[Tooltip("툴팁이 활성화 되기까지 필요한 호버링 시간")]
	private float _threshold = 0.01f;

	[SerializeField]
	[Tooltip("툴팁 활성화 시간")]
	private float _tooltipDuration;

	[SerializeField]
	[Tooltip("마우스포인터 혹은 오브젝트 위치로부터 이동할 툴팁 위치")]
	private Vector2 _tooltipPos;

	[SerializeField]
	private bool _isFollowMouse = true;

	private ButtonInfoTooltip _tooltip;

	private Selectable _comp;

	private ShortcutType _type;

	private InputCommand _command;

	private string _desc;

	private float _hoveredTime;

	private int _createCode;

	private bool _isHovered;

	private bool _active;

	private bool _init;

	public void Set(InputCommand command, string description = null)
	{
		Init();
		Clear();
		if (command != 0)
		{
			_command = command;
			_desc = description;
			_type = ShortcutType.Normal;
		}
	}

	public void Set(MenuType menuType)
	{
		Init();
		Clear();
		_command = GameSystem<InputSystem>.Instance().Keyboard.GetMenuCommand(menuType);
		_desc = menuType.GetName();
		_type = ShortcutType.Menu;
	}

	private void Init()
	{
		if (!_init)
		{
			_tooltip = UIManager.Popup.Tooltip<ButtonInfoTooltip>();
			_comp = GetComponent<Selectable>();
			Selectable comp = _comp;
			comp.OnHovered = (Action<bool>)Delegate.Combine(comp.OnHovered, new Action<bool>(OnHovered));
			_createCode = GetHashCode();
			_init = true;
		}
	}

	private void Clear()
	{
		Hide();
		_type = ShortcutType.None;
		_command = InputCommand.None;
		_desc = null;
	}

	private void OnHovered(bool isHovered)
	{
		if (_type != 0)
		{
			_hoveredTime = Time.time;
			_isHovered = isHovered;
			if (!isHovered)
			{
				Hide();
			}
		}
	}

	private void Show()
	{
		if (!_active && _comp.Hovered)
		{
			if (_type == ShortcutType.Menu && _command == InputCommand.None)
			{
				_tooltip.Set(_desc);
			}
			else
			{
				_tooltip.Set(_command, _desc);
			}
			_tooltip.MuteOpenCloseSound = true;
			_tooltip.Show(_tooltipDuration);
			_tooltip.MuteOpenCloseSound = false;
			RePosition();
			_tooltip.CreateCode = _createCode;
			_active = true;
		}
	}

	private void Hide()
	{
		if (_active)
		{
			if (_tooltip.CreateCode == _createCode)
			{
				_tooltip.Hide();
			}
			_active = false;
		}
	}

	private void RePosition()
	{
		if (_tooltip.IsVisible)
		{
			Vector3 mousePosition;
			if (_isFollowMouse)
			{
				mousePosition = Input.mousePosition;
				mousePosition.x += _tooltipPos.x;
				mousePosition.y += _tooltipPos.y;
				mousePosition = MainCamera.ScreenPosToNGUIPos(mousePosition);
			}
			else
			{
				mousePosition = UIUtility.ToRootPosition(base.gameObject);
				mousePosition.x += _tooltipPos.x;
				mousePosition.y += _tooltipPos.y;
			}
			_tooltip.SetPosition(mousePosition);
		}
	}

	private void LateUpdate()
	{
		if (_active)
		{
			if (_isFollowMouse)
			{
				RePosition();
			}
		}
		else if (_isHovered && _hoveredTime + _threshold < Time.time)
		{
			Show();
		}
	}
}
