using System;

namespace Durango.Logic.PlayGuide;

internal class KillAnimalCondition : FlowCondition
{
	private void LocalPlayer_KilledAnimal(AnimalBehavior animal)
	{
		if (string.IsNullOrEmpty(base.Param) || string.Compare(animal.EntityTypeId.ToString(), base.Param, StringComparison.OrdinalIgnoreCase) == 0)
		{
			Interrupt();
		}
	}

	protected override void OnRegister()
	{
		PlayerBehavior.LocalPlayer.KilledAnimal += LocalPlayer_KilledAnimal;
	}

	protected override void OnUnregister()
	{
		PlayerBehavior.LocalPlayer.KilledAnimal -= LocalPlayer_KilledAnimal;
	}
}
