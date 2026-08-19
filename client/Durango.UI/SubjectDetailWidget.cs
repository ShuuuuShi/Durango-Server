using System;
using Durango.Logic.LearningGuide;
using Durango.Logic.Statistics;
using JetBrains.Annotations;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class SubjectDetailWidget : MonoBehaviour
{
	[SerializeField]
	private StarHolderWidget _starHolderWidgetForDifficulty;

	[SerializeField]
	private StarHolderWidget _starHolderWidgetForCooperative;

	[SerializeField]
	private UILabel _textDescription;

	[SerializeField]
	private UISprite _rewardItemIcon;

	[SerializeField]
	private GameObject[] _rewardItemObjects;

	[SerializeField]
	private UILabel _rewardItemLabel;

	[SerializeField]
	private UILabel _rewardTitleLabel;

	[SerializeField]
	private GameObject _rewardHelper;

	[SerializeField]
	private RectLayout _layout;

	private bool _initialized;

	private Durango.Logic.LearningGuide.Advice _subject;

	public void Init()
	{
		if (!_initialized)
		{
			_starHolderWidgetForDifficulty.Init();
			_starHolderWidgetForCooperative.Init();
			UIEventListener uIEventListener = UIEventListener.Get(_rewardHelper);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
			{
				LearningGuideGroup.ShowRewardPopupWidget(_subject, isRewarded: false);
			});
			_initialized = true;
		}
	}

	public void SetSubject([NotNull] Durango.Logic.LearningGuide.Advice subject)
	{
		_subject = subject;
		_starHolderWidgetForDifficulty.SetStars(_subject.Difficulty);
		_starHolderWidgetForCooperative.SetStars(_subject.Cooperation);
		_textDescription.text = _subject.Description;
		RewardItem[] rewardItems = _subject.RewardItems;
		if (rewardItems == null || rewardItems.Length == 0)
		{
			GameObject[] rewardItemObjects = _rewardItemObjects;
			foreach (GameObject gameObject in rewardItemObjects)
			{
				gameObject.gameObject.SetActive(value: false);
			}
		}
		else
		{
			GameObject[] rewardItemObjects2 = _rewardItemObjects;
			foreach (GameObject gameObject2 in rewardItemObjects2)
			{
				gameObject2.gameObject.SetActive(value: true);
			}
			Prototype prototype = null;
			if (rewardItems.Length == 1)
			{
				prototype = PrototypeYaml.GetItemPrototype(rewardItems[0].prototype_id, rewardItems[0].level);
			}
			if (prototype != null)
			{
				_rewardItemLabel.text = prototype.Name;
				_rewardItemIcon.spriteName = prototype.Icon;
			}
			else
			{
				_rewardItemLabel.text = _subject.RewardItemsName;
				_rewardItemIcon.spriteName = "icon_reward_box";
			}
		}
		Durango.Logic.Statistics.Title title = GameSystem<StatisticsSystem>.Instance().GetTitle(_subject.RewardTitleId);
		_rewardTitleLabel.text = ((title == null) ? string.Empty : title.Name);
		_layout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}
}
