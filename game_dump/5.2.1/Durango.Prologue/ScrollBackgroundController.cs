using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Utils;
using UnityEngine;

namespace Durango.Prologue;

public class ScrollBackgroundController : Singleton<ScrollBackgroundController>
{
	[CompilerGenerated]
	private sealed class _003CStart_003Ed__14 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public ScrollBackgroundController _003C_003E4__this;

		private int _003Ccount_003E5__2;

		private float _003Cbound_003E5__3;

		private float _003CprevTime_003E5__4;

		private int _003CgodRayCount_003E5__5;

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
		public _003CStart_003Ed__14(int _003C_003E1__state)
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
			ScrollBackgroundController scrollBackgroundController = _003C_003E4__this;
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
				scrollBackgroundController._curBGColor = scrollBackgroundController._dayBGColor;
				scrollBackgroundController._curTreeColor = scrollBackgroundController._daytTreeColor;
				scrollBackgroundController.SetTreeVisible(bNormal: true, bThunder: false);
				_003Ccount_003E5__2 = scrollBackgroundController.transform.GetChild(0).childCount;
				for (int i = 0; i < _003Ccount_003E5__2; i++)
				{
					scrollBackgroundController._objects.Add(scrollBackgroundController.transform.GetChild(0).GetChild(i).gameObject);
				}
				scrollBackgroundController._objects.Sort((GameObject v1, GameObject v2) => (int)(v2.transform.localPosition.z - v1.transform.localPosition.z));
				_003Cbound_003E5__3 = scrollBackgroundController._objects[_003Ccount_003E5__2 - 1].transform.localPosition.z - scrollBackgroundController._blockSize;
				_003CprevTime_003E5__4 = Time.time;
				_003CgodRayCount_003E5__5 = scrollBackgroundController._godRays.Count;
			}
			float num2 = Time.time - _003CprevTime_003E5__4;
			_003CprevTime_003E5__4 = Time.time;
			for (int j = 0; j < _003Ccount_003E5__2; j++)
			{
				Vector3 localPosition = scrollBackgroundController._objects[j].transform.localPosition;
				localPosition.z = (localPosition.z - num2 * scrollBackgroundController._speed) % _003Cbound_003E5__3;
				scrollBackgroundController._objects[j].GetComponent<Renderer>().material.color = scrollBackgroundController._curBGColor;
				scrollBackgroundController._objects[j].transform.localPosition = localPosition;
			}
			for (int k = 0; k < _003CgodRayCount_003E5__5; k++)
			{
				if ((bool)scrollBackgroundController._godRays[k])
				{
					scrollBackgroundController._godRays[k].GetComponent<Renderer>().material.color = scrollBackgroundController._curGodRayColor;
				}
			}
			int count = scrollBackgroundController.tree_groups_normal.Count;
			for (int l = 0; l < count; l++)
			{
				Renderer[] componentsInChildren = scrollBackgroundController.tree_groups_normal[l].GetComponentsInChildren<Renderer>();
				int num3 = componentsInChildren.Length;
				for (int m = 0; m < num3; m++)
				{
					componentsInChildren[m].material.color = scrollBackgroundController._curTreeColor;
				}
			}
			_003C_003E2__current = scrollBackgroundController._waitForSeconds;
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

	[CompilerGenerated]
	private sealed class _003CcoBG_TunnelEffect_003Ed__17 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public ScrollBackgroundController _003C_003E4__this;

		public float _BG_TunnelDelay;

		public float _BG_TunnelFadeTime;

		public float _BG_TunnelDuration;

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
		public _003CcoBG_TunnelEffect_003Ed__17(int _003C_003E1__state)
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
			ScrollBackgroundController CS_0024_003C_003E8__locals0 = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				CS_0024_003C_003E8__locals0._curBGColor = CS_0024_003C_003E8__locals0._dayBGColor;
				CS_0024_003C_003E8__locals0._curTreeColor = CS_0024_003C_003E8__locals0._daytTreeColor;
				CS_0024_003C_003E8__locals0._curGodRayColor = Color.white;
				_003C_003E2__current = new WaitForSeconds(_BG_TunnelDelay);
				_003C_003E1__state = 1;
				return true;
			case 1:
			{
				_003C_003E1__state = -1;
				TweenTick tweenTick2 = TweenTick.Begin(CS_0024_003C_003E8__locals0.gameObject, _BG_TunnelFadeTime, delegate(float factor, bool isFinished)
				{
					CS_0024_003C_003E8__locals0._curBGColor = Color.Lerp(CS_0024_003C_003E8__locals0._dayBGColor, Color.clear, factor);
					CS_0024_003C_003E8__locals0._curGodRayColor = Color.Lerp(CS_0024_003C_003E8__locals0._daytTreeColor, Color.clear, factor);
				});
				tweenTick2.method = UITweener.Method.EaseOut;
				tweenTick2.PlayForward();
				CS_0024_003C_003E8__locals0.SetTreeVisible(bNormal: false, bThunder: false);
				_003C_003E2__current = new WaitForSeconds(_BG_TunnelDuration);
				_003C_003E1__state = 2;
				return true;
			}
			case 2:
			{
				_003C_003E1__state = -1;
				TweenTick tweenTick = TweenTick.Begin(CS_0024_003C_003E8__locals0.gameObject, _BG_TunnelFadeTime, delegate(float factor, bool isFinished)
				{
					CS_0024_003C_003E8__locals0._curBGColor = Color.Lerp(Color.clear, CS_0024_003C_003E8__locals0._nightBGColor, factor);
					CS_0024_003C_003E8__locals0._curGodRayColor = Color.Lerp(Color.clear, CS_0024_003C_003E8__locals0._nightTreeColor, factor);
				});
				tweenTick.method = UITweener.Method.EaseOut;
				tweenTick.PlayForward();
				CS_0024_003C_003E8__locals0.SetTreeVisible(bNormal: true, bThunder: false);
				return false;
			}
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

	private List<GameObject> _objects = new List<GameObject>();

	[SerializeField]
	private float _speed = 300000f;

	[SerializeField]
	private float _blockSize = 2000f;

	public Color _curBGColor = Color.white;

	public Color _curTreeColor = Color.white;

	public Color _curGodRayColor = Color.white;

	[SerializeField]
	private Color _dayBGColor = Color.white;

	[SerializeField]
	private Color _daytTreeColor = new Color(0.39f, 0.39f, 0.66f, 1f);

	[SerializeField]
	private Color _nightBGColor = new Color(0.12f, 0.12f, 0.2f, 1f);

	[SerializeField]
	private Color _nightTreeColor = new Color(0.12f, 0.12f, 0.2f, 1f);

	[SerializeField]
	private List<GameObject> tree_groups_normal = new List<GameObject>();

	[SerializeField]
	private List<GameObject> tree_groups_thunder = new List<GameObject>();

	[SerializeField]
	private List<GameObject> _godRays = new List<GameObject>();

	private WaitForSeconds _waitForSeconds = new WaitForSeconds(0.03f);

	private IEnumerator Start()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CStart_003Ed__14(0)
		{
			_003C_003E4__this = this
		};
	}

	public void SetTreeVisible(bool bNormal, bool bThunder)
	{
		int count = tree_groups_normal.Count;
		for (int i = 0; i < count; i++)
		{
			if ((bool)tree_groups_normal[i])
			{
				tree_groups_normal[i].SetActive(bNormal);
			}
		}
		count = tree_groups_thunder.Count;
		for (int j = 0; j < count; j++)
		{
			if ((bool)tree_groups_thunder[j])
			{
				tree_groups_thunder[j].SetActive(bThunder);
			}
		}
	}

	public void PlayTunnelEffect(float _BG_TunnelDelay, float _BG_TunnelFadeTime, float _BG_TunnelDuration)
	{
		StartCoroutine(coBG_TunnelEffect(_BG_TunnelDelay, _BG_TunnelFadeTime, _BG_TunnelDuration));
	}

	private IEnumerator coBG_TunnelEffect(float _BG_TunnelDelay, float _BG_TunnelFadeTime, float _BG_TunnelDuration)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CcoBG_TunnelEffect_003Ed__17(0)
		{
			_003C_003E4__this = this,
			_BG_TunnelDelay = _BG_TunnelDelay,
			_BG_TunnelFadeTime = _BG_TunnelFadeTime,
			_BG_TunnelDuration = _BG_TunnelDuration
		};
	}
}
