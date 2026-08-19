using System;
using System.Collections.Generic;
using Durango.Render.Particle;
using Durango.Utils;
using Shared.Region;
using UnityEngine;

namespace Durango.Environment;

public class AmbientLighting : MonoBehaviour
{
	private const float RotSpeed = 3f;

	[SerializeField]
	private float _maxWetness = 1f;

	[SerializeField]
	private float _wetParticleOnThreashold = 0.5f;

	[SerializeField]
	private ParticleType _wetParticleType;

	private readonly List<Material> _materialList = new List<Material>();

	private float _prevWetness;

	private int _wetParticleId;

	[ExposedInEditor(null)]
	private Color _prevAmbientColor;

	[ExposedInEditor(null)]
	private Color _targetAmbientColor;

	private float _colorTransitionRatio = 1f;

	private Biome _curBiome = Biome.Invalid;

	private float _curRotationDegree;

	private float _targetRotationDegree;

	private CharacterBehavior _characterBehavior;

	[ExposedInEditor(null)]
	public Color CurAmbientColor { get; private set; }

	public float Wetness { get; set; }

	public void SetupMaterials(SkinnedMeshRenderer[] renderes)
	{
		_materialList.Clear();
		for (int i = 0; i < renderes.Length; i++)
		{
			Material[] materials = renderes[i].materials;
			foreach (Material material in materials)
			{
				if (material.shader.name.Contains("LitSphere"))
				{
					_materialList.Add(material);
				}
			}
		}
		SetAmbientColorToMaterials();
		SetRotationToMaterials();
		SetWetnessToMaterials();
	}

	private void Awake()
	{
		_characterBehavior = base.gameObject.GetComponent<CharacterBehavior>();
	}

	private void Update()
	{
		if (!(_characterBehavior == null) && _characterBehavior.WillBeRendered)
		{
			UpdateAmbientColor();
			UpdateWetness();
			UpdateLitSphereRotation();
		}
	}

	private void UpdateAmbientColor()
	{
		Biome biome = _characterBehavior.GetBiome();
		if (biome != Biome.Invalid && Singleton<AmbientLightingManager>.HasInstance())
		{
			if (biome != _curBiome)
			{
				_curBiome = biome;
				_prevAmbientColor = CurAmbientColor;
				_targetAmbientColor = Singleton<AmbientLightingManager>.Instance().GetAmbientColor(_curBiome);
				_colorTransitionRatio = 0f;
			}
			if (_colorTransitionRatio < 1f)
			{
				_colorTransitionRatio = Mathf.Clamp01(_colorTransitionRatio + Time.deltaTime / Singleton<AmbientLightingManager>.Instance().TransitionTime);
				CurAmbientColor = Color.Lerp(_prevAmbientColor, _targetAmbientColor, _colorTransitionRatio);
				SetAmbientColorToMaterials();
			}
		}
	}

	private void SetAmbientColorToMaterials()
	{
		int count = _materialList.Count;
		for (int i = 0; i < count; i++)
		{
			Material material = _materialList[i];
			material.SetColor("_GroundAmbient", CurAmbientColor);
		}
	}

	private void UpdateLitSphereRotation()
	{
		if (!Singleton<NightLightGrid>.HasInstance())
		{
			return;
		}
		Vector3 currentPosition = _characterBehavior.CurrentPosition;
		_targetRotationDegree = Singleton<NightLightGrid>.Instance().GetRotationDegree(currentPosition);
		if (_targetRotationDegree != _curRotationDegree)
		{
			if (_curRotationDegree + (float)Math.PI < _targetRotationDegree)
			{
				_curRotationDegree += (float)Math.PI * 2f;
			}
			else if (_curRotationDegree - (float)Math.PI > _targetRotationDegree)
			{
				_curRotationDegree -= (float)Math.PI * 2f;
			}
			_curRotationDegree = Mathf.MoveTowards(_curRotationDegree, _targetRotationDegree, 3f * Time.deltaTime);
			SetRotationToMaterials();
		}
	}

	private void SetRotationToMaterials()
	{
		int count = _materialList.Count;
		for (int i = 0; i < count; i++)
		{
			Material material = _materialList[i];
			if (_curRotationDegree != 0f)
			{
				material.EnableKeyword("ROTATION_ON");
				material.SetFloat("_RotAngle", _curRotationDegree);
			}
			else
			{
				material.DisableKeyword("ROTATION_ON");
			}
		}
	}

	private void UpdateWetness()
	{
		if (_prevWetness != Wetness)
		{
			SetWetParticle();
			SetWetnessToMaterials();
			_prevWetness = Wetness;
		}
	}

	private void SetWetParticle()
	{
		if (!string.IsNullOrEmpty(_wetParticleType.Path))
		{
			bool flag = Wetness >= _wetParticleOnThreashold;
			if (flag && _wetParticleId == 0)
			{
				_wetParticleId = ParticleManager.EmitFollow(_wetParticleType.Path, Vector3.zero, Quaternion.identity, base.transform);
			}
			else if (!flag && _wetParticleId != 0)
			{
				ParticleManager.Stop(_wetParticleId);
				_wetParticleId = 0;
			}
		}
	}

	private void SetWetnessToMaterials()
	{
		int count = _materialList.Count;
		for (int i = 0; i < count; i++)
		{
			Material material = _materialList[i];
			if (Wetness <= 0f)
			{
				material.DisableKeyword("WETNESS_ON");
				continue;
			}
			material.EnableKeyword("WETNESS_ON");
			material.SetFloat("_Wetness", Wetness * _maxWetness);
		}
	}
}
