using System;
using System.Collections;
using Holoville.HOTween;
using Holoville.HOTween.Core;
using UnityEngine;

public class CutSceneDirector : MonoBehaviour
{
	public enum EffectType
	{
		TweenToOrigin,
		TweenFromOrigin
	}

	public enum EndEffect
	{
		None,
		BlackOut,
		WhiteOut
	}

	public IntroCutSceneGroup _introCutSceneGroup;

	public GameObject _targetCutScenePanel;

	public UILabel _targetCaptionPanel;

	public EffectType _effectType;

	public EaseType tweenPositionType = (EaseType)5;

	public EaseType tweenScale = (EaseType)3;

	public string[] _narratives;

	public float _duration = 3f;

	public bool _vibration;

	public float _vibration_beginTime;

	public float _vibration_amplitudeX = 1f;

	public float _vibration_amplitudeY = 1f;

	public float _vibration_period = 1f;

	public float _vibration_duration = 1f;

	private float _vibration_beginTimeAt;

	private float _vibration_endTimeAt;

	public float _fadeInDuration = 1f;

	public float _fadeOutBeginTime = 3f;

	public EndEffect _endEffect;

	private int _currentNarrativeID;

	public Texture2D _blackTexture;

	public Texture2D _whiteTexture;

	public float _endEffectAlpha;

	private Texture2D _curTexture;

	private void Start()
	{
		_currentNarrativeID = 0;
		DoFadeInEffect();
		((MonoBehaviour)this).StartCoroutine(coNarrativeReservation());
		switch (_effectType)
		{
		case EffectType.TweenToOrigin:
			DoTweenToOrigin();
			break;
		case EffectType.TweenFromOrigin:
			DoTweenFromOrigin();
			break;
		}
		if (_vibration)
		{
			_vibration_beginTimeAt = Time.time + _vibration_beginTime;
			_vibration_endTimeAt = _vibration_beginTimeAt + _vibration_duration;
		}
		DoFadeOutEffect();
	}

	public bool SkipCurrentNarrative()
	{
		((MonoBehaviour)this).StopAllCoroutines();
		_currentNarrativeID++;
		((MonoBehaviour)this).StartCoroutine(coNarrativeReservation());
		if (_currentNarrativeID >= _narratives.Length)
		{
			return false;
		}
		return true;
	}

	private void DoFadeInEffect()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		UITexture component = _targetCutScenePanel.GetComponent<UITexture>();
		component.alpha = 0f;
		TweenParms val = new TweenParms();
		val.Prop("alpha", (object)1f);
		val.Ease((EaseType)5);
		HOTween.To((object)component, _fadeInDuration, val);
	}

	private void DoFadeOutEffect()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		if (_endEffect == EndEffect.None)
		{
			_curTexture = null;
			return;
		}
		if (_endEffect == EndEffect.BlackOut)
		{
			_curTexture = _blackTexture;
		}
		else
		{
			_curTexture = _whiteTexture;
		}
		TweenParms val = new TweenParms();
		val.Prop("_endEffectAlpha", (object)1f);
		val.Ease((EaseType)5);
		val.Delay(_fadeOutBeginTime);
		HOTween.To((object)this, _duration - _fadeOutBeginTime, val);
	}

	private void LateUpdate()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if (_vibration && _vibration_beginTimeAt <= Time.time && Time.time < _vibration_endTimeAt)
		{
			float num = Mathf.Sin(Time.time * 2f * (float)Math.PI / _vibration_period);
			Vector3 localPosition = _targetCutScenePanel.transform.localPosition;
			localPosition.x += num * _vibration_amplitudeX;
			localPosition.y += num * _vibration_amplitudeY;
			_targetCutScenePanel.transform.localPosition = localPosition;
		}
	}

	private void DoTweenFromOrigin()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		TweenParms val = new TweenParms();
		val.Prop("localScale", (object)new Vector3(1f, 1f, 1f));
		val.Ease(tweenScale);
		val.OnComplete(new TweenCallback(OnFinish));
		HOTween.From((object)_targetCutScenePanel.transform, _duration, val);
		TweenParms val2 = new TweenParms();
		val2.Prop("localPosition", (object)new Vector3(0f, 0f, 0f));
		val2.Ease(tweenPositionType);
		HOTween.From((object)_targetCutScenePanel.transform, _duration, val2);
	}

	private void DoTweenToOrigin()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		TweenParms val = new TweenParms();
		val.Prop("localScale", (object)new Vector3(1f, 1f, 1f));
		val.Ease(tweenScale);
		val.OnComplete(new TweenCallback(OnFinish));
		HOTween.To((object)_targetCutScenePanel.transform, _duration, val);
		TweenParms val2 = new TweenParms();
		val2.Prop("localPosition", (object)new Vector3(0f, 0f, 0f));
		val2.Ease(tweenPositionType);
		HOTween.To((object)_targetCutScenePanel.transform, _duration, val2);
	}

	private IEnumerator coNarrativeReservation()
	{
		int count = _narratives.Length;
		if (count > 0)
		{
			float dt = _duration / (float)count;
			int len = _narratives.Length;
			for (int i = _currentNarrativeID; i < len; i++)
			{
				string narrative = _narratives[i];
				_targetCaptionPanel.text = narrative;
				yield return (object)new WaitForSeconds(dt);
			}
		}
		_targetCaptionPanel.text = string.Empty;
	}

	public void ForceEndNarrative()
	{
		((MonoBehaviour)this).StopAllCoroutines();
		((Component)_targetCaptionPanel).gameObject.SetActive(false);
	}

	private void OnFinish()
	{
		if (Object.op_Implicit((Object)(object)_introCutSceneGroup))
		{
			_introCutSceneGroup.ShowNextScene();
		}
	}

	private void OnGUI()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)null != (Object)(object)_curTexture)
		{
			GUI.color = new Color(1f, 1f, 1f, _endEffectAlpha);
			GUI.DrawTexture(new Rect(0f, 0f, (float)Screen.width, (float)Screen.height), (Texture)(object)_curTexture);
		}
	}
}
