using UnityEngine;

public class TileObject
{
	public enum Type
	{
		Empty,
		Artifact,
		NaturalObject
	}

	private Artifact _artifact;

	public Point2 WorldTile { get; private set; }

	public GameObject GrassSprite { get; private set; }

	public GameObject LandmarkObject { get; private set; }

	public Type TileType { get; private set; }

	public GameObject StaticObject { get; private set; }

	public Artifact Artifact
	{
		get
		{
			return _artifact;
		}
		private set
		{
			_artifact = value;
			ModularArtifact modularArtifact = ((!((Object)(object)_artifact == (Object)null)) ? _artifact.GetArtifactComponent<ModularArtifact>() : null);
			IsInside = modularArtifact != null;
		}
	}

	public NaturalObject NaturalObject { get; private set; }

	public string PoolName { get; private set; }

	public string LandmarkPoolName { get; private set; }

	public ulong EstateId { get; set; }

	public ulong OwnerId { get; set; }

	public bool IsInside { get; private set; }

	public void Reset()
	{
		RemoveStaticObject(fastRemove: true);
		RemoveLandmark();
		GrassSprite = null;
	}

	public void RemoveStaticObject(bool fastRemove = false)
	{
		Type tileType = TileType;
		if (tileType != Type.Artifact && tileType == Type.NaturalObject)
		{
			RemoveNaturalObject(fastRemove);
		}
		KSingleton<StaticObjectPool>.Instance().ReturnObject(PoolName, StaticObject);
		WorldTile = Point2.zero;
		TileType = Type.Empty;
		StaticObject = null;
		PoolName = null;
		NaturalObject = null;
		Artifact = null;
	}

	public void RemoveLandmark()
	{
		KSingleton<StaticObjectPool>.Instance().ReturnObject(LandmarkPoolName, LandmarkObject);
		LandmarkObject = null;
		LandmarkPoolName = null;
	}

	private void RemoveNaturalObject(bool fastRemove)
	{
		if ((Object)(object)NaturalObject == (Object)null)
		{
			return;
		}
		if (fastRemove)
		{
			((Component)NaturalObject).gameObject.SetActive(false);
			return;
		}
		TreeComponent treeComponent = NaturalObject.NaturalComponent as TreeComponent;
		if ((bool)treeComponent)
		{
			treeComponent.OnLoot();
		}
		else
		{
			((Component)NaturalObject).gameObject.SetActive(false);
		}
	}

	public ImmovableBase GetImmovable()
	{
		return TileType switch
		{
			Type.Empty => null, 
			Type.Artifact => Artifact, 
			Type.NaturalObject => NaturalObject, 
			_ => null, 
		};
	}

	public void SetGrassSprite(GameObject grassSprite)
	{
		GrassSprite = grassSprite;
	}

	public void RemoveGrassSprite()
	{
		if ((Object)(object)GrassSprite != (Object)null)
		{
			GrassSprite.SetActive(false);
		}
	}

	public void SetLandmarkObject(GameObject landmark, string poolName)
	{
		LandmarkObject = landmark;
		LandmarkPoolName = poolName;
	}

	public void SetNaturalObject(GameObject staticObject, NaturalObject natural, string poolName = null)
	{
		NaturalObject = natural;
		TileType = Type.NaturalObject;
		StaticObject = staticObject;
		PoolName = poolName;
	}

	public void SetArtifact(Artifact artifact)
	{
		Artifact = artifact;
		WorldTile = artifact.WorldTile;
		TileType = Type.Artifact;
		RemoveGrassSprite();
	}

	public void OverrideDepth(ref byte floor, ref float depth00, ref float depth10, ref float depth01, ref float depth11)
	{
		if ((Object)(object)Artifact == (Object)null)
		{
			floor = 0;
		}
		else
		{
			Artifact.OverrideDepth(ref floor, ref depth00, ref depth10, ref depth01, ref depth11);
		}
	}
}
