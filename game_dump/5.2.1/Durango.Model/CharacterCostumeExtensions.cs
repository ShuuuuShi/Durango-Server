namespace Durango.Model;

public static class CharacterCostumeExtensions
{
	public static int ToRequiredColorCount(this CharacterCostume.CostumeType costumeType)
	{
		if (costumeType == CharacterCostume.CostumeType.Body || costumeType == CharacterCostume.CostumeType.Head)
		{
			return 3;
		}
		return 1;
	}
}
