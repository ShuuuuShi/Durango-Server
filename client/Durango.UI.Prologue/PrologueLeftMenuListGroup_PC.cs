using System;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using UnityEngine;

namespace Durango.UI.Prologue;

public class PrologueLeftMenuListGroup_PC : PrologueLeftMenuListGroupBase
{
	[SerializeField]
	private TweenerPlayer _tweenerPlayer;

	[SerializeField]
	private Vector2 _tooltipPosition;

	public override bool Show
	{
		get
		{
			return IsShow;
		}
		set
		{
			if (IsShow != value)
			{
				IsShow = value;
				if (IsShow)
				{
					_tweenerPlayer.gameObject.SetActive(value: true);
					_tweenerPlayer.SetDeactiveWhenFinish(isDeactivate: false);
					_tweenerPlayer.Play();
				}
				else
				{
					_tweenerPlayer.SetDeactiveWhenFinish(isDeactivate: true);
					_tweenerPlayer.Play(forward: false, null);
				}
				VisibleController.Hide(base.HideUIFunc, IsShow, "LeftMenu");
			}
		}
	}

	protected override void Start()
	{
		base.Start();
		UIEventListener.Get(BackGround.gameObject).onClick = delegate
		{
			if (IsShow)
			{
				MenuClick();
			}
		};
		UIEventListener uIEventListener = UIEventListener.Get(MenuBtn.gameObject);
		uIEventListener.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onHover, TooltipBase.ToHover(delegate(GameObject go)
		{
			string desc3 = T._("메뉴");
			return OnTooltip(go, desc3);
		}));
		UIEventListener uIEventListener2 = UIEventListener.Get(SkipButton.gameObject);
		uIEventListener2.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onHover, TooltipBase.ToHover(delegate(GameObject go)
		{
			string desc2 = T._("프롤로그 건너뛰기");
			return OnTooltip(go, desc2);
		}));
		UIEventListener uIEventListener3 = UIEventListener.Get(ConfigButton.gameObject);
		uIEventListener3.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener3.onHover, TooltipBase.ToHover(delegate(GameObject go)
		{
			string desc = T._("설정");
			return OnTooltip(go, desc);
		}));
		_tweenerPlayer.gameObject.SetActive(value: false);
	}

	private ButtonInfoTooltip OnTooltip(GameObject obj, string desc)
	{
		ButtonInfoTooltip buttonInfoTooltip = UIManager.Popup.Tooltip<ButtonInfoTooltip>();
		buttonInfoTooltip.Set(desc);
		buttonInfoTooltip.Sign = 1;
		buttonInfoTooltip.Show(obj, _tooltipPosition);
		buttonInfoTooltip.HideArrow();
		buttonInfoTooltip.IntoSafeArea();
		return buttonInfoTooltip;
	}

	private void OnReceiveBackMessage(InputCommandMessage message)
	{
		if (base.gameObject.activeInHierarchy)
		{
			MenuClick();
		}
	}
}
