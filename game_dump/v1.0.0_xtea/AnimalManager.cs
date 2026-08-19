using System;
using System.Collections;
using System.Collections.Generic;
using InteractionData;
using JetBrains.Annotations;
using K1Network;
using L10N;
using Messages;
using UnityEngine;

public class AnimalManager : KSingleton<AnimalManager>
{
	private class AnimalModelInfoJson
	{
		public string TypeName;

		public string ModelPath;
	}

	private class AnimalModelMapsJson
	{
		public Dictionary<string, AnimalModelInfoJson> Models;
	}

	private class AnimalOrientation
	{
		public ulong EntityId;

		public GameObject Obj;

		public Vector3 Position;

		public float Yaw;

		public string AnimName;

		public float AnimTime;
	}

	private readonly Dictionary<ulong, AnimalBehavior> _animals = new Dictionary<ulong, AnimalBehavior>();

	private readonly Dictionary<ulong, float> _preAnimals = new Dictionary<ulong, float>();

	private AnimalModelMapsJson _animalModelsMap;

	private Transform _animalsTransform;

	private AnimalOrientation _lastPlayerTargetOrientation;

	private AnimalModelMapsJson AnimalModelsMap
	{
		get
		{
			if (_animalModelsMap == null)
			{
				_animalModelsMap = KUtility.ParseJsonFile<AnimalModelMapsJson>("ModelInfos/animal_model_map");
			}
			return _animalModelsMap;
		}
	}

	public event Action<AnimalBehavior> AnimalAppeared;

	public event Action<AnimalBehavior> AnimalDisappeared;

	private void Start()
	{
		KSingleton<GameManager>.Instance().PreReconnect += GameManager_PreReconnect;
		Connections.Frontend.On(delegate(AppearAnimal msg, PacketHeader header)
		{
			if (!_animals.ContainsKey(msg.EntityId))
			{
				float num = Time.time + 86400f;
				if (_preAnimals.ContainsKey(msg.EntityId))
				{
					_preAnimals[msg.EntityId] = num;
				}
				else
				{
					_preAnimals.Add(msg.EntityId, num);
				}
				MakeAnimalObject(msg, num);
			}
		});
		Connections.Frontend.On(delegate(FeedingSuccess msg, PacketHeader header)
		{
			if (_animals.TryGetValue(msg.PetId, out var value))
			{
				PetAI component = ((Component)value).GetComponent<PetAI>();
				if (Object.op_Implicit((Object)(object)component))
				{
					component.EatOut();
				}
			}
		});
		Connections.Frontend.On<Messages.Rider>(ReceiveRiderMsg);
		AddInterractionHandler();
	}

	private void GameManager_PreReconnect()
	{
		((MonoBehaviour)this).StopAllCoroutines();
		Dictionary<ulong, AnimalBehavior>.ValueCollection.Enumerator enumerator = _animals.Values.GetEnumerator();
		while (enumerator.MoveNext())
		{
			AnimalBehavior current = enumerator.Current;
			if ((Object)(object)current != (Object)null)
			{
				current.Suicide();
			}
			OnDisappearAnimal(current);
		}
		_animals.Clear();
		_lastPlayerTargetOrientation = null;
	}

	public AnimalBehavior GetAnimal(ulong id)
	{
		_animals.TryGetValue(id, out var value);
		return value;
	}

	public Dictionary<ulong, AnimalBehavior> GetAnimals()
	{
		return _animals;
	}

	[UsedImplicitly]
	[ExposedInEditor(null)]
	private void MakeAnimal(ushort type)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		WorldPosition position = default(WorldPosition);
		position.SetFromClientPosition(PlayerBehavior.LocalPlayer.CurrentPosition);
		AppearAnimal appearAnimal = default(AppearAnimal);
		appearAnimal.EntityId = (ulong)(Random.value * 1000000f);
		appearAnimal.EntityType = type;
		appearAnimal.Move = new Move
		{
			Movements = new Movement[1]
			{
				new Movement
				{
					Path = new Location[1]
					{
						new Location
						{
							Position = position
						}
					}
				}
			}
		};
		appearAnimal.Survival = new Messages.Survival
		{
			Life = new Gauge(new GaugeNode[1]
			{
				new GaugeNode(0.0, 100f)
			}),
			Gauges = new Dictionary<string, Gauge>()
		};
		AppearAnimal msg = appearAnimal;
		float num = Time.time + 86400f;
		_preAnimals.Add(msg.EntityId, num);
		MakeAnimalObject(msg, num);
	}

	public void MakeAnimalObject(AppearAnimal msg, float expireAt)
	{
		ulong id = msg.EntityId;
		int entityType = msg.EntityType;
		Move firstMoveMsg = msg.Move;
		int level = msg.Level;
		string modelPrefab = GetModelPrefab(entityType);
		KSingleton<AssetBundleManager>.Instance().RequestAsset(modelPrefab, typeof(GameObject), delegate(Object asset)
		{
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Expected O, but got Unknown
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			if (!(asset == (Object)null))
			{
				if (_preAnimals.ContainsKey(id))
				{
					expireAt = _preAnimals[id];
					_preAnimals.Remove(id);
					Object obj = Object.Instantiate(asset, Vector3.zero, Quaternion.identity);
					GameObject val = (GameObject)(object)((obj is GameObject) ? obj : null);
					if ((Object)(object)_animalsTransform == (Object)null)
					{
						GameObject val2 = new GameObject("Animals");
						_animalsTransform = val2.transform;
					}
					if (!((Object)(object)val == (Object)null))
					{
						val.transform.parent = _animalsTransform;
						AnimalBehavior component = val.GetComponent<AnimalBehavior>();
						component.ExpireAt = expireAt;
						Location location = PathMovable.GetLocation(firstMoveMsg, Connections.Frontend.GetBufferedServerTime());
						component.CurrentPosition = location.Position.ToClientPosition();
						component.TurnToYaw(location.Yaw, bSnap: true);
						component.Floor = location.Floor;
						component.EntityId = id;
						component.EntityTypeId = entityType;
						component.Level = level;
						_animals.Add(id, component);
						HandleMoveMsg(firstMoveMsg);
						component.SetSurvivalGauge(msg.Survival.Life, msg.Survival.Gauges);
						OnAppearAnimal(component);
					}
				}
				else if (_animals.ContainsKey(id))
				{
					_animals[id].ExpireAt = expireAt;
				}
			}
		});
	}

	public bool HandleMoveMsg(Move msg)
	{
		AnimalBehavior animal = GetAnimal(msg.EntityId);
		if ((Object)(object)animal != (Object)null)
		{
			animal.HandleMoveMsg(msg);
			return true;
		}
		return false;
	}

	public bool HandleDisappearMsg(DisappearEntity msg)
	{
		AnimalBehavior animalBehavior = _animals.Get(msg.EntityId);
		if ((Object)(object)animalBehavior != (Object)null)
		{
			animalBehavior.Suicide();
			RemoveAnimal(animalBehavior);
			return true;
		}
		if (_preAnimals.ContainsKey(msg.EntityId))
		{
			_preAnimals.Remove(msg.EntityId);
			return true;
		}
		return false;
	}

	public void RemoveAnimal(AnimalBehavior animal)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)PlayerBehavior.LocalPlayer.Target == (Object)(object)((Component)animal).gameObject)
		{
			_lastPlayerTargetOrientation = new AnimalOrientation
			{
				EntityId = animal.EntityId,
				Obj = ((Component)animal).gameObject,
				Position = animal.CurrentPosition,
				Yaw = animal.CurrentYaw,
				AnimName = ((Object)animal.CurAnimState.clip).name,
				AnimTime = animal.CurAnimState.time
			};
		}
		OnDisappearAnimal(animal);
		_animals.Remove(animal.EntityId);
	}

	public string GetModelPrefab(int entityTypeId)
	{
		if (AnimalModelsMap == null)
		{
			return string.Empty;
		}
		string key = entityTypeId.ToString();
		AnimalModelInfoJson value;
		return (!AnimalModelsMap.Models.TryGetValue(key, out value)) ? string.Empty : value.ModelPath;
	}

	private void OnAppearAnimal(AnimalBehavior animal)
	{
		if (this.AnimalAppeared != null)
		{
			this.AnimalAppeared(animal);
		}
	}

	private void OnDisappearAnimal(AnimalBehavior animal)
	{
		if (this.AnimalDisappeared != null)
		{
			this.AnimalDisappeared(animal);
		}
	}

	public void ForceAddAnimal(ulong id, AnimalBehavior animal)
	{
		_animals.Add(id, animal);
	}

	private void ReceiveRiderMsg(Messages.Rider m, PacketHeader header)
	{
		PlayerBehavior playerIncludeLocalPlayer = KSingleton<PlayerManager>.Instance().GetPlayerIncludeLocalPlayer(m.EntityId);
		if ((Object)(object)playerIncludeLocalPlayer == (Object)null)
		{
			Debug.LogError((object)("Master entity (" + m.EntityId + ") does not exist"));
			return;
		}
		GameObject gameObject = ((Component)playerIncludeLocalPlayer).gameObject;
		ulong? vehicleId = m.VehicleId;
		AnimalBehavior value;
		if (!vehicleId.HasValue)
		{
			Driver component = gameObject.GetComponent<Driver>();
			if ((Object)(object)component == (Object)null)
			{
				Debug.LogError((object)"No Driver component!");
			}
			else
			{
				component.ReturnVehicle(playReturnMotion: true);
			}
		}
		else if (_animals.TryGetValue(m.VehicleId.Value, out value) && (Object)(object)value != (Object)null)
		{
			UpdateBoardingState(m, gameObject, ((Component)value).gameObject);
		}
		else
		{
			MakeVehicle(m, gameObject);
		}
	}

	public void AnimalTamed(ulong animalEntityId, Messages.Rider rider)
	{
		((MonoBehaviour)this).StartCoroutine(CoAnimalTamed(animalEntityId, rider, 10f));
	}

	private IEnumerator CoAnimalTamed(ulong animalEntityId, Messages.Rider rider, float timeout = 10f)
	{
		float beginTime = Time.time;
		ulong? vehicleId = rider.VehicleId;
		if (!vehicleId.HasValue)
		{
			yield break;
		}
		AnimalBehavior spawnedAnimal;
		while (!_animals.TryGetValue(rider.VehicleId.Value, out spawnedAnimal))
		{
			yield return null;
			if (Time.time - beginTime > timeout)
			{
				yield break;
			}
		}
		if (_lastPlayerTargetOrientation != null && animalEntityId == _lastPlayerTargetOrientation.EntityId)
		{
			spawnedAnimal.CurrentPosition = _lastPlayerTargetOrientation.Position;
			spawnedAnimal.TurnToYaw(_lastPlayerTargetOrientation.Yaw, bSnap: true);
			spawnedAnimal.Play(_lastPlayerTargetOrientation.AnimName, loop: false, _lastPlayerTargetOrientation.AnimTime);
			PetAI petAI = ((Component)spawnedAnimal).GetComponent<PetAI>();
			if (Object.op_Implicit((Object)(object)petAI))
			{
				petAI.Tamed();
			}
			if (Object.op_Implicit((Object)(object)_lastPlayerTargetOrientation.Obj))
			{
				Object.Destroy((Object)(object)_lastPlayerTargetOrientation.Obj);
			}
		}
	}

	public void MakeVehicle(Messages.Rider m, GameObject master, Action<GameObject> onFinished = null)
	{
		ulong? vehicleId = m.VehicleId;
		if (!vehicleId.HasValue)
		{
			return;
		}
		if (_animals.TryGetValue(m.VehicleId.Value, out var value))
		{
			if ((Object)(object)value != (Object)null)
			{
				InitVehicle(m, master, ((Component)value).gameObject);
				UpdateBoardingState(m, master, ((Component)value).gameObject);
				return;
			}
			_animals.Remove(m.VehicleId.Value);
		}
		ushort? vehicleEntityType = m.VehicleEntityType;
		if (!vehicleEntityType.HasValue)
		{
			Debug.LogError((object)"VehicleEntityType != null");
			return;
		}
		string modelPrefab = GetModelPrefab(m.VehicleEntityType.Value);
		KSingleton<AssetBundleManager>.Instance().RequestAsset(modelPrefab, typeof(GameObject), delegate(Object asset)
		{
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			if (!(asset == (Object)null) && !((Object)(object)master == (Object)null))
			{
				Driver component = master.GetComponent<Driver>();
				if ((Object)(object)component == (Object)null)
				{
					Debug.LogError((object)"No Driver component!");
				}
				else
				{
					component.ReturnVehicle(playReturnMotion: false);
					GameObject obj = default(GameObject);
					ref GameObject reference = ref obj;
					Object obj2 = Object.Instantiate(asset, master.transform.position - new Vector3(0f, -10000f, 0f), Quaternion.identity);
					reference = (GameObject)(object)((obj2 is GameObject) ? obj2 : null);
					AnimalBehavior animalBehavior = InitVehicle(m, master, obj);
					if (!((Object)(object)animalBehavior == (Object)null))
					{
						_animals[animalBehavior.EntityId] = animalBehavior;
						OnAppearAnimal(animalBehavior);
						KUtility.DelayedCall((MonoBehaviour)(object)this, delegate
						{
							UpdateBoardingState(m, master, obj, startup: true);
						}, 1f);
						if (onFinished != null)
						{
							onFinished(obj);
						}
					}
				}
			}
		});
	}

	private static AnimalBehavior InitVehicle(Messages.Rider m, GameObject master, GameObject obj)
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)obj == (Object)null)
		{
			return null;
		}
		Driver component = master.GetComponent<Driver>();
		if ((Object)(object)component == (Object)null)
		{
			Debug.LogError((object)"No Driver component!");
			return null;
		}
		Vehicle component2 = obj.GetComponent<Vehicle>();
		PetAI petAI = obj.GetComponent<PetAI>();
		if ((Object)(object)petAI == (Object)null)
		{
			petAI = obj.AddComponent<PetAI>();
		}
		if ((Object)(object)component2 == (Object)null || (Object)(object)petAI == (Object)null)
		{
			Debug.LogError((object)"Requires components: vehicle, pet");
			return null;
		}
		petAI.Init(master, inCage: false, m.IsBoarding);
		AnimalBehavior component3 = obj.GetComponent<AnimalBehavior>();
		if ((Object)(object)component3 == (Object)null)
		{
			return null;
		}
		component2.MoveSpeed = ((!m.Speed.HasValue) ? 500f : ((float)(int)m.Speed.Value));
		component2.PlaybackRate = ((!m.PlaybackRate.HasValue) ? 1f : m.PlaybackRate.Value);
		component3.EntityId = m.VehicleId.Value;
		component3.EntityTypeId = m.VehicleEntityType.Value;
		component3.SetName(m.VehicleName);
		component.UseVehicle(component2, playSpawnMotion: true);
		return component3;
	}

	public void MakeCageAnimal(ulong id, int type, string animalName, GameObject owner, Action<GameObject> onFinished = null)
	{
		string modelPrefab = GetModelPrefab(type);
		KSingleton<AssetBundleManager>.Instance().RequestAsset(modelPrefab, typeof(GameObject), delegate(Object asset)
		{
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			if (!(asset == (Object)null) && !((Object)(object)owner == (Object)null))
			{
				AnimalBehavior animalBehavior = _animals.Get(id);
				if ((Object)(object)animalBehavior != (Object)null)
				{
					PetAI component = ((Component)animalBehavior).GetComponent<PetAI>();
					if (Object.op_Implicit((Object)(object)component) && !component.InCage)
					{
						component.ReturnToCage(owner);
						if (onFinished != null)
						{
							onFinished(((Component)animalBehavior).gameObject);
						}
					}
				}
				else
				{
					_animals.Remove(id);
					Object obj = Object.Instantiate(asset, owner.transform.position, Quaternion.identity);
					GameObject val = (GameObject)(object)((obj is GameObject) ? obj : null);
					if (!((Object)(object)val == (Object)null))
					{
						AnimalBehavior component2 = val.GetComponent<AnimalBehavior>();
						if (!((Object)(object)component2 == (Object)null))
						{
							component2.EntityId = id;
							component2.EntityTypeId = type;
							component2.SetName(animalName);
							_animals[component2.EntityId] = component2;
							OnAppearAnimal(component2);
							if (onFinished != null)
							{
								onFinished(val);
							}
						}
					}
				}
			}
		});
	}

	private static void UpdateBoardingState(Messages.Rider m, GameObject master, GameObject obj, bool startup = false)
	{
		if ((Object)(object)master == (Object)null)
		{
			return;
		}
		Driver component = master.GetComponent<Driver>();
		if ((Object)(object)component == (Object)null)
		{
			return;
		}
		AnimalBehavior component2 = obj.GetComponent<AnimalBehavior>();
		if (Object.op_Implicit((Object)(object)component2))
		{
			component2.SetName(m.VehicleName);
		}
		if (m.IsBoarding)
		{
			if (!component.IsRiding && (Object)(object)obj != (Object)null)
			{
				component.Mount(obj.GetComponent<Vehicle>(), startup);
			}
		}
		else if (component.IsRiding && component.IsRiding)
		{
			component.Unmount();
		}
	}

	private void AddInterractionHandler()
	{
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.Mount, delegate
		{
			Connections.Frontend.Send(default(Mount));
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.Unmount, delegate
		{
			Connections.Frontend.Send(default(Unmount));
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ReturnPet, delegate
		{
			Connections.Frontend.Send(default(ReturnPet));
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.RenamePet, delegate
		{
			if ((Object)(object)PlayerBehavior.LocalPlayer.Driver != (Object)null && (Object)(object)PlayerBehavior.LocalPlayer.Driver.Vehicle != (Object)null)
			{
				AnimalBehavior animal = ((Component)PlayerBehavior.LocalPlayer.Driver.Vehicle).GetComponent<AnimalBehavior>();
				if (Object.op_Implicit((Object)(object)animal))
				{
					TextInputWidget textInput = UIManager.Popup.TextInput;
					textInput.Show(delegate(string newName)
					{
						Connections.Frontend.Send(new RenamePet
						{
							Name = newName,
							PetId = animal.EntityId
						});
					}, T._("새로운 이름을 적어주세요"), animal.GetName());
				}
			}
		});
	}
}
