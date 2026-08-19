using System;
using UnityEngine;

[Serializable]
public class tk2dSpriteDefinition
{
	public enum ColliderType
	{
		Unset,
		None,
		Box,
		Mesh,
		Custom
	}

	public enum PhysicsEngine
	{
		Physics3D,
		Physics2D
	}

	public enum FlipMode
	{
		None,
		Tk2d,
		TPackerCW
	}

	[Serializable]
	public class AttachPoint
	{
		public string name = string.Empty;

		public Vector3 position = Vector3.zero;

		public float angle;

		public void CopyFrom(AttachPoint src)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			name = src.name;
			position = src.position;
			angle = src.angle;
		}

		public bool CompareTo(AttachPoint src)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			return name == src.name && src.position == position && src.angle == angle;
		}
	}

	public string name;

	public Vector3[] boundsData;

	public Vector3[] untrimmedBoundsData;

	public Vector2 texelSize;

	public Vector3[] positions;

	public Vector3[] normals;

	public Vector4[] tangents;

	public Vector2[] uvs;

	public Vector2[] normalizedUvs = (Vector2[])(object)new Vector2[0];

	public int[] indices = new int[6] { 0, 3, 1, 2, 3, 0 };

	public Material material;

	[NonSerialized]
	public Material materialInst;

	public int materialId;

	public string sourceTextureGUID;

	public bool extractRegion;

	public int regionX;

	public int regionY;

	public int regionW;

	public int regionH;

	public FlipMode flipped;

	public bool complexGeometry;

	public PhysicsEngine physicsEngine;

	public ColliderType colliderType;

	public tk2dSpriteColliderDefinition[] customColliders = new tk2dSpriteColliderDefinition[0];

	public Vector3[] colliderVertices;

	public int[] colliderIndicesFwd;

	public int[] colliderIndicesBack;

	public bool colliderConvex;

	public bool colliderSmoothSphereCollisions;

	public tk2dCollider2DData[] polygonCollider2D = new tk2dCollider2DData[0];

	public tk2dCollider2DData[] edgeCollider2D = new tk2dCollider2DData[0];

	public AttachPoint[] attachPoints = new AttachPoint[0];

	public bool Valid => name.Length != 0;

	public Bounds GetBounds()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		return new Bounds(new Vector3(boundsData[0].x, boundsData[0].y, boundsData[0].z), new Vector3(boundsData[1].x, boundsData[1].y, boundsData[1].z));
	}

	public Bounds GetUntrimmedBounds()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		return new Bounds(new Vector3(untrimmedBoundsData[0].x, untrimmedBoundsData[0].y, untrimmedBoundsData[0].z), new Vector3(untrimmedBoundsData[1].x, untrimmedBoundsData[1].y, untrimmedBoundsData[1].z));
	}
}
