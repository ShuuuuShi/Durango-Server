using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Durango.UI.Control;

public class LoadingIconControl : MonoBehaviour
{
	private enum LoadingEnum
	{
		Looping,
		Ratio
	}

	[CompilerGenerated]
	private sealed class _003CCoLoadingGauge_003Ed__16 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public LoadingIconControl _003C_003E4__this;

		private float _003CcurrentRatio_003E5__2;

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
		public _003CCoLoadingGauge_003Ed__16(int _003C_003E1__state)
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
			LoadingIconControl loadingIconControl = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				loadingIconControl._isLoading = true;
				loadingIconControl.GetComponent<UIWidget>().alpha = 1f;
				_003CcurrentRatio_003E5__2 = 0f;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (loadingIconControl._isLoading)
			{
				if (loadingIconControl._targetRatio != _003CcurrentRatio_003E5__2)
				{
					float num2 = Mathf.Abs(loadingIconControl._targetRatio - _003CcurrentRatio_003E5__2);
					_003CcurrentRatio_003E5__2 = ((!(num2 < 0.01f)) ? (_003CcurrentRatio_003E5__2 + Time.deltaTime) : loadingIconControl._targetRatio);
					loadingIconControl.SetRatio(_003CcurrentRatio_003E5__2);
				}
				if (_003CcurrentRatio_003E5__2 != 1f)
				{
					_003C_003E2__current = null;
					_003C_003E1__state = 1;
					return true;
				}
				TweenAlpha tweenAlpha = loadingIconControl.gameObject.GetComponent<TweenAlpha>();
				if (tweenAlpha == null)
				{
					tweenAlpha = loadingIconControl.gameObject.AddComponent<TweenAlpha>();
				}
				tweenAlpha.tweenFactor = 0f;
				tweenAlpha.from = 1f;
				tweenAlpha.to = 0f;
				tweenAlpha.delay = 0.3f;
				tweenAlpha.PlayForward();
			}
			loadingIconControl._isLoading = false;
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

	[CompilerGenerated]
	private sealed class _003CCoLoadingLoop_003Ed__14 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public LoadingIconControl _003C_003E4__this;

		public float loopSpeed;

		private float _003Ctimer_003E5__2;

		private int _003CposIndex_003E5__3;

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
		public _003CCoLoadingLoop_003Ed__14(int _003C_003E1__state)
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
			LoadingIconControl loadingIconControl = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				loadingIconControl._isLoading = true;
				loadingIconControl.GetComponent<UIWidget>().alpha = 1f;
				_003Ctimer_003E5__2 = 0f;
				_003CposIndex_003E5__3 = 0;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (loadingIconControl._isLoading)
			{
				for (int i = 0; i < 6; i++)
				{
					UISprite upper = loadingIconControl.GetUpper(i);
					upper.alpha = Mathf.Clamp01(upper.alpha - Time.deltaTime / (loopSpeed * (float)loadingIconControl._loopingUpperCount));
				}
				if (_003Ctimer_003E5__2 == 0f)
				{
					_003CposIndex_003E5__3++;
					loadingIconControl.GetUpper(_003CposIndex_003E5__3).alpha = 1f;
				}
				_003Ctimer_003E5__2 += Time.deltaTime;
				if (_003Ctimer_003E5__2 > loopSpeed)
				{
					_003Ctimer_003E5__2 = 0f;
				}
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			loadingIconControl._isLoading = false;
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
	private UISprite _loadingUpperBase;

	[SerializeField]
	private Color _loadingUpperColor;

	[SerializeField]
	private int _loopingUpperCount = 3;

	[SerializeField]
	private bool _destoryUpperWhenDisable = true;

	[SerializeField]
	private LoadingEnum _loadingType;

	private readonly List<UISprite> _loadingUppers = new List<UISprite>();

	private bool _isLoading;

	private float _targetRatio;

	private void Awake()
	{
		_loadingUpperBase.gameObject.SetActive(value: false);
	}

	private void OnEnable()
	{
		switch (_loadingType)
		{
		case LoadingEnum.Looping:
			StartLoop();
			break;
		case LoadingEnum.Ratio:
			StartLoadingGauge();
			break;
		}
	}

	private void OnDisable()
	{
		StopLoading();
	}

	private void StartLoop(float loopSpeed = 0.1f)
	{
		if (!_isLoading)
		{
			StartCoroutine(CoLoadingLoop(loopSpeed));
		}
	}

	private void StopLoading()
	{
		_isLoading = false;
		if (!_destoryUpperWhenDisable)
		{
			return;
		}
		for (int num = _loadingUppers.Count - 1; num >= 0; num--)
		{
			UISprite uISprite = _loadingUppers[num];
			if (uISprite != null)
			{
				UnityEngine.Object.Destroy(uISprite.gameObject);
			}
		}
		_loadingUppers.Clear();
	}

	private IEnumerator CoLoadingLoop(float loopSpeed)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoLoadingLoop_003Ed__14(0)
		{
			_003C_003E4__this = this,
			loopSpeed = loopSpeed
		};
	}

	private void StartLoadingGauge()
	{
		if (!_isLoading)
		{
			_targetRatio = 0f;
			StartCoroutine(CoLoadingGauge());
		}
	}

	private IEnumerator CoLoadingGauge()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoLoadingGauge_003Ed__16(0)
		{
			_003C_003E4__this = this
		};
	}

	private void SetRatio(float r)
	{
		float num = Mathf.Clamp(r * 100f, 0f, 100f);
		float num2 = 16.666666f;
		int num3 = (int)(num / num2);
		float alpha = num % num2 / num2;
		for (int i = 0; i < num3; i++)
		{
			GetUpper(i).alpha = 1f;
		}
		GetUpper(num3).alpha = alpha;
	}

	private UISprite GetUpper(int index)
	{
		index %= 6;
		if (_loadingUppers.Count <= index)
		{
			for (int i = _loadingUppers.Count; i < index + 1; i++)
			{
				GameObject gameObject = _loadingUpperBase.transform.parent.gameObject.AddChild(_loadingUpperBase.gameObject);
				_loadingUppers.Add(gameObject.GetComponent<UISprite>());
				float num = (float)(i % 6) * (float)Math.PI / 3f;
				_loadingUppers[i].transform.localPosition = (Vector3.up * Mathf.Cos(num) + Vector3.right * Mathf.Sin(num)) * 29f;
				_loadingUppers[i].transform.localEulerAngles = Vector3.back * num * 57.29578f;
				_loadingUppers[i].color = _loadingUpperColor;
				_loadingUppers[i].gameObject.SetActive(value: true);
				_loadingUppers[i].alpha = 0f;
			}
		}
		return _loadingUppers[index];
	}
}
