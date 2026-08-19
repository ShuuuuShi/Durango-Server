using System;
using UnityEngine;

[Serializable]
public class tk2dBatchedSprite
{
	public enum Type
	{
		EmptyGameObject,
		Sprite,
		TiledSprite,
		SlicedSprite,
		ClippedSprite,
		TextMesh
	}

	[Flags]
	public enum Flags
	{
		None = 0,
		Sprite_CreateBoxCollider = 1,
		SlicedSprite_BorderOnly = 2
	}

	public Type type = Type.Sprite;

	public string name = string.Empty;

	public int parentId = -1;

	public int spriteId;

	public int xRefId = -1;

	public tk2dSpriteCollectionData spriteCollection;

	public Quaternion rotation = Quaternion.identity;

	public Vector3 position = Vector3.zero;

	public Vector3 localScale = Vector3.one;

	public Color color = Color.white;

	public Vector3 baseScale = Vector3.one;

	public int renderLayer;

	[SerializeField]
	private Vector2 internalData0;

	[SerializeField]
	private Vector2 internalData1;

	[SerializeField]
	private Vector2 internalData2;

	[SerializeField]
	private Vector2 colliderData = new Vector2(0f, 1f);

	[SerializeField]
	private string formattedText = string.Empty;

	[SerializeField]
	private Flags flags;

	public tk2dBaseSprite.Anchor anchor;

	public Matrix4x4 relativeMatrix = Matrix4x4.identity;

	private Vector3 cachedBoundsCenter = Vector3.zero;

	private Vector3 cachedBoundsExtents = Vector3.zero;

	public float BoxColliderOffsetZ
	{
		get
		{
			return colliderData.x;
		}
		set
		{
			colliderData.x = value;
		}
	}

	public float BoxColliderExtentZ
	{
		get
		{
			return colliderData.y;
		}
		set
		{
			colliderData.y = value;
		}
	}

	public string FormattedText
	{
		get
		{
			return formattedText;
		}
		set
		{
			formattedText = value;
		}
	}

	public Vector2 ClippedSpriteRegionBottomLeft
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return internalData0;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			internalData0 = value;
		}
	}

	public Vector2 ClippedSpriteRegionTopRight
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return internalData1;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			internalData1 = value;
		}
	}

	public Vector2 SlicedSpriteBorderBottomLeft
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return internalData0;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			internalData0 = value;
		}
	}

	public Vector2 SlicedSpriteBorderTopRight
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return internalData1;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			internalData1 = value;
		}
	}

	public Vector2 Dimensions
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return internalData2;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			internalData2 = value;
		}
	}

	public bool IsDrawn => type != Type.EmptyGameObject;

	public Vector3 CachedBoundsCenter
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return cachedBoundsCenter;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			cachedBoundsCenter = value;
		}
	}

	public Vector3 CachedBoundsExtents
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return cachedBoundsExtents;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			cachedBoundsExtents = value;
		}
	}

	public tk2dBatchedSprite()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		parentId = -1;
	}

	public bool CheckFlag(Flags mask)
	{
		return (flags & mask) != 0;
	}

	public void SetFlag(Flags mask, bool value)
	{
		if (value)
		{
			flags |= mask;
		}
		else
		{
			flags &= ~mask;
		}
	}

	public tk2dSpriteDefinition GetSpriteDefinition()
	{
		if ((Object)(object)spriteCollection != (Object)null && spriteId != -1)
		{
			return spriteCollection.inst.spriteDefinitions[spriteId];
		}
		return null;
	}
}
