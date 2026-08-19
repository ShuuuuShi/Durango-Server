using System.Collections.Generic;

public class AssetBundleInfoHolder
{
	public string PreloadHash;

	public string PreloadCrc;

	public List<AssetBundleItemInfo> ItemList = new List<AssetBundleItemInfo>();

	public List<AssetBundleFileInfo> FileList = new List<AssetBundleFileInfo>();
}
