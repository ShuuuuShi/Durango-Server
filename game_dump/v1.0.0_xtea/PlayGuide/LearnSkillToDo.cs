using System;
using L10N;
using SkillData;

namespace PlayGuide;

public class LearnSkillToDo : ToDoBase
{
	private readonly string _id;

	private readonly string _sub;

	private readonly int _lv;

	private readonly SkillNode _skillNode;

	public LearnSkillToDo(string id, string sub, int lv)
	{
		_id = id;
		_sub = sub;
		_lv = lv;
		_skillNode = GameSystem<SkillSystem>.Instance().FindSkill(id, sub, lv);
		base.LocalText = T._("<link>{0}</link> 스킬 배우기", (_skillNode == null) ? _id : _skillNode.Name);
	}

	private void SkillLearned(SkillNode skill)
	{
		bool flag = string.IsNullOrEmpty(_id) || string.Compare(_id, skill.Id, StringComparison.OrdinalIgnoreCase) == 0;
		bool flag2 = string.IsNullOrEmpty(_sub) || string.Compare(_sub, skill.Sub, StringComparison.OrdinalIgnoreCase) == 0;
		bool flag3 = _lv <= 0 || _lv == skill.Level;
		if (flag && flag2 && flag3)
		{
			CallComplete();
		}
	}

	public override bool OnClicked()
	{
		if (_skillNode != null)
		{
			UIManager.FindScript<SkillGroup>().Open(_skillNode.Category, _skillNode.Id, _skillNode.Level);
			return true;
		}
		return false;
	}

	public override void OnAddItem()
	{
		GameSystem<SkillSystem>.Instance().SkillLearned += SkillLearned;
		Skill skill = GameSystem<SkillSystem>.Instance().FindSkill(_id, _sub);
		if (skill.Level >= _lv)
		{
			CallComplete();
		}
	}

	public override void OnRemoveItem()
	{
		GameSystem<SkillSystem>.Instance().SkillLearned -= SkillLearned;
	}
}
