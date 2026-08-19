using JetBrains.Annotations;
using L10N;
using Messages;
using UnityEngine;

public class PetMilestoneNodeWidget : UIWidget
{
	[CanBeNull]
	[SerializeField]
	private GameObject _accuiredObject;

	[CanBeNull]
	[SerializeField]
	private UILabel _bottomLabel;

	[CanBeNull]
	[SerializeField]
	private UILabel _centerLabel;

	[CanBeNull]
	[SerializeField]
	private UISprite _progressGauge;

	[CanBeNull]
	[SerializeField]
	private GameObject _readyEffect;

	public void Set(MilestoneInfo info)
	{
		if (_accuiredObject != null)
		{
			_accuiredObject.gameObject.SetActive(info.Acquired);
		}
		if (_bottomLabel != null)
		{
			_bottomLabel.text = LocalizeUtil.FormatLevel(info.Level);
		}
	}

	public void SetProgress(MilestoneInfo info, int exp, int maxExp, int petLevel)
	{
		float num = ((!((float)maxExp > 1f)) ? 0f : ((float)exp / (float)maxExp));
		if (num < 1f)
		{
			if (_progressGauge != null)
			{
				_progressGauge.fillAmount = num;
			}
			if (_centerLabel != null)
			{
				_centerLabel.text = LocalizeUtil.FormatLevel(info.Level);
				_centerLabel.SetEnable<TweenAlpha>(enable: false);
				_centerLabel.alpha = 1f;
			}
			if (_bottomLabel != null)
			{
				_bottomLabel.text = $"{exp}<weak>/{maxExp}</weak>";
				_bottomLabel.SetEnable<TweenAlpha>(enable: false);
				_bottomLabel.alpha = 1f;
			}
			if (_readyEffect != null)
			{
				_readyEffect.gameObject.SetActive(value: false);
			}
			return;
		}
		if (_progressGauge != null)
		{
			_progressGauge.fillAmount = 1f;
		}
		if (_centerLabel != null)
		{
			if (info.Acquired)
			{
				_centerLabel.text = T._("변경");
			}
			else
			{
				_centerLabel.text = T._("달성!");
			}
			UIUtility.SyncTweener(_centerLabel.SetEnable<TweenAlpha>(enable: true));
		}
		if (_bottomLabel != null)
		{
			_bottomLabel.text = ((info.Level < petLevel) ? T._("<em>속성 확정 필요</em>") : T._("속성 발견!"));
			UIUtility.SyncTweener(_bottomLabel.SetEnable<TweenAlpha>(enable: true));
		}
		if (_readyEffect != null)
		{
			_readyEffect.gameObject.SetActive(value: true);
		}
	}
}
