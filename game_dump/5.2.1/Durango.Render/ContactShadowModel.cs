using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Utils;
using UnityEngine;

namespace Durango.Render;

public class ContactShadowModel : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CStart_003Ed__26 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public ContactShadowModel _003C_003E4__this;

		private Transform _003CleftFoot_003E5__2;

		private Transform _003CrightFoot_003E5__3;

		private CharacterBehavior _003Ccharacter_003E5__4;

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
		public _003CStart_003Ed__26(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003CleftFoot_003E5__2 = null;
			_003CrightFoot_003E5__3 = null;
			_003Ccharacter_003E5__4 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			ContactShadowModel contactShadowModel = _003C_003E4__this;
			float y;
			Vector3 vector;
			Vector3 vector2;
			Vector3 vector3;
			Vector3 vector4;
			Vector3 position;
			float y2;
			float y3;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (PlayerBehavior.LocalPlayer.gameObject == contactShadowModel.gameObject)
				{
					goto IL_006a;
				}
				goto IL_0098;
			case 1:
				_003C_003E1__state = -1;
				goto IL_006a;
			case 2:
				_003C_003E1__state = -1;
				goto IL_0110;
			case 3:
				_003C_003E1__state = -1;
				goto IL_038a;
			case 4:
				{
					_003C_003E1__state = -1;
					goto IL_038a;
				}
				IL_006a:
				if (PlayerBehavior.LocalPlayer == null || !PlayerBehavior.LocalPlayer.gameObject.activeSelf)
				{
					_003C_003E2__current = new WaitForSeconds(0.1f);
					_003C_003E1__state = 1;
					return true;
				}
				contactShadowModel.Target = PlayerBehavior.LocalPlayer.gameObject;
				goto IL_0098;
				IL_038a:
				if (!(contactShadowModel.Target != null) || !contactShadowModel.Target.activeInHierarchy || !(_003Ccharacter_003E5__4 != null))
				{
					break;
				}
				if (!_003Ccharacter_003E5__4.WillBeRendered)
				{
					_003C_003E2__current = null;
					_003C_003E1__state = 2;
					return true;
				}
				goto IL_0110;
				IL_0110:
				if (_003CleftFoot_003E5__2 == null || _003CrightFoot_003E5__3 == null || contactShadowModel.Target == null || (contactShadowModel.DestroyIfInvisible && contactShadowModel.Target.transform.position.y > contactShadowModel.ShadowRemoveHeight))
				{
					if (contactShadowModel.OnRemove != null)
					{
						contactShadowModel.OnRemove(contactShadowModel);
					}
					return false;
				}
				y = Mathf.Max(0f, contactShadowModel.Target.transform.position.y);
				vector = CalcContactPosition(_003CleftFoot_003E5__2.position);
				vector.y = y;
				vector2 = Maths.Make2D(_003CleftFoot_003E5__2.position);
				vector3 = CalcContactPosition(_003CrightFoot_003E5__3.position);
				vector3.y = y;
				vector4 = Maths.Make2D(_003CrightFoot_003E5__3.position);
				position = (vector2 + vector4) * 0.5f;
				position.y = y;
				contactShadowModel.gameObject.transform.position = position;
				contactShadowModel._leftFootShadow.transform.position = vector + contactShadowModel.FootShadowOffset;
				y2 = contactShadowModel._leftFootShadow.transform.localPosition.x * contactShadowModel.FootShadowRotRatioLeft + contactShadowModel.FootShadowRotBiasLeft;
				contactShadowModel._leftFootShadow.transform.localRotation = Quaternion.Euler(0f, y2, 0f);
				contactShadowModel._rightFootShadow.transform.position = vector3 + contactShadowModel.FootShadowOffset;
				y3 = contactShadowModel._rightFootShadow.transform.localPosition.x * contactShadowModel.FootShadowRotRatioRight + contactShadowModel.FootShadowRotBiasRight;
				contactShadowModel._rightFootShadow.transform.localRotation = Quaternion.Euler(0f, y3, 0f);
				contactShadowModel._centerShadow.transform.position = (vector2 + vector4) * 0.5f + contactShadowModel.CenterShadowOffset;
				contactShadowModel._centerShadow.transform.localRotation = Quaternion.Euler(contactShadowModel.CenterShadowRot);
				if (contactShadowModel.IsRapidUpdateMode || (vector - _003CleftFoot_003E5__2.position).sqrMagnitude > 5f)
				{
					_003C_003E2__current = _wairForSecondsRapid;
					_003C_003E1__state = 3;
					return true;
				}
				_003C_003E2__current = _wairForSeconds;
				_003C_003E1__state = 4;
				return true;
				IL_0098:
				if (contactShadowModel.Target == null)
				{
					return false;
				}
				_003CleftFoot_003E5__2 = KUtility.FindTransformByName(contactShadowModel.Target, "Bip001_L_Foot");
				_003CrightFoot_003E5__3 = KUtility.FindTransformByName(contactShadowModel.Target, "Bip001_R_Foot");
				_003Ccharacter_003E5__4 = contactShadowModel.Target.GetComponent<CharacterBehavior>();
				goto IL_038a;
			}
			if (contactShadowModel.OnRemove != null)
			{
				contactShadowModel.OnRemove(contactShadowModel);
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

	public Action<ContactShadowModel> OnRemove;

	[SerializeField]
	public Vector3 FootShadowOffset;

	[SerializeField]
	public float FootShadowRotBiasLeft;

	[SerializeField]
	public float FootShadowRotRatioLeft;

	[SerializeField]
	public float FootShadowRotBiasRight;

	[SerializeField]
	public float FootShadowRotRatioRight;

	[SerializeField]
	public Vector3 CenterShadowOffset;

	[SerializeField]
	public Vector3 CenterShadowRot;

	[SerializeField]
	public float ShadowRemoveHeight;

	[SerializeField]
	private GameObject _leftFootShadow;

	[SerializeField]
	private GameObject _rightFootShadow;

	[SerializeField]
	private GameObject _centerShadow;

	private static WaitForSeconds _wairForSeconds = new WaitForSeconds(0.3f);

	private static WaitForSeconds _wairForSecondsRapid = new WaitForSeconds(0.016f);

	public GameObject Target { get; set; }

	public bool IsRapidUpdateMode { get; set; }

	public bool DestroyIfInvisible { get; set; }

	private IEnumerator Start()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CStart_003Ed__26(0)
		{
			_003C_003E4__this = this
		};
	}

	private static Vector3 CalcContactPosition(Vector3 pos)
	{
		return pos;
	}
}
