using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class AssetBundleFileInfo
{
	public string Name;

	public string Hash;

	public string[] Dependencies;

	public int Priority;

	[JsonIgnore]
	public List<string> Items;
}
