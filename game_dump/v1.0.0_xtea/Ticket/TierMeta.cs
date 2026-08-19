using System;
using UnityEngine;

namespace Ticket;

[Serializable]
public struct TierMeta
{
	[LocalizableString]
	public string Name;

	public SpriteData Icon;

	public Color Color;

	public int MinRank;

	public int MaxRank;

	public bool IsValidTier(int rank)
	{
		return MinRank <= rank && rank <= MaxRank;
	}
}
