using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Environment;
using Durango.Render.Sprite;
using Durango.Utils;
using UnityEngine;

public class ShrubComponent : NaturalComponent
{
	[CompilerGenerated]
	private sealed class _003CCoSway_003Ed__10 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public ShrubComponent _003C_003E4__this;

		public float windTime;

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
		public _003CCoSway_003Ed__10(int _003C_003E1__state)
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
			ShrubComponent shrubComponent = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				shrubComponent._isWindy = true;
				shrubComponent._curWindTime = (0f - (shrubComponent.GameObject.transform.position.x - PlayerBehavior.LocalPlayer.CurrentPosition.x)) * 0.001f - 1f;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (shrubComponent._isWindy)
			{
				if (shrubComponent._curWindTime >= windTime)
				{
					shrubComponent._isWindy = false;
				}
				shrubComponent.SwayVertices();
				shrubComponent.SetWindFactor(windTime);
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

	private Vector3[] _shakenVertices;

	private bool _isWindy;

	private float _curWindTime;

	private float _curOffset;

	private bool _isShaking;

	public ShrubComponent(NaturalSpriteObject natural)
		: base(natural)
	{
	}

	public void RefreshShakenVertices()
	{
		_shakenVertices = null;
	}

	public void Shake(bool shake)
	{
		if (shake)
		{
			PrepareVertices();
		}
		if (KUtility.GetSize(_shakenVertices) == 0)
		{
			return;
		}
		if (!shake && _isShaking)
		{
			_isShaking = false;
			Vector3[] baseVertices = base.Sprite.GetBaseVertices();
			if (baseVertices != null)
			{
				base.Sprite.SetMeshVertices(baseVertices);
				int num = baseVertices.Length;
				for (int i = 0; i < num; i++)
				{
					ref Vector3 reference = ref _shakenVertices[i];
					reference = baseVertices[i];
				}
			}
		}
		else if (shake)
		{
			_isShaking = true;
			float f = Time.time * Singleton<SpriteManager>.Instance().BushWhackFrequency;
			float num2 = Singleton<SpriteManager>.Instance().BushWhackAmplitude * Mathf.Sin(f);
			for (int j = 0; j < _shakenVertices.Length; j++)
			{
				_shakenVertices[j].x = _shakenVertices[j].x + num2 * Mathf.Max(0f, _shakenVertices[j].y);
			}
			base.Sprite.SetMeshVertices(_shakenVertices);
		}
	}

	private void PrepareVertices()
	{
		if (KUtility.GetSize(_shakenVertices) == 0)
		{
			_shakenVertices = base.Sprite.GetMeshVertices();
		}
	}

	public void Sway(float windTime)
	{
		if (!_isWindy && !_isShaking && base.GameObject.activeSelf)
		{
			PrepareVertices();
			base.Natural.StartCoroutine(CoSway(windTime));
		}
	}

	private IEnumerator CoSway(float windTime)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoSway_003Ed__10(0)
		{
			_003C_003E4__this = this,
			windTime = windTime
		};
	}

	private void SwayVertices()
	{
		if (_curWindTime < 0f || _isShaking)
		{
			return;
		}
		Vector3[] baseVertices = base.Sprite.GetBaseVertices();
		if (baseVertices != null && KUtility.GetSize(_shakenVertices) != 0)
		{
			for (int i = 0; i < _shakenVertices.Length; i++)
			{
				float windValue = Singleton<WindManager>.Instance().GetWindValue(_curOffset);
				_shakenVertices[i].x = baseVertices[i].x + windValue * Mathf.Max(0f, _shakenVertices[i].y);
			}
			base.Sprite.SetMeshVertices(_shakenVertices);
		}
	}

	private void SetWindFactor(float windTime)
	{
		_curWindTime += Time.deltaTime;
		_curOffset = _curWindTime / windTime;
	}
}
