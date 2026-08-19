using System;
using Durango.Logic;
using UnityEngine;

namespace Durango.UI;

public abstract class LandscapeMenuListBase : UIWidget, IMenuList
{
	public event Action<MenuType> MenuClicked;

	public event Action LockClicked;

	protected abstract void OnChange();

	protected override void OnDisable()
	{
		base.OnDisable();
		if (Application.isPlaying)
		{
			alpha = 0f;
		}
	}

	public abstract void Refresh();

	public abstract bool TryGetMenuItem(MenuType type, out MenuWidget comp);

	protected void OnMenuClick(MenuType type)
	{
		if (this.MenuClicked != null)
		{
			this.MenuClicked(type);
		}
	}

	protected void OnLockClick()
	{
		if (this.LockClicked != null)
		{
			this.LockClicked();
		}
	}

	public void Show(bool instant)
	{
		base.gameObject.SetActive(value: true);
		if (instant)
		{
			this.SetEnable<TweenAlpha>(enable: false);
			alpha = 1f;
		}
		else
		{
			TweenAlpha.Begin(base.gameObject, 0.2f, 1f);
		}
	}

	public virtual void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
