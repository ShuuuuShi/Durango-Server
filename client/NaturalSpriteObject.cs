using Durango.Render.Sprite;
using Durango.Terrain;
using UnityEngine;

public class NaturalSpriteObject : NaturalObject
{
	public Durango.Render.Sprite.Sprite Sprite { get; set; }

	public override float InteractionDistance
	{
		get
		{
			if (Sprite != null)
			{
				return 100f;
			}
			Collider componentInChildren = GetComponentInChildren<Collider>();
			if (componentInChildren != null)
			{
				Vector3 extents = componentInChildren.bounds.extents;
				return Mathf.Sqrt(extents.x * extents.x + extents.z * extents.z);
			}
			return 100f;
		}
	}

	public override Vector3 InteractionPosition
	{
		get
		{
			Vector3 vector = ((Sprite != null) ? Sprite.GameObject.transform.position : base.transform.position);
			return vector + InteractionOffset;
		}
	}

	public NaturalComponent NaturalComponent { get; set; }

	public Vector3 InteractionOffset { get; set; }

	public override void OnRemoved(TerrainChunkBase chunk, bool fastRemove)
	{
		if (!fastRemove)
		{
			TreeComponent treeComponent = NaturalComponent as TreeComponent;
			if ((bool)treeComponent)
			{
				treeComponent.OnLoot();
				return;
			}
		}
		TerrainChunk_Mobile terrainChunk_Mobile = chunk as TerrainChunk_Mobile;
		if (terrainChunk_Mobile != null && Sprite != null)
		{
			terrainChunk_Mobile.ReleaseSpriteToPool(Sprite);
		}
		base.gameObject.SetActive(value: false);
	}

	protected override Color GetDefaultColor()
	{
		Color white = Color.white;
		if (Sprite != null)
		{
			white.a = Sprite.GetColor().a;
		}
		return white;
	}

	protected override void SetColor(Color color)
	{
		if (Sprite != null)
		{
			Sprite.SetColor(color);
		}
	}

	public void SetInteractionOffset(Vector3 offset)
	{
		InteractionOffset = offset;
	}
}
