using System;
using L10N;
using Yaml;

namespace PlayGuide;

internal class HuntToDo : ToDoBase
{
	private readonly string _id;

	public HuntToDo(string id)
	{
		_id = id;
		int.TryParse(id, out var result);
		base.LocalText = T._("<em>{0}</em> 사냥", AnimalYaml.GetName(result));
	}

	private void LocalPlayer_KilledAnimal(AnimalBehavior animal)
	{
		if (string.IsNullOrEmpty(_id) || string.Compare(animal.EntityTypeId.ToString(), _id, StringComparison.OrdinalIgnoreCase) == 0)
		{
			CallComplete();
		}
	}

	public override void OnAddItem()
	{
		PlayerBehavior.LocalPlayer.KilledAnimal += LocalPlayer_KilledAnimal;
	}

	public override void OnRemoveItem()
	{
		PlayerBehavior.LocalPlayer.KilledAnimal -= LocalPlayer_KilledAnimal;
	}
}
