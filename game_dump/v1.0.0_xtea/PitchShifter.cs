using PigeonCoopToolkit.Utillities;
using UnityEngine;

public class PitchShifter : MonoBehaviour
{
	public Range pitchRange;

	public AudioSource src;

	private void Start()
	{
		src.pitch = Random.Range(pitchRange.Min, pitchRange.Max);
	}
}
