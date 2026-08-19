using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Utils;
using UnityEngine;

namespace Durango.Model;

public class DoorOpener : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CCoClose_003Ed__8 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public DoorOpener _003C_003E4__this;

		private float _003CinitialYaw2_003E5__2;

		private float _003CmaxYaw_003E5__3;

		private float _003Celapsed_003E5__4;

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
		public _003CCoClose_003Ed__8(int _003C_003E1__state)
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
			DoorOpener doorOpener = _003C_003E4__this;
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
				doorOpener._fullyOpen = false;
				_003CinitialYaw2_003E5__2 = doorOpener._doorTarget.localRotation.eulerAngles.y % 360f;
				_003CinitialYaw2_003E5__2 = ((!(_003CinitialYaw2_003E5__2 > 180f)) ? _003CinitialYaw2_003E5__2 : (_003CinitialYaw2_003E5__2 - 360f));
				_003CmaxYaw_003E5__3 = _003CinitialYaw2_003E5__2;
				_003Celapsed_003E5__4 = 0f;
			}
			_003CmaxYaw_003E5__3 = ((_003CinitialYaw2_003E5__2 != 0f) ? Mathf.MoveTowards(_003CmaxYaw_003E5__3, 0f, Time.deltaTime * 90f * (_003CmaxYaw_003E5__3 / _003CinitialYaw2_003E5__2)) : 0f);
			_003Celapsed_003E5__4 += Time.deltaTime * 5f;
			float num2 = Mathf.Cos(_003Celapsed_003E5__4) * _003CmaxYaw_003E5__3;
			doorOpener._doorTarget.localRotation = Quaternion.Euler(Vector3.up * num2);
			if (!Mathf.Approximately(_003CmaxYaw_003E5__3, 0f))
			{
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
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
	private Transform _doorTarget;

	private int _overlappedCount;

	private float _openTargetYaw;

	private ICoroutineBinder _closeRoutine;

	private bool _fullyOpen;

	private void OnTriggerEnter(Collider other)
	{
		if (!(other.tag != "Player"))
		{
			_overlappedCount++;
			float num = Vector3.Dot(base.transform.position - other.transform.position, base.transform.localToWorldMatrix * Vector3.right);
			_openTargetYaw = ((!(num < 0f)) ? (-80f) : 80f);
			this.StopCoroutine(_closeRoutine);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (!_fullyOpen)
		{
			_doorTarget.localRotation = Quaternion.RotateTowards(_doorTarget.localRotation, Quaternion.Euler(_openTargetYaw * Vector3.up), Time.deltaTime * 300f);
			if (Mathf.Approximately((_doorTarget.localRotation.eulerAngles.y - _openTargetYaw) % 360f, 0f))
			{
				_fullyOpen = true;
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!(other.tag != "Player"))
		{
			_overlappedCount--;
			if (_overlappedCount == 0)
			{
				this.StartCoroutine(ref _closeRoutine, CoClose());
			}
		}
	}

	private IEnumerator CoClose()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoClose_003Ed__8(0)
		{
			_003C_003E4__this = this
		};
	}
}
