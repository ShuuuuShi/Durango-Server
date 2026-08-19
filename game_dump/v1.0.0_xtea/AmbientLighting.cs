using System.Collections.Generic;
using TerrainData;
using UnityEngine;

[RequireComponent(typeof(TileBiomeUpdater))]
public class AmbientLighting : MonoBehaviour
{
	[SerializeField]
	private bool _hideWetness;

	[SerializeField]
	private bool _isStatic;

	[SerializeField]
	private bool _isManualAmbientColor;

	[SerializeField]
	private Color _manualAmbientColor = Color.black;

	[SerializeField]
	private float _maxWetness = 1f;

	[SerializeField]
	private float _wetParticleOnThreashold = 0.5f;

	[SerializeField]
	private ParticleType _wetParticleType;

	private readonly List<Material> _materialList = new List<Material>();

	private AmbientLightingManager _ambientLightingMgr;

	private float _prevWetness;

	private GameObject _wetParticle;

	private Color _curAmbientColor;

	private Color _nextAmbientColor = Color.black;

	private Biome _curBiome;

	private Biome _nextBiome = Biome.Unspecified;

	private float _startTransitionTime;

	private bool _transiting;

	private TileBiomeUpdater _tileBiomeUpdater;

	private CharacterBehavior _characterBehavior;

	private Renderer[] _renderer;

	public float Wetness { get; set; }

	[ExposedInEditor(null)]
	public Color AmbientColor { get; private set; }

	private void Awake()
	{
		_ambientLightingMgr = KSingleton<AmbientLightingManager>.Instance();
		_tileBiomeUpdater = ((Component)this).gameObject.GetComponent<TileBiomeUpdater>();
		_tileBiomeUpdater.BiomeUpdated += TileBiomeUpdater_BiomeUpdated;
		_tileBiomeUpdater.Running = !_isStatic;
		_characterBehavior = ((Component)this).gameObject.GetComponent<CharacterBehavior>();
		_renderer = ((Component)this).gameObject.GetComponentsInChildren<Renderer>();
	}

	private void Update()
	{
		if (_renderer.Length <= 0 || _renderer[0].isVisible)
		{
			UpdateLitSphereRotation();
			UpdateAmbientColor();
			ApplyWetness();
		}
	}

	private void UpdateAmbientColor()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if (!_isManualAmbientColor && _transiting)
		{
			float num = Time.time - _startTransitionTime;
			float num2 = num / _ambientLightingMgr.TransitionTime;
			num2 = Mathf.Clamp01(num2);
			AmbientColor = _curAmbientColor * (1f - num2) + _nextAmbientColor * num2;
			if (num > _ambientLightingMgr.TransitionTime)
			{
				_curBiome = _nextBiome;
				_curAmbientColor = _nextAmbientColor;
				_nextBiome = Biome.Unspecified;
				_nextAmbientColor = Color.black;
				_transiting = false;
			}
			SetAmbientColorToMaterials(AmbientColor);
		}
	}

	public void UpdateMaterials(SkinnedMeshRenderer[] renderes)
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		_materialList.Clear();
		for (int i = 0; i < renderes.Length; i++)
		{
			Material[] materials = ((Renderer)renderes[i]).materials;
			foreach (Material val in materials)
			{
				if (((Object)val.shader).name.Contains("LitSphere"))
				{
					_materialList.Add(val);
				}
			}
		}
		if (_isStatic)
		{
			Biome tileBiome = _tileBiomeUpdater.GetTileBiome();
			TileBiomeUpdater_BiomeUpdated(tileBiome);
		}
		SetAmbientColorToMaterials((!_isManualAmbientColor) ? AmbientColor : _manualAmbientColor);
		SetWetnessToMaterials();
	}

	private void TileBiomeUpdater_BiomeUpdated(Biome biome)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (biome != Biome.Unspecified && _nextBiome != biome && _curBiome != _nextBiome)
		{
			_nextBiome = biome;
			_nextAmbientColor = _ambientLightingMgr.GetAmbientColor(_nextBiome);
			_startTransitionTime = Time.time;
			AmbientColor = _curAmbientColor;
			_transiting = true;
		}
	}

	private void SetAmbientColorToMaterials(Color groundAmbient)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		int count = _materialList.Count;
		for (int i = 0; i < count; i++)
		{
			Material val = _materialList[i];
			val.SetColor("_GroundAmbient", groundAmbient);
			val.SetFloat("_MaxWetness", _maxWetness);
		}
	}

	private void UpdateLitSphereRotation()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		NightLight nearestFireLight = KSingleton<NightLightGrid>.Instance().GetNearestFireLight(((Component)this).transform.position);
		double num = ((!((Object)(object)nearestFireLight != (Object)null)) ? 0.0 : KSingleton<NightLightGrid>.Instance().GetRotationDegree(_characterBehavior.CurrentPosition, ((Component)nearestFireLight).transform.position));
		int count = _materialList.Count;
		for (int i = 0; i < count; i++)
		{
			Material val = _materialList[i];
			if ((Object)(object)nearestFireLight != (Object)null)
			{
				val.EnableKeyword("ROTATION_ON");
				val.SetFloat("_RotAngle", (float)num);
			}
			else
			{
				val.DisableKeyword("ROTATION_ON");
			}
		}
	}

	private void ApplyWetness()
	{
		if (!Mathf.Approximately(_prevWetness, Wetness))
		{
			UpdateWetParticle();
			SetWetnessToMaterials();
			_prevWetness = Wetness;
		}
	}

	private void UpdateWetParticle()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrEmpty(_wetParticleType.Path))
		{
			bool flag = Wetness >= _wetParticleOnThreashold;
			if (_hideWetness)
			{
				flag = false;
			}
			if (flag && (Object)(object)_wetParticle == (Object)null)
			{
				_wetParticle = ParticleManager.EmitSync(_wetParticleType.Path, Vector3.zero, Quaternion.identity, ((Component)this).transform);
			}
			else if (!flag && (Object)(object)_wetParticle != (Object)null)
			{
				ParticleManager.Stop(_wetParticle);
				_wetParticle = null;
			}
		}
	}

	private void SetWetnessToMaterials()
	{
		int count = _materialList.Count;
		for (int i = 0; i < count; i++)
		{
			Material val = _materialList[i];
			if (Wetness <= 0f)
			{
				val.DisableKeyword("WETNESS_ON");
				continue;
			}
			val.EnableKeyword("WETNESS_ON");
			val.SetFloat("_Wetness", Wetness * _maxWetness);
		}
	}
}
