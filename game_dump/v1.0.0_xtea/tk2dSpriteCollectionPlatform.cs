using System;
using UnityEngine;

[Serializable]
public class tk2dSpriteCollectionPlatform
{
	public string name = string.Empty;

	public tk2dSpriteCollection spriteCollection;

	public bool Valid => name.Length > 0 && (Object)(object)spriteCollection != (Object)null;

	public void CopyFrom(tk2dSpriteCollectionPlatform source)
	{
		name = source.name;
		spriteCollection = source.spriteCollection;
	}
}
