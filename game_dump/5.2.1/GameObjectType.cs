using System;

[Serializable]
public struct GameObjectType
{
	public string Path;

	public static implicit operator string(GameObjectType value)
	{
		return value.Path;
	}

	public override string ToString()
	{
		return Path;
	}
}
