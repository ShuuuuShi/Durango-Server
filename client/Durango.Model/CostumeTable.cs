using System;
using System.Collections.Generic;
using Durango.Utils;
using UnityEngine;

namespace Durango.Model;

[CreateAssetMenu]
public class CostumeTable : ScriptableObject
{
	public struct Costume
	{
		public string Path;

		public Color[] Color;
	}

	[Serializable]
	private class WeightedCostume
	{
		public GameObjectType Path;

		public string[] ColorTables;

		public int Weight;
	}

	[Serializable]
	private class CostumeSet
	{
		public List<WeightedCostume> Costumes;

		public string[] ColorTables;
	}

	[SerializeField]
	private CostumeSet _body;

	[SerializeField]
	private CostumeSet _head;

	[SerializeField]
	private CostumeSet _hair;

	[SerializeField]
	private CostumeSet _beard;

	[SerializeField]
	private CostumeSet _equipment;

	[SerializeField]
	private string _skinColor;

	private readonly XXHash _randomHash = new XXHash(0);

	public Costume GetRandomCostume(int randomKey, CharacterCostume.CostumeType type)
	{
		return type switch
		{
			CharacterCostume.CostumeType.Body => GetRandomCandidate(randomKey, _body, type), 
			CharacterCostume.CostumeType.Head => GetRandomCandidate(randomKey + 1, _head, type), 
			CharacterCostume.CostumeType.Hair => GetRandomCandidate(randomKey + 2, _hair, type), 
			CharacterCostume.CostumeType.Beard => GetRandomCandidate(randomKey + 3, _beard, type), 
			_ => default(Costume), 
		};
	}

	public Costume GetRandomEquipment(int randomKey)
	{
		return GetRandomCandidate(randomKey + 4, _equipment, CharacterCostume.CostumeType.Body);
	}

	public Color GetRandomSkinColor(int randomKey)
	{
		string text = _skinColor;
		if (string.IsNullOrEmpty(text))
		{
			text = "color_skin.raw";
		}
		Color[] all = ColorTableLoader.GetAll(text);
		int num = _randomHash.Range(0, all.Length, randomKey + 5);
		return all[num];
	}

	private Costume GetRandomCandidate(int randomKey, CostumeSet costumeSet, CharacterCostume.CostumeType type)
	{
		int size = KUtility.GetSize(costumeSet.Costumes);
		if (size == 0)
		{
			return default(Costume);
		}
		int num = 0;
		for (int i = 0; i < size; i++)
		{
			WeightedCostume weightedCostume = costumeSet.Costumes[i];
			num += weightedCostume.Weight + 1;
		}
		int num2 = _randomHash.Range(1, num + 1, randomKey);
		for (int j = 0; j < size; j++)
		{
			WeightedCostume weightedCostume2 = costumeSet.Costumes[j];
			num2 -= weightedCostume2.Weight + 1;
			if (num2 <= 0)
			{
				Costume result = default(Costume);
				result.Path = weightedCostume2.Path;
				if (!string.IsNullOrEmpty(weightedCostume2.Path))
				{
					result.Color = LoadColors(randomKey, weightedCostume2.Path, (weightedCostume2.ColorTables == null) ? costumeSet.ColorTables : weightedCostume2.ColorTables, type);
				}
				return result;
			}
		}
		return default(Costume);
	}

	private Color[] LoadColors(int randomKey, string path, string[] tables, CharacterCostume.CostumeType type)
	{
		int num = type.ToRequiredColorCount();
		Color[] array = new Color[num];
		int size = KUtility.GetSize(tables);
		for (int i = 0; i < num; i++)
		{
			Color[] array2;
			if (i >= size)
			{
				array2 = type switch
				{
					CharacterCostume.CostumeType.Skin => ColorTableLoader.GetAll("color_skin.raw"), 
					CharacterCostume.CostumeType.Hair => ColorTableLoader.GetAll("color_hair.raw"), 
					_ => ColorTableLoader.GetAllClothColor(path, i), 
				};
			}
			else
			{
				string tableName = tables[i];
				array2 = ColorTableLoader.GetAll(tableName);
			}
			int num2 = _randomHash.Range(0, array2.Length, randomKey);
			ref Color reference = ref array[i];
			reference = array2[num2];
		}
		return array;
	}
}
