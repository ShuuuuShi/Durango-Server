using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Terrain;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

public class ImmovableBase : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CCoHighlighting_003Ed__41 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public ImmovableBase _003C_003E4__this;

		private Color _003CdefaultColor_003E5__2;

		private float _003CselectedAt_003E5__3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCoHighlighting_003Ed__41(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			ImmovableBase immovableBase = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CdefaultColor_003E5__2 = immovableBase.GetDefaultColor();
				_003CselectedAt_003E5__3 = Time.time;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (immovableBase._hovered || immovableBase._selected)
			{
				Color color = _003CdefaultColor_003E5__2;
				float num2 = 1f + 0.15f * (Mathf.Sin((Time.time - _003CselectedAt_003E5__3) * 4f) + 1f);
				color.r *= num2;
				color.g *= num2;
				color.b *= num2;
				immovableBase.SetColor(color);
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			immovableBase.SetColor(immovableBase.GetDefaultColor());
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

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

	public virtual Vector3 Center => Util.WorldPositionToClientPosition(Util.TilePositionToWorldPosition(WorldTile) + new Vector3(100f, Floor.GetValueOrDefault() * 200, 100f));

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
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoHighlighting_003Ed__41(0)
		{
			_003C_003E4__this = this
		};
	}

	public virtual string GetName()
	{
		return base.name;
	}
}
