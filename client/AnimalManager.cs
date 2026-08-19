using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using InteractionData;
using JetBrains.Annotations;
using Messages;
using Shared.Battle;
using UnityEngine;
using Yaml;

public class AnimalManager : Singleton<AnimalManager>
{
	public readonly Dictionary<string, AnimalBehavior> _animals = new Dictionary<string, AnimalBehavior>();

	private AppearAnimal _appearAnimal;

	[SerializeField]
	private WildAnimalAI _wildAnimalAI;

	private readonly Dictionary<string, WildAnimalAI> _ghosts = new Dictionary<string, WildAnimalAI>();

	public event Action<AnimalBehavior> AnimalAppeared;

	public event Action<AnimalBehavior> AnimalDisappeared;

	private void Start()
	{
		Singleton<GameManager>.Instance().PreReconnect += GameManager_PreReconnect;
		GameSystem<InteractionSystem>.Instance().PreTouchTarget += OnPreTouchTarget;
		GameSystem<GatheringSystem>.Instance().CollectiblePermissionChanged += GatheringSystem_CollectiblePermissionChanged;
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.Attack, delegate(InteractionObject obj)
		{
			Connections.Frontend.PushPacket(new BattleBegun
			{
				EntityId = GameManager.PlayerId,
				EnemyId = obj.EntityId
			});
			// [แก้เอง] **ต้นเหตุอาการ "กดกากบาทแล้วหน้าต่างไม่ปิด"**
			//
			// เดิมเรียก GetTargetComponent<WildAnimalAI>().SetCombatAiActivated() ตรง ๆ โดยไม่เช็ค null
			// สัตว์ที่ server เราสร้างขึ้นเองไม่ได้ผ่าน PrepareLoad จึงไม่มี component นี้ ⇒ โยน NRE
			// ⇒ บรรทัด SetInteractionTarget(null) ข้างล่าง **ไม่เคยได้ทำงาน**
			// ⇒ เมนูโต้ตอบค้างสถานะ "กำลังแสดง" ตลอดไป
			// ⇒ UIManager.OnPreCloseUI เห็น InteractionMenu.CloseMenus() คืน true ทุกครั้ง
			//    แล้ว "กินคำสั่งปิด" ไปหมด ⇒ **ทุกหน้าต่างปิดไม่ได้ และไม่มี error ให้เห็น**
			//
			// พิสูจน์แล้วด้วยการสลับไปใช้ DLL ต้นฉบับ (6.06 MB) — กากบาทปิดได้ปกติ
			//
			// ปิดเมนู**ก่อน**เสมอ แล้วค่อยทำส่วนที่พังได้ และเช็ค null ให้ครบ
			GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
			WildAnimalAI ai = obj.GetTargetComponent<WildAnimalAI>();
			if (ai != null)
			{
				ai.SetCombatAiActivated();
			}
			GameSystem<CombatSystem>.Instance().SelectTarget(obj.EntityId);
			if (Player.Instance != null && Player.Instance._context.ActivePet.PetData.HasValue)
			{
				CharacterBehavior characterBehavior = Singleton<ObjectManager>.Instance().FindCharacter(Player.Instance._context.ActivePet.EntityId);
				if (!(characterBehavior == null))
				{
					PetAI component = characterBehavior.GetComponent<PetAI>();
					if (component == null)
					{
						UIManager.SystemMsg("Error", "오류가 발생하였습니다.");
					}
					else
					{
						component.BattleBegin();
					}
				}
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.RemoveAppearAnimal, delegate(InteractionObject obj)
		{
			GetAnimal(obj.EntityId).Disappear();
			Player.Instance._world.RemoveAppearAnimal(obj.EntityId);
		});
		Connections.Frontend.On(delegate(AppearAnimal msg, PacketHeader header)
		{
			// [แก้เอง] สัตว์ที่ server เรา broadcast มาไม่เคยผ่าน PrepareLoad มาก่อน
			// ของเดิมจึงเข้า if ไม่ได้เลย = สัตว์ไม่โผล่ในเกม → สร้างตัวมันขึ้นมาก่อนเสมอ
			// (เดิมเป็น IL patch ใน tools/DllPatcher ตอนนี้อยู่ในซอร์สแล้ว)
			MakeAnimalObject(msg, Vector3.zero);
			if (_animals.TryGetValue(msg.EntityId, out var value))
			{
				if (!(value == null))
				{
					value.Appear();
				}
				OnPostAppearAnimal(msg);
			}
		});
	}

	private void GatheringSystem_CollectiblePermissionChanged(string entityId, bool permission)
	{
		AnimalBehavior animal = GetAnimal(entityId);
		if (animal != null)
		{
			animal.IsLootable = permission;
		}
	}

	private void GameManager_PreReconnect()
	{
		StopAllCoroutines();
		AnimalBehavior[] array = new AnimalBehavior[_animals.Count];
		_animals.Values.CopyTo(array, 0);
		AnimalBehavior[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			UnityEngine.Object.Destroy(array2[i].gameObject);
		}
		_animals.Clear();
	}

	public AnimalBehavior GetAnimal(string id)
	{
		return _animals.Get(id);
	}

	public void PrepareLoad(string id)
	{
		_animals[id] = null;
	}

	public bool CheckPrepared(string id)
	{
		if (!_animals.TryGetValue(id, out var value))
		{
			return false;
		}
		if (value == null)
		{
			_animals.Remove(id);
			return true;
		}
		return false;
	}

	private void Animal_Destroyed(AnimalBehavior animal)
	{
		OnDisappearAnimal(animal);
		_animals.Remove(animal.EntityId);
	}

	public bool HandleMoveMsg(Move msg)
	{
		AnimalBehavior animal = GetAnimal(msg.EntityId);
		if (animal != null)
		{
			animal.HandleMoveMsg(msg);
			return true;
		}
		return false;
	}

	public bool HandleDisappearMsg(DisappearEntity msg)
	{
		if (!_animals.ContainsKey(msg.EntityId))
		{
			return false;
		}
		AnimalBehavior animalBehavior = _animals[msg.EntityId];
		if (animalBehavior != null)
		{
			animalBehavior.Disappear();
		}
		else
		{
			_animals.Remove(msg.EntityId);
		}
		return true;
	}

	public void OnAppearAnimal(AnimalBehavior animal)
	{
		if (this.AnimalAppeared != null)
		{
			this.AnimalAppeared(animal);
		}
	}

	public void OnPostAppearAnimal(AppearAnimal msg)
	{
		GameSystem<GatheringSystem>.Instance().UpdateCollectibleDisplay(msg.EntityId, msg.Display.CollectibleDisplay);
	}

	private void OnDisappearAnimal(AnimalBehavior animal)
	{
		if (this.AnimalDisappeared != null)
		{
			this.AnimalDisappeared(animal);
		}
	}

	public void ForceAddAnimal(string id, AnimalBehavior animal)
	{
		_animals.Add(id, animal);
		animal.Destroyed += Animal_Destroyed;
	}

	public void MakeAnimalObject(AppearAnimal msg, Vector3 pos)
	{
		if (_ghosts.ContainsKey(msg.EntityId))
		{
			return;
		}
		PrepareLoad(msg.EntityId);
		string prefabPath = AnimalYaml.GetPrefabPath(msg.EntityType);
		_ghosts[msg.EntityId] = null;
		Singleton<AssetBundleManager>.Instance().RequestAsset(prefabPath, typeof(GameObject), delegate(UnityEngine.Object asset)
		{
			if (!CheckPrepared(msg.EntityId))
			{
				_ghosts.Remove(msg.EntityId);
				UIManager.SystemMsg("Error", "메소드가 준비되지 않았습니다. 잠시 후 다시 시도해주세요.");
			}
			else if (asset == null)
			{
				_ghosts.Remove(msg.EntityId);
				UIManager.SystemMsg("Error", "에셋이 존재하지 않습니다.");
			}
			else
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(rotation: Quaternion.Euler(new Vector3(0f, UnityEngine.Random.Range(0f, 360f), 0f)), original: asset, position: pos) as GameObject;
				if (gameObject == null)
				{
					_ghosts.Remove(msg.EntityId);
					UIManager.SystemMsg("Error", "타겟이 존재하지 않습니다.");
				}
				else
				{
					WildAnimalAI wildAnimalAI = gameObject.AddMissingComponent<WildAnimalAI>();
					wildAnimalAI.Animal = msg;
					wildAnimalAI.SetAiActivated();
					_wildAnimalAI = wildAnimalAI;
					AnimalBehavior component = gameObject.GetComponent<AnimalBehavior>();
					component.EntityId = msg.EntityId;
					component.EntityTypeId = msg.EntityType;
					component.SetAlive(alive: true, fromInit: true);
					component.TurnToYaw(UnityEngine.Random.Range(0f, 360f), bSnap: true);
					component.Level = msg.Level;
					component.SetSurvivalGauge(msg.Survival.Life, null);
					Location location = PathMovable.GetLocation(msg.Move, Connections.Frontend.GetBufferedServerTime());
					component.CurrentPosition = location.Position.ToClientPosition();
					component.Floor.Value = location.Floor;
					component.Destroyed += Animal_Destroyed;
					_appearAnimal = msg;
					_animals[msg.EntityId] = component;
					_ghosts[msg.EntityId] = wildAnimalAI;
					HandleMoveMsg(msg.Move);
					OnAppearAnimal(component);
					OnPostAppearAnimal(msg);
				}
			}
		});
	}

	private void OnPreTouchTarget(InteractionObject obj, ref bool result)
	{
		if (!(obj.EntityId != _appearAnimal.EntityId))
		{
			result = true;
			InteractionMenuList menuList = GameSystem<InteractionSystem>.Instance().MenuList;
			menuList.Name = obj.GetTargetComponent<WildAnimalAI>().TargetAnimal.GetName();
			if (obj.CharacterTarget.IsAlive)
			{
				menuList.Reset();
				menuList.Add(Interaction.Attack);
				GameSystem<InteractionSystem>.Instance().ShowClientMenuList(obj.Target);
			}
			else
			{
				menuList.Reset();
				menuList.Add(Interaction.RemoveAppearAnimal);
				GameSystem<InteractionSystem>.Instance().ShowClientMenuList(obj.Target);
			}
		}
	}

	public void CheckAndMakeDamageToPlayer(AppearAnimal msg)
	{
		Damage damage = default(Damage);
		CharacterBehavior characterBehavior = Singleton<ObjectManager>.Instance().FindCharacter(msg.EntityId);
		if (characterBehavior == null)
		{
			UIManager.SystemMsg("Error", "타겟이 존재하지 않습니다.");
			return;
		}
		WildAnimalAI component = characterBehavior.GetComponent<WildAnimalAI>();
		if (component == null)
		{
			UIManager.SystemMsg("Error", "오류가 발생하였습니다.");
			return;
		}
		float value = UnityEngine.Random.value;
		if (Maths.Make2D(PlayerBehavior.LocalPlayer.transform.position - component.TargetAnimal.transform.position).magnitude < 650f && value <= 0.9f)
		{
			damage.Result = DamageResult.Hit;
		}
		else
		{
			damage.Result = DamageResult.Missed;
		}
		if (component._isBlowing)
		{
			damage.Result = DamageResult.Countered;
		}
		if (damage.Result == DamageResult.Hit)
		{
			if (!GameSystem<CombatSystem>.Instance().CombatMode)
			{
				Connections.Frontend.PushPacket(new BattleBegun
				{
					EntityId = GameManager.PlayerId,
					EnemyId = component.TargetAnimal.EntityId
				});
			}
			if (UnityEngine.Random.value <= 0.2f)
			{
				damage.Effects = DamageEffects.Critical;
				damage.Value = CalcDamageResult(damage);
			}
			else
			{
				damage.Effects = DamageEffects.Blow;
				damage.Value = CalcDamageResult(damage);
			}
		}
		else if (damage.Result == DamageResult.Missed)
		{
			damage.Effects = DamageEffects.None;
			damage.Value = CalcDamageResult(damage);
		}
		else if (damage.Result == DamageResult.Countered)
		{
			damage.Effects = DamageEffects.CrossCounter;
			damage.Value = CalcDamageResult(damage);
		}
		damage.AttackType = AttackType.LargeBody;
		damage.Direction = DamageDirection.Front;
		damage.Part = BodyPart.Body;
		Connections.Frontend.PushPacket(new Damaged
		{
			AttackerId = component.TargetAnimal.EntityId,
			Damage = damage,
			VictimId = GameManager.PlayerId,
			EventAt = Connections.Frontend.GetBufferedServerTime()
		});
	}

	[ExposedInEditor(null)]
	[UsedImplicitly]
	public void MakeAnimal(ushort type, WildAnimalAI.Type aiType)
	{
		WorldPosition position = default(WorldPosition);
		position.SetFromClientPosition(PlayerBehavior.LocalPlayer.CurrentPosition);
		string entityId = Guid.NewGuid().ToString();
		int num = UnityEngine.Random.Range(59, 90);
		float num2 = UnityEngine.Random.Range(100f, 2000f) * (float)num;
		AppearAnimal appearAnimal = default(AppearAnimal);
		appearAnimal.EntityId = entityId;
		appearAnimal.EntityType = type;
		appearAnimal.IsAlive = true;
		appearAnimal.Level = num;
		appearAnimal.Move = new Move
		{
			EntityId = entityId,
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
		appearAnimal.Survival = new Survival
		{
			EntityId = entityId,
			Life = new Gauge(num2, 0f, new GaugeNode[1]
			{
				new GaugeNode
				{
					Time = 0.0,
					Value = num2
				}
			})
		};
		appearAnimal.Display = new AnimalDisplay
		{
			EntityId = entityId,
			BaseScale = 1f
		};
		AppearAnimal appearAnimal2 = appearAnimal;
		WildAnimalAI.CurType = aiType;
		MakeAnimalObject(appearAnimal2, PlayerBehavior.LocalPlayer.CurrentPosition);
		Player.Instance._world._context.WildAnimalList.Add(appearAnimal2);
		Player.Instance._world.BroadCast(appearAnimal2);
		Player.Instance._world.Save();
	}

	private int CalcDamageResult(Damage msg)
	{
		int result = 0;
		float num = UnityEngine.Random.Range(2.5f, 9f);
		if (msg.Result == DamageResult.Hit)
		{
			if (msg.Effects == DamageEffects.Blow)
			{
				result = (int)(num * (float)_wildAnimalAI.Animal.Level);
			}
			else if (msg.Effects == DamageEffects.Critical)
			{
				result = (int)(num * (float)_wildAnimalAI.Animal.Level * 2f);
			}
		}
		else if (msg.Result == DamageResult.Countered)
		{
			result = (int)(num * (float)_wildAnimalAI.Animal.Level * 1.5f);
		}
		else if (msg.Result == DamageResult.Missed)
		{
			result = (int)(num * (float)_wildAnimalAI.Animal.Level * 0.05f);
		}
		return result;
	}
}
