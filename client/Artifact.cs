using System;
using System.Collections.Generic;
using Building;
using Durango.Logic.Estate;
using Durango.Logic.Item;
using Durango.Logic.Timer;
using Durango.Model;
using Durango.Network;
using Durango.Render.Effect;
using Durango.Render.Particle;
using Durango.Terrain;
using Durango.UI;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using Messages;
using Shared.Battle;
using Shared.Building;
using Shared.Estate;
using Shared.Etc;
using Shared.Player;
using Shared.Region;
using UnityEngine;
using Yaml;
using Yaml.Util;

public sealed class Artifact : ImmovableBase
{
	public enum Interaction
	{
		Normal,
		TouchAndContext,
		Context,
		Disable
	}

	private const string BuildingCrashEffect = "IntegratedEffects/FX_Int_Building_Crash.prefab";

	private double _postprocessSince;

	private double _postprocessUntil;

	private Interaction _interactionType;

	private bool _hasGroudSite;

	private ArtifactSiteDecoration _siteDeco;

	private GameObject _groundSiteObject;

	private bool _hasScaffolding;

	private GameObject _scaffoldingObject;

	private int _height;

	private List<ArtifactComponent> _components;

	private string _consiteAssetPath = "Models/Prop/system/site/site_01_1x1.prefab";

	private string _scaffoldingAssetPah = "Models/Prop/system/scaffolding/scaffolding_01_1x1.prefab";

	private int _attachedCount;

	private AnimatingModel _animationProp;

	private readonly AnimationSequence _animationSequence = new AnimationSequence();

	private readonly List<Pair<string, double>> _prevAnimations = new List<Pair<string, double>>();

	private float? _animationSequenceDirtyAt;

	private DamagedEffect[] _damagedEffects;

	private Artifact[] _interiors;

	private BoxCollider _heightCollider;

	private string _musicName;

	private bool _musicOcclusion;

	private uint _musicInstanceId;

	private int _musicParticleEffectId;

	private int? _floor;

	private readonly Observable<int?> _stories = new Observable<int?>();

	private readonly Observable<bool?> _hasRoof = new Observable<bool?>();

	public override float InteractionDistance => 100f * ((float)Mathf.Max(Size.x, Size.y) + 0.5f);

	public ModelComponent Models { get; private set; }

	[ExposedInEditor(false, null)]
	public string BlueprintId { get; private set; }

	public string FounderId { get; set; }

	public ArtifactState ArtifactState { get; private set; }

	public ArtifactDisplay Display { get; private set; }

	public Shared.Building.Condition Condition { get; private set; }

	[CanBeNull]
	public Gauge Durability => ArtifactState.Durability;

	[ExposedInEditor(null)]
	public BuildingState BuildState => ArtifactState.BuildingState;

	public Interaction InteractionType
	{
		get
		{
			if (BuildCompleted)
			{
				return _interactionType;
			}
			return Interaction.Normal;
		}
		set
		{
			if (_interactionType < value)
			{
				_interactionType = value;
			}
		}
	}

	public string ContextIcon { get; set; }

	[ExposedInEditor(null)]
	public bool BuildCompleted => BuildState == BuildingState.Completed;

	public int Height
	{
		get
		{
			int num = _height;
			int i = 0;
			for (int size = KUtility.GetSize(_components); i < size; i++)
			{
				num = Mathf.Max(num, _components[i].Height);
			}
			return num;
		}
		set
		{
			_height = value;
		}
	}

	public override int? Floor => _floor;

	public Observable<int?> Stories => _stories;

	public Observable<bool?> HasRoof => _hasRoof;

	public override Vector3 InteractionPosition
	{
		get
		{
			Vector3 interactionPosition = base.InteractionPosition;
			int i = 0;
			for (int size = KUtility.GetSize(_components); i < size; i++)
			{
				interactionPosition += _components[i].InteractionPositionOffset;
			}
			return interactionPosition;
		}
	}

	public string ConsiteAssetPath
	{
		get
		{
			return _consiteAssetPath;
		}
		set
		{
			_consiteAssetPath = value;
		}
	}

	public string ScaffoldingAssetPath
	{
		get
		{
			return _scaffoldingAssetPah;
		}
		set
		{
			_scaffoldingAssetPah = value;
		}
	}

	public Building.Blueprint Blueprint { get; private set; }

	public List<TagData> Tags { get; private set; }

	public string LocalizedName => (ArtifactState.ChangedName != null) ? ArtifactState.ChangedName : ((Blueprint != null) ? Blueprint.Name : string.Empty);

	public Durango.Logic.Timer.Timer PostProcessTimer { get; private set; }

	public Durango.Logic.Timer.Timer ArtifactTimer { get; private set; }

	public Color Color { get; private set; }

	public int ParticleEffectId { get; private set; }

	public float MaxHealth => ArtifactState.MaxHealth;

	public Point2 Size { get; private set; }

	public Rotation Rotation { get; private set; }

	public Vector2 CenterTile
	{
		get
		{
			Vector2 vector = base.WorldTile.ToVector2();
			return vector + Size.ToVector2() * 0.5f;
		}
	}

	public bool IsEnterable => GetArtifactComponent<EnterableArtifact>() != null;

	public override Vector3 Center => Durango.Terrain.Util.TilePositionToClientPosition(CenterTile) + Vector3.up * Floor.GetValueOrDefault() * 200f;

	public static event Action<Artifact> ArtifactStateChanged;

	public static event Action<Artifact> ArtifactDisplayChanged;

	public event System.Action ResourceLoadCompleted;

	public event Action<Artifact> DurabilityGaugeUpdated;

	static Artifact()
	{
		GameManager.Reset += delegate
		{
			Artifact.ArtifactStateChanged = null;
			Artifact.ArtifactDisplayChanged = null;
		};
	}

	private void Start()
	{
		IntegratedEffect.Precache("IntegratedEffects/FX_Int_Building_Crash.prefab");
	}

	private void OnDestroy()
	{
		ClearEffect();
		StopMusic();
	}

	private void Update()
	{
		int i = 0;
		for (int size = KUtility.GetSize(_components); i < size; i++)
		{
			_components[i].Update();
		}
		if (_animationSequenceDirtyAt.HasValue && _animationSequenceDirtyAt.Value < Time.time)
		{
			RefreshAnimationSequence(Display.Animations);
		}
		_animationSequence.Update();
	}

	public void Init([NotNull] Building.Blueprint blueprint, Point2 worldTile, Rotation rotation, Point2 size, int? floor, int? stories, bool? hasRoof, int height)
	{
		int i = 0;
		for (int size2 = KUtility.GetSize(_components); i < size2; i++)
		{
			_components[i].PreInit(blueprint.Id, worldTile, rotation, size);
		}
		BlueprintId = blueprint.Id;
		Rotation = rotation;
		Tags = new List<TagData>();
		ArtifactState = new ArtifactState
		{
			BuildingState = BuildingState.Invalid
		};
		Models = new ModelComponent(base.gameObject, base.EntityId.GetHashCode());
		Models.LoadCompleted += Models_LoadCompleted;
		Models.Unloaded += Models_Unloaded;
		SetBlueprint(blueprint);
		if (size.x > 0 && size.y > 0)
		{
			Size = size;
		}
		Height = height;
		_floor = floor;
		SetStories(stories, hasRoof);
		Vector3 localPosition = Durango.Terrain.Util.TilePositionToClientPosition(worldTile);
		if (Floor.HasValue)
		{
			localPosition.y = Floor.Value * 200;
		}
		base.transform.localRotation = Quaternion.identity;
		base.transform.localPosition = localPosition;
		UpdateCollider();
		if (!blueprint.Permanent && !blueprint.TransparentSite)
		{
			MakeGroundSite();
		}
		int j = 0;
		for (int size3 = KUtility.GetSize(_components); j < size3; j++)
		{
			_components[j].PostInit(blueprint.Id, worldTile, rotation, size);
		}
	}

	public void SetStories(int? stories, bool? hasRoof)
	{
		_hasRoof.Value = hasRoof;
		if (_stories.Value == stories)
		{
			return;
		}
		int? value = _stories.Value;
		_stories.Value = stories;
		int num = Stories.Value.GetValueOrDefault() * Size.x * Size.y;
		int size = KUtility.GetSize(_interiors);
		if (size == num)
		{
			return;
		}
		Artifact[] array = new Artifact[num];
		if (_interiors != null)
		{
			Array.Copy(_interiors, array, Mathf.Min(size, num));
			for (int i = num; i < size; i++)
			{
				if (!(_interiors[i] == null))
				{
					Durango.Utils.Singleton<ArtifactManager>.Instance().Remove(_interiors[i]);
				}
			}
		}
		_interiors = array;
		if (value.HasValue)
		{
			BuildSystem.PlayBuildFinished(Center);
		}
	}

	public void AddArtifactComponent(ArtifactComponent component)
	{
		if (_components == null)
		{
			_components = new List<ArtifactComponent>();
		}
		_components.Add(component);
		component.SetParent(this);
	}

	public TC GetArtifactComponent<TC>() where TC : ArtifactComponent
	{
		int i = 0;
		for (int size = KUtility.GetSize(_components); i < size; i++)
		{
			if (_components[i] is TC result)
			{
				return result;
			}
		}
		return (TC)null;
	}

	public override string GetName()
	{
		string text = null;
		int i = 0;
		for (int size = KUtility.GetSize(_components); i < size; i++)
		{
			text = _components[i].GetName();
			if (text != null)
			{
				break;
			}
		}
		return (text != null) ? text : LocalizedName;
	}

	protected override void SetColor(Color color)
	{
		Models.SetColor(color);
	}

	protected override Color GetDefaultColor()
	{
		return Color;
	}

	private void SetBlueprint([NotNull] Building.Blueprint blueprint)
	{
		Blueprint = blueprint;
		Size = Blueprint.Size;
	}

	private void RefreshArtifactLook()
	{
		Color artifactColor = Color.white;
		switch (BuildState)
		{
		case BuildingState.Built:
		case BuildingState.Remodeling:
			artifactColor.a = ((PostProcessTimer != null) ? 0.4f : 1f);
			break;
		case BuildingState.Completed:
			artifactColor = GetConditionColor(Condition);
			SetDamagedMaterial();
			break;
		}
		int i = 0;
		for (int size = KUtility.GetSize(_components); i < size; i++)
		{
			Color color = _components[i].GetColor();
			artifactColor *= color;
		}
		SetArtifactColor(artifactColor);
	}

	private void SetArtifactColor(Color color)
	{
		Color = color;
		SetColor(color);
	}

	private static Color GetConditionColor(Shared.Building.Condition condition)
	{
		switch (condition)
		{
		case Shared.Building.Condition.Normal:
		case Shared.Building.Condition.Old:
		case Shared.Building.Condition.Worn:
			return Color.white;
		case Shared.Building.Condition.Broken:
			return new Color(0.7f, 0.7f, 0.7f);
		default:
			return Color.white;
		}
	}

	private void SetDamagedMaterial()
	{
		if (Durability != null)
		{
			float num = Durability.Ratio();
			float num2 = Yaml.Util.Singleton<Constants>.Instance.Build.ConditionoScales.Get(2, 0f);
			float damaged = Mathf.Clamp01(1f - num / num2);
			Models.SetDamaged(damaged);
		}
	}

	public TagData GetTag(string id)
	{
		int i = 0;
		for (int size = KUtility.GetSize(Tags); i < size; i++)
		{
			if (Tags[i].Id == id)
			{
				return Tags[i];
			}
		}
		return null;
	}

	public EstateInfo GetEstateInfo()
	{
		Point2 worldTile = base.WorldTile;
		Point2 size = Size;
		EstateInfo estateInfo = null;
		for (int i = 0; i < size.x; i++)
		{
			for (int j = 0; j < size.y; j++)
			{
				EstateInfo estateInfo2 = EstateSystem.GetEstateInfo(worldTile + new Point2(i, j));
				if (estateInfo2 == null)
				{
					return null;
				}
				if (estateInfo != null && estateInfo != estateInfo2)
				{
					return null;
				}
				estateInfo = estateInfo2;
			}
		}
		return estateInfo;
	}

	public override void Select(bool selected)
	{
		if (_siteDeco != null)
		{
			_siteDeco.Visible(!selected);
		}
		bool flag = false;
		int i = 0;
		for (int size = KUtility.GetSize(_components); i < size; i++)
		{
			flag |= _components[i].OnSelectArtifact(selected);
		}
		if (!flag)
		{
			base.Select(selected);
		}
	}

	private void OnModelChanged()
	{
		SetDirtyInteractionTransform();
		_animationProp = GetComponentInChildren<AnimatingModel>();
		_damagedEffects = GetComponentsInChildren<DamagedEffect>();
		_animationSequenceDirtyAt = 0f;
		_prevAnimations.Clear();
	}

	private void Models_LoadCompleted(bool noError)
	{
		OnModelChanged();
		int i = 0;
		for (int size = KUtility.GetSize(_components); i < size; i++)
		{
			_components[i].ResourcesLoadCompleted();
		}
		if (this.ResourceLoadCompleted != null)
		{
			this.ResourceLoadCompleted();
		}
	}

	private void Models_Unloaded()
	{
		OnModelChanged();
	}

	public void SetTagList(Messages.Tag[] tags)
	{
		Tags.Clear();
		for (int i = 0; i < tags.Length; i++)
		{
			Messages.Tag tag = tags[i];
			TagData tagData = TagData.Create(tag.Id, tag.Level);
			if (tagData != null)
			{
				Tags.Add(tagData);
			}
		}
	}

	private void UpdateCollider()
	{
		int height = Height;
		if (Floor.HasValue && height > 1 && GetArtifactComponent<Stairs>() == null)
		{
			if (_heightCollider == null)
			{
				_heightCollider = base.gameObject.AddChild<BoxCollider>();
			}
			Transform transform = _heightCollider.transform;
			transform.localPosition = Vector3.up * 200f;
			Vector3 vector = new Vector3(Size.x, height - 1, Size.y) * 200f;
			_heightCollider.size = vector;
			_heightCollider.center = vector * 0.5f;
		}
		int i = 0;
		for (int size = KUtility.GetSize(_components); i < size; i++)
		{
			_components[i].OnUpdateCollider();
		}
	}

	public void CreateCollider(Vector3 size = default(Vector3), Vector3 center = default(Vector3))
	{
		BoxCollider boxCollider = base.gameObject.AddMissingComponent<BoxCollider>();
		boxCollider.isTrigger = true;
		Vector3 vector = new Vector3(Size.x, Height, Size.y) * 200f;
		if (size != default(Vector3))
		{
			vector = size;
		}
		boxCollider.center = ((!(center != default(Vector3))) ? (vector * 0.5f) : center);
		boxCollider.size = vector;
	}

	public ArtifactSiteDecoration GetSiteDecoration()
	{
		return _siteDeco;
	}

	private void MakeSiteDecoration()
	{
		if (!(_siteDeco != null))
		{
			DecorationGroup decorationGroup = UIManager.FindScript<DecorationGroup>();
			if (!(decorationGroup == null))
			{
				GameObject gameObject = decorationGroup.Register(base.transform, Durango.Utils.Singleton<ArtifactManager>.Instance().GetSiteIconPrefab(), new DecorationGroup.Option
				{
					WorldOffset = new Vector3(Size.x, 0f, Size.y) * 0.5f * 200f
				});
				_siteDeco = gameObject.GetComponent<ArtifactSiteDecoration>();
				_siteDeco.Set(this);
			}
		}
	}

	private void RemoveSiteDecoration()
	{
		if (!(_siteDeco == null))
		{
			DecorationGroup decorationGroup = UIManager.FindScript<DecorationGroup>();
			if (!(decorationGroup == null))
			{
				decorationGroup.Stop(_siteDeco.gameObject);
				_siteDeco = null;
			}
		}
	}

	private void MakeGroundSite()
	{
		if (!_hasGroudSite)
		{
			_hasGroudSite = true;
			string consiteAssetPath = ConsiteAssetPath;
			Durango.Utils.Singleton<AssetBundleManager>.Instance().RequestAsset(consiteAssetPath, typeof(GameObject), GroundSiteObjectLoaded);
		}
	}

	private void GroundSiteObjectLoaded(UnityEngine.Object asset)
	{
		if (this == null || asset == null || !_hasGroudSite)
		{
			return;
		}
		GameObject[] array = new GameObject[Size.y * Size.x];
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			array[i] = UnityEngine.Object.Instantiate(asset as GameObject);
		}
		_groundSiteObject = new GameObject("site");
		Transform transform = _groundSiteObject.transform;
		transform.parent = base.transform;
		transform.localPosition = new Vector3(0.5f, 0f, 0.5f) * 200f;
		Point2 size = Size;
		for (int j = 0; j < size.y; j++)
		{
			for (int k = 0; k < size.x; k++)
			{
				Transform transform2 = array[j * size.x + k].transform;
				transform2.parent = transform;
				transform2.localPosition = new Vector3(k, 0f, j) * 200f;
			}
		}
	}

	private void RemoveGroundSite()
	{
		_hasGroudSite = false;
		if (_groundSiteObject != null)
		{
			_groundSiteObject.transform.parent = null;
			UnityEngine.Object.Destroy(_groundSiteObject);
		}
		_groundSiteObject = null;
	}

	private void MakeScaffolding()
	{
		if (!_hasScaffolding)
		{
			_hasScaffolding = true;
			string scaffoldingAssetPath = ScaffoldingAssetPath;
			Durango.Utils.Singleton<AssetBundleManager>.Instance().RequestAsset(scaffoldingAssetPath, typeof(GameObject), ScaffoldingObjectLoaded);
		}
	}

	private void ScaffoldingObjectLoaded(UnityEngine.Object asset)
	{
		if (this == null || asset == null || !_hasScaffolding)
		{
			return;
		}
		GameObject[] array = new GameObject[Size.y * Size.x];
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			array[i] = UnityEngine.Object.Instantiate(asset as GameObject);
		}
		_scaffoldingObject = new GameObject("scaffolding");
		Transform transform = _scaffoldingObject.transform;
		transform.parent = base.transform;
		transform.localPosition = new Vector3(0.5f, 0f, 0.5f) * 200f;
		Point2 size = Size;
		System.Random random = new System.Random(base.EntityId.GetHashCode());
		for (int j = 0; j < size.y; j++)
		{
			for (int k = 0; k < size.x; k++)
			{
				Transform transform2 = array[j * size.x + k].transform;
				transform2.parent = transform;
				transform2.localPosition = new Vector3(k, 0f, j) * 200f;
				transform2.localRotation = Quaternion.Euler(0f, 90 * random.Next(4), 0f);
			}
		}
	}

	private void RemoveScaffolding()
	{
		_hasScaffolding = false;
		if (_scaffoldingObject != null)
		{
			_scaffoldingObject.transform.parent = null;
			UnityEngine.Object.Destroy(_scaffoldingObject);
		}
		_scaffoldingObject = null;
	}

	public void SetArtifactState(ArtifactState state, double eventTime)
	{
		ArtifactState artifactState = ArtifactState;
		ArtifactState = state;
		PostprocessTimeUpdate();
		bool flag = RepairTimerUpdate();
		bool flag2 = CrackTimerUpdate();
		if (!flag && !flag2 && ArtifactTimer != null)
		{
			ArtifactTimer.Stop();
		}
		if (artifactState.BuildingState != BuildState)
		{
			OnUpdateBuildState();
		}
		OnUpdateState(eventTime);
		if (Artifact.ArtifactStateChanged != null)
		{
			Artifact.ArtifactStateChanged(this);
		}
		CheckDurabilityChanged(artifactState.Durability, eventTime);
	}

	public void SetDestroyed()
	{
		IntegratedEffect.Emit("IntegratedEffects/FX_Int_Building_Crash.prefab", Biome.Invalid, Center, Quaternion.identity);
		int i = 0;
		for (int size = KUtility.GetSize(_damagedEffects); i < size; i++)
		{
			_damagedEffects[i].PlayDestoryed();
		}
	}

	private void CheckDurabilityChanged(Gauge prevDurability, double eventTime)
	{
		GaugeNode[] array = prevDurability?.Determination;
		GaugeNode[] array2 = ((ArtifactState.Durability != null) ? ArtifactState.Durability.Determination : null);
		int size = KUtility.GetSize(array);
		int size2 = KUtility.GetSize(array2);
		bool flag = false;
		if (size == size2)
		{
			for (int i = 0; i < size2; i++)
			{
				GaugeNode gaugeNode = array[i];
				GaugeNode gaugeNode2 = array2[i];
				if (gaugeNode.Time != gaugeNode2.Time || gaugeNode.Value != gaugeNode2.Value)
				{
					flag = true;
					break;
				}
			}
		}
		else
		{
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		double bufferedServerTime = Connections.Frontend.GetBufferedServerTime();
		float delay = (float)(eventTime - bufferedServerTime);
		KUtility.DelayedCall(this, delegate
		{
			if (this.DurabilityGaugeUpdated != null)
			{
				this.DurabilityGaugeUpdated(this);
			}
		}, delay);
	}

	private void OnUpdateState(double eventTime)
	{
		bool flag = false;
		int i = 0;
		for (int size = KUtility.GetSize(_components); i < size; i++)
		{
			flag |= _components[i].OnUpdateState(eventTime);
		}
		if (!flag)
		{
			RefreshArtifactLook();
		}
	}

	private void OnUpdateBuildState()
	{
		switch (BuildState)
		{
		case BuildingState.Occupied:
			MakeGroundSite();
			if (FounderId == GameManager.PlayerId)
			{
				MakeSiteDecoration();
			}
			break;
		case BuildingState.Built:
		case BuildingState.Remodeling:
			if (Blueprint.Permanent || Blueprint.TransparentSite)
			{
				RemoveGroundSite();
			}
			RemoveSiteDecoration();
			MakeScaffolding();
			break;
		case BuildingState.Completed:
			if (Blueprint.Permanent || Blueprint.TransparentSite)
			{
				RemoveGroundSite();
			}
			RemoveSiteDecoration();
			RemoveScaffolding();
			OnCompleted();
			break;
		}
		int i = 0;
		for (int size = KUtility.GetSize(_components); i < size; i++)
		{
			_components[i].OnUpdateBuildState();
		}
	}

	private void OnCompleted()
	{
		int i = 0;
		for (int size = KUtility.GetSize(_components); i < size; i++)
		{
			_components[i].OnCompleted();
		}
	}

	public void OnRemoved()
	{
		if (IsLocalPlayerInside())
		{
			OnPlayerExit();
		}
		if (_interiors != null)
		{
			Artifact[] interiors = _interiors;
			foreach (Artifact artifact in interiors)
			{
				if (!(artifact == null))
				{
					Durango.Utils.Singleton<ArtifactManager>.Instance().Remove(artifact);
				}
			}
			_interiors = null;
		}
		int j = 0;
		for (int size = KUtility.GetSize(_components); j < size; j++)
		{
			_components[j].OnRemoved();
		}
		TileObject tileObject = Durango.Utils.Singleton<TerrainBase>.Instance().GetTileObject(base.WorldTile, warning: false);
		if (tileObject != null && tileObject.Artifact != null && tileObject.Artifact != this)
		{
			tileObject.Artifact.OnChangeInterior();
		}
	}

	public void SetInterior(Point2 pos, int floor, int height, Artifact artifact)
	{
		int num = pos.x + pos.y * Size.x + (floor + height) * Size.x * Size.y;
		if (_interiors == null || num < 0 || num >= _interiors.Length)
		{
			return;
		}
		Artifact artifact2 = _interiors[num];
		_interiors[num] = artifact;
		if (height == 0)
		{
			if (artifact != null)
			{
				artifact.OnAttached();
			}
			if (artifact2 != null)
			{
				artifact2.OnDetached();
			}
		}
		OnChangeInterior();
	}

	private void OnChangeInterior()
	{
		int i = 0;
		for (int size = KUtility.GetSize(_components); i < size; i++)
		{
			_components[i].OnChangeInterior();
		}
	}

	public Artifact[] GetInteriors()
	{
		return _interiors;
	}

	public Artifact GetInterior(Point2 pos, int floor)
	{
		int num = pos.x + pos.y * Size.x + floor * Size.x * Size.y;
		if (_interiors == null || num < 0 || num >= _interiors.Length)
		{
			return null;
		}
		return _interiors[num];
	}

	public bool HasInterior()
	{
		int i = 0;
		for (int size = KUtility.GetSize(_interiors); i < size; i++)
		{
			if (_interiors[i] != null)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAvailableInterior()
	{
		return _interiors != null;
	}

	public bool IsOccupiablePos(Point2 pos)
	{
		if (!IsAvailableInterior())
		{
			return false;
		}
		if (Blueprint.UnoccupiableTiles == null)
		{
			return true;
		}
		Point2 item = pos;
		switch (Rotation)
		{
		case Rotation.Quarter:
			item.x = pos.y;
			item.y = Size.x - pos.x;
			break;
		case Rotation.Half:
			item.x = Size.x - pos.x;
			item.y = Size.y - pos.y;
			break;
		case Rotation.ThreeQuarter:
			item.x = Size.y - pos.y;
			item.y = pos.x;
			break;
		}
		return !Blueprint.UnoccupiableTiles.Contains(item);
	}

	private bool IsLocalPlayerInside()
	{
		PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
		if (localPlayer == null)
		{
			return false;
		}
		Point2 currentTile = localPlayer.CurrentTile;
		Point2 size = Size;
		return currentTile.x >= base.WorldTile.x && currentTile.y >= base.WorldTile.y && currentTile.x < base.WorldTile.x + size.x && currentTile.y < base.WorldTile.y + size.y;
	}

	private bool RepairTimerUpdate()
	{
		if (ArtifactState.TryGetRepairInfo(out var duration, out var ratio))
		{
			UpdateArtifactTimer("repairment", duration, ratio);
			return true;
		}
		return false;
	}

	private bool CrackTimerUpdate()
	{
		if (!ArtifactState.Crack.HasValue || !ArtifactState.Crack.Value.ActivatedSince.HasValue || !ArtifactState.Crack.Value.ActivatedUntil.HasValue)
		{
			return false;
		}
		double num = ArtifactState.Crack.Value.ActivatedSince.Value;
		double value = ArtifactState.Crack.Value.ActivatedUntil.Value;
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		if (num <= 0.0)
		{
			num = predictedServerTime;
		}
		if (value <= predictedServerTime || num >= value)
		{
			return false;
		}
		float num2 = (float)(value - num);
		float ratio = (float)(predictedServerTime - num) / num2;
		UpdateArtifactTimer("crack_activated", num2, ratio, isCraterTimer: true);
		return true;
	}

	private void UpdateArtifactTimer(string subject, float duration, float ratio, bool isCraterTimer = false)
	{
		if (ArtifactTimer == null || ArtifactTimer.IsStop)
		{
			ArtifactTimer = new Durango.Logic.Timer.Timer(base.EntityId, subject, duration, ratio);
			ArtifactTimer.Finished += ArtifactTimer_Finished;
			if (isCraterTimer)
			{
				CraterProgressGauge craterProgressGauge = Durango.Logic.Timer.Timer.Play<CraterProgressGauge>(ArtifactTimer);
				craterProgressGauge.SetTarget(base.gameObject, new Vector3(Size.x, 0.5f, Size.y) * 200f);
			}
			else
			{
				TimerProgressGauge timerProgressGauge = Durango.Logic.Timer.Timer.Play<TimerProgressGauge>(ArtifactTimer);
				timerProgressGauge.SetTarget(base.gameObject, new Vector3(Size.x, 1f, Size.y) * 200f * 0.5f);
			}
		}
		else
		{
			ArtifactTimer.SetDuration(base.EntityId, subject, duration, ratio);
		}
	}

	private void ArtifactTimer_Finished(Durango.Logic.Timer.Timer timer)
	{
		RefreshArtifactLook();
		ArtifactTimer = null;
	}

	private void PostprocessTimeUpdate()
	{
		_postprocessSince = 0.0;
		_postprocessUntil = 0.0;
		if (!ArtifactState.Postprocess.HasValue || (BuildState != BuildingState.Built && BuildState != BuildingState.Remodeling))
		{
			if (PostProcessTimer != null)
			{
				PostProcessTimer.Stop();
			}
			return;
		}
		Postprocess value = ArtifactState.Postprocess.Value;
		_postprocessSince = value.StartedAt;
		_postprocessUntil = value.EndsAt;
		double num = _postprocessSince;
		double postprocessUntil = _postprocessUntil;
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		if (num <= 0.0)
		{
			num = predictedServerTime;
		}
		if (postprocessUntil <= predictedServerTime || num >= postprocessUntil)
		{
			if (PostProcessTimer != null)
			{
				PostProcessTimer.Stop();
			}
			return;
		}
		float num2 = (float)(postprocessUntil - num);
		float ratio = (float)(predictedServerTime - num) / num2;
		if (PostProcessTimer == null || PostProcessTimer.IsStop)
		{
			PostProcessTimer = new Durango.Logic.Timer.Timer(base.EntityId, "postprocess", num2, ratio);
			PostProcessTimer.Finished += PostProcessTimer_Finished;
			TimerProgressGauge timerProgressGauge = Durango.Logic.Timer.Timer.Play<TimerProgressGauge>(PostProcessTimer);
			timerProgressGauge.SetTarget(base.gameObject, new Vector3(Size.x, (float)Height * 2f, Size.y) * 200f * 0.5f);
		}
		else
		{
			PostProcessTimer.SetDuration(base.EntityId, "postprocess", num2, ratio);
		}
	}

	private void PostProcessTimer_Finished(Durango.Logic.Timer.Timer timer)
	{
		RefreshArtifactLook();
		PostProcessTimer = null;
	}

	public void OnPlayerEnter()
	{
		int i = 0;
		for (int size = KUtility.GetSize(_components); i < size; i++)
		{
			_components[i].OnPlayerEnter();
		}
	}

	public void OnPlayerExit()
	{
		int i = 0;
		for (int size = KUtility.GetSize(_components); i < size; i++)
		{
			_components[i].OnPlayerExit();
		}
	}

	public void OnPlayerFloorChange()
	{
		int i = 0;
		for (int size = KUtility.GetSize(_components); i < size; i++)
		{
			_components[i].OnPlayerFloorChange();
		}
	}

	public void UpdateDisplay(ArtifactDisplay msg)
	{
		OnUpdateDisplay(msg);
		Condition = msg.Condition;
		RefreshArtifactLook();
		if (Artifact.ArtifactDisplayChanged != null)
		{
			Artifact.ArtifactDisplayChanged(this);
		}
	}

	private void OnUpdateDisplay(ArtifactDisplay msg)
	{
		Display = msg;
		_animationSequenceDirtyAt = 0f;
		bool flag = false;
		int i = 0;
		for (int size = KUtility.GetSize(_components); i < size; i++)
		{
			flag |= _components[i].OnUpdateDisplay(msg);
		}
		if (!flag)
		{
			Vector3 offset = new Vector3(Size.x, 0f, Size.y) * 200f * 0.5f;
			FillModels(Models, msg, offset, Rotation);
			UpdateEffect(msg.Effect);
			UpdateMusic(msg.Music);
		}
	}

	private void RefreshAnimationSequence(IList<Pair<string, double>> sequence)
	{
		_animationSequenceDirtyAt = null;
		if (KUtility.GetSize(sequence) == 0 || _animationProp == null)
		{
			_prevAnimations.Clear();
			_animationSequence.Reset();
			return;
		}
		int num = 0;
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		for (int num2 = sequence.Count - 1; num2 >= 0; num2--)
		{
			double item = sequence[num2].Item2;
			if (0.0 < item && item < predictedServerTime)
			{
				num = num2;
				break;
			}
		}
		bool flag = true;
		for (int i = num; i < sequence.Count; i++)
		{
			Pair<string, double> pair = sequence[i];
			int num3 = i - num;
			if (num3 < 0 || _prevAnimations.Count <= num3)
			{
				flag = false;
			}
			if (flag)
			{
				Pair<string, double> pair2 = _prevAnimations[num3];
				if (pair2.Item1 != pair.Item1 || pair2.Item2 != pair.Item2)
				{
					flag = false;
				}
			}
			if (predictedServerTime < pair.Item2)
			{
				if (flag && num3 != _prevAnimations.Count)
				{
					flag = false;
				}
				break;
			}
		}
		if (flag)
		{
			return;
		}
		List<AnimationSequenceClip> list = new List<AnimationSequenceClip>();
		_prevAnimations.Clear();
		for (int j = num; j < sequence.Count; j++)
		{
			Pair<string, double> item2 = sequence[j];
			if (predictedServerTime < item2.Item2)
			{
				_animationSequenceDirtyAt = Times.UnixTimeToUnityTime(item2.Item2);
				break;
			}
			_prevAnimations.Add(item2);
			list.Add(new AnimationSequenceClip(item2.Item1));
		}
		_animationSequence.Set(_animationProp, list);
	}

	public static void FillModels(ModelComponent models, ArtifactDisplay msg, Vector3 offset, Rotation rotation)
	{
		models.BeginLoad();
		Direction direction = ArtifactUtil.RotationToDirection(rotation);
		if (msg.Parts != null)
		{
			foreach (KeyValuePair<string, string> part in msg.Parts)
			{
				if (!string.IsNullOrEmpty(part.Value))
				{
					UpdatePart(models, null, part.Key, part.Value, offset, direction);
				}
			}
		}
		if (msg.Decorations != null)
		{
			foreach (KeyValuePair<string, Pair<string, string>> decoration in msg.Decorations)
			{
				string item = decoration.Value.Item1;
				if (!string.IsNullOrEmpty(item))
				{
					ModelComponent.IModel model = UpdatePart(models, "Decoration", decoration.Key, item, offset, direction);
					if (!string.IsNullOrEmpty(decoration.Value.Item2))
					{
						Color color = decoration.Value.Item2.ToColor();
						model.SetColor(color);
					}
				}
			}
		}
		if (msg.Textures != null)
		{
			foreach (KeyValuePair<string, string> texture in msg.Textures)
			{
				models.GetModel(texture.Key)?.SetPatternTex(texture.Value);
			}
		}
		models.EndLoad();
	}

	private static ModelComponent.IModel UpdatePart(ModelComponent models, string category, string key, string modelKey, Vector3 offset, Direction direction)
	{
		Vector3 angle = ArtifactUtil.DirectionToAngle(direction);
		return models.Load(key, modelKey, null, category).SetPosition(offset).SetAngle(angle);
	}

	private void UpdateEffect(string effectKey)
	{
		ClearEffect();
		if (!string.IsNullOrEmpty(effectKey))
		{
			ArtifactEffect artifactEffect = SingletonDict<string, ArtifactEffect>.Get(effectKey);
			if (artifactEffect != null)
			{
				string assetPath = $"{artifactEffect.path}/{artifactEffect.file_name}.prefab";
				ParticleEffectId = ParticleManager.Emit(assetPath, Center, Quaternion.identity);
			}
		}
	}

	private void UpdateMusic(Pair<string, double>? music)
	{
		string text = ((!music.HasValue) ? string.Empty : music.Value.Item1);
		if (!string.IsNullOrEmpty(text))
		{
			PlayMusic(text);
		}
		else
		{
			StopMusic();
		}
	}

	private void PlayMusic(string musicName)
	{
		if (_musicName != musicName)
		{
			StopMusic();
			_musicName = musicName;
			_musicOcclusion = GetMusicOcclusionState();
			_musicInstanceId = SoundManager.PlayEvent(_musicName, SoundPosition.Chase(base.gameObject), GetMusicSoundSwitch(_musicOcclusion), exclusive: true);
			_musicParticleEffectId = ParticleManager.Emit("Particle/Prefabs/Durango_01/02_Common/FX_Music_01.prefab", Center, Quaternion.identity);
		}
	}

	private void StopMusic()
	{
		_musicName = string.Empty;
		if (_musicInstanceId != 0)
		{
			SoundManager.StopEvent(_musicInstanceId, 1f);
			_musicInstanceId = 0u;
		}
		if (_musicParticleEffectId != 0)
		{
			ParticleManager.Stop(_musicParticleEffectId);
			_musicParticleEffectId = 0;
		}
	}

	private void RefreshMusicOcclusion()
	{
		if (_musicInstanceId != 0)
		{
			RefreshMusicSwitch(GetMusicOcclusionState());
		}
	}

	private bool GetMusicOcclusionState()
	{
		bool result = false;
		TileObject tileObject = Durango.Utils.Singleton<TerrainBase>.Instance().GetTileObject(base.WorldTile, warning: false);
		if (tileObject != null && tileObject.Artifact != null && tileObject.Artifact.EntityId != base.EntityId)
		{
			EnterableArtifact artifactComponent = tileObject.Artifact.GetArtifactComponent<EnterableArtifact>();
			if (artifactComponent != null)
			{
				result = !artifactComponent.LocalPlayerIsInHere;
			}
		}
		return result;
	}

	private static SoundSwitch GetMusicSoundSwitch(bool occlusion)
	{
		return SoundSwitch.Set("house", (!occlusion) ? "inside" : "outside");
	}

	public void RefreshMusicSwitch(bool occlusion)
	{
		if (_musicInstanceId != 0 && _musicOcclusion != occlusion)
		{
			_musicOcclusion = occlusion;
			SoundManager.SetSwitch(_musicInstanceId, GetMusicSoundSwitch(_musicOcclusion));
		}
	}

	private void ClearEffect()
	{
		if (ParticleEffectId != 0)
		{
			ParticleManager.Stop(ParticleEffectId);
			ParticleEffectId = 0;
		}
	}

	public void ArtifactPlaced()
	{
		int i = 0;
		for (int size = KUtility.GetSize(_components); i < size; i++)
		{
			_components[i].ArtifactPlaced();
		}
		if (IsLocalPlayerInside())
		{
			OnPlayerEnter();
		}
	}

	public bool IsIgnoreWaterDepth()
	{
		int i = 0;
		for (int size = KUtility.GetSize(_components); i < size; i++)
		{
			if (_components[i].IsIgnoreWaterDepth())
			{
				return true;
			}
		}
		return false;
	}

	public void OnTakeDamage(Damage damage, [CanBeNull] DamageableEntity attacker)
	{
		damage.AttackType = AttackType.Demolition;
		DamageEffectManager.PlayDamageEffectSet(attacker, damage, Center);
		int i = 0;
		for (int size = KUtility.GetSize(_damagedEffects); i < size; i++)
		{
			_damagedEffects[i].PlayDamaged(damage, attacker);
		}
	}

	public void OnAttached()
	{
		_attachedCount++;
		RefreshMusicOcclusion();
	}

	public void OnDetached()
	{
		_attachedCount--;
	}

	public bool IsFullyAttached()
	{
		return _attachedCount >= Size.x * Size.y;
	}

	public void SetEnemy(string enemy)
	{
		SearchLight componentInChildren = GetComponentInChildren<SearchLight>();
		if (componentInChildren != null)
		{
			componentInChildren.SetEnemy(enemy);
		}
	}

	public PropKey GetPropKey()
	{
		PropKey result = default(PropKey);
		result.EntityId = base.EntityId;
		result.Tile = base.WorldTile;
		return result;
	}

	public void CheckPermissionForMe([NotNull] Action<bool> onCheck)
	{
		EstateInfo estateInfo = GetEstateInfo();
		if (estateInfo == null)
		{
			onCheck(obj: false);
			return;
		}
		if (EstateSystem.IsAdmin(estateInfo))
		{
			onCheck(obj: true);
			return;
		}
		OwnerType type2 = estateInfo.License.Type;
		if ((type2 == OwnerType.PersonalPlayer || type2 == OwnerType.Player) && GameSystem<SocialSystem>.Instance().IsFriend(estateInfo.License.OwnerId))
		{
			SocialSystem.GetMyFriendType(estateInfo.License.OwnerId, delegate(Messages.FriendType type)
			{
				Shared.Player.FriendType friendType = type._FriendType;
				onCheck(CheckEstatePermission(estateInfo, friendType) || CheckArtifactPermission(estateInfo, friendType));
			});
		}
		else
		{
			onCheck(CheckEstatePermission(estateInfo, Shared.Player.FriendType.Invalid) || CheckArtifactPermission(estateInfo, Shared.Player.FriendType.Invalid));
		}
	}

	public bool CheckEstatePermission(EstateInfo locatedEstate, Shared.Player.FriendType friendTypeForMe)
	{
		if (locatedEstate == null)
		{
			return false;
		}
		OwnerType type = locatedEstate.License.Type;
		if ((type == OwnerType.System || EstateSystem.IsAdmin(locatedEstate)) && IsInclusiveOfAccessRights(Blueprint.Components))
		{
			return true;
		}
		Messages.AccessRights? accessRights = locatedEstate.License.AccessRights;
		if (!accessRights.HasValue)
		{
			return false;
		}
		Messages.AccessRights value = accessRights.Value;
		Shared.Estate.AccessRights intuitiveAccessRights = GetIntuitiveAccessRights();
		switch (type)
		{
		case OwnerType.Player:
		case OwnerType.PersonalPlayer:
		{
			if (friendTypeForMe == Shared.Player.FriendType.Invalid)
			{
				return HasEnoughtAccessRights(value.ForOthers, intuitiveAccessRights);
			}
			Shared.Estate.AccessRights combinedFlag2 = ((value.ForFriends != null) ? value.ForFriends.Get(friendTypeForMe, Shared.Estate.AccessRights.None) : Shared.Estate.AccessRights.None);
			return HasEnoughtAccessRights(combinedFlag2, intuitiveAccessRights);
		}
		case OwnerType.ClanEstate:
		{
			if (ClanSystem.IsMyClan(locatedEstate.License.OwnerId))
			{
				return HasEnoughtAccessRights(value.ForOthers, intuitiveAccessRights);
			}
			int roleId = PlayerBehavior.LocalPlayer.Clan.RoleId;
			Shared.Estate.AccessRights combinedFlag = value.ForClanMembers.Get(roleId, Shared.Estate.AccessRights.None);
			return HasEnoughtAccessRights(combinedFlag, intuitiveAccessRights);
		}
		default:
			return false;
		}
	}

	public bool CheckArtifactPermission(EstateInfo locatedEstate, Shared.Player.FriendType friendTypeForMe)
	{
		if (locatedEstate == null || !ArtifactState.Access.HasValue)
		{
			return false;
		}
		ArtifactAccess value = ArtifactState.Access.Value;
		switch (locatedEstate.License.Type)
		{
		case OwnerType.Player:
		case OwnerType.PersonalPlayer:
			if (friendTypeForMe == Shared.Player.FriendType.Invalid)
			{
				return value.Others;
			}
			return value.Friends != null && value.Friends.Get(friendTypeForMe, defaultValue: false);
		case OwnerType.ClanEstate:
		{
			if (!ClanSystem.IsMyClan(locatedEstate.License.OwnerId))
			{
				return value.Others;
			}
			int roleId = PlayerBehavior.LocalPlayer.Clan.RoleId;
			return value.CheckClanRole(roleId);
		}
		default:
			return false;
		}
	}

	private Shared.Estate.AccessRights GetIntuitiveAccessRights()
	{
		Shared.Estate.AccessRights accessRights = Shared.Estate.AccessRights.UseFacility;
		string[] components = Blueprint.Components;
		for (int i = 0; i < components.Length; i++)
		{
			switch (components[i])
			{
			case "Inventory":
				accessRights |= Shared.Estate.AccessRights.Take;
				break;
			case "Workbench":
			case "Growable":
				accessRights |= Shared.Estate.AccessRights.Give | Shared.Estate.AccessRights.Take;
				accessRights |= Shared.Estate.AccessRights.Give | Shared.Estate.AccessRights.Take;
				break;
			case "Gate":
				accessRights = Shared.Estate.AccessRights.Enter;
				break;
			}
		}
		return accessRights;
	}

	private bool IsInclusiveOfAccessRights(string[] blueprintComponents)
	{
		for (int i = 0; i < blueprintComponents.Length; i++)
		{
			switch (blueprintComponents[i])
			{
			case "Port":
			case "Crack":
				return false;
			}
		}
		return true;
	}

	public static bool HasEnoughtAccessRights(Shared.Estate.AccessRights combinedFlag, Shared.Estate.AccessRights combinedTargetFlag)
	{
		if (combinedFlag == Shared.Estate.AccessRights.None)
		{
			return false;
		}
		return (combinedFlag & combinedTargetFlag) == combinedTargetFlag;
	}

	[ExposedInEditor(null)]
	private void AdjustDurability(float ratio = 0f)
	{
		ArtifactState artifactState = ArtifactState;
		artifactState.Durability = new Gauge(1f, 0f, new GaugeNode[1]
		{
			new GaugeNode(0.0, ratio)
		});
		ArtifactState = artifactState;
		RefreshArtifactLook();
	}
}
