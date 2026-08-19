using UnityEngine;

public class PlayerTriggerToDo : PlayerTriggerBase
{
	[SerializeField]
	private string _todoName;

	protected override void DoTriggerEnter(Collider other)
	{
		if (!string.IsNullOrEmpty(_todoName))
		{
			GameSystem<ToDoListSystem>.Instance().CallComplete(_todoName);
		}
	}

	protected override void DoTriggerExit(Collider other)
	{
	}
}
