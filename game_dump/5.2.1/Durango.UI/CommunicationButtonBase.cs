using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Durango.UI;

public class CommunicationButtonBase : UIWidget
{
	[CompilerGenerated]
	private sealed class _003CCoFillAmount_003Ed__12 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public CommunicationButtonBase _003C_003E4__this;

		public float time;

		public Func<bool> checkFunc;

		public Action callback;

		private float _003CbeginTime_003E5__2;

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
		public _003CCoFillAmount_003Ed__12(int _003C_003E1__state)
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
			CommunicationButtonBase communicationButtonBase = _003C_003E4__this;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
			}
			else
			{
				_003C_003E1__state = -1;
				communicationButtonBase._fill.gameObject.SetActive(value: true);
				_003CbeginTime_003E5__2 = Time.time;
			}
			float num2 = Time.time - _003CbeginTime_003E5__2;
			float num3 = 1f - Mathf.Min(1f, num2 / time);
			communicationButtonBase._fill.fillAmount = num3;
			bool flag = checkFunc();
			if (!(num3 <= 0f) && flag && communicationButtonBase._fill.gameObject.activeSelf)
			{
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			callback();
			communicationButtonBase._fill.gameObject.SetActive(value: false);
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
	protected UISprite _sprite;

	[SerializeField]
	private UISprite _fill;

	protected Action _clicked;

	private Action _longTouched;

	public virtual bool ToggleOn { get; set; }

	public void Initailize(Action clicked, Action longTouched)
	{
		_clicked = clicked;
		_longTouched = longTouched;
	}

	private void OnLongPress()
	{
		if (_longTouched != null)
		{
			_longTouched();
		}
	}

	public void Set(string spriteName)
	{
		_sprite.spriteName = spriteName;
		UIUtility.ResizeToSquare(_sprite);
	}

	public void StartFillAmount(float time, Func<bool> checkFunc, Action callback)
	{
		StartCoroutine(CoFillAmount(time, checkFunc, callback));
	}

	private IEnumerator CoFillAmount(float time, Func<bool> checkFunc, Action callback)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoFillAmount_003Ed__12(0)
		{
			_003C_003E4__this = this,
			time = time,
			checkFunc = checkFunc,
			callback = callback
		};
	}
}
