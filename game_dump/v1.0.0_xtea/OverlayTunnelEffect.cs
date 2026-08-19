using System.Collections;
using Holoville.HOTween;
using UnityEngine;

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
		((MonoBehaviour)this).StartCoroutine(BeginEffects());
	}

	private IEnumerator BeginEffects()
	{
		((Component)_targetBlackPanelTexture).transform.position = _beginPos;
		((Component)_targetWhitePanelTexture).transform.position = _beginPos;
		_targetBlackPanelTexture.alpha = KSingleton<PrologueTunnelController>.Instance()._maxAlphaBlack;
		_targetWhitePanelTexture.alpha = KSingleton<PrologueTunnelController>.Instance()._maxAlphaWhite;
		yield return (object)new WaitForSeconds(KSingleton<PrologueTunnelController>.Instance()._preDelay);
		yield return ((MonoBehaviour)this).StartCoroutine(TunnelStart());
		yield return ((MonoBehaviour)this).StartCoroutine(TunnelStartFadeOut());
		yield return (object)new WaitForSeconds(KSingleton<PrologueTunnelController>.Instance()._tunnelLeavingDelay);
		yield return ((MonoBehaviour)this).StartCoroutine(TunnelEnd());
		yield return ((MonoBehaviour)this).StartCoroutine(TunnelEndFadeOut());
		OnFinish();
	}

	private IEnumerator TunnelStart()
	{
		TweenParms parms = new TweenParms();
		parms.Prop("position", (object)_endPos);
		parms.Ease((EaseType)0);
		HOTween.To((object)((Component)_targetBlackPanelTexture).transform, KSingleton<PrologueTunnelController>.Instance()._tunnelEnteringDuration, parms);
		yield return (object)new WaitForSeconds(KSingleton<PrologueTunnelController>.Instance()._tunnelEnteringDuration);
	}

	private IEnumerator TunnelStartFadeOut()
	{
		TweenParms parms = new TweenParms();
		parms.Prop("alpha", (object)0f);
		parms.Ease((EaseType)5);
		HOTween.To((object)_targetBlackPanelTexture, KSingleton<PrologueTunnelController>.Instance()._tunnelEnteringFadeOut, parms);
		yield return (object)new WaitForSeconds(KSingleton<PrologueTunnelController>.Instance()._tunnelEnteringFadeOut);
	}

	private IEnumerator TunnelEnd()
	{
		TweenParms parms = new TweenParms();
		parms.Prop("position", (object)_endPos);
		parms.Ease((EaseType)0);
		HOTween.To((object)((Component)_targetWhitePanelTexture).transform, KSingleton<PrologueTunnelController>.Instance()._tunnelLeavingDuration, parms);
		yield return (object)new WaitForSeconds(KSingleton<PrologueTunnelController>.Instance()._tunnelLeavingDuration);
	}

	private IEnumerator TunnelEndFadeOut()
	{
		TweenParms parms2 = new TweenParms();
		parms2.Prop("alpha", (object)0f);
		parms2.Ease((EaseType)5);
		HOTween.To((object)_targetWhitePanelTexture, KSingleton<PrologueTunnelController>.Instance()._tunnelLeavingFadeOut, parms2);
		yield return (object)new WaitForSeconds(KSingleton<PrologueTunnelController>.Instance()._tunnelLeavingFadeOut);
	}

	private void OnFinish()
	{
		((Component)this).gameObject.SetActive(false);
		GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.ReturnToSeat);
	}
}
