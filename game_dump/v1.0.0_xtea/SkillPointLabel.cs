using UnityEngine;

public class SkillPointLabel : MonoBehaviour
{
	private string _format;

	private UILabel _label;

	private UISpriteLabel _spriteLabel;

	private void Awake()
	{
		_spriteLabel = ((Component)this).GetComponent<UISpriteLabel>();
		_label = ((Component)this).GetComponent<UILabel>();
		if ((Object)(object)_label != (Object)null)
		{
			_format = _label.text;
		}
		else
		{
			_format = "{0}/{1}";
		}
	}

	private void OnEnable()
	{
		GameSystem<SkillSystem>.Instance().SkillListUpdated += OnUpdateSkills;
		OnUpdateSkills();
	}

	private void OnDisable()
	{
		GameSystem<SkillSystem>.Instance().SkillListUpdated -= OnUpdateSkills;
	}

	private void OnUpdateSkills()
	{
		string text = string.Format(_format, GameSystem<SkillSystem>.Instance().RemainSkillPoint, GameSystem<SkillSystem>.Instance().SkillPoint);
		if ((Object)(object)_spriteLabel != (Object)null)
		{
			_spriteLabel.text = text;
		}
		else if ((Object)(object)_label != (Object)null)
		{
			_label.text = text;
		}
	}
}
