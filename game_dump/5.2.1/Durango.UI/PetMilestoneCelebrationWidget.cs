using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Render.Particle;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

public class PetMilestoneCelebrationWidget : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CCoShowParticles_003Ed__12 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PetMilestoneCelebrationWidget _003C_003E4__this;

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
		public _003CCoShowParticles_003Ed__12(int _003C_003E1__state)
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
			PetMilestoneCelebrationWidget petMilestoneCelebrationWidget = _003C_003E4__this;
			int i;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (petMilestoneCelebrationWidget._startParticleDelay > 0f)
				{
					_003C_003E2__current = new WaitForSeconds(petMilestoneCelebrationWidget._startParticleDelay);
					_003C_003E1__state = 1;
					return true;
				}
				goto IL_0057;
			case 1:
				_003C_003E1__state = -1;
				goto IL_0057;
			case 2:
				{
					_003C_003E1__state = -1;
					break;
				}
				IL_0057:
				i = 0;
				for (int size = KUtility.GetSize(petMilestoneCelebrationWidget._startParticlePositions); i < size; i++)
				{
					string path = petMilestoneCelebrationWidget._startParticle.Path;
					Vector3 zero = Vector3.zero;
					Quaternion identity = Quaternion.identity;
					Transform followingParent = petMilestoneCelebrationWidget._startParticlePositions[i];
					Vector3 one = Vector3.one;
					int item = ParticleManager.EmitFollow(path, zero, identity, followingParent, useLocalPosition: true, comeForwardToCamera: false, groundDecal: false, one);
					petMilestoneCelebrationWidget._particles.Add(item);
				}
				if (petMilestoneCelebrationWidget._loopParticleDelay > 0f)
				{
					_003C_003E2__current = new WaitForSeconds(petMilestoneCelebrationWidget._loopParticleDelay);
					_003C_003E1__state = 2;
					return true;
				}
				break;
			}
			int j = 0;
			for (int size2 = KUtility.GetSize(petMilestoneCelebrationWidget._loopParticlePositions); j < size2; j++)
			{
				string path2 = petMilestoneCelebrationWidget._loopParticle.Path;
				Vector3 zero2 = Vector3.zero;
				Quaternion identity2 = Quaternion.identity;
				Transform followingParent2 = petMilestoneCelebrationWidget._loopParticlePositions[j];
				Vector3 one2 = Vector3.one;
				int item2 = ParticleManager.EmitFollow(path2, zero2, identity2, followingParent2, useLocalPosition: true, comeForwardToCamera: false, groundDecal: false, one2);
				petMilestoneCelebrationWidget._particles.Add(item2);
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
	private ParticleType _startParticle;

	[SerializeField]
	private ParticleType _loopParticle;

	[SerializeField]
	private Transform[] _startParticlePositions;

	[SerializeField]
	private Transform[] _loopParticlePositions;

	[SerializeField]
	private float _startParticleDelay;

	[SerializeField]
	private float _loopParticleDelay;

	private ICoroutineBinder _binder;

	private bool _isParticleActivated;

	private readonly List<int> _particles = new List<int>();

	private void OnDisable()
	{
		HideParticles();
	}

	private void Update()
	{
		if (!_isParticleActivated)
		{
			ShowParticles();
		}
	}

	private void ShowParticles()
	{
		_isParticleActivated = true;
		this.StartCoroutine(ref _binder, CoShowParticles());
	}

	private IEnumerator CoShowParticles()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoShowParticles_003Ed__12(0)
		{
			_003C_003E4__this = this
		};
	}

	private void HideParticles()
	{
		_isParticleActivated = false;
		for (int i = 0; i < _particles.Count; i++)
		{
			ParticleManager.Stop(_particles[i]);
		}
		_particles.Clear();
	}
}
