using System.Collections;
using Durango.Terrain;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

public class ImmovableBase : MonoBehaviour
{
	public const float DefaultInteractionDistance = 100f;

	private bool _hovered;

	private bool _selected;

	private bool _isDirtyInteractionTransform;

	private Transform _interactionTransform;

	private ICoroutineBinder _highlightBinder;

	private ChatableBase _chatable;

	[ExposedInEditor(null)]
	public string EntityId { get; private set; }

	[ExposedInEditor(false, null)]
	public ushort EntityType { get; private set; }

	[ExposedInEditor(false, null)]
	public Point2 WorldTile { get; private set; }

	public virtual int? Floor => null;

	public virtual Vector3 Center
	{
		get
		{
			Vector3 vector = Util.TilePositionToWorldPosition(WorldTile);
			return Util.WorldPositionToClientPosition(vector + new Vector3(100f, Floor.GetValueOrDefault() * 200, 100f));
		}
	}

	public Transform InteractionTransform
	{
		get
		{
			if (!_isDirtyInteractionTransform)
			{
				_isDirtyInteractionTransform = true;
				_interactionTransform = KUtility.FindTransformByName(base.gameObject, "interaction_point");
			}
			return _interactionTransform;
		}
	}

	public virtual Vector3 InteractionPosition
	{
		get
		{
			if (InteractionTransform == null)
			{
				return Center + Vector3.up * 200f * 0.5f;
			}
			return InteractionTransform.position;
		}
	}

	public virtual float InteractionDistance => 100f;

	[NotNull]
	public ChatableBase ChatableBase
	{
		get
		{
			if (_chatable == null)
			{
				_chatable = CreateChatableBase();
			}
			return _chatable;
		}
	}

	protected virtual ChatableBase CreateChatableBase()
	{
		return new ChatableImmovable<ImmovableBase>(this);
	}

	public void SetEntity(string entityId, ushort entityType, Point2 worldTile)
	{
		EntityId = entityId;
		EntityType = entityType;
		WorldTile = worldTile;
		OnSetEntity();
	}

	public void UpdateEntityId(string entityId)
	{
		EntityId = entityId;
		OnUpdateEntityId();
	}

	protected void SetDirtyInteractionTransform()
	{
		_isDirtyInteractionTransform = false;
	}

	protected virtual void OnSetEntity()
	{
	}

	protected virtual void OnUpdateEntityId()
	{
	}

	protected virtual Color GetDefaultColor()
	{
		return Color.white;
	}

	protected virtual void SetColor(Color color)
	{
	}

	public void Hover(bool hovered)
	{
		if (_hovered != hovered)
		{
			_hovered = hovered;
			if (_hovered)
			{
				this.StartCoroutine(ref _highlightBinder, CoHighlighting());
			}
		}
	}

	public virtual void Select(bool selected)
	{
		if (_selected != selected)
		{
			_selected = selected;
			if (_selected)
			{
				this.StartCoroutine(ref _highlightBinder, CoHighlighting());
			}
		}
	}

	private IEnumerator CoHighlighting()
	{
		Color defaultColor = GetDefaultColor();
		float selectedAt = Time.time;
		while (_hovered || _selected)
		{
			Color color = defaultColor;
			float val = 1f + 0.15f * (Mathf.Sin((Time.time - selectedAt) * 4f) + 1f);
			color.r *= val;
			color.g *= val;
			color.b *= val;
			SetColor(color);
			yield return null;
		}
		SetColor(GetDefaultColor());
	}

	public virtual string GetName()
	{
		return base.name;
	}
}
