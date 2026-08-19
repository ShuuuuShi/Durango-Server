using L10N;

namespace PlayGuide;

public class LevelUpToDo : ToDoBase
{
	private readonly int _targetLevel;

	public LevelUpToDo(int target)
	{
		_targetLevel = target;
		base.LocalText = T._("레벨 <em>{0}</em> 되기", target);
	}

	public override void OnAddItem()
	{
		GameSystem<StatisticsSystem>.Instance().LevelChanged += LevelUpToDo_LevelChanged;
		LevelUpToDo_LevelChanged(0, GameSystem<StatisticsSystem>.Instance().Level);
	}

	public override void OnRemoveItem()
	{
		GameSystem<StatisticsSystem>.Instance().LevelChanged -= LevelUpToDo_LevelChanged;
	}

	private void LevelUpToDo_LevelChanged(int prev, int cur)
	{
		if (prev != -1 && cur >= _targetLevel)
		{
			CallComplete();
		}
	}
}
