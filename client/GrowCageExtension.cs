using System.Collections.Generic;
using Messages;

public static class GrowCageExtension
{
	public static TaskStatus? GetTaskStatus(this GrowCage cage, string id)
	{
		Dictionary<string, TaskStatus> tasks = cage.Tasks;
		if (tasks == null)
		{
			return null;
		}
		if (tasks.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}
}
