using Durango.Prologue;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI.Prologue;

public class PrologueOverlayGroup : Singleton<PrologueOverlayGroup>
{
	public GameObject _tunnelEffect;

	public GameObject _whiteOutEffect;

	public float _bgmFadeOutDuration = 3f;

	private void Start()
	{
	}

	public void PlayTunnelEffect()
	{
		_tunnelEffect.SetActive(value: true);
	}

	public void PlayWhiteOutEffect()
	{
		_whiteOutEffect.SetActive(value: true);
		Singleton<PrologueManager>.Instance().StopPrologueSounds(_bgmFadeOutDuration);
	}
}
