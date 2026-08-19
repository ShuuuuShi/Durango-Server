public interface ICostumable
{
	bool IsMale { get; }

	CharacterCostume.SkinDirty SkinDirtyLevel { get; set; }

	void SetCostumeVisible(CharacterCostume.CostumeType type, bool isVisible);

	void ChangeCostume(string fileName);

	void ChangeCostumeColor(CharacterCostume.CostumeType type, ItemColor color);

	void ChangeEquipment(string path);

	void OnModelChanged();
}
