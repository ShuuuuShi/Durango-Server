using Durango.UI;
using Yaml;
using Yaml.Util;

namespace Durango.Logic.PlayGuide;

public class QuestTodo : ToDoBase
{
	public QuestTodo(string id, int current, int goal)
	{
		base.Key = id;
		QuestYml questYml = SingletonDict<string, QuestYml>.Instance.Get(id);
		if (questYml != null)
		{
			base.LocalText = questYml.Description;
		}
		base.TargetProgress = goal;
		CallProgressChange(current);
	}

	public override bool OnClicked()
	{
		QuestYml questYml = SingletonDict<string, QuestYml>.Get(base.Key);
		if (questYml != null)
		{
			QuestGroup questGroup = UIManager.FindScript<QuestGroup>();
			if ((bool)questGroup)
			{
				questGroup.Open(questYml.Category);
			}
		}
		return true;
	}
}
