using UnityEngine;

[AddComponentMenu("2D Toolkit/UI/Core/tk2dUIAudioManager")]
public class tk2dUIAudioManager : MonoBehaviour
{
	private static tk2dUIAudioManager instance;

	private AudioSource audioSrc;

	public static tk2dUIAudioManager Instance
	{
		get
		{
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)instance == (Object)null)
			{
				instance = Object.FindObjectOfType(typeof(tk2dUIAudioManager)) as tk2dUIAudioManager;
				if ((Object)(object)instance == (Object)null)
				{
					instance = new GameObject("tk2dUIAudioManager").AddComponent<tk2dUIAudioManager>();
				}
			}
			return instance;
		}
	}

	private void Awake()
	{
		if ((Object)(object)instance == (Object)null)
		{
			instance = this;
		}
		else if ((Object)(object)instance != (Object)(object)this)
		{
			Object.Destroy((Object)(object)this);
			return;
		}
		Setup();
	}

	private void Setup()
	{
		if ((Object)(object)audioSrc == (Object)null)
		{
			audioSrc = ((Component)this).gameObject.GetComponent<AudioSource>();
		}
		if ((Object)(object)audioSrc == (Object)null)
		{
			audioSrc = ((Component)this).gameObject.AddComponent<AudioSource>();
			audioSrc.playOnAwake = false;
		}
	}

	public void Play(AudioClip clip)
	{
		audioSrc.PlayOneShot(clip, AudioListener.volume);
	}
}
