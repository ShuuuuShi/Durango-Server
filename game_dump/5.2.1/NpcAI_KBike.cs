using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Environment;
using Durango.Model;
using Durango.Render.Camera;
using Durango.Terrain;
using Durango.Utils;
using UnityEngine;

public class NpcAI_KBike : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CCoBeginCPR_003Ed__29 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public NpcAI_KBike _003C_003E4__this;

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
		public _003CCoBeginCPR_003Ed__29(int _003C_003E1__state)
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
			NpcAI_KBike npcAI_KBike = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				npcAI_KBike._animalBehavior.Play(npcAI_KBike._cprLoopMotion);
				PlayerController.MotionUpdater.Motion("Bike_CPR", 0f, 1f, forceTransition: true);
				_003C_003E2__current = new WaitForSeconds(npcAI_KBike._cprDuration);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				npcAI_KBike._animalBehavior.Play(npcAI_KBike._getUpMotion);
				GameSystem<PlayGuideSystem>.Instance().Command.StandUp();
				_003C_003E2__current = new WaitForSeconds(npcAI_KBike._animalBehavior.CurAnimState.length);
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				npcAI_KBike._animalBehavior.Play(npcAI_KBike._duringMotion);
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
	private sealed class _003CCoRun_003Ed__33 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public NpcAI_KBike _003C_003E4__this;

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
		public _003CCoRun_003Ed__33(int _003C_003E1__state)
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
			NpcAI_KBike npcAI_KBike = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				npcAI_KBike._animalBehavior.Play(npcAI_KBike._endMotion, loop: false);
				_003C_003E2__current = new WaitForSeconds(npcAI_KBike._animalBehavior.CurAnimState.length);
				_003C_003E1__state = 1;
				return true;
			case 1:
			{
				_003C_003E1__state = -1;
				SkinnedMeshRenderer[] componentsInChildren = npcAI_KBike.GetComponentsInChildren<SkinnedMeshRenderer>();
				for (int i = 0; i < KUtility.GetSize(componentsInChildren); i++)
				{
					componentsInChildren[i].enabled = false;
				}
				_003C_003E2__current = new WaitForSeconds(3f);
				_003C_003E1__state = 2;
				return true;
			}
			case 2:
				_003C_003E1__state = -1;
				UnityEngine.Object.Destroy(npcAI_KBike.gameObject);
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
	private sealed class _003CStart_003Ed__27 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public NpcAI_KBike _003C_003E4__this;

		private SkinnedMeshRenderer[] _003Cobjects_003E5__2;

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
		public _003CStart_003Ed__27(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Cobjects_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			NpcAI_KBike npcAI_KBike = _003C_003E4__this;
			BoneLookAtTarget component;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				npcAI_KBike._animalBehavior = npcAI_KBike.GetComponent<AnimalBehavior>();
				npcAI_KBike._animalBehavior.EntityId = "666";
				goto IL_0066;
			case 1:
				_003C_003E1__state = -1;
				goto IL_0066;
			case 2:
				_003C_003E1__state = -1;
				goto IL_009e;
			case 3:
			{
				_003C_003E1__state = -1;
				for (int i = 0; i < _003Cobjects_003E5__2.Length; i++)
				{
					_003Cobjects_003E5__2[i].enabled = true;
				}
				_003C_003E2__current = new WaitForSeconds(npcAI_KBike._cameraChaseBeginDelay);
				_003C_003E1__state = 4;
				return true;
			}
			case 4:
				{
					_003C_003E1__state = -1;
					Singleton<CameraController>.Instance().Target(npcAI_KBike._cameraTarget, 0.3f).Zoom(0.7f, 0.3f)
						.Delay(npcAI_KBike.bikeSceneZoomBeginTime)
						.Target(null, npcAI_KBike.bikeSceneZoomingTime)
						.Offset(Vector3.down * 70f, npcAI_KBike.bikeSceneZoomingTime)
						.Zoom(npcAI_KBike.bikeSceneZoomRatio, npcAI_KBike.bikeSceneZoomingTime, NgInterpolate.EaseType.EaseInQuart);
					return false;
				}
				IL_009e:
				npcAI_KBike._playerTarget = PlayerBehavior.LocalPlayer.gameObject;
				if (!(npcAI_KBike._playerTarget != null))
				{
					_003C_003E2__current = new WaitForSeconds(1f);
					_003C_003E1__state = 2;
					return true;
				}
				component = npcAI_KBike.GetComponent<BoneLookAtTarget>();
				if (null != component)
				{
					component.AutoChangeTarget = false;
					component.SetLookTarget(PlayerBehavior.LocalPlayer.gameObject, findHead: true);
				}
				npcAI_KBike._animalBehavior.SetAnimationCullingType(AnimationCullingType.AlwaysAnimate);
				npcAI_KBike._animalBehavior.SetActivateRootMotion(active: false);
				if (npcAI_KBike._isRestoringStandingKCutScene)
				{
					npcAI_KBike._animalBehavior.Play(npcAI_KBike._duringMotion);
					return false;
				}
				npcAI_KBike._animalBehavior.Play(npcAI_KBike._beginMotion, loop: false);
				PlayerBehavior.LocalPlayer.Anim.Stop();
				PlayerBehavior.LocalPlayer.PlayMotionForcely("Bike_Begin");
				SoundManager.PlayEvent(npcAI_KBike._bikeAppearAudio, SoundPosition.Chase(npcAI_KBike._animalBehavior.gameObject));
				_003Cobjects_003E5__2 = npcAI_KBike._animalBehavior.GetComponentsInChildren<SkinnedMeshRenderer>();
				for (int j = 0; j < _003Cobjects_003E5__2.Length; j++)
				{
					_003Cobjects_003E5__2[j].enabled = false;
				}
				_003C_003E2__current = null;
				_003C_003E1__state = 3;
				return true;
				IL_0066:
				if (!TerrainBase.IsPlayerInitialized)
				{
					_003C_003E2__current = null;
					_003C_003E1__state = 1;
					return true;
				}
				if (TimeGauge.CheckTime(18f, 4f))
				{
					npcAI_KBike._bikeLight.SetActive(value: true);
				}
				else
				{
					npcAI_KBike._nightLight.IsLightOn = false;
				}
				npcAI_KBike.RepositionToIntro();
				goto IL_009e;
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

	[SerializeField]
	private string _beginMotion = "K_Bike_Begin";

	[SerializeField]
	private string _cprLoopMotion = "K_Bike_CPR";

	[SerializeField]
	private string _getUpMotion = "K_Bike_Getup";

	[SerializeField]
	private string _duringMotion = "K_Bike_During";

	[SerializeField]
	private string _endMotion = "K_Bike_End";

	[SerializeField]
	private SoundEventType _bikeAppearAudio;

	[SerializeField]
	private SoundEventType _bikeLeaveAudio;

	[SerializeField]
	private GameObject _cameraTarget;

	[SerializeField]
	private float _cameraChaseBeginDelay = 1f;

	[SerializeField]
	private float _cprDuration = 5f;

	[SerializeField]
	private GameObject _bikeLight;

	[SerializeField]
	private NightLight _nightLight;

	[SerializeField]
	public Vector3 _introPosFromPlayer = Vector3.zero;

	[SerializeField]
	public float _introYaw;

	[SerializeField]
	private float bikeSceneZoomBeginTime = 3f;

	[SerializeField]
	private float bikeSceneZoomRatio = 10f;

	[SerializeField]
	private float bikeSceneZoomingTime = 15f;

	private GameObject _playerTarget;

	private AnimalBehavior _animalBehavior;

	private GameObject _head;

	private GameObject _pelvis;

	private bool _isRestoringStandingKCutScene;

	public GameObject Head
	{
		get
		{
			if (_head == null)
			{
				_head = KUtility.FindObjectByName(base.gameObject, "Bip001_Head");
			}
			return _head;
		}
	}

	public GameObject Pelvis
	{
		get
		{
			if (_pelvis == null)
			{
				_pelvis = KUtility.FindObjectByName(base.gameObject, "Bip001_Pelvis");
			}
			return _pelvis;
		}
	}

	[ExposedInEditor("Intro 위치 새로 잡기")]
	public void RepositionToIntro()
	{
		Vector3 position = KUtility.FindObjectByName(PlayerBehavior.LocalPlayer.gameObject, "Bip001_Spine2").transform.position + _introPosFromPlayer;
		position.y = 0f;
		base.transform.position = position;
		base.transform.rotation = Quaternion.Euler(0f, _introYaw, 0f);
	}

	private IEnumerator Start()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CStart_003Ed__27(0)
		{
			_003C_003E4__this = this
		};
	}

	public void BeginCPR()
	{
		StartCoroutine(CoBeginCPR());
	}

	private IEnumerator CoBeginCPR()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoBeginCPR_003Ed__29(0)
		{
			_003C_003E4__this = this
		};
	}

	public void RestoreStandingKCutScene()
	{
		_isRestoringStandingKCutScene = true;
	}

	private void Update()
	{
		GameObject pelvis = Pelvis;
		if (pelvis != null)
		{
			Singleton<OccluderVisibleManager>.Instance().PushRayCastPosition(pelvis.transform.position);
		}
	}

	public void EventRun()
	{
		StartCoroutine(CoRun());
	}

	private IEnumerator CoRun()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoRun_003Ed__33(0)
		{
			_003C_003E4__this = this
		};
	}

	private void OnPlayLeaveSound()
	{
		SoundManager.PlayEvent(_bikeLeaveAudio, SoundPosition.Chase(_animalBehavior.gameObject));
	}
}
