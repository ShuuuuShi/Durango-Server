using UnityEngine;

public class AssetBundleFile
{
	public enum Status
	{
		None,
		Loading,
		Failed
	}

	public readonly string Name;

	public readonly Hash128 Hash;

	public readonly string Crc;

	public readonly int Size;

	public readonly int Priority;

	public AssetBundleFile[] Dependencies;

	public bool Queued;

	public AssetBundle Bundle;

	public Status CurrentStatus;

	public AssetBundleFile(AssetBundleFileInfo fileInfo)
	{
		Hash = Hash128.Parse(fileInfo.Hash);
		Crc = fileInfo.Crc;
		Priority = fileInfo.Priority;
		Name = fileInfo.Name;
		Size = fileInfo.Size;
		CurrentStatus = Status.None;
	}
}
