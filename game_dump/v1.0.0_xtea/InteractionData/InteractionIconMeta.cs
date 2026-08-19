using System;
using UnityEngine;

namespace InteractionData;

[Serializable]
public struct InteractionIconMeta
{
	public SpriteData Icon;

	public Color Color;

	public int Depth;

	public float Scale;
}
