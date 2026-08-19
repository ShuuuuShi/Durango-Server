using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class MeshCloner : MonoBehaviour
{
	protected class RenderSet
	{
		public SkinnedMeshRenderer OriginSmr;

		public SkinnedMeshRenderer CloneSmr;

		public GameObject CloneObj;

		public bool OrigianlActive;
	}

	protected int ModyfyRenderLayer = -1;

	protected readonly List<RenderSet> RenderSets = new List<RenderSet>();

	private bool _show = true;

	public bool Show
	{
		get
		{
			return _show;
		}
		set
		{
			_show = value;
			int count = RenderSets.Count;
			for (int i = 0; i < count; i++)
			{
				RenderSet renderSet = RenderSets[i];
				renderSet.CloneObj.SetActive(renderSet.OrigianlActive && _show);
			}
		}
	}

	protected abstract Material GetSourceMaterial();

	public void RefreshModel(bool updateMaterial = false)
	{
		int count = RenderSets.Count;
		if (count == 0)
		{
			return;
		}
		Material sourceMaterial = GetSourceMaterial();
		for (int i = 0; i < count; i++)
		{
			RenderSet renderSet = RenderSets[i];
			SkinnedMeshRenderer originSmr = renderSet.OriginSmr;
			SkinnedMeshRenderer cloneSmr = renderSet.CloneSmr;
			if ((Object)(object)originSmr == (Object)null)
			{
				continue;
			}
			cloneSmr.sharedMesh = originSmr.sharedMesh;
			cloneSmr.bones = originSmr.bones;
			cloneSmr.rootBone = originSmr.rootBone;
			if (updateMaterial)
			{
				int num = ((Renderer)originSmr).sharedMaterials.Length;
				if (num >= 2)
				{
					Material[] array = (Material[])(object)new Material[num];
					for (int j = 0; j < num; j++)
					{
						array[j] = sourceMaterial;
					}
					((Renderer)cloneSmr).sharedMaterials = array;
				}
				else
				{
					((Renderer)cloneSmr).sharedMaterial = sourceMaterial;
				}
			}
			renderSet.OrigianlActive = ((Component)originSmr).gameObject.activeInHierarchy;
		}
		Show = _show;
	}

	public void Add(SkinnedMeshRenderer[] renderers)
	{
		for (int i = 0; i < renderers.Length; i++)
		{
			RenderSet renderSet = new RenderSet();
			renderSet.OriginSmr = renderers[i];
			RenderSets.Add(renderSet);
		}
		SetUp();
	}

	public void Remove(SkinnedMeshRenderer[] renderers)
	{
		for (int i = 0; i < renderers.Length; i++)
		{
			for (int num = RenderSets.Count - 1; num >= 0; num--)
			{
				if ((Object)(object)RenderSets[num].OriginSmr == (Object)(object)renderers[i])
				{
					Object.Destroy((Object)(object)RenderSets[num].CloneObj);
					RenderSets.RemoveAt(num);
				}
			}
		}
	}

	private void SetUp()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		int count = RenderSets.Count;
		for (int i = 0; i < count; i++)
		{
			RenderSet renderSet = RenderSets[i];
			if (!((Object)(object)renderSet.CloneObj != (Object)null))
			{
				SkinnedMeshRenderer originSmr = renderSet.OriginSmr;
				GameObject val = new GameObject(((Object)((Component)originSmr).gameObject).name + " clone");
				if (ModyfyRenderLayer >= 0)
				{
					val.layer = ModyfyRenderLayer;
				}
				val.transform.parent = ((Component)this).gameObject.transform;
				renderSet.CloneObj = val;
				SkinnedMeshRenderer val2 = val.AddComponent<SkinnedMeshRenderer>();
				((Renderer)val2).shadowCastingMode = (ShadowCastingMode)0;
				((Renderer)val2).receiveShadows = false;
				renderSet.CloneSmr = val2;
				Transform transform = val.transform;
				Transform transform2 = ((Component)originSmr).transform;
				transform.position = transform2.position;
				transform.rotation = transform2.rotation;
				transform.localScale = transform2.localScale;
			}
		}
		RefreshModel(updateMaterial: true);
	}
}
