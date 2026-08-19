using System;
using System.Collections.Generic;
using Durango.Terrain;
using Durango.Utils;
using UnityEngine;

namespace Durango.Render;

public class RendererSet
{
	private static class OutlineMaterials
	{
		private class ColorComparer : IEqualityComparer<Color>
		{
			bool IEqualityComparer<Color>.Equals(Color x, Color y)
			{
				return x == y;
			}

			int IEqualityComparer<Color>.GetHashCode(Color obj)
			{
				return obj.GetHashCode();
			}
		}

		private static readonly Shader OutlineShader = Shader.Find("Durango/Custom/Outline");

		private static readonly Dictionary<Color, Material> Materials = new Dictionary<Color, Material>(new ColorComparer());

		public static Material GetOutlineMaterial(Color color)
		{
			if (Materials.TryGetValue(color, out var value))
			{
				return value;
			}
			value = new Material(OutlineShader);
			value.SetColor("_OutlineColor", color);
			Materials.Add(color, value);
			return value;
		}
	}

	public readonly Renderer Renderer;

	private BlendMode _baseBlendMode;

	private Material _material;

	private float _maxHeight = -1f;

	private readonly bool _isAnimal;

	private bool _hasOutline;

	public static readonly int PatternTex = Shader.PropertyToID("_PatternTex");

	public Material BaseMaterial { get; private set; }

	public Material Material
	{
		get
		{
			if (Renderer.sharedMaterial != _material && Renderer.sharedMaterial != BaseMaterial)
			{
				_material = Renderer.sharedMaterial;
				BaseMaterial = Renderer.sharedMaterial;
				_baseBlendMode = BlendUtil.GetBlendMode(_material);
			}
			if (_material == null)
			{
				_material = Renderer.material;
				_baseBlendMode = BlendUtil.GetBlendMode(_material);
			}
			return _material;
		}
	}

	public RendererSet(Renderer renderer, bool isAnimal)
	{
		Renderer = renderer;
		BaseMaterial = renderer.sharedMaterial;
		_isAnimal = isAnimal;
	}

	public void SetMaterialToBeShared(Material material)
	{
		_material = material;
		if (Renderer.sharedMaterial != BaseMaterial)
		{
			Renderer.sharedMaterial = material;
		}
	}

	public void SetMaterial(Material material)
	{
		Renderer.sharedMaterial = material;
	}

	private void CalcMaxHeight()
	{
		MeshRenderer meshRenderer = Renderer as MeshRenderer;
		if (meshRenderer != null)
		{
			MeshFilter component = meshRenderer.GetComponent<MeshFilter>();
			if (component == null || component.sharedMesh == null)
			{
				_maxHeight = 200f;
				return;
			}
			_maxHeight = component.sharedMesh.bounds.max.y;
		}
		SkinnedMeshRenderer skinnedMeshRenderer = Renderer as SkinnedMeshRenderer;
		if (skinnedMeshRenderer != null)
		{
			Mesh sharedMesh = skinnedMeshRenderer.sharedMesh;
			if (sharedMesh == null)
			{
				_maxHeight = 200f;
			}
			else
			{
				_maxHeight = sharedMesh.bounds.max.y;
			}
		}
	}

	public void SetColor(Color color)
	{
		Renderer.sharedMaterial = Material;
		if (_baseBlendMode == BlendMode.Opaque)
		{
			BlendMode blendMode = ((!(color.a >= 1f)) ? BlendMode.Transparent : BlendMode.Opaque);
			if (BlendUtil.GetBlendMode(Material) != blendMode)
			{
				BlendUtil.SetBlendMode(Material, blendMode);
			}
		}
		Material.color = color;
	}

	public void SetSubColor(Color subColor)
	{
		Renderer.sharedMaterial = Material;
		if (Material.HasProperty("_SubColor"))
		{
			Material.SetColor("_SubColor", subColor);
		}
	}

	public void SetThreeColor(ThreeColor color)
	{
		Renderer.sharedMaterial = Material;
		if (Material.HasProperty("_ThreeColor_1"))
		{
			Material.SetColor("_ThreeColor_1", color[0]);
			Material.SetColor("_ThreeColor_2", color[1]);
			Material.SetColor("_ThreeColor_3", color[2]);
		}
	}

	public void SetTransition(Texture2D tex, float transition)
	{
		Renderer.sharedMaterial = Material;
		if (transition <= 0f)
		{
			Material.DisableKeyword("TRANSITION_ON");
			return;
		}
		Material.EnableKeyword("TRANSITION_ON");
		Material.SetTexture("_TransTex", tex);
		Material.SetFloat("_Transition", transition);
	}

	public void SetPatternTex(Texture2D tex)
	{
		Renderer.sharedMaterial = Material;
		if (Material.HasProperty(PatternTex) && tex != null)
		{
			Material.SetTexture(PatternTex, tex);
		}
	}

	public void SetDamaged(float damagedRatio)
	{
		Renderer.sharedMaterial = Material;
		if (damagedRatio <= 0f)
		{
			Material.DisableKeyword((!_isAnimal) ? "_DAMAGED_ON" : "_ANIMALAGING_ON");
			return;
		}
		Material.EnableKeyword((!_isAnimal) ? "_DAMAGED_ON" : "_ANIMALAGING_ON");
		Material.SetFloat("_DamageRatio", damagedRatio);
		if (!_isAnimal)
		{
			Terrain_Mobile terrain_Mobile = Singleton<TerrainBase>.Instance() as Terrain_Mobile;
			if (terrain_Mobile != null)
			{
				Texture2D damagedPropTexture = terrain_Mobile.GetDamagedPropTexture();
				Material.SetTexture("_DamageTex", damagedPropTexture);
				Material.SetFloat("_DamageTexScale", (!(Material.mainTexture != null)) ? 1f : ((float)Material.mainTexture.width / 128f));
			}
		}
	}

	public void SetRimLight(Color rimLight)
	{
		Renderer.sharedMaterial = Material;
		if (rimLight == Color.black)
		{
			Material.DisableKeyword("_RIMLIGHT_ON");
			return;
		}
		Material.EnableKeyword("_RIMLIGHT_ON");
		Material.SetColor("_RimLightColor", rimLight);
	}

	public void SetOutline(Color color)
	{
		bool flag = color != Color.clear;
		if (flag != _hasOutline)
		{
			Material[] array = Renderer.sharedMaterials;
			Array.Resize(ref array, array.Length + (flag ? 1 : (-1)));
			Renderer.sharedMaterials = array;
		}
		if (flag)
		{
			Material[] sharedMaterials = Renderer.sharedMaterials;
			sharedMaterials[sharedMaterials.Length - 1] = OutlineMaterials.GetOutlineMaterial(color);
			Renderer.sharedMaterials = sharedMaterials;
		}
		_hasOutline = flag;
	}

	public void ResetMaterial(bool resetSubColor, bool resetThreeColor)
	{
		SetColor(Color.white);
		if (resetSubColor)
		{
			SetSubColor(Color.gray);
		}
		if (resetThreeColor)
		{
			SetThreeColor(ThreeColor.gray);
		}
		SetTransition(null, 0f);
		SetPatternTex(null);
		SetDamaged(0f);
		SetRimLight(Color.black);
		if (BaseMaterial != Material)
		{
			Renderer.sharedMaterial = BaseMaterial;
		}
	}
}
