using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Durango.Network;
using Durango.Offline;
using Durango.Terrain;
using Durango.Utils;
using InteractionData;
using Messages;
using UnityEngine;
using Yaml;

public class GrazingPetManager
{
	private Messages.Pet[] _pets;

	private readonly Dictionary<string, GrazingPetAI> _ghosts = new Dictionary<string, GrazingPetAI>();

	private float _updateAt;

	private readonly PlayerContext _context;

	public GrazingPetManager()
	{
		GameSystem<InteractionSystem>.Instance().PreTouchTarget += OnPreTouchTarget;
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.RemoveGrazingPet, delegate(InteractionObject obj)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("remove_grazing");
			stringBuilder.AppendFormat(" {0}", obj.EntityId);
			Connections.Frontend.Send(new Cheat
			{
				_Cheat = stringBuilder.ToString().Trim()
			});
		});
	}

	private void OnPreTouchTarget(InteractionObject obj, ref bool result)
	{
		string entityId = obj.EntityId;
		Messages.Pet? pet = null;
		Messages.Pet[] pets = _pets;
		for (int i = 0; i < pets.Length; i++)
		{
			Messages.Pet value = pets[i];
			if (value.EntityId == entityId)
			{
				pet = value;
				break;
			}
		}
		if (pet.HasValue)
		{
			result = true;
			InteractionMenuList menuList = GameSystem<InteractionSystem>.Instance().MenuList;
			menuList.Reset();
			menuList.Add(Interaction.RemoveGrazingPet);
			menuList.Name = pet.Value.Name;
			GameSystem<InteractionSystem>.Instance().ShowClientMenuList(obj.Target);
		}
	}

	public void Set(Messages.Pet[] pets)
	{
		_pets = pets;
		using Reusable<List<string>> reusable = ReusableList<string>.Pop();
		foreach (KeyValuePair<string, GrazingPetAI> ghost in _ghosts)
		{
			if (ghost.Value == null)
			{
				reusable.Value.Add(ghost.Key);
				continue;
			}
			string id = ghost.Value.TargetAnimal.EntityId;
			if (_pets == null || _pets.All((Messages.Pet p) => p.EntityId != id))
			{
				reusable.Value.Add(ghost.Key);
			}
		}
		foreach (string item in reusable.Value)
		{
			Destroy(item);
		}
	}

	private Messages.Pet? GetRandomPet()
	{
		if (_pets == null || _pets.Length == 0)
		{
			return null;
		}
		int num = UnityEngine.Random.Range(1, _pets.Length);
		int num2 = UnityEngine.Random.Range(0, _pets.Length);
		Messages.Pet? result = null;
		for (int i = 0; i < _pets.Length; i++)
		{
			Messages.Pet value = _pets[num2 % _pets.Length];
			num2++;
			if (!_ghosts.ContainsKey(value.EntityId))
			{
				num--;
				if (num <= 0)
				{
					result = value;
					break;
				}
			}
		}
		return result;
	}

	private void Make(Messages.Pet pet, Vector3 pos)
	{
		if (_ghosts.ContainsKey(pet.EntityId))
		{
			return;
		}
		string prefabPath = AnimalYaml.GetPrefabPath(pet.GetAnimalType());
		_ghosts[pet.EntityId] = null;
		Singleton<AssetBundleManager>.Instance().RequestAsset(prefabPath, typeof(GameObject), delegate(UnityEngine.Object asset)
		{
			if (asset == null)
			{
				_ghosts.Remove(pet.EntityId);
			}
			else
			{
				Quaternion rotation = Quaternion.Euler(new Vector3(0f, UnityEngine.Random.Range(0f, 360f), 0f));
				GameObject gameObject = UnityEngine.Object.Instantiate(asset, pos, rotation) as GameObject;
				if (gameObject == null)
				{
					_ghosts.Remove(pet.EntityId);
				}
				else
				{
					GrazingPetAI grazingPetAI = gameObject.AddMissingComponent<GrazingPetAI>();
					grazingPetAI.Pet = pet;
					AnimalBehavior component = gameObject.GetComponent<AnimalBehavior>();
					component.EntityId = pet.EntityId;
					component.EntityTypeId = pet.EntityType;
					component.SetAlive(alive: true, fromInit: true);
					component.SetName(pet.Name);
					component.TurnToYaw(UnityEngine.Random.Range(0f, 360f), bSnap: true);
					PetStats stat = pet.Stat;
					component.Level = pet.Statistics.Level;
					component.SetSurvivalGauge(stat.Life, null);
					component.SetOld(stat.IsOld);
					_ghosts[pet.EntityId] = grazingPetAI;
					component.ShowEyeTrail(pet.HasEyeTrail());
				}
			}
		});
	}

	private void Destroy(string entityId)
	{
		GrazingPetAI grazingPetAI = _ghosts.Get(entityId);
		if (!(grazingPetAI == null))
		{
			UnityEngine.Object.Destroy(grazingPetAI.gameObject);
			_ghosts.Remove(entityId);
		}
	}

	public void Update()
	{
		float time = Time.time;
		if (time < _updateAt)
		{
			return;
		}
		_updateAt = time + 5f;
		if (KUtility.GetSize(_pets) == 0 && _ghosts.Count == 0)
		{
			return;
		}
		float num = Mathf.Pow(0.5f, 1 + _ghosts.Count);
		if (UnityEngine.Random.value < num)
		{
			Messages.Pet? randomPet = GetRandomPet();
			if (randomPet.HasValue)
			{
				Vector3 currentPosition = PlayerBehavior.LocalPlayer.CurrentPosition;
				float f = UnityEngine.Random.value * (float)Math.PI * 2f;
				Vector3 vector = new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f));
				float num2 = UnityEngine.Random.Range(1000f, 1500f);
				currentPosition += vector * num2;
				Make(randomPet.Value, currentPosition);
			}
		}
		Point2 centerChunkCoords = Singleton<TerrainBase>.Instance().CenterChunkCoords;
		using Reusable<List<string>> reusable = ReusableList<string>.Pop();
		foreach (KeyValuePair<string, GrazingPetAI> ghost in _ghosts)
		{
			if (ghost.Value == null)
			{
				reusable.Value.Add(ghost.Key);
				continue;
			}
			Point2 point = Util.ClientPositionToChunkCoords(ghost.Value.TargetAnimal.CurrentPosition);
			Point2 point2 = centerChunkCoords - point;
			if (Mathf.Max(Mathf.Abs(point2.x), Mathf.Abs(point2.y)) > 1)
			{
				reusable.Value.Add(ghost.Key);
			}
		}
		foreach (string item in reusable.Value)
		{
			Destroy(item);
		}
	}
}
