using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Durango.Render;

public class MeshCloner
{
	private class RenderSet
	{
		public SkinnedMeshRenderer OriginSmr;

		public SkinnedMeshRenderer CloneSmr;

		public GameObject CloneObj;

		public bool OriginActive;
	}

	private readonly List<RenderSet> _renderSets = new List<RenderSet>();

	private Material _sourceMaterial;

	private int _renderLayerOverriden = -1;

	private bool _visible = true;

	public void OverrideRenderLayer(int layer)
	{
		_renderLayerOverriden = layer;
	}

	public void RefreshModel(bool updateMaterial = false)
	{
		foreach (RenderSet renderSet in _renderSets)
		{
			SkinnedMeshRenderer originSmr = renderSet.OriginSmr;
			SkinnedMeshRenderer cloneSmr = renderSet.CloneSmr;
			if (originSmr == null)
			{
				continue;
			}
			cloneSmr.sharedMesh = originSmr.sharedMesh;
			cloneSmr.bones = originSmr.bones;
			cloneSmr.rootBone = originSmr.rootBone;
			if (updateMaterial)
			{
				int num = originSmr.sharedMaterials.Length;
				if (num >= 2)
				{
					Material[] array = new Material[num];
					for (int i = 0; i < num; i++)
					{
						array[i] = _sourceMaterial;
					}
					cloneSmr.sharedMaterials = array;
				}
				else
				{
					cloneSmr.sharedMaterial = _sourceMaterial;
				}
			}
			renderSet.OriginActive = originSmr.gameObject.activeInHierarchy;
			renderSet.CloneObj.SetActive(renderSet.OriginActive && _visible);
		}
	}

	public void SetVisible(bool visible)
	{
		_visible = visible;
		foreach (RenderSet renderSet in _renderSets)
		{
			renderSet.CloneObj.SetActive(renderSet.OriginActive && _visible);
		}
	}

	public void Add(Transform parent, IList<SkinnedMeshRenderer> renderers, Material material)
	{
		_sourceMaterial = material;
		for (int i = 0; i < KUtility.GetSize(renderers); i++)
		{
			SkinnedMeshRenderer skinnedMeshRenderer = renderers[i];
			if (skinnedMeshRenderer == null)
			{
				continue;
			}
			bool flag = false;
			foreach (RenderSet renderSet2 in _renderSets)
			{
				if (renderSet2.OriginSmr == skinnedMeshRenderer)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				RenderSet renderSet = new RenderSet();
				renderSet.OriginSmr = skinnedMeshRenderer;
				_renderSets.Add(renderSet);
			}
		}
		SetUp(parent);
	}

	public void Remove(SkinnedMeshRenderer[] renderers)
	{
		for (int i = 0; i < KUtility.GetSize(renderers); i++)
		{
			for (int num = _renderSets.Count - 1; num >= 0; num--)
			{
				if (_renderSets[num].OriginSmr == renderers[i])
				{
					Object.Destroy(_renderSets[num].CloneObj);
					_renderSets.RemoveAt(num);
				}
			}
		}
	}

	public void RemoveAll()
	{
		foreach (RenderSet renderSet in _renderSets)
		{
			Object.Destroy(renderSet.CloneObj);
		}
		_renderSets.Clear();
	}

	private void SetUp(Transform parent)
	{
		foreach (RenderSet renderSet in _renderSets)
		{
			if (!(renderSet.CloneObj != null))
			{
				SkinnedMeshRenderer originSmr = renderSet.OriginSmr;
				GameObject gameObject = new GameObject(originSmr.gameObject.name + " clone");
				if (_renderLayerOverriden >= 0)
				{
					gameObject.layer = _renderLayerOverriden;
				}
				gameObject.transform.parent = parent;
				renderSet.CloneObj = gameObject;
				SkinnedMeshRenderer skinnedMeshRenderer = gameObject.AddComponent<SkinnedMeshRenderer>();
				skinnedMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
				skinnedMeshRenderer.receiveShadows = false;
				skinnedMeshRenderer.lightProbeUsage = LightProbeUsage.Off;
				skinnedMeshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
				skinnedMeshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
				skinnedMeshRenderer.quality = SkinQuality.Bone1;
				renderSet.CloneSmr = skinnedMeshRenderer;
				Transform transform = gameObject.transform;
				Transform transform2 = originSmr.transform;
				transform.position = transform2.position;
				transform.rotation = transform2.rotation;
				transform.localScale = transform2.localScale;
			}
		}
		RefreshModel(updateMaterial: true);
	}
}
