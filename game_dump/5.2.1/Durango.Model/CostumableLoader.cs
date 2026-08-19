using System;
using System.Collections.Generic;
using UnityEngine;

namespace Durango.Model;

[RequireComponent(typeof(CostumableModel))]
public class CostumableLoader : MonoBehaviour
{
	[Serializable]
	private class CostumePair
	{
		public int EntityType;

		public CostumeTable Table;
	}

	[SerializeField]
	private List<CostumePair> _costumePairs = new List<CostumePair>();

	private int _costumeKey;

	private readonly CharacterCostume.CostumeType[] _randomCostumes = new CharacterCostume.CostumeType[4]
	{
		CharacterCostume.CostumeType.Body,
		CharacterCostume.CostumeType.Head,
		CharacterCostume.CostumeType.Hair,
		CharacterCostume.CostumeType.Beard
	};

	[ExposedInEditor(null)]
	public void LoadCostume(int entityType, int costumeKey)
	{
		if (!Application.isEditor && _costumeKey == costumeKey)
		{
			return;
		}
		_costumeKey = costumeKey;
		int i = 0;
		for (int size = KUtility.GetSize(_costumePairs); i < size; i++)
		{
			CostumePair costumePair = _costumePairs[i];
			if (costumePair != null && costumePair.EntityType == entityType)
			{
				CostumeTable table = costumePair.Table;
				if (!(table == null))
				{
					LoadCostume(table, costumeKey);
					break;
				}
			}
		}
	}

	private void LoadCostume(CostumeTable table, int key)
	{
		CostumableModel component = GetComponent<CostumableModel>();
		CharacterCostume.CostumeType[] randomCostumes = _randomCostumes;
		foreach (CharacterCostume.CostumeType costumeType in randomCostumes)
		{
			if (costumeType != CharacterCostume.CostumeType.Beard || component.IsMale)
			{
				CostumeTable.Costume randomCostume = table.GetRandomCostume(key, costumeType);
				string text = randomCostume.Path;
				if (costumeType == CharacterCostume.CostumeType.Hair && !string.IsNullOrEmpty(text) && text.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
				{
					text = randomCostume.Path.Substring(0, randomCostume.Path.Length - 10);
				}
				component.ChangeCostume(costumeType, text);
				ItemColor color = LoadItemColor(randomCostume);
				component.ChangeCostumeColor(costumeType, color);
			}
		}
		Color randomSkinColor = table.GetRandomSkinColor(key);
		component.ChangeCostumeColor(CharacterCostume.CostumeType.Skin, new ItemColor(randomSkinColor));
		CostumeTable.Costume randomEquipment = table.GetRandomEquipment(key);
		component.ChangeEquipment(randomEquipment.Path);
		ItemColor color2 = LoadItemColor(randomEquipment);
		component.ChangeEquipmentColor(color2);
	}

	private static ItemColor LoadItemColor(CostumeTable.Costume costume)
	{
		int size = KUtility.GetSize(costume.Color);
		ItemColor result = new ItemColor(size);
		for (int i = 0; i < size; i++)
		{
			result[i] = costume.Color[i];
		}
		return result;
	}
}
