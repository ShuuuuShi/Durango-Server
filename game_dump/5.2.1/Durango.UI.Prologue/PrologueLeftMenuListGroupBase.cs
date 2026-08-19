using System;
using Durango.Prologue;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using UnityEngine;

namespace Durango.UI.Prologue;

public abstract class PrologueLeftMenuListGroupBase : UIBase
{
	[SerializeField]
	protected GameObject MenuBtn;

	[SerializeField]
	protected UIWidget BackGround;

	[SerializeField]
	private GameObject _hambergerBG;

	[SerializeField]
	protected MenuWidget SkipButton;

	[SerializeField]
	protected MenuWidget ConfigButton;

	protected bool IsShow;

	private int _labelMaxWidth;

	public abstract bool Show { get; set; }

	private bool MenuBGVisible
	{
		set
		{
			if (_hambergerBG != null)
			{
				_hambergerBG.SetActive(value);
			}
		}
	}

	private void Awake()
	{
		_openCloseSound = UISound.GroupType.LeftMenu;
		UIBase.UIClosed += delegate
		{
			if (!UIBase.HasOpenedUI)
			{
				MenuBGVisible = true;
			}
		};
		UIBase.UIOpened += delegate
		{
			Close();
			MenuBGVisible = false;
		};
	}

	protected virtual void Start()
	{
		AddMenuList(SkipButton, T._("프롤로그 건너뛰기"), delegate
		{
			Close();
			UIManager.MessageBox.Show(T._("프롤로그를 정말로 건너뛰시겠습니까?"), delegate(bool ok)
			{
				if (ok)
				{
					Singleton<PrologueManager>.Instance().SkipPrologue();
				}
			});
		});
		AddMenuList(ConfigButton, T._("설정"), delegate
		{
			Close();
			UIManager.Open<ConfigGroup>();
		});
		BackGround.rightAnchor.absolute = _labelMaxWidth;
		BackGround.UpdateAnchors();
		UIEventListener uIEventListener = UIEventListener.Get(MenuBtn);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			MenuClick();
		});
		MenuBGVisible = true;
	}

	private void AddMenuList(MenuWidget menu, string text, Action func)
	{
		menu.Set(text);
		menu.Clicked = (Action)Delegate.Combine(menu.Clicked, func);
		_labelMaxWidth = Mathf.Max(menu.GetPreferredSize() + 50, _labelMaxWidth);
	}

	protected void MenuClick()
	{
		if (UIBase.HasOpenedUI)
		{
			UIBase.CloseAllUI();
		}
		else if (base.IsOpened)
		{
			Close();
		}
		else
		{
			Open();
		}
	}

	protected override bool TryOpen()
	{
		if (!Show)
		{
			Show = true;
			MenuBGVisible = false;
			return true;
		}
		return false;
	}

	protected override bool TryClose()
	{
		Show = false;
		if (!UIBase.HasOpenedUI)
		{
			MenuBGVisible = true;
		}
		return true;
	}

	protected bool HideUIFunc(VisibleController script)
	{
		if (script != base.VisibleController)
		{
			return (script.Flag & VisibleType.Base) != 0;
		}
		return false;
	}
}
