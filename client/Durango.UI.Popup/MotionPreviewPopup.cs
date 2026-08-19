using Durango.Logic.Social;
using UnityEngine;

namespace Durango.UI.Popup;

public class MotionPreviewPopup : TooltipBase
{
	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private PlayerPreviewWidget _playerPreview;

	[SerializeField]
	private RectLayout _layout;

	private string _motion;

	public void Set(string motion)
	{
		_motion = motion;
	}

	protected override void FillData()
	{
		Durango.Logic.Social.Motion motion = GameSystem<SocialSystem>.Instance().Emotional.GetMotion(_motion);
		if (motion == null)
		{
			_titleLabel.text = _motion;
			_playerPreview.SetModelVisibility(isShow: false);
			return;
		}
		_titleLabel.text = motion.Name;
		if (KUtility.GetSize(motion.MotionNames) > 0)
		{
			_playerPreview.SetModelVisibility(isShow: true);
			_playerPreview.Set(0.6f);
			int num = Random.Range(0, motion.MotionNames.Length);
			_playerPreview.PlayMotion(motion.MotionNames[num]);
		}
		else
		{
			_playerPreview.SetModelVisibility(isShow: false);
		}
	}

	protected override void UpdateLayout()
	{
		_titleWidget.height = _titleLabel.height + 26;
		_layout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}
}
