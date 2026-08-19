using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Building;
using Durango.Model;
using Durango.Network;
using Durango.Render.Particle;
using Durango.Utils;
using L10N;
using Messages;
using Shared.Animal;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class ObjectManager : Durango.Utils.Singleton<ObjectManager>
{
	[CompilerGenerated]
	private sealed class _003CApplySurvivalGauge_003Ed__7 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float timeDelay;

		public CharacterBehavior character;

		public Gauge life;

		public Dictionary<string, Gauge> gauges;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CApplySurvivalGauge_003Ed__7(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = new WaitForSeconds(timeDelay);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				if (character != null)
				{
					character.SetSurvivalGauge(life, gauges);
				}
				return false;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CApplySurvivalGaugeUpdated_003Ed__8 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float timeDelay;

		public CharacterBehavior character;

		public SurvivalUpdated msg;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CApplySurvivalGaugeUpdated_003Ed__8(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = new WaitForSeconds(timeDelay);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				if (character != null)
				{
					character.UpdateSurvivalGauges(msg);
				}
				return false;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CCoDelayAnimalStatusMsg_003Ed__6 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float delay;

		public AnimalBehavior animal;

		public AnimalStatus status;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCoDelayAnimalStatusMsg_003Ed__6(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = new WaitForSeconds(delay);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				animal.Status = status;
				return false;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	private AnimalManager _animalMgr;

	private PlayerManager _playerMgr;

	private PetManager _petMgr;

	protected override void OnAwake()
	{
		_animalMgr = Durango.Utils.Singleton<AnimalManager>.Instance();
		_playerMgr = Durango.Utils.Singleton<PlayerManager>.Instance();
		_petMgr = Durango.Utils.Singleton<PetManager>.Instance();
	}

	private void Start()
	{
		Connections.Frontend.On(delegate(Move msg, PacketHeader header)
		{
			if (!_playerMgr.HandleMoveMsg(msg) && !_animalMgr.HandleMoveMsg(msg))
			{
				_petMgr.HandleMoveMsg(msg);
			}
		});
		Connections.Frontend.On(delegate(DisappearEntity msg, PacketHeader header)
		{
			if (!_playerMgr.HandleDisappearMsg(msg) && !_animalMgr.HandleDisappearMsg(msg))
			{
				Durango.Utils.Singleton<ArtifactManager>.Instance().HandleDisappearMsg(msg);
			}
		});
		Connections.Frontend.On(delegate(DisappearEntities msg, PacketHeader header)
		{
			Durango.Utils.Singleton<ArtifactManager>.Instance().HandleDisappearEntitiesMsg(msg);
		});
		Connections.Frontend.On(delegate(Survival msg, PacketHeader header)
		{
			if (Application.isEditor && msg.EntityId == PlayerBehavior.LocalPlayer.EntityId)
			{
				PlayerBehavior player2 = _playerMgr.GetPlayer(msg.EntityId);
				if (player2 != null)
				{
					player2.SetSurvivalGauge(msg.Life, msg.Gauges);
				}
			}
			float timeDelay2 = 0f;
			if (header.Time > Connections.Frontend.GetBufferedServerTime())
			{
				timeDelay2 = (float)(header.Time - Connections.Frontend.GetBufferedServerTime());
			}
			CharacterBehavior characterBehavior2 = FindCharacter(msg.EntityId);
			if (characterBehavior2 != null)
			{
				StartCoroutine(ApplySurvivalGauge(characterBehavior2, msg.Life, msg.Gauges, timeDelay2));
			}
		});
		Connections.Frontend.On(delegate(SurvivalUpdated msg, PacketHeader header)
		{
			if (Application.isEditor && msg.EntityId == PlayerBehavior.LocalPlayer.EntityId)
			{
				PlayerBehavior player = _playerMgr.GetPlayer(msg.EntityId);
				if (player != null)
				{
					player.UpdateSurvivalGauges(msg);
				}
			}
			float timeDelay = 0f;
			if (header.Time > Connections.Frontend.GetBufferedServerTime())
			{
				timeDelay = (float)(header.Time - Connections.Frontend.GetBufferedServerTime());
			}
			CharacterBehavior characterBehavior = FindCharacter(msg.EntityId);
			if (characterBehavior != null)
			{
				StartCoroutine(ApplySurvivalGaugeUpdated(characterBehavior, msg, timeDelay));
			}
		});
		Connections.Frontend.On<BattleLog>(delegate
		{
		});
		Connections.Frontend.On<LivingLog>(delegate
		{
		});
		Connections.Frontend.On(delegate(CombatInteraction msg, PacketHeader header)
		{
			if (msg.Details.ContainsKey("look_at"))
			{
				GameObject gameObject2 = FindObject(msg.EntityId);
				if ((bool)gameObject2)
				{
					BoneLookAtTarget component2 = gameObject2.GetComponent<BoneLookAtTarget>();
					if (component2 != null)
					{
						GameObject target = null;
						if (!string.IsNullOrEmpty(msg.TargetId))
						{
							target = FindObject(msg.TargetId);
						}
						component2.SetLookTarget(target, findHead: true);
					}
				}
			}
			msg.Details.ContainsKey("joined");
			msg.Details.ContainsKey("disjoined");
			msg.Details.ContainsKey("hold");
			if (msg.Details.ContainsKey("status"))
			{
				AnimalStatus status = ((!string.IsNullOrEmpty(msg.TargetId) && !(msg.TargetId == PlayerBehavior.LocalPlayer.EntityId)) ? AnimalStatus.Invalid : ((AnimalStatus)msg.Details["status"]));
				AnimalBehavior animal = Durango.Utils.Singleton<AnimalManager>.Instance().GetAnimal(msg.EntityId);
				if (animal != null)
				{
					float delay = (float)(header.Time - Connections.Frontend.GetBufferedServerTime());
					StartCoroutine(CoDelayAnimalStatusMsg(delay, animal, status));
				}
			}
			if (msg.Details.ContainsKey("notice_attack"))
			{
				AnimalBehavior animal2 = Durango.Utils.Singleton<AnimalManager>.Instance().GetAnimal(msg.EntityId);
				if (animal2 != null)
				{
					animal2.AttackNotice((double)msg.Details["notice_attack"] / 1000.0);
				}
			}
		});
		Connections.Frontend.RegisterRelayHandler(delegate(ParticleEffect msg, float timePassed)
		{
			GameObject gameObject = FindObject(msg.EntityId);
			if (!(gameObject == null))
			{
				PlayerBehavior component = gameObject.GetComponent<PlayerBehavior>();
				if (!(component != null) || component.GetVisible())
				{
					ParticleManager.Emit(gameObject, msg.Path, msg.Bone, msg.Follow);
				}
			}
		});
		Connections.Frontend.On(delegate(EntityRevived msg, PacketHeader header)
		{
			SetEntityAlive(msg.At, msg.EntityId, isAlive: true);
		});
		Connections.Frontend.On(delegate(EntityDied msg, PacketHeader header)
		{
			SetEntityAlive(msg.At, msg.EntityId, isAlive: false);
		});
	}

	private void SetEntityAlive(double at, string entityId, bool isAlive)
	{
		double num = at - Connections.Frontend.GetBufferedServerTime();
		if (PlayerBehavior.LocalPlayer.EntityId == entityId)
		{
			num = at - Connections.Frontend.GetPredictedServerTime();
		}
		KUtility.DelayedCall(this, delegate
		{
			CharacterBehavior characterBehavior = FindCharacter(entityId);
			if (characterBehavior != null)
			{
				characterBehavior.SetAlive(isAlive);
			}
			else
			{
				Artifact artifact = FindArtifact(entityId);
				if (artifact != null && !isAlive)
				{
					artifact.SetDestroyed();
				}
			}
		}, (float)num);
	}

	private static IEnumerator CoDelayAnimalStatusMsg(float delay, AnimalBehavior animal, AnimalStatus status)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoDelayAnimalStatusMsg_003Ed__6(0)
		{
			delay = delay,
			animal = animal,
			status = status
		};
	}

	private IEnumerator ApplySurvivalGauge(CharacterBehavior character, Gauge life, Dictionary<string, Gauge> gauges, float timeDelay)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CApplySurvivalGauge_003Ed__7(0)
		{
			character = character,
			life = life,
			gauges = gauges,
			timeDelay = timeDelay
		};
	}

	private IEnumerator ApplySurvivalGaugeUpdated(CharacterBehavior character, SurvivalUpdated msg, float timeDelay)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CApplySurvivalGaugeUpdated_003Ed__8(0)
		{
			character = character,
			msg = msg,
			timeDelay = timeDelay
		};
	}

	public GameObject FindObject(string id)
	{
		if (id == null)
		{
			return null;
		}
		CharacterBehavior characterBehavior = FindCharacter(id);
		if (characterBehavior != null)
		{
			return characterBehavior.gameObject;
		}
		Artifact artifact = FindArtifact(id);
		if (artifact != null)
		{
			return artifact.gameObject;
		}
		return null;
	}

	public VehicleBase FindVehicle(string id)
	{
		Artifact artifact = FindArtifact(id);
		if (artifact != null)
		{
			VehicleBase componentInChildren = artifact.GetComponentInChildren<VehicleBase>();
			if (componentInChildren != null)
			{
				return componentInChildren;
			}
		}
		PetAI petObject = _petMgr.GetPetObject(id);
		if (petObject != null)
		{
			return petObject.GetComponent<VehicleBase>();
		}
		return null;
	}

	public CharacterBehavior FindCharacter(string id)
	{
		CharacterBehavior playerIncludeLocalPlayer = _playerMgr.GetPlayerIncludeLocalPlayer(id);
		if (playerIncludeLocalPlayer != null)
		{
			return playerIncludeLocalPlayer;
		}
		playerIncludeLocalPlayer = _animalMgr.GetAnimal(id);
		if (playerIncludeLocalPlayer != null)
		{
			return playerIncludeLocalPlayer;
		}
		PetAI petObject = _petMgr.GetPetObject(id);
		if (petObject != null)
		{
			return petObject.TargetAnimal;
		}
		return null;
	}

	public Artifact FindArtifact(string id)
	{
		return Durango.Utils.Singleton<ArtifactManager>.Instance().Find(id);
	}

	public static float GetBoundRadius(int entityTypeId)
	{
		if (1000 <= entityTypeId && entityTypeId <= 2000)
		{
			return Yaml.Util.Singleton<PlayerEntities>.Instance.player.bound_radius;
		}
		if (2000 <= entityTypeId && entityTypeId <= 3000 && SingletonDict<int, Animal>.Instance.TryGetValue(entityTypeId, out var value))
		{
			return value.BoundRadius;
		}
		if (6000 <= entityTypeId && entityTypeId <= 10000)
		{
			Building.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().RecipeContainer.GetBlueprint(entityTypeId);
			if (blueprint != null)
			{
				return blueprint.BoundRadius;
			}
		}
		return 1f;
	}

	public static void PlayParticle(string entityId, string effect, string bone = null, bool follow = false)
	{
		ParticleEffect msg = default(ParticleEffect);
		msg.EntityId = entityId;
		msg.Path = effect;
		msg.Bone = bone;
		msg.Follow = follow;
		Connections.Frontend.Send(msg);
	}

	public static string GetTestTitleText()
	{
		return T._("Testing Functions");
	}
}
