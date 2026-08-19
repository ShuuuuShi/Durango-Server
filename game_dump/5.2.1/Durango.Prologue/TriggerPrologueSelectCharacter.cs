using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Render.Camera;
using Durango.UI.Prologue;
using Durango.Utils;
using Shared.Player;
using UnityEngine;

namespace Durango.Prologue;

public class TriggerPrologueSelectCharacter : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CCoWalkToHall_003Ed__21 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public TriggerPrologueSelectCharacter _003C_003E4__this;

		private float _003CbeginTime_003E5__2;

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
		public _003CCoWalkToHall_003Ed__21(int _003C_003E1__state)
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
			TriggerPrologueSelectCharacter triggerPrologueSelectCharacter = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				Singleton<PrologueManager>.Instance().BeginPlayer(triggerPrologueSelectCharacter._actor, triggerPrologueSelectCharacter._walkDestPosition);
				PlayerBehavior.LocalPlayer.SetVisible(visible: false);
				_003CbeginTime_003E5__2 = Time.time;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (!PlayerBehavior.LocalPlayer.IsLoaded && !(Time.time - _003CbeginTime_003E5__2 > 5f))
			{
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			PlayerBehavior.LocalPlayer.SetVisible(visible: true);
			Singleton<PrologueManager>.Instance().MakeLitSphereOverride(PlayerBehavior.LocalPlayer.MeshObjectTransform);
			UnityEngine.Object.Destroy(triggerPrologueSelectCharacter.gameObject);
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
	private Job _job;

	[SerializeField]
	private string _standMotion;

	[SerializeField]
	private string _walkMotion;

	private Vector3 _walkDestPosition;

	private PrologueCharacterSelectGroupBase _prologueCharacterSelectUI;

	private CostumeActorBehavior _actor;

	public Job Job => _job;

	public string StandMotion
	{
		get
		{
			return _standMotion;
		}
		set
		{
			_standMotion = value;
		}
	}

	public string WalkMotion
	{
		get
		{
			return _walkMotion;
		}
		set
		{
			_walkMotion = value;
		}
	}

	private PrologueCharacterSelectGroupBase PrologCharacterSelectUI
	{
		get
		{
			if (_prologueCharacterSelectUI == null)
			{
				_prologueCharacterSelectUI = UIManager.FindScript<PrologueCharacterSelectGroupBase>();
			}
			return _prologueCharacterSelectUI;
		}
	}

	private void Start()
	{
		_actor = base.gameObject.GetComponent<CostumeActorBehavior>();
		_walkDestPosition = CalcDestPos();
	}

	public void Select()
	{
		MaskOthers(mask: true);
		PrologCharacterSelectUI.Open();
		PrologCharacterSelectUI.SetSelectCharactInfo(_job, _actor.IsMale);
		Singleton<CameraController>.Instance().Target(base.gameObject, 0.3f);
		Singleton<CameraController>.Instance().Zoom(2f, 0.4f, NgInterpolate.EaseType.EaseInQuad).Offset(new Vector3(75f, 100f, -75f), 0.4f)
			.Target(base.gameObject, 0.4f);
		GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.OnTouchCharacter);
	}

	public void Unselect()
	{
		MaskOthers(mask: false);
	}

	private void MaskOthers(bool mask)
	{
		Transform transform = base.transform;
		Transform parent = transform.parent;
		int i = 0;
		for (int childCount = parent.childCount; i < childCount; i++)
		{
			Transform child = parent.GetChild(i);
			Color color = ((!mask || !(child != transform)) ? Color.white : new Color(0.2f, 0.2f, 0.2f));
			Renderer[] componentsInChildren = child.GetComponentsInChildren<Renderer>();
			if (componentsInChildren == null)
			{
				continue;
			}
			Renderer[] array = componentsInChildren;
			for (int j = 0; j < array.Length; j++)
			{
				Material[] materials = array[j].materials;
				if (materials != null)
				{
					Material[] array2 = materials;
					for (int k = 0; k < array2.Length; k++)
					{
						array2[k].color = color;
					}
				}
			}
		}
	}

	public void ChooseCharacter()
	{
		GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.OnChooseCharacter);
		StopAllCoroutines();
		StartCoroutine(CoWalkToHall());
	}

	private IEnumerator CoWalkToHall()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoWalkToHall_003Ed__21(0)
		{
			_003C_003E4__this = this
		};
	}

	private Vector3 CalcDestPos()
	{
		Vector3 position = base.gameObject.transform.position;
		if (Mathf.Abs(position.z) < 70f)
		{
			return position;
		}
		position.x -= 50f;
		position.y = 0f;
		position.z = 70f * Mathf.Sign(position.z);
		return position;
	}
}
