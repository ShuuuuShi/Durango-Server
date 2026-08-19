using System;
using System.Collections.Generic;
using System.Reflection;
using Durango.Utils.Extensions;
using UnityEngine;

public static class PresetColor
{
	public static readonly Color UIYellow = new Color32(byte.MaxValue, 216, 91, byte.MaxValue);

	public static readonly Color UIDarkOrange = new Color32(226, 109, 51, byte.MaxValue);

	public static readonly Color UISunglowYellow = new Color32(251, 193, 52, byte.MaxValue);

	public static readonly Color UIGreen = new Color32(17, 114, 62, byte.MaxValue);

	public static readonly Color UILightGreen = new Color32(50, 180, 70, byte.MaxValue);

	public static readonly Color UITransparentForestGreen = new Color32(43, 178, 61, 195);

	public static readonly Color UIRed = new Color32(158, 11, 15, byte.MaxValue);

	public static readonly Color UILightRed = new Color32(228, 34, 34, byte.MaxValue);

	public static readonly Color UIDarkRed = new Color32(134, 45, 45, byte.MaxValue);

	public static readonly Color UIPaleRed = new Color32(byte.MaxValue, 81, 81, byte.MaxValue);

	public static readonly Color UIBlue = new Color32(51, 51, byte.MaxValue, byte.MaxValue);

	public static readonly Color UISkyBlue = new Color32(43, 129, 201, byte.MaxValue);

	public static readonly Color UIGrass = new Color32(149, 190, 60, byte.MaxValue);

	public static readonly Color UIGray = new Color32(57, 57, 53, byte.MaxValue);

	public static readonly Color UIDarkGray = new Color32(75, 75, 70, byte.MaxValue);

	public static readonly Color UILightGray = new Color32(113, 113, 107, byte.MaxValue);

	public static readonly Color UIMoreLightGray = new Color32(132, 132, 125, byte.MaxValue);

	public static readonly Color UIWhite = new Color32(232, 229, 223, byte.MaxValue);

	public static readonly Color UISilverGray = new Color32(189, 189, 189, byte.MaxValue);

	public static readonly Color UILightSilverGray = new Color32(205, 205, 205, byte.MaxValue);

	public static readonly Color UIWhiteAlpha20 = new Color32(232, 229, 223, 51);

	public static readonly Color UIBlack = new Color32(0, 0, 0, byte.MaxValue);

	public static readonly Color UIBlackAlpha40 = new Color32(0, 0, 0, 100);

	public static readonly Color UIButtonNormal = new Color32(143, 143, 133, byte.MaxValue);

	public static readonly Color UIPurple = new Color32(142, 28, 69, byte.MaxValue);

	public static readonly Color UIBrown = new Color32(162, 145, 102, byte.MaxValue);

	public static readonly Color UILightBrown = new Color32(216, 212, 202, byte.MaxValue);

	public static readonly Color UIDarkBrown = new Color32(132, 124, 102, byte.MaxValue);

	public static readonly Color UIDarkBrownGray = new Color32(75, 66, 43, byte.MaxValue);

	public static readonly Color UIDeepDarkBrown = new Color32(30, 28, 22, byte.MaxValue);

	public static readonly Color UIGrayBrown = new Color32(154, 150, 142, byte.MaxValue);

	public static readonly Color UIRedBrown = new Color32(201, 173, 105, byte.MaxValue);

	public static readonly Color UIMoreLightBrown = new Color32(124, 113, 88, byte.MaxValue);

	public static readonly Color UINomad = new Color32(182, 177, 161, byte.MaxValue);

	public static readonly Color UIZeus = new Color32(31, 28, 21, byte.MaxValue);

	public static readonly Color UILightZeus = new Color32(39, 35, 25, byte.MaxValue);

	public static readonly Color UILaser = new Color32(201, 173, 105, byte.MaxValue);

	public static readonly Color UIFriendlyPink = new Color32(byte.MaxValue, 122, 207, byte.MaxValue);

	public static readonly Color UIBuff = new Color32(61, 163, 192, byte.MaxValue);

	public static readonly Color UIDebuff = new Color32(211, 54, 41, byte.MaxValue);

	public static readonly Color UIBeige = new Color32(byte.MaxValue, 238, 182, byte.MaxValue);

	public static readonly Color LoadingColor = new Color32(76, 68, 59, byte.MaxValue);

	public static readonly Color TryConnectColor = new Color32(181, 33, 39, byte.MaxValue);

	public static readonly Color ConnectingColor = new Color32(217, 121, 50, byte.MaxValue);

	public static readonly Color ConnectedColor = new Color32(47, 174, 39, byte.MaxValue);

	public static readonly Color ClanFlag = new Color32(241, 209, 90, byte.MaxValue);

	public static readonly Color PlayerClanFlag = new Color32(70, 220, 30, byte.MaxValue);

	public static readonly Color ClanTerritory = new Color32(byte.MaxValue, 167, 0, byte.MaxValue);

	public static readonly Color PlayerClanTerritory = new Color32(77, 233, 0, byte.MaxValue);

	public static readonly Color EstateArea = new Color32(byte.MaxValue, 167, 0, byte.MaxValue);

	public static readonly Color PlayerEstateArea = new Color32(0, 176, byte.MaxValue, byte.MaxValue);

	public static readonly Color EnemyEstateArea = new Color32(byte.MaxValue, 0, 0, byte.MaxValue);

	public static readonly Color QuestGray = new Color32(91, 91, 91, byte.MaxValue);

	public static readonly Color PlayerClan = new Color32(102, 232, 56, byte.MaxValue);

	public static readonly Color PlayerAlliance = new Color32(15, 217, 186, byte.MaxValue);

	public static readonly Color PlayerHostile = new Color32(byte.MaxValue, 34, 34, byte.MaxValue);

	public static readonly Color PlayerParty = new Color32(92, 219, byte.MaxValue, byte.MaxValue);

	public static readonly Color BrightTurquoise = new Color32(6, 221, 250, byte.MaxValue);

	public static readonly Color Pumpkin = new Color32(byte.MaxValue, 130, 34, byte.MaxValue);

	public static readonly Color Harlequin = new Color32(52, 250, 6, byte.MaxValue);

	public static readonly Color RazzleDazzleRose = new Color32(248, 41, 216, byte.MaxValue);

	public static readonly Color BrightSun = new Color32(byte.MaxValue, 233, 46, byte.MaxValue);

	public static readonly Color SpringGreen = new Color32(0, byte.MaxValue, 174, byte.MaxValue);

	public static readonly Color Aqua = new Color32(0, byte.MaxValue, 246, byte.MaxValue);

	public static readonly Color WildStrawberry = new Color32(byte.MaxValue, 61, 148, byte.MaxValue);

	public static readonly Color Lima = new Color32(70, 231, 30, byte.MaxValue);

	public static readonly Color Starship = new Color32(byte.MaxValue, 228, 0, byte.MaxValue);

	public static readonly Color Shakespeare = new Color32(32, 192, byte.MaxValue, byte.MaxValue);

	public static readonly Color Cerise = new Color32(byte.MaxValue, 42, 172, byte.MaxValue);

	public static readonly Color UIPet = new Color32(122, 192, 34, byte.MaxValue);

	public static readonly Color ProgressBarBlue = new Color32(59, 96, 123, byte.MaxValue);

	public static readonly Color ExploreRed = new Color32(184, 46, 46, byte.MaxValue);

	public static readonly Color ScannerGreen = new Color32(1, 248, 63, byte.MaxValue);

	private static Dictionary<string, Color> _colorDictionary;

	private static Dictionary<string, Color> GetColorDictionary()
	{
		if (_colorDictionary == null)
		{
			_colorDictionary = new Dictionary<string, Color>();
			Type typeFromHandle = typeof(PresetColor);
			FieldInfo[] fields = typeFromHandle.GetFields(BindingFlags.Static | BindingFlags.Public);
			FieldInfo[] array = fields;
			foreach (FieldInfo fieldInfo in array)
			{
				if (fieldInfo.FieldType == typeof(Color))
				{
					Color value = (Color)fieldInfo.GetValue(null);
					string name = fieldInfo.Name;
					string key = fieldInfo.Name.ToSnakeCase();
					_colorDictionary[name] = value;
					_colorDictionary[key] = value;
				}
			}
		}
		return _colorDictionary;
	}

	public static bool TryGet(string key, out Color color)
	{
		Dictionary<string, Color> colorDictionary = GetColorDictionary();
		return colorDictionary.TryGetValue(key, out color);
	}
}
