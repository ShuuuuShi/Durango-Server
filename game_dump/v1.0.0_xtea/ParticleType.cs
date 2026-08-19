using System;

[Serializable]
public struct ParticleType
{
	public string Path;

	public static implicit operator string(ParticleType value)
	{
		return value.Path;
	}
}
