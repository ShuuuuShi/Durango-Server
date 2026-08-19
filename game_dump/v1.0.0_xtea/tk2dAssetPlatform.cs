using System;

[Serializable]
public class tk2dAssetPlatform
{
	public string name = string.Empty;

	public float scale = 1f;

	public tk2dAssetPlatform(string name, float scale)
	{
		this.name = name;
		this.scale = scale;
	}
}
