using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Durango.Environment;

public class CloudUpdater : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CCoFlowFading_003Ed__13 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public CloudUpdater _003C_003E4__this;

		public bool appear;

		private float _003Calpha_003E5__2;

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
		public _003CCoFlowFading_003Ed__13(int _003C_003E1__state)
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
			CloudUpdater cloudUpdater = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Calpha_003E5__2 = cloudUpdater._material.GetFloat(cloudUpdater._alphaId);
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if ((appear && _003Calpha_003E5__2 > 0f) || (!appear && _003Calpha_003E5__2 < 1f))
			{
				cloudUpdater.MoveCloud();
				float num2 = cloudUpdater._fadeSpeed * Time.deltaTime;
				_003Calpha_003E5__2 = ((!appear) ? (_003Calpha_003E5__2 + num2) : (_003Calpha_003E5__2 - num2));
				cloudUpdater._material.SetFloat(cloudUpdater._alphaId, Mathf.Clamp01(_003Calpha_003E5__2));
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
	private sealed class _003CCoProcessCloud_003Ed__12 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public CloudUpdater _003C_003E4__this;

		private float _003CelapsedTime_003E5__2;

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
		public _003CCoProcessCloud_003Ed__12(int _003C_003E1__state)
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
			CloudUpdater cloudUpdater = _003C_003E4__this;
			Vector3 randomPos;
			float num2;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				goto IL_0031;
			case 1:
				_003C_003E1__state = -1;
				_003CelapsedTime_003E5__2 = 0f;
				goto IL_011f;
			case 2:
				_003C_003E1__state = -1;
				goto IL_011f;
			case 3:
				_003C_003E1__state = -1;
				_003C_003E2__current = cloudUpdater._waitForSeconds;
				_003C_003E1__state = 4;
				return true;
			case 4:
				{
					_003C_003E1__state = -1;
					goto IL_0031;
				}
				IL_011f:
				if (_003CelapsedTime_003E5__2 < cloudUpdater._runTime)
				{
					cloudUpdater.MoveCloud();
					_003CelapsedTime_003E5__2 += Time.deltaTime;
					_003C_003E2__current = null;
					_003C_003E1__state = 2;
					return true;
				}
				_003C_003E2__current = cloudUpdater.StartCoroutine(cloudUpdater.CoFlowFading(appear: false));
				_003C_003E1__state = 3;
				return true;
				IL_0031:
				randomPos = GetRandomPos(PlayerBehavior.LocalPlayer.CurrentPosition, 5000f);
				cloudUpdater._cloud.transform.rotation = Quaternion.Euler(new Vector3(90f, UnityEngine.Random.Range(0, 360), 0f));
				cloudUpdater._cloud.transform.position = randomPos;
				num2 = cloudUpdater._initialScale * (1f + UnityEngine.Random.Range(-0.2f, 0.2f));
				cloudUpdater._cloud.transform.localScale = new Vector3(num2, num2, 0f);
				_003C_003E2__current = cloudUpdater.StartCoroutine(cloudUpdater.CoFlowFading(appear: true));
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

	private const float Radius = 5000f;

	[SerializeField]
	private GameObject _cloud;

	[SerializeField]
	private float _appearPeriod;

	[SerializeField]
	private float _runTime;

	[SerializeField]
	private float _speed;

	[SerializeField]
	private float _fadeSpeed;

	private Material _material;

	private int _alphaId;

	private Vector3 _flowDir;

	private float _initialScale;

	private WaitForSeconds _waitForSeconds;

	private void Start()
	{
		_cloud.SetActive(value: true);
		_material = _cloud.GetComponent<MeshRenderer>().material;
		_alphaId = Shader.PropertyToID("_AlphaRatio");
		_material.SetFloat(_alphaId, 1f);
		float value = UnityEngine.Random.value;
		float z = Mathf.Sin((float)Math.PI * 2f * value);
		float x = Mathf.Cos((float)Math.PI * 2f * value);
		_flowDir = new Vector3(x, 0f, z);
		_initialScale = _cloud.transform.localScale.x;
		_waitForSeconds = new WaitForSeconds(_appearPeriod);
		StartCoroutine(CoProcessCloud());
	}

	private IEnumerator CoProcessCloud()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoProcessCloud_003Ed__12(0)
		{
			_003C_003E4__this = this
		};
	}

	private IEnumerator CoFlowFading(bool appear)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoFlowFading_003Ed__13(0)
		{
			_003C_003E4__this = this,
			appear = appear
		};
	}

	private void MoveCloud()
	{
		_cloud.transform.position = _cloud.transform.position + _flowDir * Time.deltaTime * _speed;
	}

	private static Vector3 GetRandomPos(Vector3 center, float radius)
	{
		float num = UnityEngine.Random.Range(-1f, 1f);
		float num2 = UnityEngine.Random.Range(-1f, 1f);
		return center + new Vector3(num * radius, 0f, num2 * radius);
	}
}
