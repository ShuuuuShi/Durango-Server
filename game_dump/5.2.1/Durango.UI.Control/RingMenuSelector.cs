using System.Collections.Generic;
using System.Linq;
using Durango.Render.Camera;
using Durango.System;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.Control;

public class RingMenuSelector : Selectable
{
	private const int MenuCount = 6;

	[SerializeField]
	[Tooltip("충돌 체크 너비")]
	private float _colliderThickness;

	[SerializeField]
	private Vector3 _mousePos;

	[SerializeField]
	private Vector3 _centerPos;

	[SerializeField]
	private Vector3 _dir;

	[SerializeField]
	private Vector3 _newDir;

	private Vector2 _defaultResolution;

	private InteractionMenuWidget_PC _currentMenu;

	private float _radius;

	public List<InteractionMenuWidget_PC> Menus { get; private set; }

	public void SetRadius(float radius)
	{
		_radius = radius;
		int num = (int)(radius * 2f + _colliderThickness);
		base.Widget.SetDimensions(num, num);
	}

	public void SetActiveMenus(List<InteractionMenuWidgetBase> activeMenus)
	{
		base.gameObject.SetActive(value: true);
		Menus = activeMenus?.Cast<InteractionMenuWidget_PC>().ToList();
		_currentMenu = null;
		SetMenusNormal();
	}

	protected override void OnInit()
	{
		_defaultResolution.x = Platform.Instance.DefaultUISize;
		_defaultResolution.y = (float)Platform.Instance.DefaultUISize * UIAnchorPolicy.DefaultAspectRatio;
	}

	protected override void OnRefresh(State state)
	{
		if (!(_currentMenu == null))
		{
			_currentMenu.SetState(state);
		}
	}

	[UsedImplicitly]
	protected override void OnClick()
	{
		if (!(_currentMenu == null))
		{
			_currentMenu.SetClick();
		}
	}

	[UsedImplicitly]
	protected override void OnRightClick()
	{
		if (!(_currentMenu == null))
		{
			_currentMenu.SetRightClick();
		}
	}

	[UsedImplicitly]
	protected override void OnLongPress()
	{
		if (!(_currentMenu == null))
		{
			_currentMenu.SetLongPress();
		}
	}

	[UsedImplicitly]
	private void OnPress(bool isPress)
	{
		if (!(_currentMenu == null))
		{
			_currentMenu.SetPress(isPress);
		}
	}

	private void Update()
	{
		if (!base.Hovered)
		{
			SetHoverCurrentMenu(isHover: false);
			_currentMenu = null;
			return;
		}
		InteractionMenuWidget_PC hoveredMenu = GetHoveredMenu();
		if (!(_currentMenu == hoveredMenu))
		{
			SetHoverCurrentMenu(isHover: false);
			_currentMenu = hoveredMenu;
			SetHoverCurrentMenu(isHover: true);
			SetMenusNormal(_currentMenu);
		}
	}

	private void SetHoverCurrentMenu(bool isHover)
	{
		if (_currentMenu != null)
		{
			_currentMenu.SetHovered(isHover);
		}
	}

	private void SetMenusNormal(InteractionMenuWidget_PC exceptionMenu = null)
	{
		if (Menus == null)
		{
			return;
		}
		foreach (InteractionMenuWidget_PC menu in Menus)
		{
			if (!(menu == exceptionMenu) && !menu.Disabled && !menu.Pressed)
			{
				menu.SetState(State.Normal);
				menu.StopPressGauge();
			}
		}
	}

	private InteractionMenuWidget_PC GetHoveredMenu()
	{
		if (Menus == null || Menus.Count != 6)
		{
			return null;
		}
		int index = GetIndex();
		if (index == -1)
		{
			return null;
		}
		foreach (InteractionMenuWidget_PC menu in Menus)
		{
			if (menu.Index % 6 == index && !menu.Empty)
			{
				return menu;
			}
		}
		return null;
	}

	private int GetIndex()
	{
		float num = (float)Platform.Instance.DefaultUISize / (float)Singleton<UIManager>.Instance().UIRoot.manualWidth;
		float num2 = (_radius - _colliderThickness * 0.5f) * num;
		float num3 = (_radius + _colliderThickness * 0.5f) * num;
		Vector3 mousePosition = Input.mousePosition;
		Vector3 vector = MainCamera.NGUIPosToScreenPos(UIUtility.ToRootPosition(base.gameObject));
		Vector3 vector2 = mousePosition - vector;
		float num4 = Mathf.Max(_defaultResolution.x / (float)Screen.width, _defaultResolution.y / (float)Screen.height);
		vector2.x *= num4;
		vector2.y *= num4;
		vector2.z = 0f;
		float magnitude = vector2.magnitude;
		if (magnitude < num2 || magnitude > num3)
		{
			return -1;
		}
		Vector3 normalized = vector2.normalized;
		float num5 = Mathf.Acos(Vector3.Dot(normalized, Vector3.up)) * 57.29578f;
		if (normalized.x < 0f)
		{
			num5 = num5 * -1f + 360f;
		}
		return (int)num5 / 60;
	}
}
