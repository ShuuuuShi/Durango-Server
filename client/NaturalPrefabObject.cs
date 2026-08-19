using Durango.Render;
using Durango.Terrain;
using Durango.Utils;
using UnityEngine;

public class NaturalPrefabObject : NaturalObject
{
	private RendererProxy _rendererProxy;

	public override float InteractionDistance
	{
		get
		{
			Collider componentInChildren = GetComponentInChildren<Collider>();
			if (componentInChildren != null)
			{
				Vector3 extents = componentInChildren.bounds.extents;
				return Mathf.Sqrt(extents.x * extents.x + extents.z * extents.z);
			}
			return 100f;
		}
	}

	public string PoolName { get; set; }

	private RendererProxy RendererProxy
	{
		get
		{
			if (_rendererProxy == null)
			{
				_rendererProxy = new RendererProxy();
				_rendererProxy.UpdateRenderers(base.gameObject);
			}
			return _rendererProxy;
		}
	}

	public override void OnRemoved(TerrainChunkBase chunk, bool fastRemove)
	{
		ReturnToPoolAndDeactive();
	}

	protected void ReturnToPoolAndDeactive()
	{
		Singleton<StaticObjectPool>.Instance().ReturnObject(PoolName, base.gameObject);
		PoolName = null;
		base.gameObject.SetActive(value: false);
	}

	protected override Color GetDefaultColor()
	{
		return Color.white;
	}

	protected override void SetColor(Color color)
	{
		RendererProxy.SetColor(color);
	}

	public void SetThreeColor(ThreeColor color)
	{
		RendererProxy.SetThreeColor(color);
	}
}
