using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class KSpriteGroup
{
	[SerializeField]
	public List<SpriteCollectionInfo> SpriteCollectionInfoList = new List<SpriteCollectionInfo>(1);

	[SerializeField]
	public bool HasShadow;

	[SerializeField]
	public bool UseAlphaBlending = true;

	[SerializeField]
	public bool HasAdditive;

	public SpriteCollectionInfo GetSpriteCollectionInfoBySpriteName(string spriteName, out int tkSpriteId)
	{
		tkSpriteId = -1;
		for (int i = 0; i < SpriteCollectionInfoList.Count; i++)
		{
			SpriteCollectionInfo spriteCollectionInfo = SpriteCollectionInfoList[i];
			tk2dSpriteCollectionData spriteCollectionData = spriteCollectionInfo.SpriteCollectionData;
			if (!((Object)(object)spriteCollectionData == (Object)null))
			{
				tkSpriteId = spriteCollectionData.GetSpriteIdByName(spriteName, -1);
				if (tkSpriteId >= 0 && spriteCollectionData.IsValidSpriteId(tkSpriteId))
				{
					return SpriteCollectionInfoList[i];
				}
			}
		}
		return null;
	}
}
