using System;

[Serializable]
public struct SoundEventType
{
	public string Path;

	public static implicit operator string(SoundEventType value)
	{
		return value.Path;
	}

	public override string ToString()
	{
		return Path;
	}
}
