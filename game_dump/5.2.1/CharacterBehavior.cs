using System;
using System.Collections.Generic;
using Durango.Environment;
using Durango.Model;
using Durango.Network;
using Durango.Render;
using Durango.Render.Effect;
using Durango.Render.Particle;
using Durango.Terrain;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using Messages;
using Shared.Battle;
using Shared.Region;
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

	[SerializeField]
	private IntegratedEffectType _hitEffect;

	private Dictionary<string, Gauge> _survivalGauges;

	private ShrubComponent _lastShakingBush;

	private SimpleTimer _bushWhackEffectTimer;

	private Transform _rootMotionTransform;

	private RootMotionMovable _rootMotionMovable;

	private bool _interactionTransformChecked;

	private Transform _interactionTransform;

	private string _currentAnimClipName;

	private ChatableBase _chatable;

	protected DamageableEntity LastAttacker;

	protected Biome CurrentBiome = Biome.Invalid;

	private readonly Observable<TerrainWater.WaterDepthLevel> _waterDepthLevel = new Observable<TerrainWater.WaterDepthLevel>(default(TerrainWater.WaterDepthLevelComparer));

	private readonly Observable<byte> _floor = new Observable<byte>();

	protected SkinnedMeshRenderer[] Renderers;

	protected Outline Outline;

	protected PlaneShadows Shadows;

	protected AmbientLighting AmbientLighting;

	private readonly Observable<bool> _isMoving = new Observable<bool>();

	public int Level { get; set; }

	public string Role { get; set; }

	public bool IsAlive { get; private set; }

	public abstract Animation Anim { get; }

	public abstract Vector3 CurrentPosition { get; set; }

	public abstract Transform MeshObjectTransform { get; }

	public abstract Transform Bip001Transform { get; }

	[CanBeNull]
	public Transform InteractionTransform
	{
		get
		{
			if (!_interactionTransformChecked)
			{
				_interactionTransformChecked = true;
				Transform transform = null;
				int i = 0;
				for (int num = InteractionBoneList.Length; i < num; i++)
				{
					transform = KUtility.FindTransformByName(base.gameObject, InteractionBoneList[i]);
					if (transform != null)
					{
						break;
					}
				}
				_interactionTransform = transform;
			}
			return _interactionTransform;
		}
	}

	public virtual Vector3 InteractionPosition
	{
		get
		{
			if (InteractionTransform == null)
			{
				return CurrentPosition + Vector3.up * 150f;
			}
			return InteractionTransform.position;
		}
	}

	public virtual bool WillBeRendered => true;

	public bool PlaneShadowEnabled
	{
		set
		{
			if (Shadows != null)
			{
				Shadows.SetVisible(value, VisibleObject.Mask.Enabled);
			}
		}
	}

	public virtual bool IsAnimPlaying => true;

	public Observable<TerrainWater.WaterDepthLevel> WaterDepthLevel => _waterDepthLevel;

	[CanBeNull]
	public virtual Transform WeaponTipTransform => null;

	public Observable<bool> IsMoving => _isMoving;

	[CanBeNull]
	public Gauge Life { get; set; }

	public string CurrentAnimClipName
	{
		get
		{
			return _currentAnimClipName;
		}
		protected set
		{
			_currentAnimClipName = value;
			CurrentAnimKeyName = value;
			if (string.IsNullOrEmpty(value))
			{
				IsLookAtMotion = false;
				return;
			}
			if (value.StartsWith("M_") || value.StartsWith("F_"))
			{
				CurrentAnimKeyName = value.Substring(2, value.Length - 2);
			}
			IsLookAtMotion = value.ContainsIgnoreCase("run") || value.ContainsIgnoreCase("walk") || value.ContainsIgnoreCase("stand");
		}
	}

	public string CurrentAnimKeyName { get; private set; }

	public bool IsLookAtMotion { get; private set; }

	public float XRadius => _xRadius;

	public float YRadius => _yRadius;

	public float CurrentYaw => base.transform.rotation.eulerAngles.y;

	public SizeLevel Size => _size;

	public Vector3 CurrentVelocity { get; set; }

	public float WaterDepth { get; protected set; }

	public Observable<byte> Floor => _floor;

	public bool IsBushWhacking { get; set; }

	public bool IsRoadRunning { get; set; }

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
	public string EntityId { get; set; }

	public int EntityTypeId { get; set; }

	public Transform RootMotionTransform
	{
		get
		{
			if (_rootMotionTransform != null)
			{
				return _rootMotionTransform;
			}
			RootMotionExporter component = GetComponent<RootMotionExporter>();
			_rootMotionTransform = FindTransformByName((!(component != null)) ? "Bip001" : component._rootMotionBoneName);
			return _rootMotionTransform;
		}
	}

	[NotNull]
	public RootMotionMovable RootMotionMovable
	{
		get
		{
			if (_rootMotionMovable != null)
			{
				return _rootMotionMovable;
			}
			_rootMotionMovable = new RootMotionMovable(this);
			return _rootMotionMovable;
		}
	}

	[NotNull]
	public ChatableBase ChatableBase
	{
		get
		{
			if (_chatable == null)
			{
				_chatable = CreateChatableBase();
			}
			return _chatable;
		}
	}

	[NotNull]
	public abstract BoneMergeable BoneMergeable { get; }

	public event Action<CharacterBehavior> SurvivalGaugeInitialized;

	public event Action<CharacterBehavior> SurvivalGaugeUpdated;

	public event Action<CharacterBehavior> Revived;

	public event Action<CharacterBehavior, bool> Died;

	public event Action<AnimalBehavior> KilledAnimal;

	public event Action<PlayerBehavior> KilledPlayer;

	public event Action<Damage, DamageableEntity> TakenDamage;

	public event Action<Point2, Point2> TileChanged;

	protected virtual ChatableBase CreateChatableBase()
	{
		return new ChatableCharacter<CharacterBehavior>(this);
	}

	public abstract void TakeBoneFlinching(BodyPart part);

	public abstract void TurnToYaw(float yaw, bool bSnap);

	public abstract string GetName();

	[CanBeNull]
	public virtual float[] GetLifeGaugeRatio()
	{
		return null;
	}

	public abstract Transform GetBodyPartTransform(BodyPart part, bool bAllowNull = false, Vector3 nearPos = default(Vector3));

	protected void Awake()
	{
		IsAlive = true;
		Outline = GetComponent<Outline>();
		Shadows = GetComponent<PlaneShadows>();
		AmbientLighting = GetComponent<AmbientLighting>();
		Renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
	}

	protected void Start()
	{
		if (AmbientLighting != null)
		{
			AmbientLighting.SetupMaterials(Renderers);
		}
	}

	public void SetAlive(bool alive, bool fromInit = false)
	{
		if (IsAlive != alive)
		{
			IsAlive = alive;
			if (IsAlive)
			{
				OnRevive();
			}
			else
			{
				OnDie(fromInit);
			}
		}
	}

	public virtual void SetSurvivalGauge(Gauge life, [CanBeNull] Dictionary<string, Gauge> gauges)
	{
		bool num = _survivalGauges == null;
		if (gauges != null)
		{
			if (gauges.TryGetValue("life", out var value))
			{
				life = value;
			}
			_survivalGauges = gauges;
		}
		Life = life;
		if (num && this.SurvivalGaugeInitialized != null)
		{
			this.SurvivalGaugeInitialized(this);
		}
		if (this.SurvivalGaugeUpdated != null)
		{
			this.SurvivalGaugeUpdated(this);
		}
	}

	public virtual void UpdateSurvivalGauges(SurvivalUpdated msg)
	{
		if (_survivalGauges == null)
		{
			return;
		}
		for (int i = 0; i < msg.Removed.Length; i++)
		{
			_survivalGauges.Remove(msg.Removed[i]);
		}
		if (msg.Updated == null)
		{
			return;
		}
		foreach (KeyValuePair<string, Gauge> item in msg.Updated)
		{
			if (item.Key == "life")
			{
				Life = item.Value;
			}
			else
			{
				_survivalGauges[item.Key] = item.Value;
			}
		}
		if (this.SurvivalGaugeUpdated != null)
		{
			this.SurvivalGaugeUpdated(this);
		}
	}

	public virtual void OnTakeDamage(Damage damage, [CanBeNull] DamageableEntity attacker)
	{
		Vector3 position = GetBodyPartTransform(damage.Part).position;
		if (string.IsNullOrEmpty(_hitEffect))
		{
			DamageEffectManager.PlayDamageEffectSet(attacker, damage, position);
		}
		else
		{
			IntegratedEffect.Emit(_hitEffect, Biome.Invalid, position, Quaternion.identity);
		}
		LastAttacker = attacker;
		if (this.TakenDamage != null)
		{
			this.TakenDamage(damage, attacker);
		}
	}

	public void TransferEvent(CharacterBehavior oldBehavior)
	{
		SurvivalGaugeInitialized += oldBehavior.SurvivalGaugeInitialized;
		SurvivalGaugeUpdated += oldBehavior.SurvivalGaugeUpdated;
		Revived += oldBehavior.Revived;
		Died += oldBehavior.Died;
		KilledAnimal += oldBehavior.KilledAnimal;
		KilledPlayer += oldBehavior.KilledPlayer;
		TakenDamage += oldBehavior.TakenDamage;
		TileChanged += oldBehavior.TileChanged;
		Observable<bool> isMoving = IsMoving;
		isMoving.Changed = (Action<bool>)Delegate.Combine(isMoving.Changed, oldBehavior.IsMoving.Changed);
		Observable<TerrainWater.WaterDepthLevel> waterDepthLevel = WaterDepthLevel;
		waterDepthLevel.Changed = (Action<TerrainWater.WaterDepthLevel>)Delegate.Combine(waterDepthLevel.Changed, oldBehavior.WaterDepthLevel.Changed);
		Observable<byte> floor = Floor;
		floor.Changed = (Action<byte>)Delegate.Combine(floor.Changed, oldBehavior.Floor.Changed);
		oldBehavior.SurvivalGaugeInitialized = null;
		oldBehavior.SurvivalGaugeUpdated = null;
		oldBehavior.Died = null;
		oldBehavior.Revived = null;
		oldBehavior.KilledAnimal = null;
		oldBehavior.KilledPlayer = null;
		oldBehavior.TakenDamage = null;
		oldBehavior.TileChanged = null;
		oldBehavior.WaterDepthLevel.Changed = null;
		oldBehavior.IsMoving.Changed = null;
		oldBehavior.Floor.Changed = null;
	}

	[CanBeNull]
	public Gauge GetGauge(string key)
	{
		if (key == "life")
		{
			return Life;
		}
		if (_survivalGauges == null || string.IsNullOrEmpty(key))
		{
			return null;
		}
		return _survivalGauges.Get(key);
	}

	[CanBeNull]
	public Transform FindTransformByName(string transformName)
	{
		return KUtility.FindTransformByName(base.gameObject, transformName);
	}

	public BodyPart FindNearestBodyPart(Vector3 pos, Vector3 dir = default(Vector3), bool use2DDistance = false)
	{
		float num = -1f;
		BodyPart result = BodyPart.Body;
		int num2 = 7;
		bool flag = dir != default(Vector3);
		for (int i = 0; i < num2; i++)
		{
			Transform bodyPartTransform = GetBodyPartTransform((BodyPart)i, bAllowNull: true);
			if (null != bodyPartTransform)
			{
				Vector3 pos2 = bodyPartTransform.position - pos;
				if (use2DDistance)
				{
					pos2 = Maths.Make2D(pos2);
				}
				float num3 = pos2.magnitude;
				float num4 = Vector3.Dot(pos2.normalized, dir);
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

	protected virtual void ProcessAffectNearObject()
	{
		if (GameManager.IsPrologueMode || !WillBeRendered)
		{
			return;
		}
		Vector3 currentPosition = CurrentPosition;
		Vector3 worldPosition = Util.ClientPositionToWorldPosition(currentPosition);
		ImmovableBase moveAffectingObject = Singleton<TerrainBase>.Instance().GetMoveAffectingObject(worldPosition);
		IsBushWhacking = Singleton<TerrainBase>.Instance().IsBushWhackableSize(moveAffectingObject);
		bool flag = Singleton<TerrainBase>.Instance().IsShakable(moveAffectingObject);
		IsRoadRunning = Singleton<TerrainBase>.Instance().IsRoad(moveAffectingObject);
		NaturalSpriteObject naturalSpriteObject = moveAffectingObject as NaturalSpriteObject;
		ShrubComponent shrubComponent = ((!(naturalSpriteObject != null)) ? null : (naturalSpriteObject.NaturalComponent as ShrubComponent));
		if (shrubComponent != null)
		{
			bool flag2 = CurrentVelocity.sqrMagnitude > 0f;
			if (flag)
			{
				shrubComponent.Shake(flag2);
			}
			if (flag2 && BushWhackEffectTimer.CheckTime())
			{
				ParticleManager.Emit("Particle/FX_Bush_Whack_01.prefab", currentPosition + _bushWhackEffectOffset, Quaternion.identity);
			}
		}
		if (!(_lastShakingBush == shrubComponent))
		{
			if (_lastShakingBush != null)
			{
				_lastShakingBush.Shake(shake: false);
			}
			_lastShakingBush = shrubComponent;
		}
	}

	public void CheckCurrentTile(bool forceUpdate = false)
	{
		Point2 point = new Point2(Util.ClientPositionToTilePosition(CurrentPosition));
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
		CurrentBiome = Biome.Invalid;
	}

	public Biome GetBiome()
	{
		if (CurrentBiome == Biome.Invalid)
		{
			CurrentBiome = Singleton<TerrainBase>.Instance().GetTileBiome(Util.ClientPositionToWorldPosition(CurrentPosition));
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

	public virtual float ProcessWaterDepth(Vector3 pos)
	{
		if ((byte)Floor == 0)
		{
			Vector2 floatTile = Util.ClientPositionToTilePosition(pos);
			WaterDepth = Singleton<TerrainBase>.Instance().GetTileDepth(floatTile);
		}
		else
		{
			WaterDepth = 0f;
		}
		TerrainWater.WaterDepthLevel waterDepthLevel = TerrainWater.GetWaterDepthLevel(WaterDepth);
		if (GetBiome() == Biome.Lava && waterDepthLevel > TerrainWater.WaterDepthLevel.Waist)
		{
			waterDepthLevel = TerrainWater.WaterDepthLevel.Waist;
		}
		WaterDepthLevel.Value = waterDepthLevel;
		return TerrainWater.GetWorldHeight(WaterDepth);
	}

	protected virtual void OnRevive()
	{
		if (this.Revived != null)
		{
			this.Revived(this);
		}
	}

	protected virtual void OnDie(bool fromInit)
	{
		if (this.Died != null)
		{
			this.Died(this, fromInit);
		}
	}

	public virtual void Select(bool selected, Color outlineColor = default(Color), float outlineWidth = 0f)
	{
		if (!(Outline == null))
		{
			if (selected && outlineColor.a > 0f && outlineWidth > 0f)
			{
				Outline.SetVisible(visible: true);
				Outline.SetColor(outlineColor);
				Outline.SetWidth(outlineWidth);
			}
			else
			{
				Outline.SetVisible(visible: false);
			}
		}
	}

	public virtual double GetMoveServerTime()
	{
		if (GameManager.IsPrologueMode)
		{
			return Time.time;
		}
		return Connections.Frontend.GetBufferedServerTime();
	}

	public Vector3 GetSidePos(bool left, float mult = 1f)
	{
		float yawDeg = CurrentYaw + ((!left) ? 90f : (-90f));
		return CurrentPosition + Maths.CalcDirectionFromYaw(yawDeg).normalized * XRadius * mult;
	}
}
