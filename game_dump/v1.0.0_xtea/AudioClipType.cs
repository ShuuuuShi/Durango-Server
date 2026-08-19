using System;

[Serializable]
public struct AudioClipType
{
	public string Path;

	public static implicit operator string(AudioClipType value)
	{
		return value.Path;
	}
}
