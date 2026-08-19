using System;
using Shared.Battle;
using Yaml;

namespace CombatData;

public class Action
{
	public string Id;

	public ActionGroup ActionGroup;

	public string Name;

	public string Icon;

	public string Description;

	public EfxType EfxType;

	public bool IsLearned;

	public ActionState State;

	public double Since;

	public double Until;

	public Action(string key, ActionSet value)
	{
		Id = key;
		ActionGroup = value.action_group;
		Name = value.name;
		Icon = value.icon;
		Description = value.description;
		EfxType = (EfxType)(int)Enum.Parse(typeof(EfxType), value.efx_type, ignoreCase: true);
	}

	public void InitDynamicValue()
	{
		IsLearned = false;
		State = ActionState.NotLearned;
		Since = -1.0;
		Until = -1.0;
	}

	public bool IsAutoAction()
	{
		return ActionGroup == ActionGroup.Normal;
	}
}
