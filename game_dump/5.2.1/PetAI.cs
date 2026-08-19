using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Durango.Logic.Map;
using Durango.Model;
using Durango.Network;
using Durango.Terrain;
using Durango.UI;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class PetAI : StateBasedAI<PetAI.State>
{
	public enum HungryState
	{
		Good,
		NoBattle,
		NoRide
	}

	public enum State
	{
		Invalid = -1,
		SpawnInCage,
		RoamingInCage,
		IdleInCage,
		SpawnNearMaster,
		Normal,
		Chase,
		Idle,
		Riding,
		Return,
		EatOut,
		Battle,
		Dead,
		Count
	}

	[CompilerGenerated]
	private sealed class _003CBattleDoing_003Ed__87 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PetAI _003C_003E4__this;

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
		public _003CBattleDoing_003Ed__87(int _003C_003E1__state)
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
			PetAI petAI = _003C_003E4__this;
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
			if (petAI.IsInterrupted)
			{
				return false;
			}
			if (!(petAI.TargetAnimal.GetMoveServerTime() - petAI._latestMovementEndTime > 3.0))
			{
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			petAI.BattleEnd();
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
	private sealed class _003CChaseDoing_003Ed__73 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PetAI _003C_003E4__this;

		private bool _003CisMoving_003E5__2;

		private float _003ClastMoveTime_003E5__3;

		private float _003CprevTime_003E5__4;

		private Pair<string, float> _003CrunMotion_003E5__5;

		private string _003CstandMotion_003E5__6;

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
		public _003CChaseDoing_003Ed__73(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003CrunMotion_003E5__5 = default(Pair<string, float>);
			_003CstandMotion_003E5__6 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			PetAI petAI = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				_003CisMoving_003E5__2 = false;
				_003ClastMoveTime_003E5__3 = Time.time;
				_003CprevTime_003E5__4 = Time.time;
				AnimationElemBase animationElements = petAI.TargetAnimal.AnimalFrameworkResource.GetAnimationElements("stand");
				_003CrunMotion_003E5__5 = petAI.GetMoveClip(700f);
				_003CstandMotion_003E5__6 = animationElements?.FirstOrDefault().Clip;
				break;
			}
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (!(petAI.Master == null) && !petAI.IsInterrupted)
			{
				float num2 = Time.time - _003CprevTime_003E5__4;
				_003CprevTime_003E5__4 = Time.time;
				float magnitude = Maths.Make2D(petAI.Master.transform.position - petAI.transform.position).magnitude;
				if (magnitude <= petAI.FollowDistance)
				{
					petAI.CurState = State.Normal;
				}
				else
				{
					if (!(magnitude > petAI.MaxFollowDistance))
					{
						Vector3 normalized = Maths.Make2D(petAI.CalcChasePosition(petAI.Master) - petAI.transform.position).normalized;
						petAI.TargetAnimal.TurnToYaw(Maths.CalcYaw(normalized), bSnap: false);
						Vector3 vector = normalized * petAI._vehicle.MoveSpeed;
						Vector3 currentPosition = petAI.TargetAnimal.CurrentPosition;
						Vector3 vector2 = petAI.ProcessCollisionWithSliding(petAI.TargetAnimal.CurrentPosition, vector * num2);
						if ((vector2 - currentPosition).magnitude / num2 / petAI._vehicle.MoveSpeed < 0.7f)
						{
							if (_003CisMoving_003E5__2)
							{
								petAI.TargetAnimal.CrossFade(_003CstandMotion_003E5__6, 0.1f);
								_003CisMoving_003E5__2 = false;
							}
							if (Time.time - _003ClastMoveTime_003E5__3 > 10f)
							{
								petAI.CurState = State.SpawnNearMaster;
							}
						}
						else
						{
							if (!_003CisMoving_003E5__2)
							{
								AnimalBehavior targetAnimal = petAI.TargetAnimal;
								string item = _003CrunMotion_003E5__5.Item1;
								float fadeTime = 0.1f;
								float item2 = _003CrunMotion_003E5__5.Item2;
								targetAnimal.CrossFade(item, fadeTime, loop: true, 0f, item2);
								_003CisMoving_003E5__2 = true;
							}
							_003ClastMoveTime_003E5__3 = Time.time;
							petAI.Locate(vector2, randomYaw: false);
						}
						_003C_003E2__current = null;
						_003C_003E1__state = 1;
						return true;
					}
					petAI.CurState = State.SpawnNearMaster;
				}
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
	private sealed class _003CCoPlayMotion_003Ed__92 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float length;

		public PetAI _003C_003E4__this;

		public string motionName;

		public float fadeInTime;

		public float playbackRate;

		public Func<bool> funcTransition;

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
		public _003CCoPlayMotion_003Ed__92(int _003C_003E1__state)
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
			PetAI petAI = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				bool loop = length >= 0f;
				petAI.TargetAnimal.CrossFade(motionName, fadeInTime, loop, 0f, playbackRate);
				if (petAI.TargetAnimal.CurAnimState != null)
				{
					length = Mathf.Max(length, petAI.TargetAnimal.CurAnimState.length);
				}
				_003CprevTime_003E5__2 = Time.time;
				break;
			}
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (!(Time.time - _003CprevTime_003E5__2 >= length) && !petAI.IsInterrupted && (funcTransition == null || !funcTransition()))
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

	[CompilerGenerated]
	private sealed class _003CCoUpdateHungry_003Ed__47 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PetAI _003C_003E4__this;

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
		public _003CCoUpdateHungry_003Ed__47(int _003C_003E1__state)
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
			PetAI petAI = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			case 2:
				_003C_003E1__state = -1;
				petAI.UpdateHungryState();
				break;
			}
			if (petAI._hungryGauge != null)
			{
				float num2 = petAI._hungryGauge.Get();
				if (num2 <= 0f)
				{
					_003C_003E2__current = HungryUpdateWaitForSeconds;
					_003C_003E1__state = 1;
					return true;
				}
				float seconds = (float)(petAI._hungryGauge.When(num2 - 1f) - Connections.Frontend.GetPredictedServerTime());
				_003C_003E2__current = new WaitForSeconds(seconds);
				_003C_003E1__state = 2;
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

	[CompilerGenerated]
	private sealed class _003CDeadDoing_003Ed__89 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PetAI _003C_003E4__this;

		private string _003Cclip_003E5__2;

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
		public _003CDeadDoing_003Ed__89(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Cclip_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			PetAI petAI = _003C_003E4__this;
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
				petAI.TargetAnimal.SetActivateRootMotion(active: true);
				_003Cclip_003E5__2 = petAI.TargetAnimal.AnimalFrameworkResource.GetAnimationElements("dead")?.FirstOrDefault().Clip;
			}
			if (petAI.TargetAnimal.CurrentAnimClipName != _003Cclip_003E5__2)
			{
				petAI.TargetAnimal.CrossFade(_003Cclip_003E5__2, 0.1f, loop: false);
			}
			if (!petAI.TargetAnimal.IsAlive)
			{
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			petAI.TransitionTo(State.Normal, force: true);
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
	private sealed class _003CEatOutDoing_003Ed__84 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PetAI _003C_003E4__this;

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
		public _003CEatOutDoing_003Ed__84(int _003C_003E1__state)
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
			PetAI petAI = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				string motionName = petAI.TargetAnimal.AnimalFrameworkResource.GetAnimationElements("eat")?.FirstOrDefault().Clip;
				_003C_003E2__current = petAI.StartCoroutine(petAI.CoPlayMotion(motionName, null, 10f));
				_003C_003E1__state = 1;
				return true;
			}
			case 1:
				_003C_003E1__state = -1;
				if (!petAI.IsInterrupted)
				{
					petAI.CurState = State.Normal;
				}
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
	private sealed class _003CIdleDoing_003Ed__75 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PetAI _003C_003E4__this;

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
		public _003CIdleDoing_003Ed__75(int _003C_003E1__state)
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
			PetAI CS_0024_003C_003E8__locals0 = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				string motionName = CS_0024_003C_003E8__locals0.TargetAnimal.AnimalFrameworkResource.GetAnimationElements("stand")?.FirstOrDefault().Clip;
				_003C_003E2__current = CS_0024_003C_003E8__locals0.StartCoroutine(CS_0024_003C_003E8__locals0.CoPlayMotion(motionName, delegate
				{
					if (CS_0024_003C_003E8__locals0.NeedToChaseMaster())
					{
						CS_0024_003C_003E8__locals0.CurState = State.Chase;
						return true;
					}
					return false;
				}, 0f));
				_003C_003E1__state = 1;
				return true;
			}
			case 1:
				_003C_003E1__state = -1;
				if (!CS_0024_003C_003E8__locals0.IsInterrupted)
				{
					CS_0024_003C_003E8__locals0.CurState = State.Normal;
				}
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
	private sealed class _003CIdleInCageDoing_003Ed__70 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PetAI _003C_003E4__this;

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
		public _003CIdleInCageDoing_003Ed__70(int _003C_003E1__state)
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
			PetAI petAI = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				string motionName = petAI.TargetAnimal.AnimalFrameworkResource.GetAnimationElements("stand")?.FirstOrDefault().Clip;
				_003C_003E2__current = petAI.StartCoroutine(petAI.CoPlayMotion(motionName, null, UnityEngine.Random.Range(1, 10)));
				_003C_003E1__state = 1;
				return true;
			}
			case 1:
				_003C_003E1__state = -1;
				if (!petAI.IsInterrupted)
				{
					petAI.CurState = State.RoamingInCage;
				}
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
	private sealed class _003CNormalDoing_003Ed__67 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PetAI _003C_003E4__this;

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
		public _003CNormalDoing_003Ed__67(int _003C_003E1__state)
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
			PetAI petAI = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (!(petAI.Master == null))
				{
					petAI.CurState = ((!petAI.NeedToChaseMaster()) ? State.Idle : State.Chase);
					_003C_003E2__current = new WaitForSeconds(1f);
					_003C_003E1__state = 1;
					return true;
				}
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
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
	private sealed class _003COnStart_003Ed__57 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PetAI _003C_003E4__this;

		private BoneLookAtTarget _003ClookAt_003E5__2;

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
		public _003COnStart_003Ed__57(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003ClookAt_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			PetAI petAI = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003ClookAt_003E5__2 = petAI.GetComponent<BoneLookAtTarget>();
				if (_003ClookAt_003E5__2 != null)
				{
					_003ClookAt_003E5__2.AutoChangeTarget = false;
				}
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (!TerrainBase.IsPlayerInitialized)
			{
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			if (_003ClookAt_003E5__2 != null)
			{
				_003ClookAt_003E5__2.SetLookTarget(petAI.Master, findHead: true);
			}
			petAI.AddToMapIndicator();
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
	private sealed class _003CReturnDoing_003Ed__80 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PetAI _003C_003E4__this;

		private Vector3 _003CreturnPos_003E5__2;

		private float _003CprevTime_003E5__3;

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
		public _003CReturnDoing_003Ed__80(int _003C_003E1__state)
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
			PetAI petAI = _003C_003E4__this;
			Pair<string, float> moveClip;
			AnimalBehavior targetAnimal;
			string item;
			float fadeTime;
			float item2;
			Vector3 vector;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (petAI.Master == null)
				{
					petAI.RemovePet();
					return false;
				}
				if (!petAI.InCage)
				{
					_003C_003E2__current = new WaitForSeconds(2f);
					_003C_003E1__state = 1;
					return true;
				}
				goto IL_0067;
			case 1:
				_003C_003E1__state = -1;
				goto IL_0067;
			case 2:
				{
					_003C_003E1__state = -1;
					break;
				}
				IL_0067:
				if (petAI.Master == null)
				{
					petAI.RemovePet();
					return false;
				}
				moveClip = petAI.GetMoveClip(petAI._vehicle.MoveSpeed);
				targetAnimal = petAI.TargetAnimal;
				item = moveClip.Item1;
				fadeTime = 0.1f;
				item2 = moveClip.Item2;
				targetAnimal.CrossFade(item, fadeTime, loop: true, 0f, item2);
				vector = Maths.Make2D(petAI.transform.position - petAI.Master.transform.position).normalized;
				if (vector == Vector3.zero)
				{
					vector = Vector3.right;
				}
				_003CreturnPos_003E5__2 = petAI.transform.position + vector * 3000f;
				_003CprevTime_003E5__3 = Time.time;
				break;
			}
			float num2 = Time.time - _003CprevTime_003E5__3;
			_003CprevTime_003E5__3 = Time.time;
			if (!(Maths.Make2D(_003CreturnPos_003E5__2 - petAI.transform.position).magnitude <= 100f))
			{
				vector = Maths.Make2D(_003CreturnPos_003E5__2 - petAI.transform.position).normalized;
				petAI.TargetAnimal.TurnToYaw(Maths.CalcYaw(vector), bSnap: false);
				Vector3 vector2 = vector * petAI._vehicle.MoveSpeed;
				petAI.TargetAnimal.CurrentPosition += vector2 * num2;
				_003C_003E2__current = null;
				_003C_003E1__state = 2;
				return true;
			}
			petAI.RemovePet();
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
	private sealed class _003CRidingDoing_003Ed__78 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PetAI _003C_003E4__this;

		private Pair<string, float> _003CrunMotion_003E5__2;

		private string _003CstandMotion_003E5__3;

		private bool _003CprevMoving_003E5__4;

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
		public _003CRidingDoing_003Ed__78(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003CrunMotion_003E5__2 = default(Pair<string, float>);
			_003CstandMotion_003E5__3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			PetAI CS_0024_003C_003E8__locals0 = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				AnimationElemBase animationElements = CS_0024_003C_003E8__locals0.TargetAnimal.AnimalFrameworkResource.GetAnimationElements("stand");
				_003CrunMotion_003E5__2 = CS_0024_003C_003E8__locals0.GetMoveClip(CS_0024_003C_003E8__locals0._vehicle.MoveSpeed);
				_003CstandMotion_003E5__3 = animationElements?.FirstOrDefault().Clip;
				_003CprevMoving_003E5__4 = false;
				goto IL_0187;
			}
			case 1:
				_003C_003E1__state = -1;
				goto IL_015a;
			case 2:
				{
					_003C_003E1__state = -1;
					goto IL_0187;
				}
				IL_015a:
				_003CprevMoving_003E5__4 = CS_0024_003C_003E8__locals0.TargetAnimal.IsMoving;
				_003C_003E2__current = null;
				_003C_003E1__state = 2;
				return true;
				IL_0187:
				if (!CS_0024_003C_003E8__locals0.IsInterrupted)
				{
					if ((!string.IsNullOrEmpty(CS_0024_003C_003E8__locals0._vehicle.StopMotion) & _003CprevMoving_003E5__4) && !CS_0024_003C_003E8__locals0.TargetAnimal.IsMoving)
					{
						_003C_003E2__current = CS_0024_003C_003E8__locals0.StartCoroutine(CS_0024_003C_003E8__locals0.CoPlayMotion(CS_0024_003C_003E8__locals0._vehicle.StopMotion, () => CS_0024_003C_003E8__locals0.TargetAnimal.IsMoving));
						_003C_003E1__state = 1;
						return true;
					}
					if ((bool)CS_0024_003C_003E8__locals0.TargetAnimal.IsMoving)
					{
						AnimalBehavior targetAnimal = CS_0024_003C_003E8__locals0.TargetAnimal;
						string item = _003CrunMotion_003E5__2.Item1;
						float item2 = _003CrunMotion_003E5__2.Item2;
						targetAnimal.CrossFade(item, -1f, loop: true, 0f, item2);
					}
					else
					{
						CS_0024_003C_003E8__locals0.TargetAnimal.CrossFade(_003CstandMotion_003E5__3);
					}
					goto IL_015a;
				}
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
	private sealed class _003CRoamingInCageDoing_003Ed__69 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PetAI _003C_003E4__this;

		private float _003CprevTime_003E5__2;

		private Vector3 _003CdestPos_003E5__3;

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
		public _003CRoamingInCageDoing_003Ed__69(int _003C_003E1__state)
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
			PetAI petAI = _003C_003E4__this;
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
				Pair<string, float> moveClip = petAI.GetMoveClip(petAI._vehicle.WalkSpeed);
				AnimalBehavior targetAnimal = petAI.TargetAnimal;
				string item = moveClip.Item1;
				float fadeTime = 0.1f;
				float item2 = moveClip.Item2;
				targetAnimal.CrossFade(item, fadeTime, loop: true, 0f, item2);
				_003CprevTime_003E5__2 = Time.time;
				_003CdestPos_003E5__3 = petAI.CalcRoamingPositionInCage();
			}
			if (petAI.Master == null || petAI.IsInterrupted)
			{
				return false;
			}
			float num2 = Time.time - _003CprevTime_003E5__2;
			_003CprevTime_003E5__2 = Time.time;
			if (!(Maths.Make2D(_003CdestPos_003E5__3 - petAI.transform.position).magnitude <= 100f))
			{
				Vector3 normalized = Maths.Make2D(_003CdestPos_003E5__3 - petAI.transform.position).normalized;
				petAI.TargetAnimal.TurnToYaw(Maths.CalcYaw(normalized), bSnap: false);
				Vector3 vector = normalized * petAI._vehicle.WalkSpeed;
				petAI.TargetAnimal.CurrentPosition += vector * num2;
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			petAI.CurState = State.IdleInCage;
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
	private sealed class _003CSpawnAlreadyDead_003Ed__58 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PetAI _003C_003E4__this;

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
		public _003CSpawnAlreadyDead_003Ed__58(int _003C_003E1__state)
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
			PetAI petAI = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				goto IL_004b;
			case 1:
				_003C_003E1__state = -1;
				goto IL_004b;
			case 2:
				{
					_003C_003E1__state = -1;
					break;
				}
				IL_004b:
				if (!TerrainBase.IsPlayerInitialized)
				{
					_003C_003E2__current = new WaitForSeconds(0.5f);
					_003C_003E1__state = 1;
					return true;
				}
				break;
			}
			if (petAI.Master == null)
			{
				_003C_003E2__current = null;
				_003C_003E1__state = 2;
				return true;
			}
			petAI.Locate(petAI.MasterPos);
			petAI.TargetAnimal.TurnToYaw(petAI.TargetAnimal.EntityId.GetHashCode() % 360, bSnap: true);
			petAI.TargetAnimal.SetActivateRootMotion(active: true);
			string motionName = petAI.TargetAnimal.AnimalFrameworkResource.GetAnimationElements("dead")?.FirstOrDefault().Clip;
			petAI.TargetAnimal.PlayToLast(motionName);
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
	private sealed class _003CSpawnInCageDoing_003Ed__68 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PetAI _003C_003E4__this;

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
		public _003CSpawnInCageDoing_003Ed__68(int _003C_003E1__state)
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
			PetAI petAI = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (!TerrainBase.IsPlayerInitialized)
			{
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			petAI.Locate(petAI.CalcRoamingPositionInCage());
			petAI.CurState = State.IdleInCage;
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
	private sealed class _003CSpawnNearMasterDoing_003Ed__63 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PetAI _003C_003E4__this;

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
		public _003CSpawnNearMasterDoing_003Ed__63(int _003C_003E1__state)
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
			PetAI petAI = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				goto IL_004b;
			case 1:
				_003C_003E1__state = -1;
				goto IL_004b;
			case 2:
				{
					_003C_003E1__state = -1;
					break;
				}
				IL_004b:
				if (!TerrainBase.IsPlayerInitialized)
				{
					_003C_003E2__current = new WaitForSeconds(0.5f);
					_003C_003E1__state = 1;
					return true;
				}
				break;
			}
			if (petAI.Master == null)
			{
				_003C_003E2__current = null;
				_003C_003E1__state = 2;
				return true;
			}
			Vector3 vector = default(Vector3);
			for (int i = 0; i < 30; i++)
			{
				vector = petAI.GetRandomMasterSurroundingPos(1000f);
				if (!Durango.Utils.Singleton<TerrainBase>.Instance().IsCollidableMasked(Util.ClientPositionToWorldPosition(vector)))
				{
					break;
				}
				_ = 29;
			}
			petAI.Locate(vector);
			petAI.CurState = ((!petAI.GetComponent<CharacterBehavior>().IsAlive) ? State.Dead : State.Normal);
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

	public const float HungryPointForRide = 0f;

	private const float PenetrateAvoidTime = 10f;

	private const float SpawnDistanceFromMaster = 1000f;

	public static readonly float HungryRatioForBattle;

	private static readonly WaitForSeconds HungryUpdateWaitForSeconds;

	private static int _globalPosIndex;

	private VehiclePet _vehicle;

	private int _myPosIndex;

	private bool _isMapIndicatorAdded;

	private Vector3 _minArea = Vector3.zero;

	private Vector3 _maxArea = Vector3.zero;

	private GameObject _mealProp;

	private bool _isInitiallyLocated;

	private ICoroutineBinder _binder;

	[CanBeNull]
	private Gauge _hungryGauge;

	private double _latestMovementEndTime;

	protected override State InvalidState => State.Invalid;

	protected override int StateEnumCount => 12;

	public AnimalBehavior TargetAnimal { get; private set; }

	public bool InCage { get; private set; }

	public string OwnerName => _vehicle.OwnerName;

	public int AnimalEntityType { get; private set; }

	private float FollowDistance => _vehicle.FollowDistance;

	private float DistanceThreshould => _vehicle.DistanceThreshould;

	private float MaxFollowDistance => _vehicle.MaxFollowDistance;

	public HungryState Hungry { get; private set; }

	public void SetHungryGauge(Gauge hungry)
	{
		_hungryGauge = hungry;
		UpdateHungryState();
		this.StartCoroutine(ref _binder, CoUpdateHungry());
	}

	private IEnumerator CoUpdateHungry()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoUpdateHungry_003Ed__47(0)
		{
			_003C_003E4__this = this
		};
	}

	private void UpdateHungryState()
	{
		if (_hungryGauge != null)
		{
			HungryState hungry = HungryState.Good;
			float num = _hungryGauge.Get();
			float num2 = _hungryGauge.Max();
			if (num <= 0f * num2)
			{
				hungry = HungryState.NoRide;
			}
			else if (num <= HungryRatioForBattle * num2)
			{
				hungry = HungryState.NoBattle;
			}
			Hungry = hungry;
		}
	}

	public void Init(int animalEntityType)
	{
		_vehicle = GetComponent<VehiclePet>();
		AnimalEntityType = animalEntityType;
		InCage = false;
		_minArea = default(Vector3);
		_maxArea = default(Vector3);
		CharacterBehavior component = GetComponent<CharacterBehavior>();
		component.SurvivalGaugeUpdated += SurvivalGaugeUpdated;
		component.SurvivalGaugeInitialized += SurvivalGaugeInitialized;
		TargetAnimal.PathMovable.MovementProcessed += MovementProcessed;
	}

	public void SetInCage(Vector3 minArea, Vector3 maxArea)
	{
		InCage = true;
		_minArea = minArea;
		_maxArea = maxArea;
	}

	public void SetMaster(GameObject master, bool isRiding)
	{
		base.Master = master;
		base.CurState = (isRiding ? State.Riding : ((!InCage) ? State.SpawnNearMaster : State.SpawnInCage));
	}

	private void OnDestroy()
	{
		if (base.CurState != State.Invalid)
		{
			CharacterBehavior component = GetComponent<CharacterBehavior>();
			component.SurvivalGaugeUpdated -= SurvivalGaugeUpdated;
			component.SurvivalGaugeInitialized -= SurvivalGaugeInitialized;
			TargetAnimal.PathMovable.MovementProcessed -= MovementProcessed;
		}
	}

	private void MovementProcessed(Movement movement)
	{
		if (base.CurState != State.Battle)
		{
			BattleBegin();
		}
		_latestMovementEndTime = movement.Path[movement.Path.Length - 1].Time;
	}

	private void SurvivalGaugeUpdated(CharacterBehavior character)
	{
		if (!character.IsAlive && _isInitiallyLocated)
		{
			if (IsLocalPlayersPet())
			{
				GameSystem<PlayGuideSystem>.Instance().NotifyEventOccured("pet", "dead");
			}
			base.CurState = State.Dead;
		}
	}

	private void SurvivalGaugeInitialized(CharacterBehavior character)
	{
		if (!character.IsAlive)
		{
			base.CurState = State.Dead;
			StartCoroutine(SpawnAlreadyDead());
		}
	}

	protected override void OnAwake()
	{
		_myPosIndex = _globalPosIndex;
		_globalPosIndex++;
		TargetAnimal = GetComponent<AnimalBehavior>();
		TargetAnimal.SetActivateRootMotion(active: false);
	}

	protected override IEnumerator OnStart()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003COnStart_003Ed__57(0)
		{
			_003C_003E4__this = this
		};
	}

	private IEnumerator SpawnAlreadyDead()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSpawnAlreadyDead_003Ed__58(0)
		{
			_003C_003E4__this = this
		};
	}

	private void AddToMapIndicator()
	{
		if (!_isMapIndicatorAdded)
		{
			_isMapIndicatorAdded = true;
			MapIconIndicator orAdd = MapIndicators.GetOrAdd<MapIconIndicator>(TargetAnimal.EntityId, IndicatorType.Pet);
			orAdd.SetTarget(TargetAnimal.gameObject);
			orAdd.SetIcon("icon_map_animal", PresetColor.UISkyBlue, (!IsLocalPlayersPet()) ? 10 : 16, 30);
		}
	}

	protected override void DefineStates()
	{
		AddState(State.SpawnInCage, new StateElem
		{
			Doing = SpawnInCageDoing
		});
		AddState(State.RoamingInCage, new StateElem
		{
			Doing = RoamingInCageDoing
		});
		AddState(State.IdleInCage, new StateElem
		{
			Doing = IdleInCageDoing
		});
		AddState(State.SpawnNearMaster, new StateElem
		{
			Doing = SpawnNearMasterDoing
		});
		AddState(State.Normal, new StateElem
		{
			Entered = NormalEntered,
			Doing = NormalDoing
		});
		AddState(State.Chase, new StateElem
		{
			Doing = ChaseDoing
		});
		AddState(State.Idle, new StateElem
		{
			Doing = IdleDoing
		});
		AddState(State.Riding, new StateElem
		{
			Doing = RidingDoing
		});
		AddState(State.Return, new StateElem
		{
			Doing = ReturnDoing
		});
		AddState(State.EatOut, new StateElem
		{
			Entered = EatOutEntered,
			Doing = EatOutDoing,
			Exited = EatOutExited
		});
		AddState(State.Battle, new StateElem
		{
			Entered = BattleEntered,
			Doing = BattleDoing,
			Exited = BattleExited
		});
		AddState(State.Dead, new StateElem
		{
			Doing = DeadDoing,
			Exited = DeadExited
		});
	}

	protected override bool IsAIEnded()
	{
		return false;
	}

	protected override bool IsTerminalState(State state)
	{
		if (state == State.Return || state == State.Dead)
		{
			return true;
		}
		return false;
	}

	private IEnumerator SpawnNearMasterDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSpawnNearMasterDoing_003Ed__63(0)
		{
			_003C_003E4__this = this
		};
	}

	private void Locate(Vector3 newPos, bool randomYaw = true)
	{
		float y = TargetAnimal.ProcessWaterDepth(newPos);
		newPos.y = y;
		TargetAnimal.CurrentPosition = newPos;
		if (randomYaw)
		{
			TargetAnimal.TurnToYaw(UnityEngine.Random.Range(0, 360), bSnap: true);
		}
		_isInitiallyLocated = true;
	}

	public void Tamed()
	{
		base.CurState = State.Normal;
	}

	private void NormalEntered()
	{
		string motionName = TargetAnimal.AnimalFrameworkResource.GetAnimationElements("stand")?.FirstOrDefault().Clip;
		TargetAnimal.CrossFade(motionName, 0.1f);
	}

	private IEnumerator NormalDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CNormalDoing_003Ed__67(0)
		{
			_003C_003E4__this = this
		};
	}

	private IEnumerator SpawnInCageDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSpawnInCageDoing_003Ed__68(0)
		{
			_003C_003E4__this = this
		};
	}

	private IEnumerator RoamingInCageDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CRoamingInCageDoing_003Ed__69(0)
		{
			_003C_003E4__this = this
		};
	}

	private IEnumerator IdleInCageDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CIdleInCageDoing_003Ed__70(0)
		{
			_003C_003E4__this = this
		};
	}

	private Vector3 CalcRoamingPositionInCage()
	{
		return new Vector3(UnityEngine.Random.Range(_minArea.x, _maxArea.x), 0f, UnityEngine.Random.Range(_minArea.z, _maxArea.z));
	}

	private bool NeedToChaseMaster()
	{
		return base.DistanceToMaster > FollowDistance + DistanceThreshould;
	}

	private IEnumerator ChaseDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CChaseDoing_003Ed__73(0)
		{
			_003C_003E4__this = this
		};
	}

	private Vector3 CalcChasePosition([NotNull] GameObject master)
	{
		float num = (float)((_myPosIndex + 1) / 2) * 20f;
		if (_myPosIndex % 2 == 0)
		{
			num = 0f - num;
		}
		Vector3 vector = Quaternion.Euler(0f, num, 0f) * master.transform.forward;
		Vector3 vector2 = master.transform.position + vector * FollowDistance;
		DebugExtension.DebugCircle(vector2, 50f, 5f);
		return vector2;
	}

	private IEnumerator IdleDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CIdleDoing_003Ed__75(0)
		{
			_003C_003E4__this = this
		};
	}

	public void BeginRide()
	{
		TargetAnimal.PathMovable.Clear();
		Locate(base.MasterPos, randomYaw: false);
		base.CurState = State.Riding;
	}

	public void EndRide()
	{
		base.CurState = State.Normal;
	}

	private IEnumerator RidingDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CRidingDoing_003Ed__78(0)
		{
			_003C_003E4__this = this
		};
	}

	public void Return()
	{
		if (!InCage && base.CurState != State.Return)
		{
			if (base.CurState == State.Dead)
			{
				RemovePet();
			}
			else
			{
				base.CurState = State.Return;
			}
		}
	}

	private IEnumerator ReturnDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CReturnDoing_003Ed__80(0)
		{
			_003C_003E4__this = this
		};
	}

	public void RemovePet()
	{
		if (base.CurState == State.Riding)
		{
			_vehicle.DetachDriver();
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void EatOut()
	{
		base.CurState = State.EatOut;
	}

	private void EatOutEntered()
	{
		Durango.Utils.Singleton<AssetBundleManager>.Instance().RequestAsset("Models/Prop/tool/basket_feed_01.prefab", typeof(GameObject), delegate(UnityEngine.Object asset)
		{
			if (!(this == null) && !((GameObject)asset == null))
			{
				GetComponentInChildren<Animation>().Sample();
				Vector3 position = base.transform.position + base.transform.forward * _vehicle.EatDistance;
				position.y = 0f;
				_mealProp = (GameObject)UnityEngine.Object.Instantiate(asset);
				_mealProp.transform.position = position;
				_mealProp.transform.rotation = Quaternion.identity;
			}
		});
	}

	private IEnumerator EatOutDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEatOutDoing_003Ed__84(0)
		{
			_003C_003E4__this = this
		};
	}

	private void EatOutExited()
	{
		UnityEngine.Object.Destroy(_mealProp);
	}

	private void BattleEntered()
	{
		TargetAnimal.SetActivateRootMotion(active: true);
	}

	private IEnumerator BattleDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CBattleDoing_003Ed__87(0)
		{
			_003C_003E4__this = this
		};
	}

	private void BattleExited()
	{
		TargetAnimal.SetActivateRootMotion(active: false);
	}

	private IEnumerator DeadDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CDeadDoing_003Ed__89(0)
		{
			_003C_003E4__this = this
		};
	}

	private void DeadExited()
	{
		TargetAnimal.SetActivateRootMotion(active: false);
	}

	public bool IsLocalPlayersPet()
	{
		if (PlayerBehavior.LocalPlayer == null)
		{
			return false;
		}
		return base.Master == PlayerBehavior.LocalPlayer.gameObject;
	}

	private IEnumerator CoPlayMotion(string motionName, Func<bool> funcTransition, float length = -1f, float fadeInTime = 0.1f, float playbackRate = 1f)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoPlayMotion_003Ed__92(0)
		{
			_003C_003E4__this = this,
			motionName = motionName,
			funcTransition = funcTransition,
			length = length,
			fadeInTime = fadeInTime,
			playbackRate = playbackRate
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

	public void BattleBegin()
	{
		base.CurState = State.Battle;
		_latestMovementEndTime = TargetAnimal.GetMoveServerTime();
	}

	public void BattleEnd()
	{
		base.CurState = State.Normal;
		TargetAnimal.RootMotionMovable.ResetRootMotionOffset();
		TargetAnimal.PathMovable.Clear();
	}

	private Pair<string, float> GetMoveClip(float moveSpeed)
	{
		AnimationElemMoveSet animationElemMoveSet = TargetAnimal.AnimalFrameworkResource.GetAnimationElements("move_motion_sets") as AnimationElemMoveSet;
		MoveSet moveSet = animationElemMoveSet?.elems.FirstOrDefault();
		if (moveSet == null)
		{
			return default(Pair<string, float>);
		}
		if (!string.IsNullOrEmpty(_vehicle.MoveSet))
		{
			foreach (MoveSet elem in animationElemMoveSet.elems)
			{
				if (string.Equals(elem.name, _vehicle.MoveSet, StringComparison.OrdinalIgnoreCase))
				{
					moveSet = elem;
					break;
				}
			}
		}
		MoveMotionInfo moveMotion = moveSet.GetMoveMotion(moveSpeed);
		if (moveMotion == null)
		{
			return default(Pair<string, float>);
		}
		return new Pair<string, float>(moveMotion.FirstOrDefault().Clip, moveSpeed / moveMotion.base_move_speed);
	}

	static PetAI()
	{
		HungryRatioForBattle = Yaml.Util.Singleton<Constants>.Instance.Pet.Battle.HungryRatioEnterBattle;
		HungryUpdateWaitForSeconds = new WaitForSeconds(1f);
	}
}
