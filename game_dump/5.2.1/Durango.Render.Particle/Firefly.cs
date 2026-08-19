using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Terrain;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.Render.Particle;

public class Firefly : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CStart_003Ed__6 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Firefly _003C_003E4__this;

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
		public _003CStart_003Ed__6(int _003C_003E1__state)
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
			Firefly firefly = _003C_003E4__this;
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
			float normalizedTime = TimeGauge.GetNormalizedTime();
			float num2 = UnityEngine.Random.value * 2f - 1f;
			normalizedTime += num2 * (1f / 24f);
			firefly._isActiveTime = IsActiveTime(normalizedTime);
			firefly.UpdateParticle();
			float num3 = ((!firefly._isActiveTime) ? (5f / 6f - normalizedTime) : (5f / 24f - normalizedTime));
			if (num3 < 0f)
			{
				num3 += 1f;
			}
			float realTimeFromNormalizedTime = TimeGauge.GetRealTimeFromNormalizedTime(num3);
			_003C_003E2__current = new WaitForSeconds(realTimeFromNormalizedTime);
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

	private const float StartTime = 5f / 6f;

	private const float EndTime = 5f / 24f;

	private const float RandomInterval = 1f / 24f;

	private int _particleId;

	private bool _isActiveTime;

	private static bool _particleAllowed;

	private IEnumerator Start()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CStart_003Ed__6(0)
		{
			_003C_003E4__this = this
		};
	}

	private void EmitParticle()
	{
		if (_particleId == 0)
		{
			float num = UnityEngine.Random.value - 0.5f;
			float num2 = UnityEngine.Random.value - 0.5f;
			Vector3 pos = new Vector3(num * 200f, 0f, num2 * 200f);
			_particleId = ParticleManager.EmitFollow("Particle/FX_Prop_FireFly_01.prefab", pos, Quaternion.identity, base.transform, useLocalPosition: true, comeForwardToCamera: false, groundDecal: false, default(Vector3), null, reusable: true, limit: false);
		}
	}

	private void StopParticle()
	{
		if (_particleId != 0)
		{
			ParticleManager.Stop(_particleId, immediately: false);
			_particleId = 0;
		}
	}

	private static bool IsActiveTime(float normalizedTime)
	{
		if (!(5f / 6f < normalizedTime))
		{
			return normalizedTime < 5f / 24f;
		}
		return true;
	}

	private void OnDisable()
	{
		_isActiveTime = false;
		StopParticle();
	}

	private void UpdateParticle()
	{
		if (_isActiveTime && _particleAllowed)
		{
			EmitParticle();
		}
		else
		{
			StopParticle();
		}
	}

	public static void ChangeFireflyOption(bool allow)
	{
		_particleAllowed = allow;
		if (Singleton<TerrainBase>.HasInstance())
		{
			Singleton<TerrainBase>.Instance().gameObject.BroadcastMessage("OnFireflyOptionChanged", SendMessageOptions.DontRequireReceiver);
		}
	}

	[UsedImplicitly]
	private void OnFireflyOptionChanged()
	{
		UpdateParticle();
	}
}
