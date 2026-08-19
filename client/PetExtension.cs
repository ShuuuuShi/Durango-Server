using Durango.UI;
using Messages;
using Shared.Animal;
using Yaml;
using Yaml.Util;

public static class PetExtension
{
	public static bool HasEyeTrail(this Messages.Pet pet)
	{
		return pet.Rank == PetRank.S;
	}

	public static string GetPetName(this Messages.Pet pet, bool includeRank = false)
	{
		string text;
		if (string.IsNullOrEmpty(pet.Name))
		{
			Yaml.Pet pet2 = SingletonDict<int, Yaml.Pet>.Get(pet.EntityType);
			text = ((pet2 != null) ? pet2.Name.ToString() : pet.EntityType.ToString());
		}
		else
		{
			text = pet.Name;
		}
		if (includeRank)
		{
			text = PetUtil.GetRankedName(text, pet.Rank);
		}
		return text;
	}

	public static int GetAnimalType(this Messages.Pet pet)
	{
		return SingletonDict<int, Yaml.Pet>.Get(pet.EntityType)?.VehicleEntityType ?? 0;
	}
}
