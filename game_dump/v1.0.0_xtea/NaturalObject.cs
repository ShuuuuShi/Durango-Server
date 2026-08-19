using UnityEngine;
using Yaml.Util;

public class NaturalObject : ImmovableBase
{
	private RendererProxy _rendererProxy;

	private Vector3 _interactionOffset = Vector3.up * 50f;

	public NaturalComponent NaturalComponent { get; set; }

	public override float InteractionDistance
	{
		get
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			if (KSprite != null)
			{
				return 100f;
			}
			Collider componentInChildren = ((Component)this).GetComponentInChildren<Collider>();
			if ((Object)(object)componentInChildren != (Object)null)
			{
				Bounds bounds = componentInChildren.bounds;
				Vector3 extents = ((Bounds)(ref bounds)).extents;
				return Mathf.Sqrt(extents.x * extents.x + extents.z * extents.z);
			}
			return 100f;
		}
	}

	public override Vector3 InteractionPosition
	{
		get
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = ((KSprite != null) ? KSprite.GameObject.transform.position : ((Component)this).transform.position);
			return val + _interactionOffset;
		}
	}

	public KSprite KSprite { get; set; }

	private RendererProxy RendererProxy
	{
		get
		{
			if (_rendererProxy == null)
			{
				_rendererProxy = new RendererProxy();
				_rendererProxy.UpdateRenderers(((Component)this).GetComponentsInChildren<Renderer>());
			}
			return _rendererProxy;
		}
	}

	protected override Color GetDefaultColor()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Color white = Color.white;
		if (KSprite != null)
		{
			white.a = KSprite.GetColor().a;
		}
		return white;
	}

	protected override void SetColor(Color color)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (KSprite != null)
		{
			KSprite.SetColor(color);
		}
		else
		{
			RendererProxy.Color = color;
		}
	}

	public void SetInteractionOffset(Vector3 offset)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		_interactionOffset = offset;
	}

	public override string GetName()
	{
		BiomeSpriteInfo biomeSpriteInfo = TerrainDataHelper.GetBiomeSpriteInfo(base.EntityType);
		string text = ((biomeSpriteInfo != null) ? SingletonDict<string, Gettext>.Get(biomeSpriteInfo.CollectibleId) : ((Gettext)null));
		return (!string.IsNullOrEmpty(text)) ? text : base.EntityType.ToString();
	}
}
