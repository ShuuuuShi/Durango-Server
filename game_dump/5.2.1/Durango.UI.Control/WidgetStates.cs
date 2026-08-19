using System;
using UnityEngine;

namespace Durango.UI.Control;

[Serializable]
public class WidgetStates
{
	[Flags]
	public enum UseState
	{
		Color = 1,
		Alpha = 2,
		Active = 4,
		Scale = 8,
		Tweener = 0x10
	}

	public class TypeAttribute : Attribute
	{
		public Type EnumType { get; private set; }

		public TypeAttribute(Type type)
		{
			if (type.IsEnum)
			{
				EnumType = type;
			}
		}
	}

	[Serializable]
	public class Item
	{
		[SerializeField]
		private UIWidget _widget;

		[SerializeField]
		private State[] _states;

		public void Apply(int value)
		{
			if (_states == null || value < 0 || value >= _states.Length)
			{
				return;
			}
			State state = _states[value];
			if ((state.Use & UseState.Color) > (UseState)0)
			{
				_widget.color = state.Color;
			}
			if ((state.Use & UseState.Alpha) > (UseState)0)
			{
				_widget.alpha = state.Alpha;
			}
			if ((state.Use & UseState.Scale) > (UseState)0)
			{
				_widget.transform.localScale = state.Scale;
			}
			if ((state.Use & UseState.Tweener) > (UseState)0)
			{
				UITweener[] tweeners = state.GetTweeners(_widget);
				int i = 0;
				for (int size = KUtility.GetSize(tweeners); i < size; i++)
				{
					tweeners[i].ResetToBeginning();
					tweeners[i].PlayForward();
				}
			}
			if ((state.Use & UseState.Active) > (UseState)0)
			{
				_widget.gameObject.SetActive(state.IsActive);
			}
		}
	}

	[Serializable]
	public struct State
	{
		public UseState Use;

		public Color Color;

		[Range(0f, 1f)]
		public float Alpha;

		public Vector3 Scale;

		public bool IsActive;

		private bool _tweenerFlag;

		private UITweener[] _tweeners;

		public UITweener[] GetTweeners(UIWidget widget)
		{
			if (Application.isPlaying)
			{
				if (!_tweenerFlag)
				{
					_tweenerFlag = true;
					_tweeners = widget.GetComponents<UITweener>();
				}
				return _tweeners;
			}
			return widget.GetComponents<UITweener>();
		}
	}

	[SerializeField]
	private Item[] _items;

	private int? _lateValue;

	public void Apply(int value)
	{
		if (_items != null && (!Application.isPlaying || !_lateValue.HasValue || _lateValue.Value != value))
		{
			_lateValue = value;
			Item[] items = _items;
			for (int i = 0; i < items.Length; i++)
			{
				items[i].Apply(value);
			}
		}
	}
}
