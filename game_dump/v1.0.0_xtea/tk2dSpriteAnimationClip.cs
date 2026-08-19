using System;

[Serializable]
public class tk2dSpriteAnimationClip
{
	public enum WrapMode
	{
		Loop,
		LoopSection,
		Once,
		PingPong,
		RandomFrame,
		RandomLoop,
		Single
	}

	public string name = "Default";

	public tk2dSpriteAnimationFrame[] frames = new tk2dSpriteAnimationFrame[0];

	public float fps = 30f;

	public int loopStart;

	public WrapMode wrapMode;

	public bool Empty => name.Length == 0 || frames == null || frames.Length == 0;

	public tk2dSpriteAnimationClip()
	{
	}

	public tk2dSpriteAnimationClip(tk2dSpriteAnimationClip source)
	{
		CopyFrom(source);
	}

	public void CopyFrom(tk2dSpriteAnimationClip source)
	{
		name = source.name;
		if (source.frames == null)
		{
			frames = null;
		}
		else
		{
			frames = new tk2dSpriteAnimationFrame[source.frames.Length];
			for (int i = 0; i < frames.Length; i++)
			{
				if (source.frames[i] == null)
				{
					frames[i] = null;
					continue;
				}
				frames[i] = new tk2dSpriteAnimationFrame();
				frames[i].CopyFrom(source.frames[i]);
			}
		}
		fps = source.fps;
		loopStart = source.loopStart;
		wrapMode = source.wrapMode;
		if (wrapMode == WrapMode.Single && frames.Length > 1)
		{
			frames = new tk2dSpriteAnimationFrame[1] { frames[0] };
			Debug.LogError((object)$"Clip: '{name}' Fixed up frames for WrapMode.Single");
		}
	}

	public void Clear()
	{
		name = string.Empty;
		frames = new tk2dSpriteAnimationFrame[0];
		fps = 30f;
		loopStart = 0;
		wrapMode = WrapMode.Loop;
	}

	public tk2dSpriteAnimationFrame GetFrame(int frame)
	{
		return frames[frame];
	}
}
