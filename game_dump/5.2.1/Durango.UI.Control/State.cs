using System;
using UnityEngine;

namespace Durango.UI.Control;

[Serializable]
public struct State
{
	public UseState Use;

	public int ColorSet;

	public Color Color;

	public Vector3 Scale;

	public bool IsForward;

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
