using System;

[Serializable]
public struct ParticleType
{
	public string Path;

	public static implicit operator string(ParticleType value)
	{
		return value.Path;
	}

	public override string ToString()
	{
		return Path;
	}
}
