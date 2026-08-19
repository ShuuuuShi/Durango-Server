using System;
using System.Text;
using Durango.Utils;
using JetBrains.Annotations;

[Serializable]
public class AssetBundleItemInfo
{
	public const string Extension = ".bundle";

	public const string PreloadName = "preload.bundle";

	public string Name;

	public bool SavePerDirectory;

	public static string GetParentName(string uniqueName)
	{
		int length = uniqueName.LastIndexOf('$');
		return uniqueName.Substring(0, length) + ".bundle";
	}

	public static string GetAssetName(string uniqueName)
	{
		int num = uniqueName.LastIndexOf('$') + 1;
		int num2 = uniqueName.LastIndexOf('.', uniqueName.Length - 8);
		return uniqueName.Substring(num, num2 - num);
	}

	public static string GetUniqueName(string path)
	{
		using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
		StringBuilder value = reusable.Value;
		value.Append(path);
		value.Replace('/', '$');
		value.Replace("\\", "$");
		value.Append(".bundle");
		int i = 0;
		for (int length = value.Length; i < length; i++)
		{
			value[i] = char.ToLower(value[i]);
		}
		return value.ToString();
	}

	public static string GetCrcName([NotNull] string bundleFileName, [NotNull] string crc, [CanBeNull] string prefix = null)
	{
		using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
		StringBuilder value = reusable.Value;
		value.Append(prefix);
		value.Append(bundleFileName);
		int num = value.Length - ".bundle".Length;
		if (num < 0)
		{
			value.Append(".");
			value.Append(crc);
			return value.ToString();
		}
		value.Insert(num, ".");
		value.Insert(num + 1, crc);
		return value.ToString();
	}
}
