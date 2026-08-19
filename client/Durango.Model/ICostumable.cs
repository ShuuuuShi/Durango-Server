namespace Durango.Model;

public interface ICostumable
{
	bool IsMale { get; }

	string SkinEffect { get; set; }

	void ChangeCostume(CharacterCostume.CostumeType type, string fileName);

	string GetCostumeName(CharacterCostume.CostumeType type);

	void ChangeCostumeColor(CharacterCostume.CostumeType type, ItemColor color);

	ItemColor GetCostumeColor(CharacterCostume.CostumeType type);

	void ChangeEquipment(string path);

	string GetEquipmentName();

	void ChangeEquipmentColor(ItemColor color);

	ItemColor GetEquipmentColor();

	void ChangeAccessory(string bone, string path);
}
