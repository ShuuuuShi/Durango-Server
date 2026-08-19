using System.Collections;
using Durango.UI.Prologue;
using Durango.Utils;
using UnityEngine;

namespace Durango.Prologue;

[ExecuteInEditMode]
public class OverlayTunnelEffect : MonoBehaviour
{
	public Vector3 _beginPos;

	public Vector3 _endPos;

	public PrologueOverlayGroup _prologueOverlayGroup;

	public UITexture _targetBlackPanelTexture;

	public UITexture _targetWhitePanelTexture;

	private void OnEnable()
	{
		StartCoroutine(BeginEffects());
	}

	private IEnumerator BeginEffects()
	{
		_targetBlackPanelTexture.transform.position = _beginPos;
		_targetWhitePanelTexture.transform.position = _beginPos;
		_targetBlackPanelTexture.alpha = Singleton<PrologueTunnelController>.Instance()._maxAlphaBlack;
		_targetWhitePanelTexture.alpha = Singleton<PrologueTunnelController>.Instance()._maxAlphaWhite;
		yield return new WaitForSeconds(Singleton<PrologueTunnelController>.Instance()._preDelay);
		yield return StartCoroutine(TunnelStart());
		yield return StartCoroutine(TunnelStartFadeOut());
		yield return new WaitForSeconds(Singleton<PrologueTunnelController>.Instance()._tunnelLeavingDelay);
		yield return StartCoroutine(TunnelEnd());
		yield return StartCoroutine(TunnelEndFadeOut());
		OnFinish();
	}

	private IEnumerator TunnelStart()
	{
		TweenPosition tween = TweenPosition.Begin(_targetBlackPanelTexture.gameObject, Singleton<PrologueTunnelController>.Instance()._tunnelEnteringDuration, _endPos);
		tween.PlayForward();
		yield return new WaitForSeconds(Singleton<PrologueTunnelController>.Instance()._tunnelEnteringDuration);
	}

	private IEnumerator TunnelStartFadeOut()
	{
		TweenAlpha tween = TweenAlpha.Begin(_targetBlackPanelTexture.gameObject, Singleton<PrologueTunnelController>.Instance()._tunnelEnteringFadeOut, 0f);
		tween.method = UITweener.Method.EaseOut;
		tween.PlayForward();
		yield return new WaitForSeconds(Singleton<PrologueTunnelController>.Instance()._tunnelEnteringFadeOut);
	}

	private IEnumerator TunnelEnd()
	{
		TweenPosition tween = TweenPosition.Begin(_targetWhitePanelTexture.gameObject, Singleton<PrologueTunnelController>.Instance()._tunnelLeavingDuration, _endPos);
		tween.PlayForward();
		yield return new WaitForSeconds(Singleton<PrologueTunnelController>.Instance()._tunnelLeavingDuration);
	}

	private IEnumerator TunnelEndFadeOut()
	{
		TweenAlpha tween = TweenAlpha.Begin(_targetWhitePanelTexture.gameObject, Singleton<PrologueTunnelController>.Instance()._tunnelLeavingFadeOut, 0f);
		tween.method = UITweener.Method.EaseOut;
		tween.PlayForward();
		yield return new WaitForSeconds(Singleton<PrologueTunnelController>.Instance()._tunnelLeavingFadeOut);
	}

	private void OnFinish()
	{
		base.gameObject.SetActive(value: false);
		GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.ReturnToSeat);
	}
}
