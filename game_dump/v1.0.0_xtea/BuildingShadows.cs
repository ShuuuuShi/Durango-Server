using System;
using UnityEngine;
using UnityEngine.Rendering;

public class BuildingShadows : MonoBehaviour
{
	[SerializeField]
	private Material _material;

	[SerializeField]
	private MeshRenderer[] _renderers;

	private GameObject[] _cloneObjects;

	public void Show(bool show)
	{
		if (_cloneObjects != null)
		{
			int num = _cloneObjects.Length;
			for (int i = 0; i < num; i++)
			{
				_cloneObjects[i].SetActive(show);
			}
		}
	}

	public void SetUp(Func<MeshRenderer, bool> skipFunc = null)
	{
		if (_cloneObjects != null)
		{
			RemoveShadow();
		}
		MeshRenderer[] componentsInChildren = ((Component)this).GetComponentsInChildren<MeshRenderer>();
		if (componentsInChildren != null && componentsInChildren.Length != 0 && !((Object)(object)componentsInChildren[0] == (Object)null))
		{
			_renderers = componentsInChildren;
			_cloneObjects = MakeShadow(_renderers, skipFunc);
		}
	}

	public GameObject[] MakeShadow(MeshRenderer[] meshRenderers, Func<MeshRenderer, bool> skipFunc = null)
	{
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected O, but got Unknown
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		if (meshRenderers == null || meshRenderers.Length == 0 || (Object)(object)meshRenderers[0] == (Object)null)
		{
			return null;
		}
		int num = 0;
		int i = 0;
		for (int num2 = ((skipFunc != null) ? meshRenderers.Length : 0); i < num2; i++)
		{
			if ((Object)(object)meshRenderers[i] == (Object)null || (skipFunc != null && skipFunc(meshRenderers[i])))
			{
				num++;
			}
		}
		int num3 = meshRenderers.Length - num;
		GameObject[] array = (GameObject[])(object)new GameObject[num3];
		int num4 = 0;
		foreach (MeshRenderer val in meshRenderers)
		{
			if (skipFunc != null && skipFunc(val))
			{
				continue;
			}
			MeshFilter component = ((Component)val).gameObject.GetComponent<MeshFilter>();
			if ((Object)(object)component == (Object)null)
			{
				continue;
			}
			GameObject val2 = new GameObject(((Object)((Component)val).gameObject).name + "_shadow");
			val2.transform.parent = ((Component)val).gameObject.transform;
			array[num4] = val2;
			MeshFilter val3 = val2.AddComponent<MeshFilter>();
			val3.sharedMesh = component.sharedMesh;
			MeshRenderer val4 = val2.AddComponent<MeshRenderer>();
			((Renderer)val4).shadowCastingMode = (ShadowCastingMode)0;
			((Renderer)val4).receiveShadows = false;
			Transform transform = val2.transform;
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			transform.localScale = Vector3.one;
			int num5 = ((Renderer)val).sharedMaterials.Length;
			if (num5 >= 2)
			{
				Material[] array2 = (Material[])(object)new Material[num5];
				for (int k = 0; k < num5; k++)
				{
					array2[k] = _material;
				}
				((Renderer)val4).sharedMaterials = array2;
			}
			else
			{
				((Renderer)val4).sharedMaterial = _material;
			}
			num4++;
		}
		return array;
	}

	public void RemoveShadow()
	{
		if (_cloneObjects == null)
		{
			return;
		}
		for (int i = 0; i < _cloneObjects.Length; i++)
		{
			if (Application.isPlaying)
			{
				Object.Destroy((Object)(object)_cloneObjects[i]);
			}
			else
			{
				Object.DestroyImmediate((Object)(object)_cloneObjects[i]);
			}
		}
		_cloneObjects = null;
		_renderers = null;
	}
}
