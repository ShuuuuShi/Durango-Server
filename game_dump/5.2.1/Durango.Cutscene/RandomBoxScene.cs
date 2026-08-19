using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Model;
using Durango.Render.Camera;
using Durango.Render.Screen;
using Durango.UI;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.Cutscene;

public class RandomBoxScene : SceneBase
{
	public enum BoxType
	{
		X1,
		X10
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass44_0
	{
		public RandomBoxScene _003C_003E4__this;

		public AnimationState targetAnimationState;

		public float exitTime;

		internal bool _003CCoPlayAnimation_003Eb__0()
		{
			if (!_003C_003E4__this._stopFlag)
			{
				return targetAnimationState.time >= exitTime;
			}
			return true;
		}
	}

	[CompilerGenerated]
	private sealed class _003CCoBlur_003Ed__39 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public RandomBoxScene _003C_003E4__this;

		public float delay;

		public float transitionDuration;

		public float size;

		private CutScenePostProcess _003Cblur_003E5__2;

		private float _003Csource_003E5__3;

		private float _003Ctime_003E5__4;

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
		public _003CCoBlur_003Ed__39(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Cblur_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			RandomBoxScene randomBoxScene = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Cblur_003E5__2 = randomBoxScene._cameraWiggle.GetComponent<CutScenePostProcess>();
				_003C_003E2__current = new WaitForSeconds(delay);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003Csource_003E5__3 = _003Cblur_003E5__2.BlurSize;
				_003Ctime_003E5__4 = 0f;
				break;
			case 2:
				_003C_003E1__state = -1;
				break;
			}
			if (_003Ctime_003E5__4 <= transitionDuration)
			{
				float t = _003Ctime_003E5__4 / transitionDuration;
				_003Cblur_003E5__2.BlurSize = Mathf.Lerp(_003Csource_003E5__3, size, t);
				_003Ctime_003E5__4 += Time.deltaTime;
				_003C_003E2__current = null;
				_003C_003E1__state = 2;
				return true;
			}
			_003Cblur_003E5__2.BlurSize = size;
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
	private sealed class _003CCoPlay_003Ed__37 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Action callback;

		public RandomBoxScene _003C_003E4__this;

		private RandomBoxCutsceneUI _003CcutsceneUI_003E5__2;

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
		public _003CCoPlay_003Ed__37(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003CcutsceneUI_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			RandomBoxScene randomBoxScene = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CcutsceneUI_003E5__2 = UIManager.FindScript<CutsceneGroup>().CurrentCutsceneUI as RandomBoxCutsceneUI;
				if (_003CcutsceneUI_003E5__2 == null)
				{
					callback();
					return false;
				}
				randomBoxScene.Blur(0.1f, randomBoxScene._blurInFrame, randomBoxScene._blurInDuration);
				randomBoxScene.Blur(0f, randomBoxScene._blurOutFrame, randomBoxScene._blurOutDuration);
				randomBoxScene._instanceIdList.Add(SoundManager.PlayEvent("ui_random_box_start", SoundPosition.Empty, SoundSwitch.Empty));
				randomBoxScene.StartCoroutine(randomBoxScene.CoPlayAnimation(randomBoxScene._cameraAnimatingProp, "begin"));
				_003C_003E2__current = randomBoxScene.StartCoroutine(randomBoxScene.CoPlayAnimation(randomBoxScene._boxAnimatingProp, "begin"));
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				randomBoxScene.CalculateTapeLength();
				_003CcutsceneUI_003E5__2.StartGuide();
				randomBoxScene._cameraWiggle.Play(value: true);
				randomBoxScene._instanceIdList.Add(SoundManager.PlayEvent("ui_random_box_open_parameter", SoundPosition.Empty, SoundSwitch.Empty));
				randomBoxScene.StartCoroutine(randomBoxScene.CoPlayAnimation(randomBoxScene._cameraAnimatingProp, "looping"));
				_003C_003E2__current = randomBoxScene.StartCoroutine(randomBoxScene.CoPlayAnimation(randomBoxScene._boxAnimatingProp, "looping", loop: false, 0f, 0f));
				_003C_003E1__state = 2;
				return true;
			case 2:
			{
				_003C_003E1__state = -1;
				_003CcutsceneUI_003E5__2.StopGuide();
				randomBoxScene._cameraWiggle.Play(value: false);
				randomBoxScene._instanceIdList.Add(SoundManager.PlayEvent("ui_random_box_open_end", SoundPosition.Empty, SoundSwitch.Empty));
				UnboxingSoundSpeed = 0f;
				randomBoxScene.StartCoroutine(randomBoxScene.CoPlayAnimation(randomBoxScene._cameraAnimatingProp, "end"));
				RandomBoxScene randomBoxScene2 = randomBoxScene;
				RandomBoxScene randomBoxScene3 = randomBoxScene;
				AnimatingModel boxAnimatingProp = randomBoxScene._boxAnimatingProp;
				string animationName = "end";
				float endAnimaitionExitPoint = randomBoxScene._endAnimaitionExitPoint;
				_003C_003E2__current = randomBoxScene2.StartCoroutine(randomBoxScene3.CoPlayAnimation(boxAnimatingProp, animationName, loop: false, 0f, 1f, endAnimaitionExitPoint));
				_003C_003E1__state = 3;
				return true;
			}
			case 3:
				_003C_003E1__state = -1;
				callback();
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
	private sealed class _003CCoPlayAnimation_003Ed__44 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public RandomBoxScene _003C_003E4__this;

		public AnimatingModel target;

		public string animationName;

		public bool loop;

		public float beginTime;

		public float playbackRate;

		public float exitPoint;

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
		public _003CCoPlayAnimation_003Ed__44(int _003C_003E1__state)
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
			RandomBoxScene randomBoxScene = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				_003C_003Ec__DisplayClass44_0 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass44_0
				{
					_003C_003E4__this = _003C_003E4__this
				};
				if (!(target == null))
				{
					randomBoxScene._stopFlag = false;
					float num2 = target.Play(animationName, loop, beginTime, playbackRate);
					CS_0024_003C_003E8__locals0.targetAnimationState = target.GetCurAnimState();
					if (!loop)
					{
						CS_0024_003C_003E8__locals0.targetAnimationState.wrapMode = WrapMode.ClampForever;
					}
					CS_0024_003C_003E8__locals0.exitTime = num2 * Mathf.Clamp(exitPoint, 0f, 1f);
					_003C_003E2__current = new WaitUntil(() => CS_0024_003C_003E8__locals0._003C_003E4__this._stopFlag || CS_0024_003C_003E8__locals0.targetAnimationState.time >= CS_0024_003C_003E8__locals0.exitTime);
					_003C_003E1__state = 1;
					return true;
				}
				break;
			}
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

	private const string BeginningAnimatioin = "begin";

	private const string UnboxingAnimation = "looping";

	private const string EndAnimation = "end";

	private const string CutsceneBeginning = "ui_random_box_start";

	private const string UnboxingSound = "ui_random_box_open_parameter";

	private const string UnboxingBeginningSound = "ui_random_box_open_begin";

	private const string UnboxingEndSound = "ui_random_box_open_end";

	private const int SoundScale = 100;

	[EnumList(typeof(BoxType), false, 0, -1)]
	[SerializeField]
	private GameObjectType[] _boxPath;

	[SerializeField]
	private float _blurInFrame;

	[SerializeField]
	private float _blurInDuration;

	[SerializeField]
	private float _blurOutFrame;

	[SerializeField]
	private float _blurOutDuration;

	[Range(0f, 1f)]
	[SerializeField]
	private float _endAnimaitionExitPoint;

	[SerializeField]
	private float _sensitivity;

	[SerializeField]
	private float _slowdownValue;

	[SerializeField]
	private float _unboxingEndPoint;

	[SerializeField]
	private float _unboxingSoundSensitivity;

	[SerializeField]
	private GameObject _stagePrefab;

	[SerializeField]
	private GameObject _cameraPrefab;

	[SerializeField]
	private GameObject _effectPrefab;

	private AnimatingModel _cameraAnimatingProp;

	private AnimatingModel _boxAnimatingProp;

	private Camera _cutsceneCamera;

	private CameraWiggle _cameraWiggle;

	private bool _stopFlag;

	private float _tapeLength;

	private Transform _tapeBeginning;

	private Transform _tapeEnd;

	private readonly List<uint> _instanceIdList = new List<uint>();

	private static float UnboxingSoundSpeed
	{
		set
		{
			float value2 = Mathf.Clamp(value, -100f, 100f);
			SoundManager.SetRTPC(new SoundParameters("box_open_speed", value2));
		}
	}

	private void OnDisable()
	{
		foreach (uint instanceId in _instanceIdList)
		{
			SoundManager.StopEvent(instanceId);
		}
		_instanceIdList.Clear();
	}

	private void Awake()
	{
		_cameraWiggle = GetComponentInChildren<CameraWiggle>();
		_cutsceneCamera = GetComponentInChildren<Camera>();
	}

	public override void Play(Action callback, params object[] args)
	{
		int num = 0;
		if (args.Length != 0 && args[0] is int)
		{
			num = (int)args[0];
		}
		string assetPath = _boxPath[num];
		TargetLocator box = GetComponentInChildren<TargetLocator>();
		UnityEngine.Object.Instantiate(_stagePrefab, base.transform);
		UnityEngine.Object.Instantiate(_effectPrefab, base.transform);
		_cameraAnimatingProp = UnityEngine.Object.Instantiate(_cameraPrefab, base.transform).GetComponent<AnimatingModel>();
		Singleton<AssetBundleManager>.Instance().RequestAsset(assetPath, typeof(GameObject), delegate(UnityEngine.Object asset)
		{
			try
			{
				_boxAnimatingProp = KUtility.Instantiate<AnimatingModel>(asset, box.transform);
				RandomBox component = _boxAnimatingProp.GetComponent<RandomBox>();
				_tapeBeginning = component.TapeBeginning;
				_tapeEnd = component.TapeEnd;
				CameraAnimation component2 = _cameraAnimatingProp.GetComponent<CameraAnimation>();
				box.Origin = component2.BoxLocation;
				CameraLocator componentInChildren = GetComponentInChildren<CameraLocator>();
				componentInChildren.OriginGameObject = component2.CameraOrigin;
				componentInChildren.TargetGameObject = component2.CameraTarget;
				StopAllCoroutines();
				StartCoroutine(CoPlay(callback));
			}
			catch (Exception)
			{
				callback();
			}
		});
	}

	private void CalculateTapeLength()
	{
		Vector2 vector = _cutsceneCamera.WorldToScreenPoint(_tapeBeginning.position);
		Vector2 vector2 = _cutsceneCamera.WorldToScreenPoint(_tapeEnd.position);
		_tapeLength = (vector - vector2).magnitude;
	}

	private IEnumerator CoPlay([NotNull] Action callback)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoPlay_003Ed__37(0)
		{
			_003C_003E4__this = this,
			callback = callback
		};
	}

	private void Blur(float size, float delay, float transitionDuration)
	{
		StartCoroutine(CoBlur(size, delay, transitionDuration));
	}

	private IEnumerator CoBlur(float size, float delay, float transitionDuration)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoBlur_003Ed__39(0)
		{
			_003C_003E4__this = this,
			size = size,
			delay = delay,
			transitionDuration = transitionDuration
		};
	}

	public void Unbox(Vector2 delta)
	{
		if (_boxAnimatingProp == null)
		{
			return;
		}
		AnimationState curAnimState = _boxAnimatingProp.CurAnimState;
		if (!(curAnimState == null) && !(curAnimState.name != "looping") && !(delta.x * delta.y < 0f))
		{
			float num = delta.magnitude / _tapeLength;
			int num2 = ((delta.x > 0f) ? 1 : (-1));
			float num3 = (float)num2 * num * _sensitivity;
			if (curAnimState.normalizedTime <= 0f && num3 > 0f)
			{
				_instanceIdList.Add(SoundManager.PlayEvent("ui_random_box_open_begin", SoundPosition.Empty, SoundSwitch.Empty));
			}
			UnboxingSoundSpeed = num3 * 100f * _unboxingSoundSensitivity;
			curAnimState.normalizedTime = Mathf.Clamp(curAnimState.normalizedTime + num3, 0f, 1f);
			curAnimState.speed = num2;
		}
	}

	private void Update()
	{
		_cutsceneCamera.targetTexture = Singleton<MainCamera>.Instance().TargetTexture;
	}

	private void LateUpdate()
	{
		SlowdownUnboxingAnimationSpeed();
	}

	private void SlowdownUnboxingAnimationSpeed()
	{
		if (!(_boxAnimatingProp == null))
		{
			AnimationState curAnimState = _boxAnimatingProp.GetCurAnimState();
			if (!(curAnimState == null) && !(curAnimState.name != "looping") && !(curAnimState.normalizedTime > _unboxingEndPoint))
			{
				UnboxingSoundSpeed = curAnimState.speed;
				curAnimState.speed = Mathf.Lerp(curAnimState.speed, 0f, _slowdownValue);
			}
		}
	}

	private IEnumerator CoPlayAnimation(AnimatingModel target, string animationName, bool loop = false, float beginTime = 0f, float playbackRate = 1f, float exitPoint = 1f)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoPlayAnimation_003Ed__44(0)
		{
			_003C_003E4__this = this,
			target = target,
			animationName = animationName,
			loop = loop,
			beginTime = beginTime,
			playbackRate = playbackRate,
			exitPoint = exitPoint
		};
	}

	public static void Load([NotNull] Action cutsceneEnded, BoxType boxType)
	{
		ResourceSingleton<Loader>.Instance().LoadCutscene(Type.RandomBox, cutsceneEnded, (int)boxType);
	}
}
