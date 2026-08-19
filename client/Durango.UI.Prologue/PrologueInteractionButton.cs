using System;
using System.Collections.Generic;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI.Prologue;

public class PrologueInteractionButton : MonoBehaviour
{
	[SerializeField]
	private UISprite _interactionIcon;

	[SerializeField]
	private float _speed;

	[SerializeField]
	private float _hideRatio;

	private UIWidget _widget;

	private Transform _transform;

	private Transform _iconTransform;

	private TweenAlpha _alphaTweener;

	private UISliceSprite _sliceSprite;

	private bool _isGradationIcon;

	private readonly Dictionary<PrologueInteractionButton, Vector3> _conflictDictionary = new Dictionary<PrologueInteractionButton, Vector3>();

	public InteractionObject InteractionTarget { get; private set; }

	public bool Valid { get; set; }

	public Vector3 PosDiff { get; private set; }

	public bool TouchFlag { get; set; }

	public UIWidget Widget
	{
		get
		{
			if (_widget == null)
			{
				_widget = GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public Transform Transform
	{
		get
		{
			if (_transform == null)
			{
				_transform = base.transform;
			}
			return _transform;
		}
	}

	public Transform IconTransform
	{
		get
		{
			if (_iconTransform == null)
			{
				_iconTransform = _interactionIcon.transform;
			}
			return _iconTransform;
		}
	}

	public TweenAlpha AlphaTweener
	{
		get
		{
			if (_alphaTweener == null)
			{
				_alphaTweener = _interactionIcon.GetComponent<TweenAlpha>();
			}
			return _alphaTweener;
		}
	}

	public UISliceSprite SliceSprite
	{
		get
		{
			if (_sliceSprite == null)
			{
				_sliceSprite = GetComponent<UISliceSprite>();
				if (_sliceSprite == null)
				{
					_sliceSprite = base.gameObject.AddComponent<UISliceSprite>();
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
			float alpha = _interactionIcon.alpha;
			_interactionIcon.color = value;
			_interactionIcon.alpha = alpha;
		}
	}

	public PrologueInteractionButtonControl.InteractionIconType Type { get; private set; }

	public void Set(InteractionObject obj)
	{
		InteractionTarget = obj;
		switch (obj.ObjectType)
		{
		case InteractionObject.Type.Animal:
			Type = PrologueInteractionButtonControl.InteractionIconType.Animal;
			break;
		case InteractionObject.Type.Prop:
			Type = PrologueInteractionButtonControl.InteractionIconType.Prop;
			break;
		case InteractionObject.Type.PropSelectableByClient:
			Type = PrologueInteractionButtonControl.InteractionIconType.Prop;
			break;
		case InteractionObject.Type.PrologueSelectCharacter:
			Type = PrologueInteractionButtonControl.InteractionIconType.PrologueCharacter;
			break;
		case InteractionObject.Type.Vehicle:
			Type = PrologueInteractionButtonControl.InteractionIconType.Prop;
			break;
		default:
			Type = PrologueInteractionButtonControl.InteractionIconType.Default;
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

	private void UpdateIconGradation(UIGeometry.Arguments arguments)
	{
		float num = Time.time * (float)Math.PI * 2f / _speed % ((float)Math.PI * 2f);
		int size = arguments.cols.size;
		Color color = _interactionIcon.color;
		Color b = color * _hideRatio;
		for (int i = 0; i < size; i++)
		{
			float num2 = Mathf.Atan2(arguments.verts[i].y, arguments.verts[i].x);
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
			Color value = Color.Lerp(color, b, num4 * num4);
			arguments.cols[i] = value;
		}
	}

	private void OnFillSprite(UIWidget widget, int bufferOffset, UIGeometry.Arguments arguments)
	{
		if (_isGradationIcon)
		{
			UpdateIconGradation(arguments);
		}
	}

	public void AddConflict(PrologueInteractionButton key, Vector3 dot)
	{
		if (_conflictDictionary.ContainsKey(key))
		{
			Vector3 vector = _conflictDictionary[key];
			if (!(vector == dot))
			{
				SliceSprite.RemoveSlice(vector);
				if (SliceSprite.AddSlice(dot))
				{
					_conflictDictionary[key] = dot;
				}
				else
				{
					_conflictDictionary.Remove(key);
				}
			}
		}
		else if (SliceSprite.AddSlice(dot))
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
