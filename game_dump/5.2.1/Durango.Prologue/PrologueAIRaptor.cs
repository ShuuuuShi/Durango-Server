using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Model;
using Durango.Network;
using Durango.Render.Particle;
using Durango.Utils;
using Messages;
using Shared.Battle;
using UnityEngine;

namespace Durango.Prologue;

public class PrologueAIRaptor : StateBasedAI<PrologueAIRaptor.State>
{
	public enum State
	{
		Invalid = -1,
		WaitForBattleBegin,
		WaitForPreparePlayer,
		Normal,
		Chase,
		Flinch,
		Blow,
		Leap,
		Roaming,
		Dead,
		Count
	}

	public class MoveInfo : WeightedCandidate
	{
		public string Motion;

		public float MoveSpeed;
	}

	[CompilerGenerated]
	private sealed class _003CBlowDoing_003Ed__76 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PrologueAIRaptor _003C_003E4__this;

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
		public _003CBlowDoing_003Ed__76(int _003C_003E1__state)
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
			PrologueAIRaptor prologueAIRaptor = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = new WaitForSeconds(prologueAIRaptor.TargetAnimal.CurAnimState.length);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				prologueAIRaptor.CurState = State.Normal;
				return false;
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

	[CompilerGenerated]
	private sealed class _003CChaseDoing_003Ed__65 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PrologueAIRaptor _003C_003E4__this;

		private float _003CprevTime_003E5__2;

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
		public _003CChaseDoing_003Ed__65(int _003C_003E1__state)
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
			PrologueAIRaptor prologueAIRaptor = _003C_003E4__this;
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
				prologueAIRaptor.TargetAnimal.CrossFade(prologueAIRaptor._walkMotion, 0.1f);
				_003CprevTime_003E5__2 = Time.time;
			}
			if (null == prologueAIRaptor._victim || prologueAIRaptor.IsInterrupted)
			{
				return false;
			}
			float num2 = Time.time - _003CprevTime_003E5__2;
			_003CprevTime_003E5__2 = Time.time;
			Vector3 vector = Maths.Make2D(prologueAIRaptor._victim.transform.position - prologueAIRaptor.transform.position);
			if (!(vector.magnitude <= prologueAIRaptor._engageDistance))
			{
				float yaw = Maths.CalcYawWithTarget(prologueAIRaptor._victim.transform.position, prologueAIRaptor.transform.position);
				prologueAIRaptor.TargetAnimal.TurnToYaw(yaw, bSnap: false);
				Vector3 vector2 = vector.normalized * prologueAIRaptor._walkSpeed;
				prologueAIRaptor.TargetAnimal.CurrentPosition += vector2 * num2;
				prologueAIRaptor.TargetAnimal.CurrentPosition = new Vector3(Mathf.Clamp(prologueAIRaptor.TargetAnimal.CurrentPosition.x, prologueAIRaptor._minArea.x, prologueAIRaptor._maxArea.x), 0f, Mathf.Clamp(prologueAIRaptor.TargetAnimal.CurrentPosition.z, prologueAIRaptor._minArea.z, prologueAIRaptor._maxArea.z));
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			if (Time.time > prologueAIRaptor._attackCooldownEnd)
			{
				prologueAIRaptor.CurState = State.Leap;
			}
			else
			{
				prologueAIRaptor.CurState = State.Roaming;
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

	[CompilerGenerated]
	private sealed class _003CDeadDoing_003Ed__87 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PrologueAIRaptor _003C_003E4__this;

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
		public _003CDeadDoing_003Ed__87(int _003C_003E1__state)
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
			PrologueAIRaptor prologueAIRaptor = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = new WaitForSeconds(prologueAIRaptor._playTrexCutSceneDelay);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				return false;
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

	[CompilerGenerated]
	private sealed class _003CFlinchDoing_003Ed__71 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PrologueAIRaptor _003C_003E4__this;

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
		public _003CFlinchDoing_003Ed__71(int _003C_003E1__state)
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
			PrologueAIRaptor prologueAIRaptor = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = new WaitForSeconds(prologueAIRaptor._flinchEndTime - Time.time);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				prologueAIRaptor.CurState = State.Roaming;
				return false;
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

	[CompilerGenerated]
	private sealed class _003CLeapDoing_003Ed__82 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PrologueAIRaptor _003C_003E4__this;

		private Vector3 _003CnewPos_003E5__2;

		private TweenPosition _003Ctween_003E5__3;

		private float _003CattackAt_003E5__4;

		private bool _003Cattacked_003E5__5;

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
		public _003CLeapDoing_003Ed__82(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Ctween_003E5__3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			PrologueAIRaptor prologueAIRaptor = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				_003CnewPos_003E5__2 = Vector3.MoveTowards(prologueAIRaptor.TargetAnimal.transform.position, PlayerBehavior.LocalPlayer.CurrentPosition, prologueAIRaptor._maxLeapDist);
				float yaw = Maths.CalcYawWithTarget(_003CnewPos_003E5__2, prologueAIRaptor.transform.position);
				prologueAIRaptor.TargetAnimal.TurnToYaw(yaw, bSnap: false);
				prologueAIRaptor.UnPinUpRootBone();
				prologueAIRaptor.TargetAnimal.Play(prologueAIRaptor._threatMotion, loop: false);
				ParticleManager.Emit("Particle/FX_SkillActivated_Common_01.prefab", prologueAIRaptor.TargetAnimal.CurrentPosition, Quaternion.identity);
				_003C_003E2__current = new WaitForSeconds(prologueAIRaptor.TargetAnimal.CurAnimState.length);
				_003C_003E1__state = 1;
				return true;
			}
			case 1:
				_003C_003E1__state = -1;
				if (prologueAIRaptor.IsInterrupted)
				{
					return false;
				}
				if (!prologueAIRaptor._threatedFlagForGuide)
				{
					GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.LearnDodge);
					prologueAIRaptor._threatedFlagForGuide = true;
				}
				_003C_003E2__current = new WaitForSeconds(0.01f);
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				prologueAIRaptor.PinUpRootBone();
				prologueAIRaptor.TargetAnimal.Play(prologueAIRaptor._leapMotion, loop: false);
				_003Ctween_003E5__3 = TweenPosition.Begin(prologueAIRaptor.TargetAnimal.gameObject, prologueAIRaptor.TargetAnimal.CurAnimState.length, _003CnewPos_003E5__2);
				_003Ctween_003E5__3.method = UITweener.Method.EaseInOut;
				_003Ctween_003E5__3.PlayForward();
				_003CattackAt_003E5__4 = Time.time + 0.6f;
				_003Cattacked_003E5__5 = false;
				break;
			case 3:
				_003C_003E1__state = -1;
				break;
			}
			if (_003Ctween_003E5__3.enabled)
			{
				if (prologueAIRaptor.IsInterrupted)
				{
					_003Ctween_003E5__3.enabled = false;
					return false;
				}
				if (!_003Cattacked_003E5__5 && Time.time >= _003CattackAt_003E5__4)
				{
					prologueAIRaptor.CheckAndMakeDamageToPlayer();
					_003Cattacked_003E5__5 = true;
				}
				_003C_003E2__current = null;
				_003C_003E1__state = 3;
				return true;
			}
			prologueAIRaptor.CurState = State.Normal;
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
	private sealed class _003CNormalDoing_003Ed__62 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PrologueAIRaptor _003C_003E4__this;

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
		public _003CNormalDoing_003Ed__62(int _003C_003E1__state)
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
			PrologueAIRaptor prologueAIRaptor = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (null != prologueAIRaptor._victim)
				{
					if ((prologueAIRaptor._victim.transform.position - prologueAIRaptor.transform.position).magnitude > prologueAIRaptor._engageDistance)
					{
						prologueAIRaptor.CurState = State.Chase;
					}
					else if (Time.time > prologueAIRaptor._attackCooldownEnd)
					{
						prologueAIRaptor.CurState = State.Leap;
					}
					else
					{
						prologueAIRaptor.CurState = State.Roaming;
					}
				}
				_003C_003E2__current = new WaitForSeconds(0.3f);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				return false;
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

	[CompilerGenerated]
	private sealed class _003COnAfterDoingState_003Ed__54 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

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
		public _003COnAfterDoingState_003Ed__54(int _003C_003E1__state)
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
			if (_003C_003E1__state != 0)
			{
				return false;
			}
			_003C_003E1__state = -1;
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
	private sealed class _003COnBeforeDoingState_003Ed__53 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PrologueAIRaptor _003C_003E4__this;

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
		public _003COnBeforeDoingState_003Ed__53(int _003C_003E1__state)
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
			PrologueAIRaptor prologueAIRaptor = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				prologueAIRaptor._victim = PlayerBehavior.LocalPlayer.gameObject;
				if (prologueAIRaptor._victim == null)
				{
					_003C_003E2__current = new WaitForSeconds(1f);
					_003C_003E1__state = 1;
					return true;
				}
				prologueAIRaptor.GetComponent<BoneLookAtTarget>().SetLookTarget(prologueAIRaptor._victim);
				return false;
			case 1:
				_003C_003E1__state = -1;
				return false;
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

	[CompilerGenerated]
	private sealed class _003COnStart_003Ed__52 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PrologueAIRaptor _003C_003E4__this;

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
		public _003COnStart_003Ed__52(int _003C_003E1__state)
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
			PrologueAIRaptor prologueAIRaptor = _003C_003E4__this;
			if (num != 0)
			{
				return false;
			}
			_003C_003E1__state = -1;
			prologueAIRaptor.TargetAnimal.EntityId = FakeEntityId;
			Singleton<AnimalManager>.Instance().ForceAddAnimal(prologueAIRaptor.TargetAnimal.EntityId, prologueAIRaptor.GetComponent<AnimalBehavior>());
			prologueAIRaptor.CurState = State.WaitForBattleBegin;
			prologueAIRaptor.GetComponent<BoneLookAtTarget>().AutoChangeTarget = false;
			prologueAIRaptor.TargetAnimal.Play(prologueAIRaptor._standMotion);
			prologueAIRaptor._deadDestPos += prologueAIRaptor._deadDestPosOriginOffset;
			prologueAIRaptor.PinUpRootBone();
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
	private sealed class _003CRoamingDoing_003Ed__79 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PrologueAIRaptor _003C_003E4__this;

		private Vector3 _003CnewPos_003E5__2;

		private float _003CmoveSpeed_003E5__3;

		private float _003CprevTime_003E5__4;

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
		public _003CRoamingDoing_003Ed__79(int _003C_003E1__state)
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
			PrologueAIRaptor prologueAIRaptor = _003C_003E4__this;
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
				_003CnewPos_003E5__2 = PlayerBehavior.LocalPlayer.CurrentPosition + new Vector3(UnityEngine.Random.Range(0f - prologueAIRaptor._roamingRadiusFromPlayer, prologueAIRaptor._roamingRadiusFromPlayer), 0f, UnityEngine.Random.Range(0f - prologueAIRaptor._roamingRadiusFromPlayer, prologueAIRaptor._roamingRadiusFromPlayer));
				_003CnewPos_003E5__2 = prologueAIRaptor.ClampPos(_003CnewPos_003E5__2);
				if (_randomMoveCandidates == null)
				{
					_randomMoveCandidates = new MoveInfo[3]
					{
						new MoveInfo
						{
							Weight = prologueAIRaptor._moveRunRatio,
							Motion = prologueAIRaptor._runMotion,
							MoveSpeed = prologueAIRaptor._runSpeed
						},
						new MoveInfo
						{
							Weight = prologueAIRaptor._moveWalkRatio,
							Motion = prologueAIRaptor._walkMotion,
							MoveSpeed = prologueAIRaptor._walkSpeed
						},
						new MoveInfo
						{
							Weight = prologueAIRaptor._moveLimpRatio,
							Motion = prologueAIRaptor._limpMotion,
							MoveSpeed = prologueAIRaptor._limpSpeed
						}
					};
				}
				MoveInfo moveInfo = WeightedCandidate.Select(_randomMoveCandidates);
				if (moveInfo == null)
				{
					return false;
				}
				string motion = moveInfo.Motion;
				_003CmoveSpeed_003E5__3 = moveInfo.MoveSpeed;
				prologueAIRaptor.TargetAnimal.CrossFade(motion, 0.1f);
				_003CprevTime_003E5__4 = Time.time;
			}
			if (null == prologueAIRaptor._victim || prologueAIRaptor.IsInterrupted)
			{
				return false;
			}
			float num2 = Time.time - _003CprevTime_003E5__4;
			_003CprevTime_003E5__4 = Time.time;
			Vector3 vector = Maths.Make2D(_003CnewPos_003E5__2 - prologueAIRaptor.transform.position);
			if (!(vector.magnitude <= 50f))
			{
				float yaw = Maths.CalcYawWithTarget(_003CnewPos_003E5__2, prologueAIRaptor.transform.position);
				prologueAIRaptor.TargetAnimal.TurnToYaw(yaw, bSnap: false);
				Vector3 vector2 = vector.normalized * _003CmoveSpeed_003E5__3;
				prologueAIRaptor.TargetAnimal.CurrentPosition += vector2 * num2;
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			prologueAIRaptor.CurState = State.Normal;
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
	private sealed class _003CWaitForBattleBeginDoing_003Ed__58 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

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
		public _003CWaitForBattleBeginDoing_003Ed__58(int _003C_003E1__state)
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
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				return false;
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

	[CompilerGenerated]
	private sealed class _003CWaitForPreparePlayerDoing_003Ed__59 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PrologueAIRaptor _003C_003E4__this;

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
		public _003CWaitForPreparePlayerDoing_003Ed__59(int _003C_003E1__state)
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
			PrologueAIRaptor prologueAIRaptor = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				prologueAIRaptor.TargetAnimal.CrossFade(prologueAIRaptor._threatMotion);
				_003C_003E2__current = new WaitForSeconds(1.5f);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				prologueAIRaptor.CurState = State.Normal;
				return false;
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

	public static readonly string FakeEntityId = "666";

	[SerializeField]
	private string _standMotion = "Raptor_Stand";

	[SerializeField]
	private string _walkMotion = "Raptor_Walk";

	[SerializeField]
	private float _walkSpeed = 120f;

	[SerializeField]
	private float _moveWalkRatio = 0.33f;

	[SerializeField]
	private string _runMotion = "Raptor_Run";

	[SerializeField]
	private float _runSpeed = 360f;

	[SerializeField]
	private float _moveRunRatio = 0.33f;

	[SerializeField]
	private string _limpMotion = "Raptor_Limp";

	[SerializeField]
	private float _limpSpeed = 120f;

	[SerializeField]
	private float _moveLimpRatio = 0.33f;

	[SerializeField]
	private float _attackCoolTime = 4f;

	[SerializeField]
	private float _engageDistance = 300f;

	[SerializeField]
	private float _hitRatio = 0.5f;

	[SerializeField]
	private float _criticalRatio = 0.1f;

	[SerializeField]
	private int _damageMin = 1;

	[SerializeField]
	private int _damageMax = 20;

	[SerializeField]
	private int _damageCritical = 100;

	[SerializeField]
	private string _leapMotion = "Raptor_Attack_Jump";

	[SerializeField]
	private string _flinchMotion = "Raptor_Flinch";

	[SerializeField]
	private string _blowMotion = "Raptor_DamageBlow";

	[SerializeField]
	private string _groggyMotion = "Raptor_Groggy";

	[SerializeField]
	private string _threatMotion = "Raptor_Threat";

	[SerializeField]
	private Vector3 _minArea = Vector3.zero;

	[SerializeField]
	private Vector3 _maxArea = Vector3.zero;

	[SerializeField]
	private Vector3 _deadDestPos;

	[SerializeField]
	private Vector3 _deadDestPosOriginOffset = new Vector3(0f, 0f, 3000f);

	[SerializeField]
	private float _roamingRadiusFromPlayer = 500f;

	[SerializeField]
	private float _maxLeapDist = 500f;

	[SerializeField]
	private float _playTrexCutSceneDelay = 1f;

	[SerializeField]
	private float _deadYaw = -90f;

	private float _attackCooldownEnd;

	private GameObject _victim;

	private AnimalBehavior _targetAnimal;

	private int _attackCount;

	private bool _isDeadPhase;

	private static MoveInfo[] _randomMoveCandidates;

	private float _flinchEndTime;

	[SerializeField]
	private float _blowDistance = 500f;

	[SerializeField]
	private float _blowTime = 0.3f;

	private bool _threatedFlagForGuide;

	protected override State InvalidState => State.Invalid;

	private AnimalBehavior TargetAnimal
	{
		get
		{
			if (null == _targetAnimal)
			{
				_targetAnimal = GetComponent<AnimalBehavior>();
				Gauge life = new Gauge(100f, 0f, new GaugeNode[1]
				{
					new GaugeNode
					{
						Time = 0.0,
						Value = 100f
					}
				});
				_targetAnimal.SetSurvivalGauge(life, null);
			}
			return _targetAnimal;
		}
	}

	protected override int StateEnumCount => 9;

	protected override void DefineStates()
	{
		AddState(State.WaitForBattleBegin, new StateElem
		{
			Entered = WaitForBattleBeginEntered,
			Doing = WaitForBattleBeginDoing,
			Exited = WaitForBattleBeginExited
		});
		AddState(State.WaitForPreparePlayer, new StateElem
		{
			Doing = WaitForPreparePlayerDoing
		});
		AddState(State.Normal, new StateElem
		{
			Entered = NormalEntered,
			Doing = NormalDoing,
			Exited = NormalExited
		});
		AddState(State.Chase, new StateElem
		{
			Entered = ChaseEntered,
			Doing = ChaseDoing,
			Exited = ChaseExited
		});
		AddState(State.Flinch, new StateElem
		{
			Entered = FlinchEntered,
			Doing = FlinchDoing,
			Exited = FlinchExited
		});
		AddState(State.Blow, new StateElem
		{
			Entered = BlowEntered,
			Doing = BlowDoing,
			Exited = BlowExited
		});
		AddState(State.Leap, new StateElem
		{
			Entered = LeapEntered,
			Doing = LeapDoing,
			Exited = LeapExited
		});
		AddState(State.Roaming, new StateElem
		{
			Entered = RoamingEntered,
			Doing = RoamingDoing,
			Exited = RoamingExited
		});
		AddState(State.Dead, new StateElem
		{
			Entered = DeadEntered,
			Doing = DeadDoing,
			Exited = DeadExited
		});
	}

	protected override bool IsAIEnded()
	{
		return _isDeadPhase;
	}

	protected override bool IsTerminalState(State state)
	{
		return state == State.Dead;
	}

	protected override IEnumerator OnStart()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003COnStart_003Ed__52(0)
		{
			_003C_003E4__this = this
		};
	}

	protected override IEnumerator OnBeforeDoingState()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003COnBeforeDoingState_003Ed__53(0)
		{
			_003C_003E4__this = this
		};
	}

	protected override IEnumerator OnAfterDoingState()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003COnAfterDoingState_003Ed__54(0);
	}

	public void SetAiActivated()
	{
		base.CurState = State.WaitForPreparePlayer;
	}

	private void WaitForBattleBeginEntered()
	{
		TargetAnimal.CrossFade(_standMotion, 0.1f);
	}

	private void WaitForBattleBeginExited()
	{
	}

	private IEnumerator WaitForBattleBeginDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CWaitForBattleBeginDoing_003Ed__58(0);
	}

	private IEnumerator WaitForPreparePlayerDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CWaitForPreparePlayerDoing_003Ed__59(0)
		{
			_003C_003E4__this = this
		};
	}

	private void NormalEntered()
	{
		TargetAnimal.CrossFade(_standMotion, 0.1f);
	}

	private void NormalExited()
	{
	}

	private IEnumerator NormalDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CNormalDoing_003Ed__62(0)
		{
			_003C_003E4__this = this
		};
	}

	private void ChaseEntered()
	{
	}

	private void ChaseExited()
	{
	}

	private IEnumerator ChaseDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CChaseDoing_003Ed__65(0)
		{
			_003C_003E4__this = this
		};
	}

	private void CheckAndMakeDamageToPlayer()
	{
		Damage damage = default(Damage);
		if (_attackCount == 0)
		{
			damage.Value = 0;
			damage.Result = DamageResult.Dodged;
		}
		else if (Maths.Make2D(PlayerBehavior.LocalPlayer.CurrentPosition - base.transform.position).magnitude < 300f && UnityEngine.Random.value < _hitRatio)
		{
			damage.Result = DamageResult.Hit;
		}
		else
		{
			damage.Result = DamageResult.Missed;
		}
		if (damage.Result == DamageResult.Hit)
		{
			if (UnityEngine.Random.value < _criticalRatio)
			{
				damage.Effects |= DamageEffects.Critical;
			}
			damage.Value = CalcDamage((damage.Effects & DamageEffects.Critical) != 0);
			damage.Effects |= DamageEffects.Blow;
		}
		damage.AttackType = AttackType.SmallTear;
		damage.Direction = DamageDirection.Front;
		damage.Part = BodyPart.Body;
		_attackCount++;
		Connections.Frontend.PushPacket(new Damaged
		{
			AttackerId = FakeEntityId,
			Damage = damage,
			VictimId = GameManager.PlayerId
		});
	}

	private int CalcDamage(bool isCriticalHit)
	{
		if (isCriticalHit)
		{
			return _damageCritical;
		}
		return UnityEngine.Random.Range(_damageMin, _damageMax);
	}

	public void EventFlinch()
	{
		base.CurState = State.Flinch;
	}

	private void FlinchEntered()
	{
		TargetAnimal.Play(_flinchMotion, loop: false);
		_flinchEndTime = Time.time + TargetAnimal.CurAnimState.length;
	}

	private void FlinchExited()
	{
	}

	private IEnumerator FlinchDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CFlinchDoing_003Ed__71(0)
		{
			_003C_003E4__this = this
		};
	}

	public void EventBlow()
	{
		base.CurState = State.Blow;
	}

	private void BlowEntered()
	{
		TargetAnimal.Play(_blowMotion, loop: false);
		Vector3 vector = Maths.ProjectDirection(PlayerBehavior.LocalPlayer.transform);
		vector.y = 0f;
		vector.z = 0f;
		vector = vector.normalized;
		TargetAnimal.TurnToYaw(Maths.CalcYaw(vector) + 180f, bSnap: true);
		Vector3 destPos = TargetAnimal.CurrentPosition + vector * _blowDistance;
		destPos = ClampPos(destPos);
		TweenPosition tweenPosition = TweenPosition.Begin(TargetAnimal.gameObject, _blowTime, destPos);
		tweenPosition.method = UITweener.Method.EaseInOut;
		tweenPosition.PlayForward();
		PinUpRootBone();
	}

	private Vector3 ClampPos(Vector3 destPos)
	{
		destPos.x = Mathf.Clamp(destPos.x, _minArea.x, _maxArea.x);
		destPos.z = Mathf.Clamp(destPos.z, _minArea.z, _maxArea.z);
		return destPos;
	}

	private void BlowExited()
	{
	}

	private IEnumerator BlowDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CBlowDoing_003Ed__76(0)
		{
			_003C_003E4__this = this
		};
	}

	private void RoamingEntered()
	{
	}

	private void RoamingExited()
	{
	}

	private IEnumerator RoamingDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CRoamingDoing_003Ed__79(0)
		{
			_003C_003E4__this = this
		};
	}

	private void LeapEntered()
	{
		_attackCooldownEnd = Time.time + _attackCoolTime;
	}

	private void LeapExited()
	{
	}

	private IEnumerator LeapDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CLeapDoing_003Ed__82(0)
		{
			_003C_003E4__this = this
		};
	}

	private static TweenScale CreateTweenScale(string buttonPath)
	{
		Transform transform = Singleton<UIManager>.Instance().FindTransform(buttonPath);
		if ((bool)transform)
		{
			TweenScale tweenScale = transform.gameObject.AddComponent<TweenScale>();
			tweenScale.to = new Vector3(1.2f, 1.2f, 1.2f);
			tweenScale.duration = 0.2f;
			tweenScale.style = UITweener.Style.PingPong;
			return tweenScale;
		}
		return null;
	}

	public void EventDead()
	{
		base.CurState = State.Dead;
		_isDeadPhase = true;
	}

	private void DeadEntered()
	{
		PinUpRootBone();
		TargetAnimal.Play(_blowMotion, loop: false);
		TargetAnimal.TurnToYaw(_deadYaw, bSnap: false);
		TweenPosition tweenPosition = TweenPosition.Begin(TargetAnimal.gameObject, _playTrexCutSceneDelay, _deadDestPos);
		tweenPosition.method = UITweener.Method.EaseInOut;
		tweenPosition.PlayForward();
		GameSystem<PrologueGuideSystem>.Instance().HideGuideMask();
		Singleton<PrologueManager>.Instance().DelayedCall(BeginFinalCutScene, _playTrexCutSceneDelay);
	}

	private void DeadExited()
	{
		UnPinUpRootBone();
	}

	private IEnumerator DeadDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CDeadDoing_003Ed__87(0)
		{
			_003C_003E4__this = this
		};
	}

	private void BeginFinalCutScene()
	{
		Singleton<TrainTrexController>.Instance().PlayTrexCutScene();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void PinUpRootBone()
	{
		TargetAnimal.SetActivateRootMotion(active: true);
	}

	private void UnPinUpRootBone()
	{
		TargetAnimal.SetActivateRootMotion(active: false);
		TargetAnimal.MeshObjectTransform.localPosition = Vector3.zero;
	}

	public void OnTakeDamage(Damage damage, bool isDead)
	{
		if (damage.Value > 0 && !_isDeadPhase)
		{
			if (isDead)
			{
				EventDead();
				PlayerBehavior.LocalPlayer.OnKilledAnimal(base.gameObject.GetComponent<AnimalBehavior>());
			}
			else if ((damage.Effects & DamageEffects.Blow) > DamageEffects.None)
			{
				EventBlow();
			}
			else if ((damage.Effects & DamageEffects.KnockBack) > DamageEffects.None)
			{
				EventFlinch();
			}
		}
	}
}
