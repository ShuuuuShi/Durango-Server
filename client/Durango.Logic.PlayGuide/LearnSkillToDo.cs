using System;
using Durango.Logic.Skill;
using Durango.UI;
using L10N;

namespace Durango.Logic.PlayGuide;

public class LearnSkillToDo : ToDoBase
{
	private readonly string _id;

	private readonly string _sub;

	private readonly int _lv;

	private readonly Node _skillNode;

	public LearnSkillToDo(string idAndSub, int lv)
	{
		if (!string.IsNullOrEmpty(idAndSub))
		{
			string[] array = idAndSub.Split(':');
			_id = array[0];
			_sub = ((array.Length <= 1) ? null : array[1]);
			if (string.IsNullOrEmpty(_sub))
			{
				_sub = "__base__";
			}
		}
		_lv = lv;
		_skillNode = GameSystem<SkillSystem>.Instance().FindSkill(_id, _sub, lv);
		base.LocalText = T._("<link>{0}</link> 스킬 배우기", (_skillNode == null) ? _id : _skillNode.Name);
	}

	private void SkillLevelChanged(Durango.Logic.Skill.Skill skill)
	{
		if (skill.Level != 0)
		{
			bool flag = string.Compare(_id, skill.Id, StringComparison.OrdinalIgnoreCase) == 0;
			bool flag2 = string.Compare(_sub, skill.SubId, StringComparison.OrdinalIgnoreCase) == 0;
			bool flag3 = _lv <= 0 || _lv == skill.Level;
			if (flag && flag2 && flag3)
			{
				CallComplete();
			}
		}
	}

	public override bool OnClicked()
	{
		if (_skillNode != null)
		{
			UIManager.FindScript<SkillGroup>().Open(_skillNode);
			return true;
		}
		return false;
	}

	public override void OnAddItem()
	{
		GameSystem<SkillSystem>.Instance().SkillLevelChanged += SkillLevelChanged;
		Durango.Logic.Skill.Skill skill = GameSystem<SkillSystem>.Instance().FindSkill(_id, _sub);
		if (skill == null || skill.Level >= _lv)
		{
			CallComplete();
		}
	}

	public override void OnRemoveItem()
	{
		GameSystem<SkillSystem>.Instance().SkillLevelChanged -= SkillLevelChanged;
	}
}
