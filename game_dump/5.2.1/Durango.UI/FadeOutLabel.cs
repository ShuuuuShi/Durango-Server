using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

public class FadeOutLabel : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CCoUpdateAlpha_003Ed__12 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public FadeOutLabel _003C_003E4__this;

		private float _003CshowTime_003E5__2;

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
		public _003CCoUpdateAlpha_003Ed__12(int _003C_003E1__state)
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
			FadeOutLabel fadeOutLabel = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CshowTime_003E5__2 = 0f;
				fadeOutLabel.SetTransform();
				goto IL_0065;
			case 1:
				_003C_003E1__state = -1;
				goto IL_0065;
			case 2:
				{
					_003C_003E1__state = -1;
					break;
				}
				IL_0065:
				if (_003CshowTime_003E5__2 < fadeOutLabel._showTime)
				{
					_003CshowTime_003E5__2 += Time.deltaTime;
					_003C_003E2__current = null;
					_003C_003E1__state = 1;
					return true;
				}
				break;
			}
			if (fadeOutLabel._label.alpha > 0f)
			{
				fadeOutLabel._label.alpha -= 1f / fadeOutLabel._fadeoutTime * Time.deltaTime;
				_003C_003E2__current = null;
				_003C_003E1__state = 2;
				return true;
			}
			Singleton<MapIndicators>.Instance().HideToolTipLabel();
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

	[SerializeField]
	private UILabel _label;

	[SerializeField]
	private float _showTime;

	[SerializeField]
	private float _fadeoutTime;

	[SerializeField]
	private Vector2 _posOffset;

	private UIWidget _widget;

	public UIWidget Widget
	{
		get
		{
			if (_widget == null)
			{
				return _widget = GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public MapIndicator Indicator { get; private set; }

	public void Show(MapIndicator indicator, string text)
	{
		Indicator = indicator;
		_label.text = text;
		base.gameObject.SetActive(value: true);
		_label.alpha = 1f;
		StopAllCoroutines();
		StartCoroutine(CoUpdateAlpha());
	}

	private IEnumerator CoUpdateAlpha()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoUpdateAlpha_003Ed__12(0)
		{
			_003C_003E4__this = this
		};
	}

	private void SetTransform()
	{
		_label.transform.parent = Indicator.Widget.transform;
		Vector2 vector = Vector3.Lerp(Indicator.Widget.localCorners[0], Indicator.Widget.localCorners[3], 0.5f);
		_label.SetPosition(vector + _posOffset, 0.5f, 1f);
	}
}
