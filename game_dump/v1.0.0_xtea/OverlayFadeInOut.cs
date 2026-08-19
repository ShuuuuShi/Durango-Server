using Holoville.HOTween;
using Holoville.HOTween.Core;
using UnityEngine;

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
			_curAlpha = 1f;
			_destAlpha = 0f;
			break;
		case EffectType.FadeOut:
			_curAlpha = 0f;
			_destAlpha = 1f;
			break;
		}
		DoFadeEffect();
	}

	private void DoFadeEffect()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		TweenParms val = new TweenParms();
		val.Prop("_curAlpha", (object)_destAlpha);
		val.Ease((EaseType)0);
		val.OnComplete(new TweenCallback(OnFinish));
		HOTween.To((object)this, _duration, val);
	}

	private void OnGUI()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)null != (Object)(object)_whiteTexture)
		{
			GUI.color = new Color(_color.r, _color.g, _color.b, _curAlpha);
			GUI.DrawTexture(new Rect(0f, 0f, (float)Screen.width, (float)Screen.height), (Texture)(object)_whiteTexture);
		}
	}

	private void OnFinish()
	{
		if ((Object)null != (Object)(object)_onFinishEventTarget && !string.IsNullOrEmpty(_onFinishCmd))
		{
			_onFinishEventTarget.SendMessage(_onFinishCmd);
		}
		if (_finishPrologueAfterEnd)
		{
			KSingleton<PrologueManager>.Instance().PrologueFinished();
		}
		if (_effectType == EffectType.FadeIn)
		{
			((Component)this).gameObject.SetActive(false);
		}
		else
		{
			((MonoBehaviour)this).Invoke("DelayedFinish", 2f);
		}
	}

	private void DelayedFinish()
	{
		((Component)this).gameObject.SetActive(false);
	}
}
