using System;
using System.Collections.Generic;
using UnityEngine;

[ResourcePath("equipment_table")]
public class EquipmentTable : ResourceSingleton<EquipmentTable>
{
	public enum Category
	{
		Body,
		Hair,
		Head,
		Beard
	}

	[Serializable]
	public struct Equipments
	{
		[SerializeField]
		public List<string> List;
	}

	[SerializeField]
	private Equipments[] _males;

	[SerializeField]
	private Equipments[] _females;

	public string GetNext(Category category, bool isMale, string path)
	{
		List<string> targetList = GetTargetList(category, isMale);
		if (targetList == null)
		{
			return string.Empty;
		}
		int index = (targetList.IndexOfIgnoreCase(path) + 1) % targetList.Count;
		return targetList[index];
	}

	public string GetContains(Category category, bool isMale, string key)
	{
		List<string> targetList = GetTargetList(category, isMale);
		if (targetList == null)
		{
			return string.Empty;
		}
		for (int i = 0; i < targetList.Count; i++)
		{
			if (targetList[i].ContainsIgnoreCase(key))
			{
				return targetList[i];
			}
		}
		return string.Empty;
	}

	public string GetRandom(Category category, bool isMale)
	{
		List<string> targetList = GetTargetList(category, isMale);
		if (targetList == null)
		{
			return string.Empty;
		}
		int index = Random.Range(0, targetList.Count);
		return targetList[index];
	}

	private List<string> GetTargetList(Category category, bool isMale)
	{
		Equipments[] array = ((!isMale) ? _females : _males);
		if (array == null || array.Length == 0 || array.Length <= (int)category)
		{
			return null;
		}
		List<string> list = array[(int)category].List;
		if (list == null || list.Count == 0)
		{
			return null;
		}
		return list;
	}
}
