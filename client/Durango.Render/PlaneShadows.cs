using System.Collections.Generic;
using Durango.Utils;
using UnityEngine;

namespace Durango.Render;

public class PlaneShadows : VisibleObject
{
	private const string SimpleShadowPath = "Models/Effect/character_ao.prefab";

	private GameObject _simpleShadow;

	private readonly List<SkinnedMeshRenderer> _skinnedMeshRenderers = new List<SkinnedMeshRenderer>();

	private ShadowOption _option;

	protected new void Awake()
	{
		base.Awake();
		RefreshOption(force: true);
	}

	protected new void Start()
	{
		base.Start();
		PlaneShadowManager.OptionChanged += PlaneManager_OptionChanged;
	}

	private void OnDestroy()
	{
		PlaneShadowManager.OptionChanged -= PlaneManager_OptionChanged;
	}

	private void PlaneManager_OptionChanged()
	{
		RefreshOption();
	}

	private void OnOptionUpdated(ShadowOption prev, ShadowOption cur)
	{
		if (prev != cur)
		{
			Clear();
		}
		switch (cur)
		{
		case ShadowOption.Normal:
			MeshCloner.Add(base.transform, _skinnedMeshRenderers, Singleton<PlaneShadowManager>.Instance().Material);
			RefreshVisiblility();
			break;
		case ShadowOption.Simple:
			if (_simpleShadow != null)
			{
				break;
			}
			Singleton<AssetBundleManager>.Instance().RequestAsset("Models/Effect/character_ao.prefab", typeof(GameObject), delegate(Object asset)
			{
				if (!(this == null) && !(asset == null) && _option == ShadowOption.Simple)
				{
					GameObject gameObject = (GameObject)Object.Instantiate(asset);
					if (!(gameObject == null))
					{
						_simpleShadow = gameObject;
						_simpleShadow.transform.parent = base.transform;
						_simpleShadow.transform.localPosition = Vector3.zero;
						_simpleShadow.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
						CapsuleCollider component = GetComponent<CapsuleCollider>();
						float num = ((!(component != null)) ? 200f : component.height) / base.transform.localScale.x;
						_simpleShadow.transform.localScale = new Vector3(num, num, 1f);
						RefreshVisiblility();
					}
				}
			});
			break;
		}
	}

	private void RefreshVisiblility()
	{
		OnVisibleChanged(IsVisible());
	}

	protected override void OnVisibleChanged(bool visible)
	{
		switch (_option)
		{
		case ShadowOption.Normal:
			MeshCloner.SetVisible(visible);
			break;
		case ShadowOption.Simple:
			if (_simpleShadow != null)
			{
				_simpleShadow.gameObject.SetActive(visible);
			}
			break;
		}
	}

	public void Clear()
	{
		MeshCloner.RemoveAll();
		if (_simpleShadow != null)
		{
			Object.Destroy(_simpleShadow);
		}
	}

	public override void Add(SkinnedMeshRenderer[] renderers)
	{
		if (_option == ShadowOption.Normal)
		{
			MeshCloner.Add(base.transform, renderers, Singleton<PlaneShadowManager>.Instance().Material);
		}
		if (renderers != null)
		{
			_skinnedMeshRenderers.AddRange(renderers);
		}
	}

	public override void Remove(SkinnedMeshRenderer[] renderers)
	{
		if (_option == ShadowOption.Normal)
		{
			MeshCloner.Remove(renderers);
		}
		if (renderers != null)
		{
			foreach (SkinnedMeshRenderer item in renderers)
			{
				_skinnedMeshRenderers.Remove(item);
			}
		}
	}

	public void RefreshOption(bool force = false)
	{
		ShadowOption option = _option;
		ShadowOption shadowOption = PlaneShadowManager.Option;
		if (PlayerBehavior.LocalPlayer != null && base.gameObject == PlayerBehavior.LocalPlayer.gameObject)
		{
			shadowOption = ShadowOption.Normal;
		}
		if (option != shadowOption || force)
		{
			_option = shadowOption;
			OnOptionUpdated(option, shadowOption);
		}
	}
}
