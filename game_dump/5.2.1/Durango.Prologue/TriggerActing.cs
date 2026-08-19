using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Utils;
using UnityEngine;

namespace Durango.Prologue;

public class TriggerActing : TriggerOnce
{
	[CompilerGenerated]
	private sealed class _003CcoWalkToSit_003Ed__12 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public TriggerActing _003C_003E4__this;

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
		public _003CcoWalkToSit_003Ed__12(int _003C_003E1__state)
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
			TriggerActing triggerActing = _003C_003E4__this;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
				if ((triggerActing._moveDestPosition - triggerActing._actor.transform.position).magnitude < 10f)
				{
					triggerActing._actor.transform.position = triggerActing._moveDestPosition;
					triggerActing._actor.transform.localRotation = Quaternion.Euler(0f, triggerActing._destYaw, 0f);
					triggerActing._actor.Play(triggerActing._afterWalkMotion);
					if ((bool)triggerActing._onFinishListener)
					{
						if (!triggerActing._onFinishListener.activeSelf)
						{
							triggerActing._onFinishListener.SetActive(value: true);
						}
						if (triggerActing._onFinishCmd != string.Empty)
						{
							triggerActing._onFinishListener.SendMessage(triggerActing._onFinishCmd);
						}
					}
					return false;
				}
			}
			else
			{
				_003C_003E1__state = -1;
				triggerActing._actor.CrossFade(triggerActing._walkMotion, 0.5f);
				triggerActing.RotateToPosition(triggerActing._moveDestPosition);
			}
			triggerActing._actor.transform.position = Vector3.MoveTowards(triggerActing._actor.transform.position, triggerActing._moveDestPosition, triggerActing._moveSpeed * Time.deltaTime);
			_003C_003E2__current = null;
			_003C_003E1__state = 1;
			return true;
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

	public CostumeActorBehavior _actor;

	public string _walkMotion;

	public string _afterWalkMotion;

	public float _moveSpeed = 200f;

	public Vector3 _moveDestPosition;

	public float _destYaw;

	public GameObject _onFinishListener;

	public string _onFinishCmd;

	protected override bool TriggerEntered(Collider other)
	{
		BeginEvent();
		return true;
	}

	private void BeginEvent()
	{
		StartCoroutine(coWalkToSit());
	}

	protected override bool TriggerExited(Collider other)
	{
		return true;
	}

	public void RotateToPosition(Vector3 pos)
	{
		float y = Maths.CalcYawWithTarget(pos, _actor.transform.position);
		_actor.transform.localRotation = Quaternion.Euler(0f, y, 0f);
	}

	private IEnumerator coWalkToSit()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CcoWalkToSit_003Ed__12(0)
		{
			_003C_003E4__this = this
		};
	}
}
