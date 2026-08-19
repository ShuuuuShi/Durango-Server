using UnityEngine;

[AddComponentMenu("2D Toolkit/Backend/tk2dSpriteAnimation")]
public class tk2dSpriteAnimation : MonoBehaviour
{
	public tk2dSpriteAnimationClip[] clips;

	public tk2dSpriteAnimationClip FirstValidClip
	{
		get
		{
			for (int i = 0; i < clips.Length; i++)
			{
				if (!clips[i].Empty && (Object)(object)clips[i].frames[0].spriteCollection != (Object)null && clips[i].frames[0].spriteId != -1)
				{
					return clips[i];
				}
			}
			return null;
		}
	}

	public tk2dSpriteAnimationClip GetClipByName(string name)
	{
		for (int i = 0; i < clips.Length; i++)
		{
			if (clips[i].name == name)
			{
				return clips[i];
			}
		}
		return null;
	}

	public tk2dSpriteAnimationClip GetClipById(int id)
	{
		if (id < 0 || id >= clips.Length || clips[id].Empty)
		{
			return null;
		}
		return clips[id];
	}

	public int GetClipIdByName(string name)
	{
		for (int i = 0; i < clips.Length; i++)
		{
			if (clips[i].name == name)
			{
				return i;
			}
		}
		return -1;
	}

	public int GetClipIdByName(tk2dSpriteAnimationClip clip)
	{
		for (int i = 0; i < clips.Length; i++)
		{
			if (clips[i] == clip)
			{
				return i;
			}
		}
		return -1;
	}
}
