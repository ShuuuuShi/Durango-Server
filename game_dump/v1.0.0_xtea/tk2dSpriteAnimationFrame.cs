using System;

[Serializable]
public class tk2dSpriteAnimationFrame
{
	public tk2dSpriteCollectionData spriteCollection;

	public int spriteId;

	public bool triggerEvent;

	public string eventInfo = string.Empty;

	public int eventInt;

	public float eventFloat;

	public void CopyFrom(tk2dSpriteAnimationFrame source)
	{
		CopyFrom(source, full: true);
	}

	public void CopyTriggerFrom(tk2dSpriteAnimationFrame source)
	{
		triggerEvent = source.triggerEvent;
		eventInfo = source.eventInfo;
		eventInt = source.eventInt;
		eventFloat = source.eventFloat;
	}

	public void ClearTrigger()
	{
		triggerEvent = false;
		eventInt = 0;
		eventFloat = 0f;
		eventInfo = string.Empty;
	}

	public void CopyFrom(tk2dSpriteAnimationFrame source, bool full)
	{
		spriteCollection = source.spriteCollection;
		spriteId = source.spriteId;
		if (full)
		{
			CopyTriggerFrom(source);
		}
	}
}
