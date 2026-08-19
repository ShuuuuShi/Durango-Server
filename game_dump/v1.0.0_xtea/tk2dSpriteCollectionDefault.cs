using System;
using UnityEngine;

[Serializable]
public class tk2dSpriteCollectionDefault
{
	public bool additive;

	public Vector3 scale = new Vector3(1f, 1f, 1f);

	public tk2dSpriteCollectionDefinition.Anchor anchor = tk2dSpriteCollectionDefinition.Anchor.MiddleCenter;

	public tk2dSpriteCollectionDefinition.Pad pad;

	public tk2dSpriteCollectionDefinition.ColliderType colliderType;
}
