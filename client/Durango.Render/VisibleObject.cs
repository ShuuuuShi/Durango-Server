using System;
using Durango.Model;
using UnityEngine;

namespace Durango.Render;

public abstract class VisibleObject : MonoBehaviour, IBoneMergedObserver
{
	[Flags]
	public enum Mask
	{
		Default = 1,
		Enabled = 2,
		Render = 4,
		Inside = 8
	}

	private int _visibleMask;

	private SkinnedMeshRenderer[] _renderers;

	protected readonly MeshCloner MeshCloner = new MeshCloner();

	void IBoneMergedObserver.OnAttached(SkinnedMeshRenderer[] renderers)
	{
		Add(renderers);
	}

	void IBoneMergedObserver.OnDetached(SkinnedMeshRenderer[] renderers)
	{
		Remove(renderers);
	}

	protected void Awake()
	{
		_renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
	}

	protected void Start()
	{
		Add(_renderers);
	}

	public virtual void SetVisible(bool visible, Mask mask = Mask.Default)
	{
		bool flag = IsVisible();
		if (visible)
		{
			_visibleMask &= (int)(~mask);
		}
		else
		{
			_visibleMask |= (int)mask;
		}
		bool flag2 = IsVisible();
		if (flag != flag2)
		{
			OnVisibleChanged(flag2);
		}
	}

	public bool IsVisible()
	{
		return _visibleMask == 0;
	}

	protected virtual void OnVisibleChanged(bool visible)
	{
	}

	public void RefreshModel(bool updateMaterial = false)
	{
		MeshCloner.RefreshModel(updateMaterial);
	}

	public abstract void Add(SkinnedMeshRenderer[] renderers);

	public abstract void Remove(SkinnedMeshRenderer[] renderers);
}
