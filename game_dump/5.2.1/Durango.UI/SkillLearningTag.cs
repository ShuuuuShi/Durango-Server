using Durango.Logic;
using Durango.Logic.LearningGuide;
using Durango.Logic.Skill;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class SkillLearningTag : MonoBehaviour
{
	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private GameObject _effect;

	public void Refresh([NotNull] Category category)
	{
		Refresh(GameSystem<LearningGuideSystem>.Instance().GetSkillLearningState(category));
	}

	public void Refresh([NotNull] Group skillGroup)
	{
		Refresh(GameSystem<LearningGuideSystem>.Instance().GetSkillLearningState(skillGroup));
	}

	public void Refresh([NotNull] Node skill)
	{
		Refresh(GameSystem<LearningGuideSystem>.Instance().GetSkillLearningState(skill, includePreviousNodes: true));
	}

	public void Refresh(Learning state)
	{
		LearningGuideGroup.LearningTagOption learningTagOption = UIManager.FindScript<LearningGuideGroup>().GetLearningTagOption(state);
		if (learningTagOption.BackgroundColor != Color.clear)
		{
			if (_effect != null)
			{
				_effect.SetActive(learningTagOption.ShowEffect);
			}
			_background.color = learningTagOption.BackgroundColor;
			learningTagOption.SpriteData.Set(_icon);
			base.gameObject.SetActive(value: true);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
