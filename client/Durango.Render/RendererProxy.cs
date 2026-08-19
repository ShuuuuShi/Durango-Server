using System;
using System.Collections.Generic;
using Durango.Utils;
using UnityEngine;

namespace Durango.Render;

public class RendererProxy
{
	[Flags]
	private enum UpdateFlag : uint
	{
		Color = 1u,
		SubColor = 2u,
		ThreeColor = 4u,
		Transition = 8u,
		PatternTex = 0x10u,
		Damaged = 0x20u,
		RimLight = 0x40u,
		Outline = 0x80u,
		All = uint.MaxValue
	}

	private Color _color = Color.white;

	private Color _subColor = Color.gray;

	private ThreeColor _threeColor = ThreeColor.gray;

	private Texture2D _transitionTex;

	private Texture2D _patternTex;

	private float _transition;

	private bool _subColorModified;

	private bool _threeColorModified;

	private float _damagedRatio;

	private Color _rimLight = Color.black;

	private readonly List<RendererSet> _rendererSets = new List<RendererSet>();

	public void Clear()
	{
		_rendererSets.Clear();
	}

	public bool IsEmpty()
	{
		return _rendererSets.Count == 0;
	}

	public void UpdateRenderers(GameObject target, bool isAnimal = false, bool isProp = false)
	{
		Renderer[] componentsInChildren = target.GetComponentsInChildren<Renderer>();
		Clear();
		Add(componentsInChildren, isAnimal, isProp || target.layer == LayerHelper.PropLayer);
	}

	public void Add(Renderer[] renderers, bool isAnimal, bool skipDefaultLayer = false)
	{
		int i = 0;
		for (int size = KUtility.GetSize(renderers); i < size; i++)
		{
			Renderer renderer = renderers[i];
			if (IsColorable(renderer, skipDefaultLayer))
			{
				_rendererSets.Add(new RendererSet(renderer, isAnimal));
			}
		}
		if (isAnimal)
		{
			UpdateMaterials(UpdateFlag.All);
		}
	}

	public void Remove(Renderer[] renderers)
	{
		int i = 0;
		for (int size = KUtility.GetSize(renderers); i < size; i++)
		{
			for (int num = _rendererSets.Count - 1; num >= 0; num--)
			{
				if (_rendererSets[num].Renderer == renderers[i])
				{
					_rendererSets.RemoveAt(num);
				}
			}
		}
	}

	private static bool IsColorable(Renderer renderer, bool skipDefaultLayer)
	{
		if (renderer == null || renderer.sharedMaterial == null || !renderer.sharedMaterial.HasProperty("_Color"))
		{
			return false;
		}
		if (skipDefaultLayer && renderer.gameObject.layer == LayerHelper.DefaultLayer)
		{
			return false;
		}
		return renderer.sharedMaterial.shader != null;
	}

	public void SetMaterialsToBeShared(Dictionary<Material, Material> materials)
	{
		int i = 0;
		for (int count = _rendererSets.Count; i < count; i++)
		{
			Material baseMaterial = _rendererSets[i].BaseMaterial;
			Material material = materials.Get(baseMaterial);
			if (material == null)
			{
				material = new Material(baseMaterial);
				materials.Add(baseMaterial, material);
			}
			_rendererSets[i].SetMaterialToBeShared(material);
		}
		UpdateMaterials(UpdateFlag.All);
	}

	public void SetColor(Color color)
	{
		bool flag = _color != color;
		_color = color;
		if (flag)
		{
			UpdateMaterials(UpdateFlag.Color);
		}
	}

	public void SetSubColor(Color subColor)
	{
		bool flag = _subColor != subColor;
		_subColor = subColor;
		if (flag)
		{
			_subColorModified = true;
			UpdateMaterials(UpdateFlag.SubColor);
		}
	}

	public void SetThreeColor(ThreeColor threeColor)
	{
		bool flag = _threeColor != threeColor;
		_threeColor = threeColor;
		if (flag)
		{
			_threeColorModified = true;
			UpdateMaterials(UpdateFlag.ThreeColor);
		}
	}

	public void SetTransition(Texture2D tex, float transition)
	{
		bool flag = _transitionTex != tex || _transition != transition;
		_transitionTex = tex;
		_transition = transition;
		if (flag)
		{
			UpdateMaterials(UpdateFlag.Transition);
		}
	}

	public void SetPatternTex(Texture2D tex)
	{
		bool flag = _patternTex != tex;
		_patternTex = tex;
		if (flag)
		{
			UpdateMaterials(UpdateFlag.PatternTex);
		}
	}

	public bool HasPatternTex()
	{
		foreach (RendererSet rendererSet in _rendererSets)
		{
			if (rendererSet.BaseMaterial.HasProperty(RendererSet.PatternTex))
			{
				return true;
			}
		}
		return false;
	}

	public void SetDamaged(float damageRatio)
	{
		bool flag = damageRatio != _damagedRatio;
		_damagedRatio = damageRatio;
		if (flag)
		{
			UpdateMaterials(UpdateFlag.Damaged);
		}
	}

	public void SetRimLight(Color rimLight)
	{
		bool flag = rimLight != _rimLight;
		_rimLight = rimLight;
		if (flag)
		{
			UpdateMaterials(UpdateFlag.RimLight);
		}
	}

	public void SetOutline(Color outline)
	{
		int i = 0;
		for (int count = _rendererSets.Count; i < count; i++)
		{
			_rendererSets[i].SetOutline(outline);
		}
	}

	public void SetMaterial(Material material)
	{
		foreach (RendererSet rendererSet in _rendererSets)
		{
			rendererSet.SetMaterial(material);
		}
	}

	private void UpdateMaterials(UpdateFlag flag)
	{
		int i = 0;
		for (int count = _rendererSets.Count; i < count; i++)
		{
			if (_color == Color.white && _subColor == Color.gray && _threeColor == ThreeColor.gray && _transition <= 0f && _patternTex == null && _damagedRatio <= 0f && _rimLight == Color.black)
			{
				_rendererSets[i].ResetMaterial(_subColorModified, _threeColorModified);
				continue;
			}
			if ((flag & UpdateFlag.Color) != 0)
			{
				_rendererSets[i].SetColor(_color);
			}
			if ((flag & UpdateFlag.SubColor) != 0 && _subColorModified)
			{
				_rendererSets[i].SetSubColor(_subColor);
			}
			if ((flag & UpdateFlag.ThreeColor) != 0 && _threeColorModified)
			{
				_rendererSets[i].SetThreeColor(_threeColor);
			}
			if ((flag & UpdateFlag.Transition) != 0)
			{
				_rendererSets[i].SetTransition(_transitionTex, _transition);
			}
			if ((flag & UpdateFlag.PatternTex) != 0 && _patternTex != null)
			{
				_rendererSets[i].SetPatternTex(_patternTex);
			}
			if ((flag & UpdateFlag.Damaged) != 0)
			{
				_rendererSets[i].SetDamaged(_damagedRatio);
			}
			if ((flag & UpdateFlag.RimLight) != 0)
			{
				_rendererSets[i].SetRimLight(_rimLight);
			}
		}
	}
}
