using System;
using System.Text;
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
		StringBuilder stringBuilder = new StringBuilder(path);
		stringBuilder.Replace('/', '$');
		stringBuilder.Replace("\\", "$");
		stringBuilder.Append(".bundle");
		return stringBuilder.ToString().ToLower();
	}

	public static string GetBundleFileName(string name)
	{
		return name + ".bundle";
	}

	public static string GetHashedName([NotNull] string bundleFileName, [NotNull] string hash)
	{
		int num = bundleFileName.Length - ".bundle".Length;
		if (num < 0)
		{
			return bundleFileName + "." + hash;
		}
		return bundleFileName.Insert(num, "." + hash);
	}
}
