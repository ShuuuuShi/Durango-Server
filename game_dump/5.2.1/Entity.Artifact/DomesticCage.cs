using System.Collections.Generic;
using System.Linq;
using Messages;
using UnityEngine;

namespace Entity.Artifact;

public class DomesticCage : CageBase
{
	public override bool OnUpdateState(double eventTime)
	{
		Set(base.Artifact.ArtifactState.DomesticCage);
		return false;
	}

	private void Set(Messages.DomesticCage? msg)
	{
		if (msg.HasValue)
		{
			SetAnimals(msg.Value.Reins);
		}
		else
		{
			SetAnimals(null);
		}
	}

	private void SetAnimals(IList<DomesticationInfo> pets)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, AnimalBehavior> animal in _animals)
		{
			string id = animal.Key;
			if (pets == null || pets.All((DomesticationInfo p) => p.ItemId != id))
			{
				list.Add(id);
			}
		}
		foreach (string item in list)
		{
			AnimalBehavior animalBehavior = _animals.Get(item);
			if (animalBehavior != null)
			{
				Object.Destroy(animalBehavior.gameObject);
			}
			_animals.Remove(item);
		}
		int i = 0;
		for (int size = KUtility.GetSize(pets); i < size; i++)
		{
			if (!_animals.ContainsKey(pets[i].ItemId))
			{
				MakeAnimal(pets[i].ItemId, pets[i].EntityType, null);
			}
		}
	}
}
