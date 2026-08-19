using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Durango.Prologue;

public class TrainShakeCameraController : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CStart_003Ed__11 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public TrainShakeCameraController _003C_003E4__this;

		private float _003CendTime_003E5__2;

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
		public _003CStart_003Ed__11(int _003C_003E1__state)
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
			TrainShakeCameraController trainShakeCameraController = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				trainShakeCameraController._transformCached = trainShakeCameraController.transform;
				goto IL_0035;
			case 1:
				_003C_003E1__state = -1;
				if (Time.time > _003CendTime_003E5__2)
				{
					_003C_003E2__current = new WaitForSeconds(UnityEngine.Random.Range(trainShakeCameraController._minVibInterval, trainShakeCameraController._maxVibInterval));
					_003C_003E1__state = 2;
					return true;
				}
				goto IL_0052;
			case 2:
				{
					_003C_003E1__state = -1;
					goto IL_0035;
				}
				IL_0035:
				_003CendTime_003E5__2 = Time.time + UnityEngine.Random.Range(trainShakeCameraController._minVibDuration, trainShakeCameraController._maxVibDuration);
				goto IL_0052;
				IL_0052:
				trainShakeCameraController._shakeDisplace.x = Mathf.Sin(Time.time * trainShakeCameraController._period) * trainShakeCameraController._u;
				trainShakeCameraController._shakeDisplace.y = Mathf.Sin(Time.time * trainShakeCameraController._period) * trainShakeCameraController._v;
				trainShakeCameraController._shakeDisplace.z = Mathf.Sin(Time.time * trainShakeCameraController._period) * trainShakeCameraController._w;
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
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
	private int _maxVib = 3;

	[SerializeField]
	private float _u = 10f;

	[SerializeField]
	private float _v = 10f;

	[SerializeField]
	private float _w = 10f;

	[SerializeField]
	private float _period = 3f;

	[SerializeField]
	private float _minVibDuration = 0.1f;

	[SerializeField]
	private float _maxVibDuration = 0.5f;

	[SerializeField]
	private float _minVibInterval = 0.5f;

	[SerializeField]
	private float _maxVibInterval = 2f;

	private Vector3 _shakeDisplace = Vector3.zero;

	private Transform _transformCached;

	private IEnumerator Start()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CStart_003Ed__11(0)
		{
			_003C_003E4__this = this
		};
	}

	private void LateUpdate()
	{
		_transformCached.localPosition += new Vector3(_shakeDisplace.x, _shakeDisplace.y, _shakeDisplace.z);
	}
}
