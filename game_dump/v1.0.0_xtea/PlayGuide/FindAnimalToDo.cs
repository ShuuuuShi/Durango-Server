using UnityEngine;

namespace PlayGuide;

public class FindAnimalToDo : ToDoBase
{
	private readonly int _typeId;

	private float _prevTime;

	public FindAnimalToDo(string typeId)
	{
		if (string.IsNullOrEmpty(typeId))
		{
			_typeId = 0;
		}
		else
		{
			int.TryParse(typeId, out _typeId);
		}
	}

	public override void Process()
	{
		float time = Time.time;
		if (!((double)(time - _prevTime) < 1.0))
		{
			_prevTime = time;
			if (Util.CheckNearAnimal(_typeId))
			{
				CallComplete();
			}
		}
	}
}
