using L10N;
using UnityEngine;

namespace Durango.UI.Popup;

public class GuideTooltip_PC : GuideTooltip
{
	[SerializeField]
	private UILabel _spaceBar;

	[SerializeField]
	private float _unskippableTime;

	private bool _skipEnabled;

	public static bool IsShow { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		_spaceBar.text = string.Format("<shortcut_box>{0}</shortcut_box>  {1}", InputCommand.ConfirmModalPopup, T._("다음"));
		GuideTooltip.SpotlightGuideUnskippableTime = _unskippableTime;
	}

	protected override void OnShow()
	{
		base.OnShow();
		EnableSpaceBar(enable: false);
		IsShow = true;
	}

	protected override void OnHide()
	{
		base.OnHide();
		EnableSpaceBar(enable: false);
		IsShow = false;
	}

	protected override void RestoreHideWhenTouch()
	{
		EnableSpaceBar(enable: true);
	}

	private void EnableSpaceBar(bool enable)
	{
		_spaceBar.gameObject.SetActive(enable);
		_skipEnabled = enable;
	}

	protected override void OnTryConfirmOnModal()
	{
		if (IsShow && _skipEnabled)
		{
			Hide();
		}
	}
}
