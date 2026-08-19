using Holoville.HOTween;
using UnityEngine;

public class TriggerVolumeController : MonoBehaviour
{
	public float _destVolume = 1f;

	public float _fadeTime = 1f;

	public float _indoorCutOffFrequency = 600f;

	public float _outdoorCutOffFrequency = 22000f;

	public AudioSource _targetAudioSource;

	private float _initVolume;

	private void OnTriggerEnter(Collider other)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		PlayerBehavior component = ((Component)other).gameObject.GetComponent<PlayerBehavior>();
		if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)_targetAudioSource))
		{
			AudioLowPassFilter component2 = ((Component)_targetAudioSource).GetComponent<AudioLowPassFilter>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				TweenParms val = new TweenParms();
				val.Prop("cutoffFrequency", (object)_outdoorCutOffFrequency);
				val.Ease((EaseType)5);
				HOTween.To((object)component2, _fadeTime, val);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		PlayerBehavior component = ((Component)other).gameObject.GetComponent<PlayerBehavior>();
		if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)_targetAudioSource))
		{
			AudioLowPassFilter component2 = ((Component)_targetAudioSource).GetComponent<AudioLowPassFilter>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				TweenParms val = new TweenParms();
				val.Prop("cutoffFrequency", (object)_indoorCutOffFrequency);
				val.Ease((EaseType)5);
				HOTween.To((object)component2, _fadeTime, val);
			}
		}
	}
}
