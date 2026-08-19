using System;
using Durango.Render.Camera;
using Durango.Render.Particle;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.Render.Sprite;

public class Sprite
{
	private const float RockColliderSizeZ = 100f;

	[NotNull]
	private readonly tk2dSprite _tk2DSprite;

	private readonly BoxCollider _boxCollider;

	[NotNull]
	private string _spriteName = string.Empty;

	private bool _hasShadow;

	private tk2dSprite _shadowSprite;

	private bool _hasAdditive;

	private GameObject _additiveObject;

	private float _brightnessRatio;

	private float _yawRatio;

	private float _scaleRatio;

	private Vector2 _antiDepthFighting;

	private int _particleId;

	private string _particleName;

	public SpriteObjectType SpriteObjectType { get; private set; }

	public tk2dSpriteDefinition.ColliderSizeType SpriteColliderSize { get; private set; }

	public GameObject GameObject { get; private set; }

	public NaturalSpriteObject NaturalObject { get; private set; }

	public bool IsSwayable { get; private set; }

	public bool IsShakable { get; private set; }

	public string StumpName { get; private set; }

	public Quaternion InitialRotation { get; private set; }

	public event Action TransformUpdated;

	public Sprite(GameObject obj)
	{
		GameObject = obj;
		NaturalObject = obj.GetComponent<NaturalSpriteObject>();
		if (NaturalObject != null)
		{
			NaturalObject.Sprite = this;
		}
		_tk2DSprite = obj.GetComponent<tk2dSprite>();
		_boxCollider = obj.GetComponent<BoxCollider>();
	}

	public tk2dSpriteDefinition GetSpriteDefinition()
	{
		if (_tk2DSprite.Collection == null || _tk2DSprite.Collection.spriteDefinitions.Length <= _tk2DSprite.spriteId)
		{
			return null;
		}
		return _tk2DSprite.Collection.spriteDefinitions[_tk2DSprite.spriteId];
	}

	public Color GetColor()
	{
		return _tk2DSprite.color;
	}

	public void SetColor(Color color, bool applyShadow = false)
	{
		_tk2DSprite.color = color;
		if (applyShadow && _hasShadow && _shadowSprite != null)
		{
			_shadowSprite.color = color;
		}
	}

	public void SetAlpha(float alpha)
	{
		Color color = _tk2DSprite.color;
		color.a = alpha;
		SetColor(color, applyShadow: true);
	}

	public bool SetSpriteByName(SpriteObjectType spriteObjectType, [NotNull] string spriteName, bool allowAdditive = true, string particle = null)
	{
		SpriteObjectType = spriteObjectType;
		SpriteGroup kSpriteGroup = Singleton<SpriteManager>.Instance().GetKSpriteGroup(SpriteObjectType);
		if (kSpriteGroup == null)
		{
			return false;
		}
		_hasShadow = kSpriteGroup.HasShadow;
		_hasAdditive = kSpriteGroup.HasAdditive && allowAdditive;
		_spriteName = spriteName;
		_particleName = particle;
		UpdateNaturalComponent();
		UpdateSpriteCollection();
		return true;
	}

	public void SetTransformParams(float scaleRatio, float yawRatio, Vector2 antiDepthFighting, float brightnessRatio)
	{
		_scaleRatio = scaleRatio;
		_yawRatio = yawRatio;
		_antiDepthFighting = antiDepthFighting;
		_brightnessRatio = brightnessRatio;
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
		tk2dSpriteDefinition spriteDefinition = GetSpriteDefinition();
		if (spriteDefinition == null)
		{
			return;
		}
		tk2dSpriteDefinition.AttachPoint[] attachPoints = spriteDefinition.attachPoints;
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
				offset.x *= _tk2DSprite.scale.x * _tk2DSprite.transform.lossyScale.x;
				offset.y *= _tk2DSprite.scale.y * _tk2DSprite.transform.lossyScale.y;
				offset.z = 0f - (offset.x /= Mathf.Sqrt(2f));
				break;
			}
		}
	}

	private void UpdateUV2()
	{
		if (!(_tk2DSprite.mesh == null) && _tk2DSprite.mesh.vertices.Length != 0 && SpriteObjectType == SpriteObjectType.Puddle && (_tk2DSprite.mesh.uv2 == null || _tk2DSprite.mesh.uv2.Length == 0))
		{
			Vector2[] array = new Vector2[_tk2DSprite.mesh.vertexCount];
			Vector2 vector = (array[0] = new Vector2(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f)));
			ref Vector2 reference = ref array[1];
			reference = vector + new Vector2(0.8f, 0f);
			ref Vector2 reference2 = ref array[2];
			reference2 = vector + new Vector2(0f, 0.5f);
			ref Vector2 reference3 = ref array[3];
			reference3 = vector + new Vector2(0.8f, 0.5f);
			_tk2DSprite.mesh.uv2 = array;
		}
	}

	private void UpdateNaturalComponent()
	{
		if (NaturalObject == null)
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
		{
			ShrubComponent shrubComponent = NaturalObject.NaturalComponent as ShrubComponent;
			if (shrubComponent == null)
			{
				NaturalObject.NaturalComponent = new ShrubComponent(NaturalObject);
			}
			else
			{
				shrubComponent.RefreshShakenVertices();
			}
			break;
		}
		case SpriteObjectType.Rock:
		case SpriteObjectType.Pebble:
			NaturalObject.NaturalComponent = null;
			break;
		}
	}

	private void UpdateSpriteCollection()
	{
		SpriteCollectionInfo spriteCollectionInfo = Singleton<SpriteManager>.Instance().GetSpriteCollectionInfo(_spriteName);
		if (spriteCollectionInfo != null && !(spriteCollectionInfo.SpriteCollectionData == null))
		{
			int spriteIdByName = spriteCollectionInfo.SpriteCollectionData.GetSpriteIdByName(_spriteName);
			_tk2DSprite.SetSprite(spriteCollectionInfo.SpriteCollectionData, spriteIdByName);
			UpdateUV2();
			UpdateShadow(spriteCollectionInfo, spriteIdByName);
			UpdateAdditive(spriteCollectionInfo);
			UpdateCollider();
			UpdateTransform();
			GameObject.SetActive(value: true);
			if (_particleId != 0)
			{
				ParticleManager.Stop(_particleId);
			}
			if (!string.IsNullOrEmpty(_particleName))
			{
				_particleId = ParticleManager.EmitFollow(_particleName, Vector3.zero, Quaternion.identity, GameObject.transform, useLocalPosition: true, comeForwardToCamera: false, groundDecal: false, default(Vector3), null, reusable: true, limit: false);
			}
			if (NaturalObject != null)
			{
				Vector3 offset = ((SpriteObjectType != SpriteObjectType.Tree) ? (Vector3.up * 50f) : (Vector3.up * 150f));
				TryGetInteractionOffset(ref offset);
				NaturalObject.SetInteractionOffset(offset);
			}
		}
	}

	public void UpdateTransform()
	{
		tk2dSpriteDefinition spriteDefinition = GetSpriteDefinition();
		if (spriteDefinition != null)
		{
			StumpName = spriteDefinition.StumpName;
			IsSwayable = spriteDefinition.IsSwayable;
			IsShakable = spriteDefinition.IsShakable;
			SpriteColliderSize = spriteDefinition.SpriteColliderSize;
			float a = ((SpriteObjectType != 0 && SpriteObjectType != SpriteObjectType.Puddle) ? 1f : Singleton<SpriteManager>.Instance().GrassAlpha);
			float num = Mathf.Lerp(spriteDefinition.MinBrightness, spriteDefinition.MaxBrightness, _brightnessRatio);
			_tk2DSprite.color = new Color(num, num, num, a);
			float num2 = Mathf.Lerp(spriteDefinition.MinSizeRatio, spriteDefinition.MaxSizeRatio, _scaleRatio);
			bool flag = SpriteObjectType == SpriteObjectType.Puddle;
			GameObject.transform.localPosition = new Vector3(_antiDepthFighting.x, (!flag) ? 0f : 30f, _antiDepthFighting.y);
			GameObject.transform.localScale = new Vector3(num2 * 0.5f, num2 * ((!flag) ? 0.61f : 0.83f), 1f);
			InitialRotation = Quaternion.Euler(flag ? 90 : 0, 45f, (spriteDefinition.RandomYaw < 1) ? 0f : Mathf.Lerp(-60f, 60f, _yawRatio));
			GameObject.transform.rotation = InitialRotation;
			if (this.TransformUpdated != null)
			{
				this.TransformUpdated();
			}
		}
	}

	private void UpdateShadow([NotNull] SpriteCollectionInfo info, int spriteId)
	{
		if (!_hasShadow)
		{
			if (_shadowSprite != null)
			{
				_shadowSprite.gameObject.SetActive(value: false);
			}
			return;
		}
		if (_shadowSprite == null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Singleton<SpriteManager>.Instance().SpriteRenderingObject);
			gameObject.transform.parent = GameObject.transform;
			gameObject.name = "Shadow";
			Transform transform = gameObject.transform;
			transform.localRotation = Quaternion.identity;
			transform.localPosition = Vector3.zero;
			transform.localScale = Vector3.one;
			_shadowSprite = gameObject.GetComponent<tk2dSprite>();
		}
		_shadowSprite.gameObject.SetActive(value: true);
		_shadowSprite.SetSprite(info.SpriteCollectionData, spriteId);
		_shadowSprite.scale = _tk2DSprite.scale;
		_shadowSprite.color = Color.white;
		_shadowSprite.GetComponent<Renderer>().sharedMaterial = info.ShadowMaterial;
		Bounds bounds = _shadowSprite.mesh.bounds;
		bounds.SetMinMax(bounds.min, bounds.max + new Vector3(bounds.extents.x * 2f, 0f, 0f));
		_shadowSprite.mesh.bounds = bounds;
	}

	private void UpdateAdditive([NotNull] SpriteCollectionInfo info)
	{
		if (_hasAdditive)
		{
			int spriteIdByName = info.SpriteCollectionData.GetSpriteIdByName(_spriteName + "_light", -1);
			if (spriteIdByName == -1)
			{
				_hasAdditive = false;
			}
			else
			{
				if (_additiveObject == null)
				{
					_additiveObject = UnityEngine.Object.Instantiate(Singleton<SpriteManager>.Instance().SpriteRenderingObject);
					_additiveObject.transform.parent = GameObject.transform;
					_additiveObject.name = "Additive";
					_additiveObject.layer = OverlayCamera.Layer;
					Transform transform = _additiveObject.transform;
					transform.localRotation = Quaternion.identity;
					transform.localPosition = Vector3.zero;
					transform.localScale = Vector3.one;
				}
				_additiveObject.SetActive(value: true);
				tk2dSprite component = _additiveObject.GetComponent<tk2dSprite>();
				component.SetSprite(info.SpriteCollectionData, spriteIdByName);
				component.scale = _tk2DSprite.scale;
				_additiveObject.GetComponent<Renderer>().sharedMaterial = info.AdditiveMaterial;
				AdditiveSpriteModifier additiveSpriteModifier = _additiveObject.GetComponent<AdditiveSpriteModifier>();
				if (additiveSpriteModifier == null)
				{
					additiveSpriteModifier = _additiveObject.AddComponent<AdditiveSpriteModifier>();
				}
				additiveSpriteModifier.Initialize(component);
			}
		}
		if (!_hasAdditive && _additiveObject != null)
		{
			_additiveObject.SetActive(value: false);
		}
	}

	private void UpdateCollider()
	{
		if (!(_boxCollider == null))
		{
			Bounds spriteBounds = GetSpriteBounds();
			_boxCollider.center = spriteBounds.center;
			_boxCollider.size = spriteBounds.size;
			_boxCollider.enabled = true;
			_boxCollider.isTrigger = SpriteObjectType != SpriteObjectType.Rock;
			TreeComponent treeComponent = NaturalObject.NaturalComponent as TreeComponent;
			if ((bool)treeComponent)
			{
				treeComponent.SpriteHeight = spriteBounds.size.y;
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

	private Bounds GetSpriteBounds()
	{
		Bounds bounds = _tk2DSprite.GetBounds();
		Vector3 size = bounds.size;
		size.z = ((SpriteObjectType == SpriteObjectType.Rock) ? 100f : 0f);
		bounds.size = size;
		return bounds;
	}
}
