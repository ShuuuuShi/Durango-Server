using UnityEngine;

namespace PlayGuide;

internal class FindAnimalCondition : FlowCondition
{
	private float _prevTime;

	private int _typeId;

	protected override void OnRegister()
	{
		if (string.IsNullOrEmpty(base.Param))
		{
			_typeId = 0;
		}
		else
		{
			int.TryParse(base.Param, out _typeId);
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
				Interrupt();
			}
		}
	}
}
