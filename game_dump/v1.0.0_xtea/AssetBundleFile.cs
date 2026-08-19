using UnityEngine;

public class AssetBundleFile
{
	public enum Status
	{
		None,
		Loading,
		Failed
	}

	public string Name;

	public Hash128 Hash;

	public AssetBundleFile[] Dependencies;

	public int Priority;

	public bool Queued;

	public AssetBundle Bundle;

	public Status CurrentStatus;
}
