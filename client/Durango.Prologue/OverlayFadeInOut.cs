using Durango.UI.Prologue;
using Durango.Utils;
using UnityEngine;

namespace Durango.Prologue;

public class OverlayFadeInOut : MonoBehaviour
{
	public enum EffectType
	{
		FadeIn,
		FadeOut
	}

	public Color _color;

	public PrologueOverlayGroup _prologueOverlayGroup;

	public EffectType _effectType;

	public float _duration = 3f;

	private float _initAlpha;

	private int _currentNarrativeID;

	public Texture2D _whiteTexture;

	public float _curAlpha;

	private float _destAlpha = 1f;

	public GameObject _onFinishEventTarget;

	public string _onFinishCmd;

	[SerializeField]
	private bool _finishPrologueAfterEnd;

	private void OnEnable()
	{
		switch (_effectType)
		{
		case EffectType.FadeIn:
			_initAlpha = 1f;
			_curAlpha = 1f;
			_destAlpha = 0f;
			break;
		case EffectType.FadeOut:
			_initAlpha = 0f;
			_curAlpha = 0f;
			_destAlpha = 1f;
			break;
		}
		DoFadeEffect();
	}

	private void DoFadeEffect()
	{
		TweenTick tweenTick = TweenTick.Begin(base.gameObject, _duration, delegate(float factor, bool isFinished)
		{
			_curAlpha = Mathf.Lerp(_initAlpha, _destAlpha, factor);
		});
		tweenTick.SetOnFinished(OnFinish);
		tweenTick.PlayForward();
	}

	private void OnGUI()
	{
		if (null != _whiteTexture)
		{
			GUI.color = new Color(_color.r, _color.g, _color.b, _curAlpha);
			GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), _whiteTexture);
		}
	}

	private void OnFinish()
	{
		if (null != _onFinishEventTarget && !string.IsNullOrEmpty(_onFinishCmd))
		{
			_onFinishEventTarget.SendMessage(_onFinishCmd);
		}
		if (_finishPrologueAfterEnd)
		{
			Singleton<PrologueManager>.Instance().PrologueFinished();
		}
		if (_effectType == EffectType.FadeIn)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			Invoke("DelayedFinish", 2f);
		}
	}

	private void DelayedFinish()
	{
		base.gameObject.SetActive(value: false);
	}
}
