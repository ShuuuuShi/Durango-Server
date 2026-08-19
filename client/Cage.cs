using System.Collections.Generic;
using System.Linq;
using Messages;
using UnityEngine;

public class Cage : CageBase
{
	public override bool OnUpdateState(double eventTime)
	{
		object cage = base.Artifact.ArtifactState.Cage;
		if (cage is Messages.Cage)
		{
			SetAnimals(((Messages.Cage)cage).Pets.Data);
		}
		else if (cage is GrowCage)
		{
			SetAnimals(((GrowCage)cage).Pets.Data);
		}
		else
		{
			SetAnimals(null);
		}
		return false;
	}

	private void SetAnimals(IList<Pet> pets)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, AnimalBehavior> animal in _animals)
		{
			string id = animal.Key;
			if (pets == null || pets.All((Pet p) => p.EntityId != id))
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
			Pet pet2 = pets[i];
			if (!_animals.ContainsKey(pet2.EntityId))
			{
				PetStats stat = pet2.Stat;
				MakeAnimal(pet2.EntityId, pet2.GetAnimalType(), delegate(PetAI pet)
				{
					pet.TargetAnimal.SetOld(stat.IsOld);
				});
			}
		}
	}
}
