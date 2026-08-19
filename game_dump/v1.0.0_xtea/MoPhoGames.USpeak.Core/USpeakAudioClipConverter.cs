using MoPhoGames.USpeak.Core.Utils;
using UnityEngine;

namespace MoPhoGames.USpeak.Core;

public class USpeakAudioClipConverter
{
	public static short[] AudioDataToShorts(float[] samples, int channels, float gain = 1f)
	{
		short[] @short = USpeakPoolUtils.GetShort(samples.Length * channels);
		for (int i = 0; i < samples.Length; i++)
		{
			float num = samples[i] * gain;
			if (Mathf.Abs(num) > 1f)
			{
				num = ((!(num > 0f)) ? (-1f) : 1f);
			}
			float num2 = num * 3267f;
			@short[i] = (short)num2;
		}
		return @short;
	}

	public static float[] ShortsToAudioData(short[] data, int channels, int frequency, bool threedimensional, float gain)
	{
		float[] @float = USpeakPoolUtils.GetFloat(data.Length);
		for (int i = 0; i < @float.Length; i++)
		{
			int num = data[i];
			@float[i] = (float)num / 3267f * gain;
		}
		return @float;
	}
}
