using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Durango.Model;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.Utils;

public static class ColorTableLoader
{
	public const string ClothFile = "color_create.raw";

	public const string SkinFile = "color_skin.raw";

	public const string HairFile = "color_hair.raw";

	public const string EyeFile = "color_eyes.raw";

	public const string LipMaleFile = "color_lips_male.raw";

	public const string LipFemaleFile = "color_lips_female.raw";

	public const string LipMaleRandomFile = "color_lips_male_random.raw";

	public const string LipFemaleRandomFile = "color_lips_female_random.raw";

	public const string ColorTableClothesMapFileName = "ColorTable/color_table_clothes_map";

	public const string PortraitBg = "color_portrait_bg.raw";

	private static readonly Dictionary<string, ColorTable> ColorTableDict = new Dictionary<string, ColorTable>();

	private static List<ClothesColorTableInfo> _colorTableClothesMap;

	public static List<ClothesColorTableInfo> ColorTableClothesMap
	{
		get
		{
			if (_colorTableClothesMap == null)
			{
				_colorTableClothesMap = Json.ReadFromFile<List<ClothesColorTableInfo>>("ColorTable/color_table_clothes_map");
			}
			return _colorTableClothesMap;
		}
	}

	public static ColorTable Load([NotNull] string tableName)
	{
		ColorTable colorTable = ColorTableDict.Get(tableName);
		if (colorTable == null)
		{
			colorTable = new ColorTable(tableName);
			ColorTableDict.Add(tableName, colorTable);
		}
		return colorTable;
	}

	public static Color GetRandom([NotNull] string tableName)
	{
		return Load(tableName).GetRandom();
	}

	public static Color GetRandom([NotNull] string tableName, int hashKey)
	{
		return Load(tableName).GetRandom(hashKey);
	}

	public static Color[] GetAll([NotNull] string tableName)
	{
		return Load(tableName).GetAll();
	}

	public static string GetRepresentModelName(string modelPath)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(modelPath);
		if (fileNameWithoutExtension == null)
		{
			return null;
		}
		string text = Regex.Replace(fileNameWithoutExtension, "^hq_?|[mf]_", string.Empty);
		if (fileNameWithoutExtension.ContainsIgnoreCase("_head_") && text[text.Length - 3] == '_' && char.IsDigit(text[text.Length - 2]) && char.IsDigit(text[text.Length - 1]))
		{
			text = text.Substring(0, text.Length - 3);
		}
		return text;
	}

	private static ColorTable GetColorTablesByPath(string path, int index)
	{
		path = GetRepresentModelName(path).ToLower();
		int count = ColorTableClothesMap.Count;
		for (int i = 0; i < count; i++)
		{
			ClothesColorTableInfo clothesColorTableInfo = ColorTableClothesMap[i];
			if (string.Equals(path, clothesColorTableInfo.Keyword, StringComparison.OrdinalIgnoreCase))
			{
				return (KUtility.GetSize(clothesColorTableInfo.ColorTableNames) <= index) ? null : Load(clothesColorTableInfo.ColorTableNames[index]);
			}
		}
		return null;
	}

	public static Color GetRandomClothColor(string path, int index)
	{
		return GetColorTablesByPath(path, index)?.GetRandom() ?? Color.magenta;
	}

	public static Color[] GetAllClothColor(string path, int index)
	{
		ColorTable colorTablesByPath = GetColorTablesByPath(path, index);
		if (colorTablesByPath == null)
		{
			return new Color[1] { Color.magenta };
		}
		return colorTablesByPath.GetAll();
	}

	public static ItemColor GetRandomCostumePartColor(CharacterCostume.CostumeType type, string costumePathName, bool isMale, ItemColor[] costumeColors)
	{
		int num = type.ToRequiredColorCount();
		ItemColor result = new ItemColor(num);
		for (int i = 0; i < num; i++)
		{
			switch (type)
			{
			case CharacterCostume.CostumeType.Skin:
				result[i] = GetRandom("color_skin.raw");
				continue;
			case CharacterCostume.CostumeType.Hair:
				result[i] = GetRandom("color_hair.raw");
				continue;
			case CharacterCostume.CostumeType.Eye:
				result[i] = GetRandom("color_eyes.raw");
				continue;
			case CharacterCostume.CostumeType.Lip:
				result[i] = GetRandom((!isMale) ? "color_lips_female_random.raw" : "color_lips_male_random.raw");
				continue;
			}
			if (string.IsNullOrEmpty(costumePathName))
			{
				if (costumeColors[(int)type].HasValue)
				{
					result[i] = costumeColors[(int)type][i];
				}
			}
			else
			{
				result[i] = GetRandomClothColor(costumePathName, i);
			}
		}
		return result;
	}
}
