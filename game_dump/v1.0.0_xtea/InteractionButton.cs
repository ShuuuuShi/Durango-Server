using System;
using System.Collections.Generic;
using InteractionData;
using Messages;
using UnityEngine;

public class InteractionButton : MonoBehaviour
{
	[SerializeField]
	private UISprite _interactionIcon;

	[SerializeField]
	public float _speed;

	[SerializeField]
	public float _hideRatio;

	private UIWidget _widget;

	private Transform _transform;

	private Transform _iconTransform;

	private TweenAlpha _alphaTweener;

	private UISliceSprite _sliceSprite;

	private bool _isGradationIcon;

	private readonly Dictionary<InteractionButton, Vector3> _conflictDictionary = new Dictionary<InteractionButton, Vector3>();

	public InteractionObject InteractionTarget { get; private set; }

	public bool Valid { get; set; }

	public Vector3 PosDiff { get; private set; }

	public bool TouchFlag { get; set; }

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public Transform Transform
	{
		get
		{
			if ((Object)(object)_transform == (Object)null)
			{
				_transform = ((Component)this).transform;
			}
			return _transform;
		}
	}

	public Transform IconTransform
	{
		get
		{
			if ((Object)(object)_iconTransform == (Object)null)
			{
				_iconTransform = ((Component)_interactionIcon).transform;
			}
			return _iconTransform;
		}
	}

	public TweenAlpha AlphaTweener
	{
		get
		{
			if ((Object)(object)_alphaTweener == (Object)null)
			{
				_alphaTweener = ((Component)_interactionIcon).GetComponent<TweenAlpha>();
			}
			return _alphaTweener;
		}
	}

	public UISliceSprite SliceSprite
	{
		get
		{
			if ((Object)(object)_sliceSprite == (Object)null)
			{
				_sliceSprite = ((Component)this).GetComponent<UISliceSprite>();
				if ((Object)(object)_sliceSprite == (Object)null)
				{
					_sliceSprite = ((Component)this).gameObject.AddComponent<UISliceSprite>();
					_sliceSprite.Target = _interactionIcon;
				}
				_sliceSprite.OnPostFill = OnFillSprite;
			}
			return _sliceSprite;
		}
	}

	public UISprite Icon => _interactionIcon;

	public Color Color
	{
		set
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			float alpha = _interactionIcon.alpha;
			_interactionIcon.color = value;
			_interactionIcon.alpha = alpha;
		}
	}

	public InteractionIconType Type { get; private set; }

	public void Set(InteractionObject obj)
	{
		InteractionTarget = obj;
		switch (obj.ObjectType)
		{
		case InteractionObject.Type.Animal:
			Type = InteractionIconType.Animal;
			break;
		case InteractionObject.Type.Prop:
		{
			ulong entityId = ObjectIdentifier.GetEntityId(obj.Target);
			int entityType = ObjectIdentifier.GetEntityType(obj.Target);
			EntityTile? homePoint = GameSystem<MapSystem>.Instance().Points.HomePoint;
			bool flag = (homePoint.HasValue && homePoint.Value.EntityId == entityId) || ObjectIdentifier.IsWarphole(entityType) || ObjectIdentifier.IsCrater(entityType) || ObjectIdentifier.IsPort(entityType);
			Type = ((!flag) ? InteractionIconType.Prop : InteractionIconType.Special);
			break;
		}
		case InteractionObject.Type.PropSelectableByClient:
			Type = InteractionIconType.Prop;
			break;
		case InteractionObject.Type.PrologueSelectCharacter:
			Type = InteractionIconType.PrologueCharacter;
			break;
		case InteractionObject.Type.Vehicle:
			Type = InteractionIconType.Prop;
			break;
		default:
			Type = InteractionIconType.Default;
			break;
		}
	}

	public void TweenAlpha(float from, float to, float duration)
	{
		TweenAlpha alphaTweener = AlphaTweener;
		alphaTweener.from = from;
		alphaTweener.to = to;
		alphaTweener.duration = duration;
		alphaTweener.tweenFactor = 0f;
		alphaTweener.PlayForward();
	}

	public void SetPosition(Vector3 pos)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = Transform.localPosition;
		if (localPosition == pos)
		{
			PosDiff = Vector3.zero;
			return;
		}
		Transform.localPosition = pos;
		PosDiff = localPosition - pos;
	}

	public void UpdateIconGradation()
	{
		_isGradationIcon = true;
		_interactionIcon.MarkAsChanged();
	}

	public void ResetIconGradation()
	{
		_isGradationIcon = false;
		_interactionIcon.MarkAsChanged();
	}

	private void UpdateIconGradation(UIGeometry geometry)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		float num = Time.time * (float)Math.PI * 2f / _speed % ((float)Math.PI * 2f);
		int size = geometry.cols.size;
		Color color = _interactionIcon.color;
		Color val = color * _hideRatio;
		for (int i = 0; i < size; i++)
		{
			float num2 = Mathf.Atan2(geometry.verts[i].y, geometry.verts[i].x);
			if (num2 < 0f)
			{
				num2 = (float)Math.PI * 2f + num2;
			}
			float num3 = Mathf.Abs(num2 - num);
			if (num3 > (float)Math.PI)
			{
				num3 = (float)Math.PI * 2f - num3;
			}
			float num4 = num3 / (float)Math.PI;
			Color value = Color.Lerp(color, val, num4 * num4);
			geometry.cols[i] = value;
		}
	}

	private void OnFillSprite(UIWidget widget, int bufferOffset, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		if (_isGradationIcon)
		{
			UpdateIconGradation(widget.geometry);
		}
	}

	public void AddConflict(InteractionButton key, Vector3 dot)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if (_conflictDictionary.ContainsKey(key))
		{
			Vector3 val = _conflictDictionary[key];
			if (!(val == dot))
			{
				SliceSprite.RemoveSlice(Vector2.op_Implicit(val));
				if (SliceSprite.AddSlice(Vector2.op_Implicit(dot)))
				{
					_conflictDictionary[key] = dot;
				}
				else
				{
					_conflictDictionary.Remove(key);
				}
			}
		}
		else if (SliceSprite.AddSlice(Vector2.op_Implicit(dot)))
		{
			_conflictDictionary.Add(key, dot);
		}
	}

	public void ClearConflict()
	{
		SliceSprite.ClearSlices();
		_conflictDictionary.Clear();
	}
}
