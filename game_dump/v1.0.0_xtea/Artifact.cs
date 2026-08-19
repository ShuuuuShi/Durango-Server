using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Building_;
using ItemSystem;
using JetBrains.Annotations;
using Messages;
using MsgPack;
using Shared.Battle;
using Shared.Building;
using Shared.Etc;
using TerrainData;
using TimerData;
using UnityEngine;
using Yaml;
using Yaml.Util;

public sealed class Artifact : SizableImmovableBase
{
	public delegate void UpdateDisplayDelegate(MessagePackObjectDictionary displayInfo);

	private bool _hasShadow = true;

	private double _postprocessSince;

	private double _postprocessUntil;

	private bool _interactionDisabled;

	private bool _hasGroudSite;

	private GameObject _groundSiteObject;

	private bool _hasScaffolding;

	private GameObject _scaffoldingObject;

	private bool _isUpdatingShadow;

	private List<ArtifactComponent> _components;

	private string _consiteAssetPath = "Models/Prop/system/site/site_01_1x1.prefab";

	private string _scaffoldingAssetPah = "Models/Prop/system/scaffolding/scaffolding_01_1x1.prefab";

	public override float InteractionDistance => 100f * ((float)Mathf.Max(base.Size.x, base.Size.y) + 0.5f);

	public ModelComponent Models { get; private set; }

	[ExposedInEditor(false, null)]
	public string ArtifactId { get; private set; }

	public ulong FounderId { get; set; }

	public ArtifactState ArtifactState { get; private set; }

	public Condition Condition { get; private set; }

	public Gauge Durability => ArtifactState.Durability;

	[ExposedInEditor(null)]
	public BuildingState BuildState => ArtifactState.BuildingState;

	public bool InteractionDisabled
	{
		get
		{
			return BuildCompleted && _interactionDisabled;
		}
		set
		{
			_interactionDisabled = value;
		}
	}

	[ExposedInEditor(null)]
	public bool BuildCompleted => BuildState == BuildingState.Completed;

	public override int Height
	{
		get
		{
			int num = base.Height;
			int i = 0;
			for (int size = KUtility.GetSize(_components); i < size; i++)
			{
				num = Mathf.Max(num, _components[i].Height);
			}
			return num;
		}
	}

	public override Vector3 InteractionPosition
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = base.InteractionPosition;
			int i = 0;
			for (int size = KUtility.GetSize(_components); i < size; i++)
			{
				val += _components[i].InteractionPositionOffset;
			}
			return val;
		}
	}

	public bool HasShadow
	{
		get
		{
			return _hasShadow;
		}
		set
		{
			_hasShadow = value;
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

	public bool RotationDisabled { get; private set; }

	public Building_.Blueprint Blueprint { get; private set; }

	public List<TagData> Tags { get; private set; }

	public string LocalizedName => (ArtifactState.ChangedName != null) ? ArtifactState.ChangedName : ((Blueprint != null) ? Blueprint.LocalizedName : string.Empty);

	public TimerData.Timer PostProcessTimer { get; private set; }

	public TimerData.Timer ArtifactTimer { get; private set; }

	public Color Color { get; private set; }

	public int InitFrame { get; private set; }

	public GameObject Effect { get; private set; }

	public float MaxHealth => ArtifactState.MaxHealth;

	public event System.Action ResourceLoadCompleted;

	public event System.Action ArtifactDisplayUpdated;

	public static event Action<Artifact> ArtifactStateChanged;

	public event Action<Artifact> DurabilityGaugeUpdated;

	private void Start()
	{
		IntegratedEffect.Precache("Particle/FX_Int_Building_Crash.prefab");
	}

	public void Init(string artifactId, int worldTileX, int worldTileY, Rotation rotation, Point2 size, int height)
	{
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		int i = 0;
		for (int size2 = KUtility.GetSize(_components); i < size2; i++)
		{
			_components[i].PreInit(artifactId, worldTileX, worldTileY, rotation, size);
		}
		ArtifactId = artifactId;
		base.Rotation = rotation;
		Tags = new List<TagData>();
		ArtifactState = new ArtifactState
		{
			BuildingState = BuildingState.Invalid
		};
		Models = new ModelComponent(((Component)this).gameObject, base.EntityId.GetHashCode());
		Models.LoadCompleted += Models_LoadCompleted;
		Models.Unloaded += Models_Unloaded;
		InitFrame = Time.frameCount;
		SetBlueprint(GameSystem<RecipeSystem>.Instance().GetBlueprint(ArtifactId));
		if (size.x > 0 && size.y > 0)
		{
			base.Size = size;
		}
		Height = height;
		if (Blueprint == null)
		{
			Debug.LogError((object)("Unknown Building Name(ID): " + ArtifactId));
			return;
		}
		((Component)this).gameObject.transform.localPosition = Vector3.zero;
		((Component)this).gameObject.transform.localRotation = Quaternion.identity;
		((Component)this).gameObject.tag = "Artifact";
		UpdateCollider();
		if (!Blueprint.Permanent && !Blueprint.TransparentSite)
		{
			MakeGroundSite();
		}
		int j = 0;
		for (int size3 = KUtility.GetSize(_components); j < size3; j++)
		{
			_components[j].PostInit(artifactId, worldTileX, worldTileY, rotation, size);
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

	public T GetArtifactComponent<T>() where T : ArtifactComponent
	{
		int i = 0;
		for (int size = KUtility.GetSize(_components); i < size; i++)
		{
			if (_components[i] is T result)
			{
				return result;
			}
		}
		return (T)null;
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

	private void SetBlueprint(Building_.Blueprint blueprint)
	{
		Blueprint = blueprint;
		if (Blueprint != null)
		{
			base.Size = Blueprint.Size;
			RotationDisabled = Blueprint.RotationDisabled;
		}
	}

	private void RefreshArtifactColor()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		Color artifactColor = Color.white;
		switch (BuildState)
		{
		case BuildingState.Built:
			artifactColor.a = ((PostProcessTimer != null) ? 0.4f : 1f);
			break;
		case BuildingState.Completed:
			artifactColor = GetConditionColor(Condition);
			break;
		}
		SetArtifactColor(artifactColor);
	}

	private void SetArtifactColor(Color color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		Color = color;
		SetColor(color);
	}

	private static Color GetConditionColor(Condition condition)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		return (Color)(condition switch
		{
			Condition.Normal => Color.white, 
			Condition.Worn => new Color(1f, 0.5f, 0.5f), 
			Condition.Old => new Color(0.8f, 0.4f, 0.4f), 
			Condition.Broken => new Color(0.4f, 0.4f, 0.4f), 
			_ => Color.white, 
		});
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

	[UsedImplicitly]
	protected override void OnSelected(bool selected)
	{
		bool flag = false;
		int i = 0;
		for (int size = KUtility.GetSize(_components); i < size; i++)
		{
			flag |= _components[i].OnSelectArtifact(selected);
		}
		if (!flag)
		{
			base.OnSelected(selected);
		}
	}

	protected override void SetColor(Color color)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Models.SetColor(color);
	}

	protected override Color GetDefaultColor()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return Color;
	}

	private void OnModelChanged()
	{
		((MonoBehaviour)this).StartCoroutine(CoUpdateShadow());
	}

	private IEnumerator CoUpdateShadow()
	{
		if (!_isUpdatingShadow)
		{
			_isUpdatingShadow = true;
			yield return (object)new WaitForEndOfFrame();
			UpdateShadow();
			UpdateShadowVisible();
			_isUpdatingShadow = false;
		}
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
		int i = 0;
		for (int size = KUtility.GetSize(_components); i < size; i++)
		{
			_components[i].OnUpdateCollider();
		}
	}

	public void CreateCollider([Optional] Vector3 size, [Optional] Vector3 center)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		BoxCollider val = ((Component)this).gameObject.AddMissingComponent<BoxCollider>();
		((Collider)val).isTrigger = true;
		Vector3 val2 = new Vector3((float)base.Size.x, (float)Height, (float)base.Size.y) * 200f;
		if (size != default(Vector3))
		{
			val2 = size;
		}
		val.center = ((!(center != default(Vector3))) ? (val2 * 0.5f) : center);
		val.size = val2;
	}

	private void UpdateShadow()
	{
		if (HasShadow)
		{
			BuildingShadows component = ((Component)this).gameObject.GetComponent<BuildingShadows>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.SetUp(ShadowSkipFunction);
			}
		}
	}

	private bool ShadowSkipFunction([NotNull] MeshRenderer meshRenderer)
	{
		string name = ((Object)meshRenderer).name;
		string text = ((!((Object)(object)((Renderer)meshRenderer).sharedMaterial == (Object)null)) ? ((Object)((Renderer)meshRenderer).sharedMaterial.shader).name : string.Empty);
		if (text.Contains("Particle") || text.Contains("Plane") || text.Contains("Floor"))
		{
			return true;
		}
		if (name.StartsWith("site") || name.StartsWith("LightMask") || name.StartsWith("scaffolding_") || name.EndsWith("_shadow"))
		{
			return true;
		}
		int i = 0;
		for (int size = KUtility.GetSize(_components); i < size; i++)
		{
			if (_components[i].ShadowSkipFunction(meshRenderer))
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateShadowVisible()
	{
		if (HasShadow)
		{
			BuildingShadows component = ((Component)this).gameObject.GetComponent<BuildingShadows>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.Show(BuildState != BuildingState.Occupied);
			}
		}
	}

	private void MakeGroundSite()
	{
		if (!_hasGroudSite)
		{
			_hasGroudSite = true;
			string consiteAssetPath = ConsiteAssetPath;
			KSingleton<AssetBundleManager>.Instance().RequestAsset(consiteAssetPath, typeof(GameObject), GroundSiteObjectLoaded);
		}
	}

	private void GroundSiteObjectLoaded(Object asset)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)this == (Object)null || asset == (Object)null || !_hasGroudSite)
		{
			return;
		}
		GameObject[] array = (GameObject[])(object)new GameObject[base.Size.y * base.Size.x];
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			array[i] = Object.Instantiate<GameObject>((GameObject)(object)((asset is GameObject) ? asset : null));
		}
		_groundSiteObject = new GameObject("site");
		Transform transform = _groundSiteObject.transform;
		transform.parent = ((Component)this).gameObject.transform;
		transform.localPosition = new Vector3(0.5f, 0f, 0.5f) * 200f;
		Point2 size = base.Size;
		for (int j = 0; j < size.y; j++)
		{
			for (int k = 0; k < size.x; k++)
			{
				Transform transform2 = array[j * size.x + k].transform;
				transform2.parent = transform;
				transform2.localPosition = new Vector3((float)k, 0f, (float)j) * 200f;
			}
		}
	}

	private void RemoveGroundSite()
	{
		_hasGroudSite = false;
		if ((Object)(object)_groundSiteObject != (Object)null)
		{
			_groundSiteObject.transform.parent = null;
			Object.Destroy((Object)(object)_groundSiteObject);
		}
		_groundSiteObject = null;
	}

	private void MakeScaffolding()
	{
		if (!_hasScaffolding)
		{
			_hasScaffolding = true;
			string scaffoldingAssetPath = ScaffoldingAssetPath;
			KSingleton<AssetBundleManager>.Instance().RequestAsset(scaffoldingAssetPath, typeof(GameObject), ScaffoldingObjectLoaded);
		}
	}

	private void ScaffoldingObjectLoaded(Object asset)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)this == (Object)null || asset == (Object)null || !_hasScaffolding)
		{
			return;
		}
		GameObject[] array = (GameObject[])(object)new GameObject[base.Size.y * base.Size.x];
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			array[i] = Object.Instantiate<GameObject>((GameObject)(object)((asset is GameObject) ? asset : null));
		}
		_scaffoldingObject = new GameObject("scaffolding");
		Transform transform = _scaffoldingObject.transform;
		transform.parent = ((Component)this).gameObject.transform;
		transform.localPosition = new Vector3(0.5f, 0f, 0.5f) * 200f;
		Point2 size = base.Size;
		Random random = new Random(base.EntityId.GetHashCode());
		for (int j = 0; j < size.y; j++)
		{
			for (int k = 0; k < size.x; k++)
			{
				Transform transform2 = array[j * size.x + k].transform;
				transform2.parent = transform;
				transform2.localPosition = new Vector3((float)k, 0f, (float)j) * 200f;
				transform2.localRotation = Quaternion.Euler(0f, (float)(90 * random.Next(4)), 0f);
			}
		}
	}

	private void RemoveScaffolding()
	{
		_hasScaffolding = false;
		if ((Object)(object)_scaffoldingObject != (Object)null)
		{
			_scaffoldingObject.transform.parent = null;
			Object.Destroy((Object)(object)_scaffoldingObject);
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
		if (this.DurabilityGaugeUpdated != null)
		{
			double bufferedServerTime_Enhanced = Connections.Frontend.GetBufferedServerTime_Enhanced();
			float delay = (float)(eventTime - bufferedServerTime_Enhanced);
			KUtility.DelayedCall((MonoBehaviour)(object)this, delegate
			{
				this.DurabilityGaugeUpdated(this);
			}, delay);
		}
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
			RefreshArtifactColor();
		}
	}

	private void OnUpdateBuildState()
	{
		switch (BuildState)
		{
		case BuildingState.Occupied:
			MakeGroundSite();
			break;
		case BuildingState.Built:
			if (Blueprint.Permanent || Blueprint.TransparentSite)
			{
				RemoveGroundSite();
			}
			MakeScaffolding();
			break;
		case BuildingState.Completed:
			if (Blueprint.Permanent || Blueprint.TransparentSite)
			{
				RemoveGroundSite();
			}
			RemoveScaffolding();
			OnCompleted();
			break;
		}
		UpdateShadowVisible();
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
		int i = 0;
		for (int size = KUtility.GetSize(_components); i < size; i++)
		{
			_components[i].OnRemoved();
		}
	}

	private bool RepairTimerUpdate()
	{
		if (!ArtifactState.Repairement.HasValue)
		{
			return false;
		}
		double num = ArtifactState.Repairement.Value.Key;
		double value = ArtifactState.Repairement.Value.Value;
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
		UpdateArtifactTimer("repairment", num2, ratio);
		return true;
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
		UpdateArtifactTimer("crack_activated", num2, ratio);
		return true;
	}

	private void UpdateArtifactTimer(string subject, float duration, float ratio)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if (ArtifactTimer == null || ArtifactTimer.IsStop)
		{
			ArtifactTimer = new TimerData.Timer(base.EntityId, subject, duration, ratio);
			ArtifactTimer.Finished += OnFinishArtifactTimer;
			TimerProgressGauge timerProgressGauge = TimerData.Timer.Play<TimerProgressGauge>(ArtifactTimer);
			timerProgressGauge.SetTarget(((Component)this).gameObject, new Vector3((float)base.Size.x, 1f, (float)base.Size.y) * 200f * 0.5f);
		}
		else
		{
			ArtifactTimer.SetDuration(base.EntityId, subject, duration, ratio);
		}
	}

	private void PostprocessTimeUpdate()
	{
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		_postprocessSince = 0.0;
		_postprocessUntil = 0.0;
		if (BuildState != BuildingState.Built || !ArtifactState.Postprocess.HasValue)
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
			PostProcessTimer = new TimerData.Timer(base.EntityId, "postprocess", num2, ratio);
			PostProcessTimer.Finished += OnFinishPostprogressGauge;
			TimerProgressGauge timerProgressGauge = TimerData.Timer.Play<TimerProgressGauge>(PostProcessTimer);
			timerProgressGauge.SetTarget(((Component)this).gameObject, new Vector3((float)base.Size.x, (float)Height * 2f, (float)base.Size.y) * 200f * 0.5f);
		}
		else
		{
			PostProcessTimer.SetDuration(base.EntityId, "postprocess", num2, ratio);
		}
	}

	private void OnFinishPostprogressGauge(TimerData.Timer timer)
	{
		RefreshArtifactColor();
		PostProcessTimer = null;
	}

	private void OnFinishArtifactTimer(TimerData.Timer timer)
	{
		RefreshArtifactColor();
		ArtifactTimer = null;
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

	public void UpdateDisplay(ArtifactDisplay msg)
	{
		OnUpdateDisplay(msg);
		if (this.ArtifactDisplayUpdated != null)
		{
			this.ArtifactDisplayUpdated();
		}
	}

	private void OnUpdateDisplay(ArtifactDisplay msg)
	{
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		int i = 0;
		for (int size = KUtility.GetSize(_components); i < size; i++)
		{
			flag |= _components[i].OnUpdateDisplay(msg);
		}
		if (flag)
		{
			return;
		}
		Models.BeginLoad();
		foreach (KeyValuePair<string, string> part in msg.Parts)
		{
			if (!string.IsNullOrEmpty(part.Value))
			{
				UpdatePart(null, part.Key, part.Value);
			}
		}
		foreach (KeyValuePair<string, KeyValuePair<string, string>> decoration in msg.Decorations)
		{
			string key = decoration.Value.Key;
			if (!string.IsNullOrEmpty(key))
			{
				ModelComponent.IModel model = UpdatePart("Decoration", decoration.Key, key);
				if (!string.IsNullOrEmpty(decoration.Value.Value))
				{
					Color color = KUtility.ToColor(decoration.Value.Value);
					model.SetColor(color);
				}
			}
		}
		UpdateEffect(msg.Effect);
		Models.EndLoad();
		Condition = msg.Condition;
		RefreshArtifactColor();
	}

	private ModelComponent.IModel UpdatePart(string category, string key, string modelKey)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = new Vector3((float)base.Size.x, 0f, (float)base.Size.y) * 200f * 0.5f;
		Vector3 angle = KUtility.DirectionToAngle(KUtility.RotationToDirection(base.Rotation));
		return Models.Load(key, modelKey, null, category).SetPosition(position).SetAngle(angle);
	}

	private void UpdateEffect(string effectKey)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		ClearEffect();
		if (!string.IsNullOrEmpty(effectKey))
		{
			ArtifactEffect artifactEffect = SingletonDict<string, ArtifactEffect>.Get(effectKey);
			if (artifactEffect != null)
			{
				string assetPath = $"{artifactEffect.path}/{artifactEffect.file_name}.prefab";
				Effect = ParticleManager.EmitSync(assetPath, Center, Quaternion.identity);
			}
		}
	}

	private void ClearEffect()
	{
		if ((Object)(object)Effect != (Object)null)
		{
			ParticleManager.Stop(Effect);
			Effect = null;
		}
	}

	private void OnDestroy()
	{
		ClearEffect();
	}

	public void ArtifactPlaced()
	{
		int i = 0;
		for (int size = KUtility.GetSize(_components); i < size; i++)
		{
			_components[i].ArtifactPlaced();
		}
		PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
		if (!((Object)(object)localPlayer == (Object)null))
		{
			Point2 currentTile = localPlayer.CurrentTile;
			Point2 size2 = base.Size;
			if (currentTile.x >= base.WorldTile.x && currentTile.y >= base.WorldTile.y && currentTile.x < base.WorldTile.x + size2.x && currentTile.y < base.WorldTile.y + size2.y)
			{
				OnPlayerEnter();
			}
		}
	}

	public void OverrideDepth(ref byte floor, ref float depth00, ref float depth10, ref float depth01, ref float depth11)
	{
		int i = 0;
		for (int size = KUtility.GetSize(_components); i < size; i++)
		{
			_components[i].OverrideDepth(ref floor, ref depth00, ref depth10, ref depth01, ref depth11);
		}
	}

	public void OnTakeDamage(Damage damage, GameObject attacker)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		damage.AttackType = AttackType.Demolition;
		DamageEffectManager.PlayDamagerEffectSet(attacker, damage, Center);
		if (GetLife().Get() <= 0f)
		{
			IntegratedEffect.Emit("Particle/FX_Int_Building_Crash.prefab", Biome.Unspecified, Center, Quaternion.identity);
		}
	}

	public Gauge GetLife()
	{
		return Durability;
	}
}
