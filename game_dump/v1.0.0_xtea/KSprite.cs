using JetBrains.Annotations;
using UnityEngine;

public class KSprite
{
	[NotNull]
	private readonly tk2dSprite _tk2DSprite;

	private readonly BoxCollider _boxCollider;

	private string _spriteName;

	private bool _hasShadow;

	private tk2dSprite _shadowSprite;

	private bool _hasAdditive;

	private GameObject _additiveObject;

	public SpriteObjectType SpriteObjectType { get; private set; }

	public GameObject GameObject { get; private set; }

	public NaturalObject NaturalObject { get; private set; }

	public string StumpName { get; private set; }

	public KSprite(GameObject obj)
	{
		GameObject = obj;
		NaturalObject = obj.GetComponent<NaturalObject>();
		if ((Object)(object)NaturalObject != (Object)null)
		{
			NaturalObject.KSprite = this;
		}
		_tk2DSprite = obj.GetComponent<tk2dSprite>();
		_boxCollider = obj.GetComponent<BoxCollider>();
	}

	public Color GetColor()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return _tk2DSprite.color;
	}

	public void SetColor(Color color, bool applyShadow = false)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		_tk2DSprite.color = color;
		if (applyShadow && _hasShadow && (Object)(object)_shadowSprite != (Object)null)
		{
			_shadowSprite.color = color;
		}
	}

	public void SetAlpha(float alpha)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Color color = _tk2DSprite.color;
		color.a = alpha;
		SetColor(color, applyShadow: true);
	}

	public bool SetSpriteByName(SpriteObjectType spriteObjectType, string spriteName, string stumpName = null, float brightness = 1f)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		SpriteObjectType = spriteObjectType;
		KSpriteGroup kSpriteGroup = KSingleton<SpriteManager>.Instance().GetKSpriteGroup(SpriteObjectType);
		if (kSpriteGroup == null)
		{
			return false;
		}
		_hasShadow = kSpriteGroup.HasShadow;
		_hasAdditive = kSpriteGroup.HasAdditive;
		_spriteName = spriteName;
		StumpName = stumpName;
		float num = ((SpriteObjectType != 0) ? 1f : KSingleton<SpriteManager>.Instance().GrassAlpha);
		_tk2DSprite.color = new Color(brightness, brightness, brightness, num);
		UpdateNaturalComponent();
		UpdateSpriteCollection();
		return true;
	}

	public void CheckLoaded()
	{
		if (!GameObject.activeSelf)
		{
			UpdateSpriteCollection();
		}
	}

	private void TryGetInteractionOffset(ref Vector3 offset)
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		tk2dSpriteCollectionData collection = _tk2DSprite.Collection;
		if ((Object)(object)collection == (Object)null || collection.spriteDefinitions == null || collection.spriteDefinitions.Length <= _tk2DSprite.spriteId)
		{
			return;
		}
		tk2dSpriteDefinition tk2dSpriteDefinition2 = collection.spriteDefinitions[_tk2DSprite.spriteId];
		tk2dSpriteDefinition.AttachPoint[] attachPoints = tk2dSpriteDefinition2.attachPoints;
		if (attachPoints == null)
		{
			return;
		}
		int i = 0;
		for (int num = attachPoints.Length; i < num; i++)
		{
			if (!(attachPoints[i].name != "Interaction"))
			{
				offset = attachPoints[i].position;
				offset.x *= _tk2DSprite.scale.x * ((Component)_tk2DSprite).transform.lossyScale.x;
				offset.y *= _tk2DSprite.scale.y * ((Component)_tk2DSprite).transform.lossyScale.y;
				offset.z = 0f - (offset.x /= Mathf.Sqrt(2f));
				break;
			}
		}
	}

	private void UpdateNaturalComponent()
	{
		if ((Object)(object)NaturalObject == (Object)null)
		{
			return;
		}
		switch (SpriteObjectType)
		{
		case SpriteObjectType.Tree:
			if (!(NaturalObject.NaturalComponent is TreeComponent))
			{
				NaturalObject.NaturalComponent = new TreeComponent(NaturalObject);
			}
			break;
		case SpriteObjectType.Shrub:
			if (!(NaturalObject.NaturalComponent is ShrubComponent))
			{
				NaturalObject.NaturalComponent = new ShrubComponent(NaturalObject);
			}
			break;
		case SpriteObjectType.Rock:
		case SpriteObjectType.Pebble:
			NaturalObject.NaturalComponent = null;
			break;
		}
	}

	private void UpdateSpriteCollection()
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		SpriteCollectionInfo spriteCollectionInfo = KSingleton<SpriteManager>.Instance().GetSpriteCollectionInfo(_spriteName);
		if (spriteCollectionInfo != null && !((Object)(object)spriteCollectionInfo.SpriteCollectionData == (Object)null))
		{
			int spriteIdByName = spriteCollectionInfo.SpriteCollectionData.GetSpriteIdByName(_spriteName);
			_tk2DSprite.SetSprite(spriteCollectionInfo.SpriteCollectionData, spriteIdByName);
			UpdateShadow(spriteCollectionInfo, spriteIdByName);
			UpdateAdditive(spriteCollectionInfo);
			UpdateCollider();
			GameObject.SetActive(true);
			if ((Object)(object)NaturalObject != (Object)null)
			{
				Vector3 offset = ((SpriteObjectType != SpriteObjectType.Tree) ? (Vector3.up * 50f) : (Vector3.up * 150f));
				TryGetInteractionOffset(ref offset);
				NaturalObject.SetInteractionOffset(offset);
			}
		}
	}

	private void UpdateShadow([NotNull] SpriteCollectionInfo info, int spriteId)
	{
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		if (!_hasShadow)
		{
			if ((Object)(object)_shadowSprite != (Object)null)
			{
				((Component)_shadowSprite).gameObject.SetActive(false);
			}
			return;
		}
		if ((Object)(object)_shadowSprite == (Object)null)
		{
			GameObject val = Object.Instantiate<GameObject>(KSingleton<SpriteManager>.Instance().SpriteRenderingObject);
			val.transform.parent = GameObject.transform;
			((Object)val).name = "Shadow";
			val.tag = "Shadow";
			Transform transform = val.transform;
			transform.localRotation = Quaternion.identity;
			transform.localPosition = Vector3.zero;
			transform.localScale = Vector3.one;
			_shadowSprite = val.GetComponent<tk2dSprite>();
		}
		((Component)_shadowSprite).gameObject.SetActive(true);
		_shadowSprite.SetSprite(info.SpriteCollectionData, spriteId);
		_shadowSprite.scale = _tk2DSprite.scale;
		_shadowSprite.color = Color.white;
		((Component)_shadowSprite).GetComponent<Renderer>().sharedMaterial = info.ShadowMaterial;
		Bounds bounds = _shadowSprite.mesh.bounds;
		((Bounds)(ref bounds)).SetMinMax(((Bounds)(ref bounds)).min, ((Bounds)(ref bounds)).max + new Vector3(((Bounds)(ref bounds)).extents.x * 2f, 0f, 0f));
		_shadowSprite.mesh.bounds = bounds;
	}

	private void UpdateAdditive([NotNull] SpriteCollectionInfo info)
	{
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		if (_hasAdditive)
		{
			int spriteIdByName = info.SpriteCollectionData.GetSpriteIdByName(_spriteName + "_light", -1);
			if (spriteIdByName == -1)
			{
				_hasAdditive = false;
			}
			else
			{
				if ((Object)(object)_additiveObject == (Object)null)
				{
					_additiveObject = Object.Instantiate<GameObject>(KSingleton<SpriteManager>.Instance().SpriteRenderingObject);
					_additiveObject.transform.parent = GameObject.transform;
					((Object)_additiveObject).name = "Additive";
					_additiveObject.layer = OverlayCamera.Layer;
					Transform transform = _additiveObject.transform;
					transform.localRotation = Quaternion.identity;
					transform.localPosition = Vector3.zero;
					transform.localScale = Vector3.one;
				}
				_additiveObject.SetActive(true);
				tk2dSprite component = _additiveObject.GetComponent<tk2dSprite>();
				component.SetSprite(info.SpriteCollectionData, spriteIdByName);
				component.scale = _tk2DSprite.scale;
				_additiveObject.GetComponent<Renderer>().sharedMaterial = info.AdditiveMaterial;
				AdditiveSpriteModifier additiveSpriteModifier = _additiveObject.GetComponent<AdditiveSpriteModifier>();
				if ((Object)(object)additiveSpriteModifier == (Object)null)
				{
					additiveSpriteModifier = _additiveObject.AddComponent<AdditiveSpriteModifier>();
				}
				additiveSpriteModifier.Initialize(component);
			}
		}
		if (!_hasAdditive && (Object)(object)_additiveObject != (Object)null)
		{
			_additiveObject.SetActive(false);
		}
	}

	private void UpdateCollider()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_boxCollider == (Object)null))
		{
			Bounds bounds = _tk2DSprite.GetBounds();
			_boxCollider.center = ((Bounds)(ref bounds)).center;
			_boxCollider.size = ((Bounds)(ref bounds)).size;
			((Collider)_boxCollider).enabled = true;
			((Collider)_boxCollider).isTrigger = SpriteObjectType != SpriteObjectType.Rock;
			TreeComponent treeComponent = NaturalObject.NaturalComponent as TreeComponent;
			if ((bool)treeComponent)
			{
				treeComponent.SpriteHeight = ((Bounds)(ref bounds)).size.y;
			}
		}
	}

	public void SetMeshVertices(Vector3[] vertices)
	{
		_tk2DSprite.mesh.vertices = vertices;
	}

	public Vector3[] GetMeshVertices()
	{
		return _tk2DSprite.mesh.vertices;
	}

	public Vector3[] GetBaseVertices()
	{
		return _tk2DSprite.meshVertices;
	}
}
