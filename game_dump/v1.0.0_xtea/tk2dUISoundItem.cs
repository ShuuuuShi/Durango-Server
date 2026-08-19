using UnityEngine;

[AddComponentMenu("2D Toolkit/UI/tk2dUISoundItem")]
public class tk2dUISoundItem : tk2dUIBaseItemControl
{
	public AudioClip downButtonSound;

	public AudioClip upButtonSound;

	public AudioClip clickButtonSound;

	public AudioClip releaseButtonSound;

	private void OnEnable()
	{
		if (Object.op_Implicit((Object)(object)uiItem))
		{
			if ((Object)(object)downButtonSound != (Object)null)
			{
				uiItem.OnDown += PlayDownSound;
			}
			if ((Object)(object)upButtonSound != (Object)null)
			{
				uiItem.OnUp += PlayUpSound;
			}
			if ((Object)(object)clickButtonSound != (Object)null)
			{
				uiItem.OnClick += PlayClickSound;
			}
			if ((Object)(object)releaseButtonSound != (Object)null)
			{
				uiItem.OnRelease += PlayReleaseSound;
			}
		}
	}

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)uiItem))
		{
			if ((Object)(object)downButtonSound != (Object)null)
			{
				uiItem.OnDown -= PlayDownSound;
			}
			if ((Object)(object)upButtonSound != (Object)null)
			{
				uiItem.OnUp -= PlayUpSound;
			}
			if ((Object)(object)clickButtonSound != (Object)null)
			{
				uiItem.OnClick -= PlayClickSound;
			}
			if ((Object)(object)releaseButtonSound != (Object)null)
			{
				uiItem.OnRelease -= PlayReleaseSound;
			}
		}
	}

	private void PlayDownSound()
	{
		PlaySound(downButtonSound);
	}

	private void PlayUpSound()
	{
		PlaySound(upButtonSound);
	}

	private void PlayClickSound()
	{
		PlaySound(clickButtonSound);
	}

	private void PlayReleaseSound()
	{
		PlaySound(releaseButtonSound);
	}

	private void PlaySound(AudioClip source)
	{
		tk2dUIAudioManager.Instance.Play(source);
	}
}
