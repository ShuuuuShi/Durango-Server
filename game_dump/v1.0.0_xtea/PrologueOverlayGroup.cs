using Holoville.HOTween;
using UnityEngine;

public class PrologueOverlayGroup : KSingleton<PrologueOverlayGroup>
{
	public GameObject _tunnelEffect;

	public GameObject _whiteOutEffect;

	public float _bgmFadeOutDuration = 3f;

	private AudioSource _bgmAudioSource;

	private AudioSource _envAudioSource;

	private void Start()
	{
		_bgmAudioSource = GameObject.Find("BGMSound").GetComponent<AudioSource>();
		_envAudioSource = GameObject.Find("ENVSound").GetComponent<AudioSource>();
	}

	public void PlayTunnelEffect()
	{
		_tunnelEffect.SetActive(true);
	}

	public void PlayWhiteOutEffect()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		_whiteOutEffect.SetActive(true);
		TweenParms val = new TweenParms();
		val.Prop("volume", (object)0f);
		val.Ease((EaseType)6);
		HOTween.To((object)_bgmAudioSource, _bgmFadeOutDuration, val);
		HOTween.To((object)_envAudioSource, _bgmFadeOutDuration, val);
	}
}
