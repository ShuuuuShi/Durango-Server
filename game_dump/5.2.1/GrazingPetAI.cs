using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Durango.Terrain;
using Durango.Utils;
using Messages;
using UnityEngine;

public class GrazingPetAI : StateBasedAI<GrazingPetAI.State>
{
	public enum State
	{
		Invalid = -1,
		Idle,
		Roming,
		Count
	}

	[CompilerGenerated]
	private sealed class _003COnIdle_003Ed__19 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public GrazingPetAI _003C_003E4__this;

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
		public _003COnIdle_003Ed__19(int _003C_003E1__state)
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
			GrazingPetAI grazingPetAI = _003C_003E4__this;
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
			}
			float value = UnityEngine.Random.value;
			if (!(value > 0.5f))
			{
				float seconds = 1f;
				if (value < 0.2f)
				{
					if (grazingPetAI.TargetAnimal.AnimalFrameworkResource.GetAnimationElements("idle") is AnimationElem animationElem)
					{
						grazingPetAI.TargetAnimal.Play(animationElem.motion, loop: true, 0f, grazingPetAI.PlaybackRate);
						seconds = animationElem.Clip.length / grazingPetAI.PlaybackRate;
					}
				}
				else if (grazingPetAI.TargetAnimal.AnimalFrameworkResource.GetAnimationElements("stand") is AnimationElem animationElem2)
				{
					grazingPetAI.TargetAnimal.Play(animationElem2.motion, loop: true, 0f, grazingPetAI.PlaybackRate);
					float num2 = animationElem2.Clip.length / grazingPetAI.PlaybackRate;
					seconds = UnityEngine.Random.Range(num2, num2 * 3f);
				}
				_003C_003E2__current = new WaitForSeconds(seconds);
				_003C_003E1__state = 1;
				return true;
			}
			grazingPetAI.CurState = State.Roming;
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
	private sealed class _003COnRoming_003Ed__20 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public GrazingPetAI _003C_003E4__this;

		private float _003CmoveSpeed_003E5__2;

		private float _003Ctimer_003E5__3;

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
		public _003COnRoming_003Ed__20(int _003C_003E1__state)
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
			GrazingPetAI grazingPetAI = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				goto IL_0029;
			case 1:
				_003C_003E1__state = -1;
				goto IL_01e4;
			case 2:
				{
					_003C_003E1__state = -1;
					goto IL_0029;
				}
				IL_0029:
				if (!(UnityEngine.Random.value > 0.7f))
				{
					MoveSet moveSet = ((grazingPetAI.TargetAnimal.AnimalFrameworkResource.GetAnimationElements("move_motion_sets") is AnimationElemMoveSet animationElemMoveSet) ? animationElemMoveSet.elems.FirstOrDefault() : null);
					if (moveSet != null)
					{
						MoveMotionInfo moveMotion = moveSet.GetMoveMotion(0f);
						grazingPetAI.TargetAnimal.SetRotateSpeed(moveMotion.rot_speed);
						grazingPetAI.TargetAnimal.Play(moveMotion.motion, loop: true, 0f, grazingPetAI.PlaybackRate);
						_003CmoveSpeed_003E5__2 = moveMotion.base_move_speed;
						float yaw = UnityEngine.Random.value * 360f;
						_003Ctimer_003E5__3 = UnityEngine.Random.Range(2f, 7f);
						grazingPetAI.TargetAnimal.TurnToYaw(yaw, bSnap: false);
						goto IL_01e4;
					}
					goto IL_01f4;
				}
				grazingPetAI.CurState = State.Idle;
				return false;
				IL_01e4:
				if (_003Ctimer_003E5__3 > 0f)
				{
					float currentYaw = grazingPetAI.TargetAnimal.CurrentYaw;
					Vector3 delta = new Vector3(Mathf.Sin(currentYaw * ((float)Math.PI / 180f)), 0f, Mathf.Cos(currentYaw * ((float)Math.PI / 180f))) * _003CmoveSpeed_003E5__2 * Time.deltaTime;
					Vector3 currentPosition = grazingPetAI.TargetAnimal.CurrentPosition;
					Vector3 vector = grazingPetAI.ProcessCollisionWithSliding(grazingPetAI.TargetAnimal.CurrentPosition, delta);
					Vector2 floatTile = Util.ClientPositionToTilePosition(vector);
					if (TerrainWater.IsTooDeepToSwim(Singleton<TerrainBase>.Instance().GetTileDepth(floatTile), 0f))
					{
						grazingPetAI.CurState = State.Idle;
						return false;
					}
					if ((vector - currentPosition).magnitude / Time.deltaTime / _003CmoveSpeed_003E5__2 < 0.7f)
					{
						grazingPetAI.CurState = State.Idle;
						return false;
					}
					grazingPetAI.TargetAnimal.CurrentPosition = vector;
					_003Ctimer_003E5__3 -= Time.deltaTime;
					_003C_003E2__current = null;
					_003C_003E1__state = 1;
					return true;
				}
				goto IL_01f4;
				IL_01f4:
				_003C_003E2__current = null;
				_003C_003E1__state = 2;
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

	public AnimalBehavior TargetAnimal { get; private set; }

	protected override State InvalidState => State.Invalid;

	protected override int StateEnumCount => 2;

	public Pet Pet { get; set; }

	private float PlaybackRate
	{
		get
		{
			if (Pet.Stat.PlaybackRate > 0f)
			{
				return Pet.Stat.PlaybackRate;
			}
			return 1f;
		}
	}

	protected override void OnAwake()
	{
		TargetAnimal = GetComponent<AnimalBehavior>();
		TargetAnimal.SetActivateRootMotion(active: false);
	}

	protected override IEnumerator OnStart()
	{
		base.CurState = State.Idle;
		return base.OnStart();
	}

	private void Update()
	{
		Vector3 currentPosition = TargetAnimal.CurrentPosition;
		currentPosition.y = TargetAnimal.ProcessWaterDepth(currentPosition);
		TargetAnimal.CurrentPosition = currentPosition;
	}

	protected override void DefineStates()
	{
		AddState(State.Idle, new StateElem
		{
			Doing = OnIdle
		});
		AddState(State.Roming, new StateElem
		{
			Doing = OnRoming
		});
	}

	private IEnumerator OnIdle()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003COnIdle_003Ed__19(0)
		{
			_003C_003E4__this = this
		};
	}

	private IEnumerator OnRoming()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003COnRoming_003Ed__20(0)
		{
			_003C_003E4__this = this
		};
	}

	private Vector3 ProcessCollisionWithSliding(Vector3 beginPos, Vector3 delta)
	{
		if (delta == Vector3.zero)
		{
			return beginPos;
		}
		delta = Collisions.ProcessSimpleSliding(Collisions.CreateCollisionParam(beginPos, delta));
		return beginPos + delta;
	}

	protected override bool IsAIEnded()
	{
		return false;
	}

	protected override bool IsTerminalState(State state)
	{
		return false;
	}
}
