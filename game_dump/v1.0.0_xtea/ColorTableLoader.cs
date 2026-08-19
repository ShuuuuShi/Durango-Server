using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public static class ColorTableLoader
{
	private static Dictionary<string, ColorTable> _clothesColorTableCache = new Dictionary<string, ColorTable>();

	public static string ColorTableClothesMapFileName = "ColorTable/color_table_clothes_map";

	private static List<ClothesColorTableInfo> _colorTableClothesMap;

	private static List<ClothesColorTableInfo> ColorTableClothesMap
	{
		get
		{
			if (_colorTableClothesMap == null)
			{
				_colorTableClothesMap = KUtility.ParseJsonFile<List<ClothesColorTableInfo>>(ColorTableClothesMapFileName);
			}
			return _colorTableClothesMap;
		}
	}

	private static ColorTable GetColorTable(string colorTableName)
	{
		if (!_clothesColorTableCache.ContainsKey(colorTableName))
		{
			_clothesColorTableCache.Add(colorTableName, new ColorTable(colorTableName));
		}
		return _clothesColorTableCache[colorTableName];
	}

	public static string GetRepresentModelName(string modelPath)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(modelPath);
		if (fileNameWithoutExtension == null)
		{
			return null;
		}
		string text = Regex.Replace(fileNameWithoutExtension, "^[mf]_", string.Empty);
		if (fileNameWithoutExtension.ContainsIgnoreCase("_head_") && text[text.Length - 3] == '_' && char.IsDigit(text[text.Length - 2]) && char.IsDigit(text[text.Length - 1]))
		{
			text = text.Substring(0, text.Length - 3);
		}
		return text;
	}

	private static ColorTable[] GetColorTablesClothes(string clothKeyword)
	{
		clothKeyword = GetRepresentModelName(clothKeyword).ToLower();
		int count = ColorTableClothesMap.Count;
		for (int i = 0; i < count; i++)
		{
			if (clothKeyword == ColorTableClothesMap[i].Keyword.ToLower())
			{
				ColorTable[] array = new ColorTable[3];
				for (int j = 0; j < 3; j++)
				{
					array[j] = GetColorTable(ColorTableClothesMap[i].ColorTableNames[j]);
				}
				return array;
			}
		}
		return null;
	}

	public static Color GetRandomClothColor(string clothPathName, int index)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		ColorTable[] colorTablesClothes = GetColorTablesClothes(clothPathName);
		if (colorTablesClothes == null)
		{
			Debug.LogError((object)("Color table is missing : " + clothPathName));
			return Color.magenta;
		}
		if (index >= colorTablesClothes.Length)
		{
			Debug.LogError((object)("Invalid color table index  : " + index));
			return Color.magenta;
		}
		return colorTablesClothes[index].GetRandomColor();
	}

	public static Color GetRandomClothColor()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return GetColorTable("color_create.raw").GetRandomColor();
	}

	public static Color GetRandomSkinColor()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return GetColorTable("color_skin.raw").GetRandomColor();
	}

	public static Color GetRandomHairColor()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return GetColorTable("color_hair.raw").GetRandomColor();
	}

	public static Color GetRandomEyeColor()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return GetColorTable("color_eyes.raw").GetRandomColor();
	}

	public static Color GetRandomLipColor(bool isMale)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		ColorTable colorTable = GetColorTable((!isMale) ? "color_lips_female_random.raw" : "color_lips_male_random.raw");
		return colorTable.GetRandomColor();
	}
}
