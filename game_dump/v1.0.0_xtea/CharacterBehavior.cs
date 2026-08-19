using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Messages;
using Shared.Animal;
using Shared.Battle;
using Survival;
using TerrainData;
using UnityEngine;

public abstract class CharacterBehavior : MonoBehaviour
{
	public enum SizeLevel
	{
		Small,
		Medium,
		Large
	}

	private static readonly string[] InteractionBoneList = new string[3] { "Interaction_Point", "Root_Motion", "Bip001" };

	protected readonly SurvivalGauges SurvivalGauges = new SurvivalGauges();

	protected GameObject LastAttacker;

	protected Biome CurrentBiome = Biome.Unspecified;

	[SerializeField]
	private float _xRadius;

	[SerializeField]
	private float _yRadius;

	[SerializeField]
	private SizeLevel _size = SizeLevel.Medium;

	[SerializeField]
	private float _bushWhackEffectPeriod = 0.5f;

	[SerializeField]
	private Vector3 _bushWhackEffectOffset = Vector3.up * 120f;

	private bool _survivalGaugeInitialized;

	private ShrubComponent _lastShakingBush;

	private SimpleTimer _bushWhackEffectTimer;

	private Transform _rootMotionTransform;

	private RootMotionMovable _rootMotionMovable;

	private bool _interactionTransformChecked;

	private Transform _interactionTransform;

	[ExposedInEditor(null)]
	public bool DebugPath { get; set; }

	[ExposedInEditor(null)]
	public string DebugPathFilter { get; set; }

	public int Level { get; set; }

	public abstract bool IsAlive { get; }

	public abstract bool IsAimTarget { get; set; }

	public abstract Vector3 CurrentPosition { get; set; }

	public abstract Transform MeshObjectTransform { get; }

	public abstract Transform Bip001Transform { get; }

	public Transform InteractionTransform
	{
		get
		{
			if (!_interactionTransformChecked)
			{
				_interactionTransformChecked = true;
				Transform val = null;
				int i = 0;
				for (int num = InteractionBoneList.Length; i < num; i++)
				{
					val = KUtility.FindTransformByName(((Component)this).gameObject, InteractionBoneList[i]);
					if ((Object)(object)val != (Object)null)
					{
						break;
					}
				}
				_interactionTransform = val;
			}
			return _interactionTransform;
		}
	}

	public virtual Vector3 InteractionPosition
	{
		get
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)InteractionTransform == (Object)null)
			{
				return CurrentPosition + Vector3.up * 150f;
			}
			return InteractionTransform.position;
		}
	}

	public virtual bool IsVisible => true;

	public virtual bool IsAnimPlaying => true;

	public virtual bool IsPlayer => false;

	public virtual TerrainWater.WaterDepthLevel WaterDepthLevel { get; set; }

	public virtual Transform WeaponTipTransform => null;

	public virtual bool IsMoving { get; set; }

	public AnimalStatus Status { get; set; }

	public Gauge Life { get; set; }

	public bool IsLookAtAvailable
	{
		get
		{
			string currentAnimationClipName = GetCurrentAnimationClipName();
			if (string.IsNullOrEmpty(currentAnimationClipName))
			{
				return false;
			}
			return currentAnimationClipName.ContainsIgnoreCase("run") || currentAnimationClipName.ContainsIgnoreCase("walk") || currentAnimationClipName.ContainsIgnoreCase("stand");
		}
	}

	public float XRadius => _xRadius;

	public float YRadius => _yRadius;

	public float CurrentYaw
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			Quaternion rotation = ((Component)this).transform.rotation;
			return ((Quaternion)(ref rotation)).eulerAngles.y;
		}
	}

	public SizeLevel Size => _size;

	public Vector3 CurrentVelocity { get; set; }

	public float WaterDepth { get; protected set; }

	public byte Floor { get; set; }

	protected bool IsBushWhacking { get; set; }

	protected bool IsRoadRunning { get; set; }

	private SimpleTimer BushWhackEffectTimer
	{
		get
		{
			if (_bushWhackEffectTimer == null)
			{
				_bushWhackEffectTimer = new SimpleTimer(_bushWhackEffectPeriod);
				ParticleManager.Cache("Particle/FX_Bush_Whack_01.prefab");
			}
			return _bushWhackEffectTimer;
		}
	}

	public Point2 CurrentTile { get; private set; }

	[ExposedInEditor(false, null)]
	public ulong EntityId { get; set; }

	public int EntityTypeId { get; set; }

	public float ExpireAt { get; set; }

	public Transform RootMotionTransform
	{
		get
		{
			if ((Object)(object)_rootMotionTransform != (Object)null)
			{
				return _rootMotionTransform;
			}
			RootMotionExporter component = ((Component)this).GetComponent<RootMotionExporter>();
			_rootMotionTransform = FindTransformByName((!((Object)(object)component != (Object)null)) ? "Bip001" : component._rootMotionBoneName);
			return _rootMotionTransform;
		}
	}

	public RootMotionMovable RootMotionMovable
	{
		get
		{
			if (_rootMotionMovable != null)
			{
				return _rootMotionMovable;
			}
			_rootMotionMovable = new RootMotionMovable(this, ((Component)this).transform, RootMotionTransform);
			return _rootMotionMovable;
		}
	}

	public abstract ChatableBase ChatableBase { get; }

	public abstract BoneMergeable BoneMergeable { get; }

	public event Action<CharacterBehavior> SurvivalGaugeInitialized;

	public event Action<CharacterBehavior> SurvivalGaugeUpdated;

	public event Action<AnimalBehavior> KilledAnimal;

	public event Action<PlayerBehavior> KilledPlayer;

	public event Action<Damage, GameObject> TakenDamage;

	public event Action<Point2, Point2> TileChanged;

	public abstract void TakeBoneFlinching(BodyPart part);

	public abstract void TurnToYaw(float yaw, bool bSnap);

	public abstract string GetName();

	public abstract string GetAttackNameForDeathMsg();

	public abstract string GetCurrentAnimationClipName();

	public abstract Transform GetHeadTransform();

	public abstract Transform GetBodyPartTransform(BodyPart part, bool bAllowNull = false, [Optional] Vector3 nearPos);

	public virtual void SetSurvivalGauge(Gauge life, Dictionary<string, Gauge> gauges)
	{
		if (gauges.TryGetValue("life", out var value))
		{
			life = value;
		}
		Life = life;
		SurvivalGauges.SetGauges(gauges);
		if (!_survivalGaugeInitialized)
		{
			_survivalGaugeInitialized = true;
			if (this.SurvivalGaugeInitialized != null)
			{
				this.SurvivalGaugeInitialized(this);
			}
		}
		if (this.SurvivalGaugeUpdated != null)
		{
			this.SurvivalGaugeUpdated(this);
		}
	}

	public virtual void OnTakeDamage(Damage damage, GameObject attacker)
	{
		LastAttacker = attacker;
		if (this.TakenDamage != null)
		{
			this.TakenDamage(damage, attacker);
		}
	}

	public virtual void SetWeaponVisible(bool visible)
	{
	}

	protected virtual void MoveMotionChangedByObject()
	{
	}

	public void TransferEvent(CharacterBehavior oldBehavior)
	{
		this.SurvivalGaugeInitialized = (Action<CharacterBehavior>)Delegate.Combine(this.SurvivalGaugeInitialized, oldBehavior.SurvivalGaugeInitialized);
		this.SurvivalGaugeUpdated = (Action<CharacterBehavior>)Delegate.Combine(this.SurvivalGaugeUpdated, oldBehavior.SurvivalGaugeUpdated);
		this.KilledAnimal = (Action<AnimalBehavior>)Delegate.Combine(this.KilledAnimal, oldBehavior.KilledAnimal);
		this.KilledPlayer = (Action<PlayerBehavior>)Delegate.Combine(this.KilledPlayer, oldBehavior.KilledPlayer);
		this.TileChanged = (Action<Point2, Point2>)Delegate.Combine(this.TileChanged, oldBehavior.TileChanged);
		oldBehavior.SurvivalGaugeInitialized = null;
		oldBehavior.SurvivalGaugeUpdated = null;
		oldBehavior.KilledAnimal = null;
		oldBehavior.KilledPlayer = null;
		oldBehavior.TileChanged = null;
	}

	public Gauge GetGauge(string key)
	{
		if (key == "life")
		{
			return Life;
		}
		return SurvivalGauges.GetGauge(key);
	}

	public void PlayDamagerEffectSet(GameObject attacker, Damage damage)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = GetBodyPartTransform(damage.Part).position;
		DamageEffectManager.PlayDamagerEffectSet(attacker, damage, position);
	}

	public GameObject FindObjectByName(string objectName)
	{
		return KUtility.FindObjectByName(((Component)this).gameObject, objectName);
	}

	public Transform FindTransformByName(string transformName)
	{
		return KUtility.FindTransformByName(((Component)this).gameObject, transformName);
	}

	public BodyPart FindNearestBodyPart(Vector3 pos, [Optional] Vector3 dir, bool use2DDistance = false)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		float num = -1f;
		BodyPart result = BodyPart.Body;
		int num2 = 7;
		bool flag = dir != default(Vector3);
		for (int i = 0; i < num2; i++)
		{
			Transform bodyPartTransform = GetBodyPartTransform((BodyPart)i, bAllowNull: true);
			if ((Object)null != (Object)(object)bodyPartTransform)
			{
				Vector3 pos2 = bodyPartTransform.position - pos;
				if (use2DDistance)
				{
					pos2 = KMathUtil.Make2D(pos2);
				}
				float num3 = ((Vector3)(ref pos2)).magnitude;
				float num4 = Vector3.Dot(((Vector3)(ref pos2)).normalized, dir);
				if (flag)
				{
					num3 /= (num4 + 1f) * 0.5f + Mathf.Epsilon;
				}
				if (num < 0f || num3 < num)
				{
					num = num3;
					result = (BodyPart)i;
				}
			}
		}
		return result;
	}

	protected void ProcessMotionStateAffectedByObject()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		if (GameManager.IsPrologueMode)
		{
			return;
		}
		Vector3 worldPosition = TerrainA6.ClientPositionToWorldPosition(CurrentPosition);
		ImmovableBase moveAffectingObject = TerrainA6.GetMoveAffectingObject(worldPosition);
		bool isBushWhacking = IsBushWhacking;
		bool isRoadRunning = IsRoadRunning;
		IsBushWhacking = TerrainA6.IsBushWhackableSize(moveAffectingObject);
		bool flag = TerrainA6.IsShakable(moveAffectingObject);
		IsRoadRunning = TerrainA6.IsRoad(moveAffectingObject);
		NaturalObject naturalObject = moveAffectingObject as NaturalObject;
		ShrubComponent shrubComponent = ((!((Object)(object)naturalObject != (Object)null)) ? null : (naturalObject.NaturalComponent as ShrubComponent));
		if (shrubComponent != null)
		{
			Vector3 currentVelocity = CurrentVelocity;
			bool flag2 = ((Vector3)(ref currentVelocity)).sqrMagnitude > 0f;
			if (flag)
			{
				shrubComponent.Shake(flag2);
			}
			if (flag2 && BushWhackEffectTimer.CheckTime())
			{
				ParticleManager.Emit("Particle/FX_Bush_Whack_01.prefab", CurrentPosition + _bushWhackEffectOffset, Quaternion.identity);
			}
		}
		if (_lastShakingBush != shrubComponent)
		{
			if (_lastShakingBush != null)
			{
				_lastShakingBush.Shake(shake: false);
			}
			_lastShakingBush = shrubComponent;
		}
		if (isBushWhacking != IsBushWhacking || isRoadRunning != IsRoadRunning)
		{
			MoveMotionChangedByObject();
		}
	}

	public void CheckCurrentTile(bool forceUpdate = false)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		Point2 point = new Point2(TerrainA6.ClientPositionToTilePosition(CurrentPosition));
		if (forceUpdate || !(point == CurrentTile))
		{
			Point2 currentTile = CurrentTile;
			CurrentTile = point;
			OnTileChanged(currentTile, CurrentTile);
			if (this.TileChanged != null)
			{
				this.TileChanged(currentTile, CurrentTile);
			}
		}
	}

	protected virtual void OnTileChanged(Point2 prev, Point2 current)
	{
		CurrentBiome = Biome.Unspecified;
	}

	public Biome GetBiome()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (CurrentBiome == Biome.Unspecified)
		{
			CurrentBiome = TerrainA6.GetTileBiome(TerrainA6.ClientPositionToWorldPosition(CurrentPosition));
		}
		return CurrentBiome;
	}

	public void OnKilledAnimal(AnimalBehavior victim)
	{
		if (this.KilledAnimal != null)
		{
			this.KilledAnimal(victim);
		}
	}

	public void OnKilledPlayer(PlayerBehavior victim)
	{
		if (this.KilledPlayer != null)
		{
			this.KilledPlayer(victim);
		}
	}

	protected virtual void ProcessDepth()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		Vector3 currentPosition = CurrentPosition;
		Vector2 floatTile = TerrainA6.ClientPositionToTilePosition(currentPosition);
		byte floor = Floor;
		WaterDepth = TerrainA6.GetTileDepth(floatTile, ref floor);
		WaterDepthLevel = TerrainWater.GetWaterDepthLevel(WaterDepth);
		float worldHeight = TerrainWater.GetWorldHeight(WaterDepth);
		currentPosition.y = worldHeight;
		CurrentPosition = currentPosition;
	}

	public Gauge GetLife()
	{
		return Life;
	}
}
