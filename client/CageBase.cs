using System;
using System.Collections.Generic;
using Durango.Terrain;
using Durango.Utils;
using UnityEngine;
using Yaml;

public abstract class CageBase : ArtifactComponent
{
	private const float Margin = 200f;

	protected readonly Dictionary<string, AnimalBehavior> _animals = new Dictionary<string, AnimalBehavior>();

	public Vector3 ClientPos => Util.TilePositionToClientPosition(base.Artifact.WorldTile);

	public Vector3 MinArea => ClientPos + new Vector3(200f, 0f, 200f);

	public Vector3 MaxArea => ClientPos + new Vector3(base.Artifact.Size.x, 0f, base.Artifact.Size.y) * 200f - new Vector3(200f, 0f, 200f);

	public abstract override bool OnUpdateState(double eventTime);

	public sealed override void OnRemoved()
	{
		foreach (AnimalBehavior value in _animals.Values)
		{
			if (value != null)
			{
				UnityEngine.Object.Destroy(value.gameObject);
			}
		}
		_animals.Clear();
		base.OnRemoved();
	}

	protected void MakeAnimal(string entityId, int animalId, Action<PetAI> onLoaded)
	{
		if (_animals.ContainsKey(entityId))
		{
			return;
		}
		_animals.Add(entityId, null);
		string prefabPath = AnimalYaml.GetPrefabPath(animalId);
		Singleton<AssetBundleManager>.Instance().RequestAsset(prefabPath, typeof(GameObject), delegate(UnityEngine.Object asset)
		{
			if (_animals.ContainsKey(entityId))
			{
				int num = Math.Abs(entityId.GetHashCode());
				Vector3 vector = (MaxArea - MinArea) * 0.1f;
				Vector3 spawnPosition = new Vector3((float)(num % 10) * vector.x + MinArea.x, 0f, (float)(num % 100) * 0.1f * vector.z + MinArea.z);
				AnimalBehavior animalBehavior = PetManager.InstantiateAnimalObject(asset, spawnPosition, entityId, animalId, isAlive: true);
				if (!(animalBehavior == null))
				{
					Collider[] componentsInChildren = animalBehavior.GetComponentsInChildren<Collider>();
					Collider[] array = componentsInChildren;
					foreach (Collider obj in array)
					{
						UnityEngine.Object.Destroy(obj);
					}
					_animals[animalBehavior.EntityId] = animalBehavior;
					PetAI component = animalBehavior.GetComponent<PetAI>();
					component.SetInCage(MinArea, MaxArea);
					component.SetMaster(base.Artifact.gameObject, isRiding: false);
					VehiclePet component2 = animalBehavior.GetComponent<VehiclePet>();
					if (component2 != null)
					{
						component2.SetupSaddle(setupSaddle: false);
					}
					if (onLoaded != null)
					{
						onLoaded(component);
					}
				}
			}
		});
	}
}
