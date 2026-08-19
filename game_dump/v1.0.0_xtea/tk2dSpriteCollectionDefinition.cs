using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class tk2dSpriteCollectionDefinition
{
	public enum Anchor
	{
		UpperLeft,
		UpperCenter,
		UpperRight,
		MiddleLeft,
		MiddleCenter,
		MiddleRight,
		LowerLeft,
		LowerCenter,
		LowerRight,
		Custom
	}

	public enum Pad
	{
		Default,
		BlackZeroAlpha,
		Extend,
		TileXY,
		TileX,
		TileY
	}

	public enum ColliderType
	{
		UserDefined,
		ForceNone,
		BoxTrimmed,
		BoxCustom,
		Polygon,
		Advanced
	}

	public enum PolygonColliderCap
	{
		None,
		FrontAndBack,
		Front,
		Back
	}

	public enum ColliderColor
	{
		Default,
		Red,
		White,
		Black
	}

	public enum Source
	{
		Sprite,
		SpriteSheet,
		Font
	}

	public enum DiceFilter
	{
		Complete,
		SolidOnly,
		TransparentOnly
	}

	[Serializable]
	public class ColliderData
	{
		public enum Type
		{
			Box,
			Circle
		}

		public string name = string.Empty;

		public Type type;

		public Vector2 origin = Vector2.op_Implicit(Vector3.zero);

		public Vector2 size = Vector2.op_Implicit(Vector3.zero);

		public float angle;

		public void CopyFrom(ColliderData src)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			name = src.name;
			type = src.type;
			origin = src.origin;
			size = src.size;
			angle = src.angle;
		}

		public bool CompareTo(ColliderData src)
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			return name == src.name && type == src.type && origin == src.origin && size == src.size && angle == src.angle;
		}
	}

	public string name = string.Empty;

	public bool disableTrimming;

	public bool additive;

	public Vector3 scale = new Vector3(1f, 1f, 1f);

	public Texture2D texture;

	[NonSerialized]
	public Texture2D thumbnailTexture;

	public int materialId;

	public Anchor anchor = Anchor.MiddleCenter;

	public float anchorX;

	public float anchorY;

	public Object overrideMesh;

	public bool doubleSidedSprite;

	public bool customSpriteGeometry;

	public tk2dSpriteColliderIsland[] geometryIslands = new tk2dSpriteColliderIsland[0];

	public bool dice;

	public int diceUnitX = 64;

	public int diceUnitY = 64;

	public DiceFilter diceFilter;

	public Pad pad;

	public int extraPadding;

	public Source source;

	public bool fromSpriteSheet;

	public bool hasSpriteSheetId;

	public int spriteSheetId;

	public int spriteSheetX;

	public int spriteSheetY;

	public bool extractRegion;

	public int regionX;

	public int regionY;

	public int regionW;

	public int regionH;

	public int regionId;

	public ColliderType colliderType;

	public List<ColliderData> colliderData = new List<ColliderData>();

	public Vector2 boxColliderMin;

	public Vector2 boxColliderMax;

	public tk2dSpriteColliderIsland[] polyColliderIslands;

	public PolygonColliderCap polyColliderCap = PolygonColliderCap.FrontAndBack;

	public bool colliderConvex;

	public bool colliderSmoothSphereCollisions;

	public ColliderColor colliderColor;

	public List<tk2dSpriteDefinition.AttachPoint> attachPoints = new List<tk2dSpriteDefinition.AttachPoint>();

	public void CopyFrom(tk2dSpriteCollectionDefinition src)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		name = src.name;
		disableTrimming = src.disableTrimming;
		additive = src.additive;
		scale = src.scale;
		texture = src.texture;
		materialId = src.materialId;
		anchor = src.anchor;
		anchorX = src.anchorX;
		anchorY = src.anchorY;
		overrideMesh = src.overrideMesh;
		doubleSidedSprite = src.doubleSidedSprite;
		customSpriteGeometry = src.customSpriteGeometry;
		geometryIslands = src.geometryIslands;
		dice = src.dice;
		diceUnitX = src.diceUnitX;
		diceUnitY = src.diceUnitY;
		diceFilter = src.diceFilter;
		pad = src.pad;
		source = src.source;
		fromSpriteSheet = src.fromSpriteSheet;
		hasSpriteSheetId = src.hasSpriteSheetId;
		spriteSheetX = src.spriteSheetX;
		spriteSheetY = src.spriteSheetY;
		spriteSheetId = src.spriteSheetId;
		extractRegion = src.extractRegion;
		regionX = src.regionX;
		regionY = src.regionY;
		regionW = src.regionW;
		regionH = src.regionH;
		regionId = src.regionId;
		colliderType = src.colliderType;
		boxColliderMin = src.boxColliderMin;
		boxColliderMax = src.boxColliderMax;
		polyColliderCap = src.polyColliderCap;
		colliderColor = src.colliderColor;
		colliderConvex = src.colliderConvex;
		colliderSmoothSphereCollisions = src.colliderSmoothSphereCollisions;
		extraPadding = src.extraPadding;
		this.colliderData = new List<ColliderData>(src.colliderData.Count);
		foreach (ColliderData colliderDatum in src.colliderData)
		{
			ColliderData colliderData = new ColliderData();
			colliderData.CopyFrom(colliderDatum);
			this.colliderData.Add(colliderData);
		}
		if (src.polyColliderIslands != null)
		{
			polyColliderIslands = new tk2dSpriteColliderIsland[src.polyColliderIslands.Length];
			for (int i = 0; i < polyColliderIslands.Length; i++)
			{
				polyColliderIslands[i] = new tk2dSpriteColliderIsland();
				polyColliderIslands[i].CopyFrom(src.polyColliderIslands[i]);
			}
		}
		else
		{
			polyColliderIslands = new tk2dSpriteColliderIsland[0];
		}
		if (src.geometryIslands != null)
		{
			geometryIslands = new tk2dSpriteColliderIsland[src.geometryIslands.Length];
			for (int j = 0; j < geometryIslands.Length; j++)
			{
				geometryIslands[j] = new tk2dSpriteColliderIsland();
				geometryIslands[j].CopyFrom(src.geometryIslands[j]);
			}
		}
		else
		{
			geometryIslands = new tk2dSpriteColliderIsland[0];
		}
		attachPoints = new List<tk2dSpriteDefinition.AttachPoint>(src.attachPoints.Count);
		foreach (tk2dSpriteDefinition.AttachPoint attachPoint2 in src.attachPoints)
		{
			tk2dSpriteDefinition.AttachPoint attachPoint = new tk2dSpriteDefinition.AttachPoint();
			attachPoint.CopyFrom(attachPoint2);
			attachPoints.Add(attachPoint);
		}
	}

	public void Clear()
	{
		tk2dSpriteCollectionDefinition src = new tk2dSpriteCollectionDefinition();
		CopyFrom(src);
	}

	public bool CompareTo(tk2dSpriteCollectionDefinition src)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		if (name != src.name)
		{
			return false;
		}
		if (additive != src.additive)
		{
			return false;
		}
		if (scale != src.scale)
		{
			return false;
		}
		if ((Object)(object)texture != (Object)(object)src.texture)
		{
			return false;
		}
		if (materialId != src.materialId)
		{
			return false;
		}
		if (anchor != src.anchor)
		{
			return false;
		}
		if (anchorX != src.anchorX)
		{
			return false;
		}
		if (anchorY != src.anchorY)
		{
			return false;
		}
		if (overrideMesh != src.overrideMesh)
		{
			return false;
		}
		if (dice != src.dice)
		{
			return false;
		}
		if (diceUnitX != src.diceUnitX)
		{
			return false;
		}
		if (diceUnitY != src.diceUnitY)
		{
			return false;
		}
		if (diceFilter != src.diceFilter)
		{
			return false;
		}
		if (pad != src.pad)
		{
			return false;
		}
		if (extraPadding != src.extraPadding)
		{
			return false;
		}
		if (doubleSidedSprite != src.doubleSidedSprite)
		{
			return false;
		}
		if (customSpriteGeometry != src.customSpriteGeometry)
		{
			return false;
		}
		if (geometryIslands != src.geometryIslands)
		{
			return false;
		}
		if (geometryIslands != null && src.geometryIslands != null)
		{
			if (geometryIslands.Length != src.geometryIslands.Length)
			{
				return false;
			}
			for (int i = 0; i < geometryIslands.Length; i++)
			{
				if (!geometryIslands[i].CompareTo(src.geometryIslands[i]))
				{
					return false;
				}
			}
		}
		if (source != src.source)
		{
			return false;
		}
		if (fromSpriteSheet != src.fromSpriteSheet)
		{
			return false;
		}
		if (hasSpriteSheetId != src.hasSpriteSheetId)
		{
			return false;
		}
		if (spriteSheetId != src.spriteSheetId)
		{
			return false;
		}
		if (spriteSheetX != src.spriteSheetX)
		{
			return false;
		}
		if (spriteSheetY != src.spriteSheetY)
		{
			return false;
		}
		if (extractRegion != src.extractRegion)
		{
			return false;
		}
		if (regionX != src.regionX)
		{
			return false;
		}
		if (regionY != src.regionY)
		{
			return false;
		}
		if (regionW != src.regionW)
		{
			return false;
		}
		if (regionH != src.regionH)
		{
			return false;
		}
		if (regionId != src.regionId)
		{
			return false;
		}
		if (colliderType != src.colliderType)
		{
			return false;
		}
		if (boxColliderMin != src.boxColliderMin)
		{
			return false;
		}
		if (boxColliderMax != src.boxColliderMax)
		{
			return false;
		}
		if (polyColliderIslands != src.polyColliderIslands)
		{
			return false;
		}
		if (polyColliderIslands != null && src.polyColliderIslands != null)
		{
			if (polyColliderIslands.Length != src.polyColliderIslands.Length)
			{
				return false;
			}
			for (int j = 0; j < polyColliderIslands.Length; j++)
			{
				if (!polyColliderIslands[j].CompareTo(src.polyColliderIslands[j]))
				{
					return false;
				}
			}
		}
		if (colliderData.Count != src.colliderData.Count)
		{
			return false;
		}
		for (int k = 0; k < colliderData.Count; k++)
		{
			if (!colliderData[k].CompareTo(src.colliderData[k]))
			{
				return false;
			}
		}
		if (polyColliderCap != src.polyColliderCap)
		{
			return false;
		}
		if (colliderColor != src.colliderColor)
		{
			return false;
		}
		if (colliderSmoothSphereCollisions != src.colliderSmoothSphereCollisions)
		{
			return false;
		}
		if (colliderConvex != src.colliderConvex)
		{
			return false;
		}
		if (attachPoints.Count != src.attachPoints.Count)
		{
			return false;
		}
		for (int l = 0; l < attachPoints.Count; l++)
		{
			if (!attachPoints[l].CompareTo(src.attachPoints[l]))
			{
				return false;
			}
		}
		return true;
	}
}
