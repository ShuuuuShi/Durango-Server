using Durango.Render.Sprite;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.Terrain;

public class TileObject
{
	[CanBeNull]
	private Artifact _artifact;

	public Artifact Artifact
	{
		get
		{
			return _artifact;
		}
		private set
		{
			if (!(value == _artifact))
			{
				Artifact artifact = _artifact;
				_artifact = value;
				if (_artifact != null)
				{
					value.OnAttached();
				}
				if (artifact != null)
				{
					artifact.OnDetached();
				}
			}
		}
	}

	public NaturalObject NaturalObject { get; private set; }

	public Durango.Render.Sprite.Sprite GrassSprite { get; private set; }

	public GameObject LandmarkObject { get; private set; }

	public string LandmarkPoolName { get; private set; }

	public bool IsIndoor { get; set; }

	public bool IsGrassRemoved { get; set; }

	public int? Stories
	{
		get
		{
			if (Artifact == null || !Artifact.BuildCompleted)
			{
				return null;
			}
			return Artifact.Stories;
		}
	}

	public bool IsEmpty()
	{
		if (NaturalObject == null)
		{
			return Artifact == null;
		}
		return false;
	}

	public void Reset()
	{
		RemoveImmovable(null, fastRemove: true);
		RemoveLandmark();
		GrassSprite = null;
		IsGrassRemoved = false;
	}

	public void RemoveImmovable([CanBeNull] TerrainChunkBase chunk, bool fastRemove = false)
	{
		if (NaturalObject != null)
		{
			NaturalObject.OnRemoved(chunk, fastRemove);
			NaturalObject = null;
		}
		Artifact = null;
		IsIndoor = false;
	}

	public void RemoveLandmark()
	{
		Singleton<StaticObjectPool>.Instance().ReturnObject(LandmarkPoolName, LandmarkObject);
		LandmarkObject = null;
		LandmarkPoolName = null;
	}

	public ImmovableBase GetImmovable()
	{
		if (NaturalObject != null)
		{
			return NaturalObject;
		}
		return Artifact;
	}

	public void SetGrassSprite(Durango.Render.Sprite.Sprite grassSprite)
	{
		GrassSprite = grassSprite;
	}

	public void SetLandmarkObject(GameObject landmark, string poolName)
	{
		LandmarkObject = landmark;
		LandmarkPoolName = poolName;
	}

	public void SetNaturalObject([NotNull] NaturalObject natural)
	{
		NaturalObject = natural;
	}

	public void SetArtifact([NotNull] Artifact artifact)
	{
		Artifact = artifact;
	}

	public bool IsIgnoreWaterDepth()
	{
		if (Artifact != null)
		{
			return Artifact.IsIgnoreWaterDepth();
		}
		return false;
	}
}
