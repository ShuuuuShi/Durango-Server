using System;

[Serializable]
public struct IntegratedEffectType
{
	public string Path;

	public static implicit operator string(IntegratedEffectType value)
	{
		return value.Path;
	}
}
