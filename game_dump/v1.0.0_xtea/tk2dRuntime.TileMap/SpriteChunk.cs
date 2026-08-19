using System;
using System.Collections.Generic;
using UnityEngine;

namespace tk2dRuntime.TileMap;

[Serializable]
public class SpriteChunk
{
	private bool dirty;

	public int[] spriteIds;

	public GameObject gameObject;

	public Mesh mesh;

	public MeshCollider meshCollider;

	public Mesh colliderMesh;

	public List<EdgeCollider2D> edgeColliders = new List<EdgeCollider2D>();

	public bool Dirty
	{
		get
		{
			return dirty;
		}
		set
		{
			dirty = value;
		}
	}

	public bool IsEmpty => spriteIds.Length == 0;

	public bool HasGameData => (Object)(object)gameObject != (Object)null || (Object)(object)mesh != (Object)null || (Object)(object)meshCollider != (Object)null || (Object)(object)colliderMesh != (Object)null || edgeColliders.Count > 0;

	public SpriteChunk()
	{
		spriteIds = new int[0];
	}

	public void DestroyGameData(tk2dTileMap tileMap)
	{
		if ((Object)(object)mesh != (Object)null)
		{
			tileMap.DestroyMesh(mesh);
		}
		if ((Object)(object)gameObject != (Object)null)
		{
			tk2dUtil.DestroyImmediate((Object)(object)gameObject);
		}
		gameObject = null;
		mesh = null;
		DestroyColliderData(tileMap);
	}

	public void DestroyColliderData(tk2dTileMap tileMap)
	{
		if ((Object)(object)colliderMesh != (Object)null)
		{
			tileMap.DestroyMesh(colliderMesh);
		}
		if ((Object)(object)meshCollider != (Object)null && (Object)(object)meshCollider.sharedMesh != (Object)null && (Object)(object)meshCollider.sharedMesh != (Object)(object)colliderMesh)
		{
			tileMap.DestroyMesh(meshCollider.sharedMesh);
		}
		if ((Object)(object)meshCollider != (Object)null)
		{
			tk2dUtil.DestroyImmediate((Object)(object)meshCollider);
		}
		meshCollider = null;
		colliderMesh = null;
		if (edgeColliders.Count > 0)
		{
			for (int i = 0; i < edgeColliders.Count; i++)
			{
				tk2dUtil.DestroyImmediate((Object)(object)edgeColliders[i]);
			}
			edgeColliders.Clear();
		}
	}
}
