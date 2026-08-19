using System;
using System.Collections;
using System.Collections.Generic;
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
		if (args.Length > 0 && args[0] is int)
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
		RandomBoxCutsceneUI cutsceneUI = UIManager.FindScript<CutsceneGroup>().CurrentCutsceneUI as RandomBoxCutsceneUI;
		if (cutsceneUI == null)
		{
			callback();
			yield break;
		}
		Blur(0.1f, _blurInFrame, _blurInDuration);
		Blur(0f, _blurOutFrame, _blurOutDuration);
		_instanceIdList.Add(SoundManager.PlayEvent("ui_random_box_start", SoundPosition.Empty, SoundSwitch.Empty));
		StartCoroutine(CoPlayAnimation(_cameraAnimatingProp, "begin"));
		yield return StartCoroutine(CoPlayAnimation(_boxAnimatingProp, "begin"));
		CalculateTapeLength();
		cutsceneUI.StartGuide();
		_cameraWiggle.Play(value: true);
		_instanceIdList.Add(SoundManager.PlayEvent("ui_random_box_open_parameter", SoundPosition.Empty, SoundSwitch.Empty));
		StartCoroutine(CoPlayAnimation(_cameraAnimatingProp, "looping"));
		yield return StartCoroutine(CoPlayAnimation(_boxAnimatingProp, "looping", loop: false, 0f, 0f));
		cutsceneUI.StopGuide();
		_cameraWiggle.Play(value: false);
		_instanceIdList.Add(SoundManager.PlayEvent("ui_random_box_open_end", SoundPosition.Empty, SoundSwitch.Empty));
		UnboxingSoundSpeed = 0f;
		StartCoroutine(CoPlayAnimation(_cameraAnimatingProp, "end"));
		RandomBoxScene randomBoxScene = this;
		RandomBoxScene randomBoxScene2 = this;
		AnimatingModel boxAnimatingProp = _boxAnimatingProp;
		string animationName = "end";
		float endAnimaitionExitPoint = _endAnimaitionExitPoint;
		yield return randomBoxScene.StartCoroutine(randomBoxScene2.CoPlayAnimation(boxAnimatingProp, animationName, loop: false, 0f, 1f, endAnimaitionExitPoint));
		callback();
	}

	private void Blur(float size, float delay, float transitionDuration)
	{
		StartCoroutine(CoBlur(size, delay, transitionDuration));
	}

	private IEnumerator CoBlur(float size, float delay, float transitionDuration)
	{
		CutScenePostProcess blur = _cameraWiggle.GetComponent<CutScenePostProcess>();
		yield return new WaitForSeconds(delay);
		float source = blur.BlurSize;
		float time = 0f;
		while (time <= transitionDuration)
		{
			float blurSize = time / transitionDuration;
			blur.BlurSize = Mathf.Lerp(source, size, blurSize);
			time += Time.deltaTime;
			yield return null;
		}
		blur.BlurSize = size;
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
		if (!(target == null))
		{
			_stopFlag = false;
			float currentAnimationLength = target.Play(animationName, loop, beginTime, playbackRate);
			AnimationState targetAnimationState = target.GetCurAnimState();
			if (!loop)
			{
				targetAnimationState.wrapMode = WrapMode.ClampForever;
			}
			float exitTime = currentAnimationLength * Mathf.Clamp(exitPoint, 0f, 1f);
			yield return new WaitUntil(() => _stopFlag || targetAnimationState.time >= exitTime);
		}
	}

	public static void Load([NotNull] Action cutsceneEnded, BoxType boxType)
	{
		ResourceSingleton<Loader>.Instance().LoadCutscene(Type.RandomBox, cutsceneEnded, (int)boxType);
	}
}
