using System;
using System.Collections;
using UnityEngine;

public class ImmovableBase : MonoBehaviour
{
	public const float DefaultInteractionDistance = 100f;

	protected bool Selected;

	protected bool IsGlitter;

	private bool _hasInteractionTransform;

	private Transform _interactionTransform;

	[ExposedInEditor(false, null)]
	public ulong EntityId { get; private set; }

	[ExposedInEditor(false, null)]
	public ushort EntityType { get; private set; }

	[ExposedInEditor(false, null)]
	public Point2 WorldTile { get; private set; }

	public virtual Vector3 Center
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = TerrainA6.TilePositionToWorldPosition(WorldTile);
			return TerrainA6.WorldPositionToClientPosition(val + new Vector3(100f, 0f, 100f));
		}
	}

	public Transform InteractionTransform
	{
		get
		{
			if (!_hasInteractionTransform)
			{
				_hasInteractionTransform = true;
				_interactionTransform = KUtility.FindTransformByName(((Component)this).gameObject, "interaction_point");
			}
			return _interactionTransform;
		}
	}

	public virtual Vector3 InteractionPosition
	{
		get
		{
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)InteractionTransform == (Object)null)
			{
				return Center + Vector3.up * 200f * 0.5f;
			}
			return InteractionTransform.position;
		}
	}

	public virtual float InteractionDistance => 100f;

	public void SetEntity(ulong entityId, ushort entityType, Point2 worldTile)
	{
		EntityId = entityId;
		EntityType = entityType;
		WorldTile = worldTile;
	}

	public void UpdateEntityId(ulong entityId)
	{
		EntityId = entityId;
	}

	protected virtual Color GetDefaultColor()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return Color.white;
	}

	protected virtual void SetColor(Color color)
	{
	}

	protected virtual void OnSelected(bool selected)
	{
		if (Selected != selected)
		{
			Selected = selected;
			if (Selected)
			{
				((MonoBehaviour)this).StartCoroutine(CoSelected());
			}
		}
	}

	private IEnumerator CoSelected()
	{
		Color defaultColor = GetDefaultColor();
		float selectedAt = Time.time;
		while (Selected)
		{
			Color color = defaultColor;
			float val = 0.5f + 0.25f * (Mathf.Cos((Time.time - selectedAt) * 5f) + 1f);
			color.g *= val;
			color.b *= val;
			SetColor(color);
			yield return null;
		}
		SetColor(GetDefaultColor());
	}

	protected virtual void OnGlitter(float delay)
	{
		if (!IsGlitter)
		{
			((MonoBehaviour)this).StartCoroutine(CoGlitter(delay));
		}
	}

	private IEnumerator CoGlitter(float delay)
	{
		IsGlitter = true;
		if (delay > 0f)
		{
			yield return (object)new WaitForSeconds(delay);
		}
		Color defaultColor = GetDefaultColor();
		float startAt = Time.time;
		while (Time.time - startAt < 1f)
		{
			Color color2 = defaultColor;
			float val = 0.5f * (Mathf.Cos((Time.time - startAt) * (float)Math.PI * 2f) + 1f);
			float a = color2.a;
			color2 = Color.Lerp(Color.clear, color2, val);
			color2.a = a;
			SetColor(color2);
			yield return null;
		}
		SetColor(GetDefaultColor());
		IsGlitter = false;
	}

	public virtual string GetName()
	{
		return ((Object)this).name;
	}
}
